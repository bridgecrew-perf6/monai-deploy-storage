<p align="center">
<img src="https://raw.githubusercontent.com/Project-MONAI/MONAI/dev/docs/images/MONAI-logo-color.png" width="50%" alt='project-monai'>
</p>

💡 If you want to know more about MONAI Deploy WG vision, overall structure, and guidelines, please read [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) first.

# MONAI Deploy Storage

[![License](https://img.shields.io/badge/license-Apache%202.0-green.svg)](LICENSE)
[![codecov](https://codecov.io/gh/Project-MONAI/monai-deploy-storage/branch/master/graph/badge.svg?token=a7lu3x6kEo)](https://codecov.io/gh/Project-MONAI/monai-deploy-storage)
[![ci](https://github.com/Project-MONAI/monai-deploy-storage/actions/workflows/ci.yml/badge.svg)](https://github.com/Project-MONAI/monai-deploy-storage/actions/workflows/ci.yml)
[![Nuget](https://img.shields.io/nuget/dt/Monai.Deploy.Storage?label=NuGet%20Download)](https://www.nuget.org/packages/Monai.Deploy.Storage/)

The MONAI Deploy Storage library for MONAI Deploy clinical data pipelines system enables users to extend the system to external storage services by implementing the [IStorageService API](src/Storage/API/IStorageService.cs). The API allows users to plug in any other storage services, such as [AWS S3](https://aws.amazon.com/pm/serv-s3/) and [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/).

Currently supported storage services:

- [MinIO](https://min.io/)*

\* Services provided may not be free or requires special license agreements.  Please refer to the service providers' website for additional terms and conditions.

If you would like to use a storage service provider not listed above, please file an [issue](https://github.com/Project-MONAI/monai-deploy-storage/issues) and contribute to the repository.

## Releases

The MONAI Deploy Storage library is release in NuGet format which is available on both [NuGet.Org](https://www.nuget.org/packages/Monai.Deploy.Storage/) and [GitHub](https://github.com/Project-MONAI/monai-deploy-storage/packages/1350678).

### Official Builds

Official releases are built and released from the `main` branch.

### RC Builds

Release candidates are built and released from the `release/*` branches.

### Development Builds

Development builds are made from all branches except the `main` branch and the `release/*` branches.  The NuGet packages are released to [GitHub](https://github.com/Project-MONAI/monai-deploy-storage/packages/1350678) only.

## Contributing

For guidance on making a contribution to MONAI Deploy Workflow Manager, see the [contributing guidelines](https://github.com/Project-MONAI/monai-deploy/blob/main/CONTRIBUTING.md).

## Community

To participate, please join the MONAI Deploy Workflow Manager weekly meetings on the [calendar](https://calendar.google.com/calendar/u/0/embed?src=c_954820qfk2pdbge9ofnj5pnt0g@group.calendar.google.com&ctz=America/New_York) and review the [meeting notes](https://docs.google.com/document/d/1ipCGxlq0Pd7Xnil2zGa1va99K7VbdhwcJiqel9aWzyA/edit?usp=sharing).

Join the conversation on Twitter [@ProjectMONAI](https://twitter.com/ProjectMONAI) or join our [Slack channel](https://forms.gle/QTxJq3hFictp31UM9).

Ask and answer questions over on [MONAI Deploy Storage's GitHub Discussions tab](https://github.com/Project-MONAI/monai-deploy-storage/discussions).

## Links

- Website: <https://monai.io>
- Code: <https://github.com/Project-MONAI/monai-deploy-storage>
- Project tracker: <https://github.com/Project-MONAI/monai-deploy-storage/projects>
- Issue tracker: <https://github.com/Project-MONAI/monai-deploy-storage/issues>
- Test status: <https://github.com/Project-MONAI/monai-deploy-storage/actions>
