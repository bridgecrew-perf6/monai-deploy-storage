﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Amazon.SecurityToken;

namespace Monai.Deploy.Storage.MinIO
{
    public interface IAmazonSecurityTokenServiceClientFactory
    {
        AmazonSecurityTokenServiceClient GetClient();
    }
}
