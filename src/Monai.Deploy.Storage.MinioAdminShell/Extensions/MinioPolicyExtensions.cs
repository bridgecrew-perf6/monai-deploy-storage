﻿using Monai.Deploy.Storage.MinioAdmin.Models;

namespace Monai.Deploy.Storage.MinioAdmin.Extensions
{
    public static class MinioPolicyExtensions
    {
        public static string GetString(this MinioPolicy policy) => policy switch
        {
            MinioPolicy.ConsoleAdmin => "consoleAdmin",
            MinioPolicy.Diagnostics => "diagnostics",
            MinioPolicy.ReadOnly => "readonly",
            MinioPolicy.Readwrite => "readwrite",
            MinioPolicy.WriteOnly => "writeonly",
            _ => "",
        };
    }
}
