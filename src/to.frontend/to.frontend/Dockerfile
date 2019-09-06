FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY to.build.sln ./
COPY src src/
COPY docker-compose.dcproj ./docker-compose.dcproj
RUN dotnet restore -nowarn:msb3202,nu1503 ./to.build.sln && \
    dotnet build -c release -o /src/contracts/netstandard2.0/ src/to.contracts/to.contracts/to.contracts.csproj && \
    dotnet build -c Release to.build.sln

RUN dotnet test to.build.sln 
WORKDIR /src/src/to.frontend/to.frontend

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base as final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "to.frontend.dll"]
