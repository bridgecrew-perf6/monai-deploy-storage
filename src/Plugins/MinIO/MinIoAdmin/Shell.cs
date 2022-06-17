﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Diagnostics;
using Amazon.SecurityToken.Model;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.Storage.Minio.Extensions;
using Monai.Deploy.Storage.MinIO.MinIoAdmin.Interfaces;
using Monai.Deploy.Storage.MinIO.MinIoAdmin.Models;
using Monai.Deploy.Storage.S3Policy;
using Monai.Deploy.Storage.S3Policy.Policies;

namespace Monai.Deploy.Storage.MinIO.MinIoAdmin
{
    public class Shell : IMinioAdmin
    {
        private readonly string _executableLocation;
        private readonly string _serviceName;
        private readonly string _endpoint;
        private readonly string _accessKey;
        private readonly string _secretKey;

        public Shell(IOptions<StorageServiceConfiguration> options, ILogger<MinIoStorageService> logger)
        {
            Guard.Against.Null(options, nameof(options));
            Guard.Against.Null(logger, nameof(logger));

            var configuration = options.Value;
            ValidateConfiguration(configuration);

            _executableLocation = options.Value.Settings[ConfigurationKeys.McExecutablePath];
            _serviceName = options.Value.Settings[ConfigurationKeys.McServiceName];
            _endpoint = options.Value.Settings[ConfigurationKeys.EndPoint];
            _accessKey = options.Value.Settings[ConfigurationKeys.AccessKey];
            _secretKey = options.Value.Settings[ConfigurationKeys.AccessToken];
        }

        private void ValidateConfiguration(StorageServiceConfiguration configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));

            foreach (var key in ConfigurationKeys.McRequiredKeys)
            {
                if (!configuration.Settings.ContainsKey(key))
                {
                    throw new ConfigurationException($"IMinioAdmin Shell is missing configuration for {key}.");
                }
            }
        }

        private string CreateUserCmd(string username, string secretKey) => $"admin user add {_serviceName} {username} {secretKey}";

        private string SetConnectionCmd() => $"alias set {_serviceName} http://{_endpoint} {_accessKey} {_secretKey}";

        private string GetConnectionsCmd() => "alias list";

        private string GetUsersCmd() => "admin user list minio";

        public bool SetPolicy(MinioPolicyType policyType, List<string> policies, string itemName)
        {
            var policiesStr = string.Join(',', policies);
            var setPolicyCmd = $"admin policy set {_serviceName} {policiesStr} {policyType.ToString().ToLower()}={itemName}";
            var result = Execute(setPolicyCmd);

            var expectedResult = $"Policy `{policiesStr}` is set on {policyType.ToString().ToLower()} `{itemName}`";
            if (!result.Any(r => r.Contains(expectedResult)))
            {
                return false;
            }
            return true;
        }

        private List<string> Execute(string cmd)
        {
            if (cmd.StartsWith("mc"))
            {
                throw new InvalidOperationException($"Incorrect command \"{cmd}\"");
            }

            using (var process = CreateProcess(cmd))
            {
                var (lines, errors) = RunProcess(process);
                if (errors.Any())
                {
                    throw new InvalidOperationException($"Unknown Error {errors.SelectMany(e => e)}");
                }

                return lines;
            }
        }

        private static (List<string> Output, List<string> Errors) RunProcess(Process process)
        {
            var output = new List<string>();
            var errors = new List<string>();
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                if (line == null) continue;
                output.Add(line);
            }
            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                if (line == null) continue;
                errors.Add(line);
            }

            process.WaitForExit();
            return (output, errors);
        }

        private Process CreateProcess(string cmd)
        {
            var startinfo = new ProcessStartInfo()
            {
                FileName = _executableLocation,
                Arguments = cmd,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process()
            {
                StartInfo = startinfo
            };

            return process;
        }

        public bool HasConnection()
        {
            var result = Execute(GetConnectionsCmd());
            return result.Any(r => r.Contains(_serviceName));
        }

        public bool SetConnection()
        {
            if (HasConnection())
            {
                return true;
            }
            var result = Execute(SetConnectionCmd());
            if (result.Any(r => r.Contains($"Added `{_serviceName}` successfully.")))
            {
                return true;
            }
            return false;
        }

        public bool UserAlreadyExists(string username)
        {
            var result = Execute(GetUsersCmd());
            return result.Any(r => r.Contains(username));
        }

        public void RemoveUser(string username)
        {
            var result = Execute($"admin user remove {_serviceName} {username}");

            if (!result.Any(r => r.Contains($"Removed user `{username}` successfully.")))
            {
                throw new InvalidOperationException("Unable to remove user");
            }
        }

        public Credentials CreateReadOnlyUser(string username, PolicyRequest[] policyRequests)
        {
            if (UserAlreadyExists(username))
            {
                throw new InvalidOperationException("User already exists");
            }

            var credentials = new Credentials();
            var userSecretKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            credentials.SecretAccessKey = userSecretKey;
            credentials.AccessKeyId = username;

            var result = Execute(CreateUserCmd(username, userSecretKey));

            if (!result.Any(r => r.Contains($"Added user `{username}` successfully.")))
            {
                RemoveUser(username);
                throw new InvalidOperationException($"Unknown Output {result.SelectMany(e => e)}");
            }

            var minioPolicies = new List<string>()
            {
                MinioPolicy.ReadOnly.GetString()
            };

            Task.Run(() => CreatePolicyAsync(policyRequests, username)).Wait();
            var setPolicyResult = SetPolicy(MinioPolicyType.User, minioPolicies, credentials.AccessKeyId);
            if (!setPolicyResult)
            {
                RemoveUser(username);
                throw new InvalidOperationException("Failed to set policy, user has been removed");
            }

            return credentials;
        }

        /// <summary>
        /// Admin policy command requires json file for policy so we create file
        /// and remove it after setting the admin policy for the user.
        /// </summary>
        /// <param name="policyRequests"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task CreatePolicyAsync(PolicyRequest[] policyRequests, string username)
        {
            await CreatePolicyFile(policyRequests, username).ConfigureAwait(false);
            var result = Execute($"admin policy {_serviceName} pol_{username} {username}.json");
            if (!result.Any(r => r.Contains($"Added policy `pol_{username}` successfully.")))
            {
                RemoveUser(username);
                File.Delete($"{username}.json");
                throw new InvalidOperationException("Failed to create policy, user has been removed");
            }
            File.Delete($"{username}.json");
        }

        private static async Task CreatePolicyFile(PolicyRequest[] policyRequests, string username)
        {
            var policy = PolicyExtensions.ToPolicy(policyRequests);
            var jsonPolicy = policy.ToJson();
            var lines = new List<string>() { jsonPolicy };
            await File.WriteAllLinesAsync($"{username}.json", lines).ConfigureAwait(false);
        }
    }
}