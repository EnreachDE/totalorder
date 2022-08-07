# Build, test and deploy

To build the app, either open to.build.sln in Visual Studio or use

``` shell
dotnet build to.build.sln
```

To run unit test, use the test runner in Visual Studio or run

``` shell
dotnet test to.build.sln
```

## Deployment via github actions

The repository defines a github action workflow which deploys the web app docker container to totalorder.de. The docker connection is secured via TLS. TLS client certificates and commit encrypted in the repo. They are encrypted via

```shell
gpg --symmetric --cipher-algo AES256 cert.pem
````

The passphrase is stored as secret in the github repo settings.
