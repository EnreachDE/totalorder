# TotalOrder

![badge](https://github.com/swyx/totalorder/workflows/Docker%20Image%20CI/badge.svg)

Swyx TotalOrder (STO) brings honest subjectivity back into ordering backlog issues or any other list of items. STO is the tool of choice to enable relative estimation of fixed teams or larger temporary groups.

## Rough functionality

If you want to get an evaluation for a list of items, register with STO, create a voting distribute the voting url or QR code to your team. The url or QA code  gives team members access to the voting and allows them to arrange the items on the dimension in question.

The voting owner sees the calculated overall order as the result. Voting owners must register and log in to create and manage votings. Per default anybody with the voting link or QR code can order the items. No account necessary. However, if you create a voting with option _one voting per user_ everybody who wants to vote needs to create an account and login.

## totalorder.de

The Totalorder voting web app is hosted at [totalorder.de](http://totalorder.de)

## Build

The web app is an asp.net core application intended to run as a docker container. To build it, run

``` shell
docker-compose build
```

This also runs the totalorder unit tests.

To run the app use

``` shell
docker-compose build
```

For building and running without docker use

``` shell
dotnet build to.build.sln
```

``` shell
dotnet test to.build.sln
```

``` shell
dotnet run src/to.frontend/to.frontend.csproj
```
