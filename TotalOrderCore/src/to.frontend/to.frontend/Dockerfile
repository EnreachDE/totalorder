FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY to.build.sln ./
COPY src/to.frontend/to.frontend/to.frontend.csproj src/to.frontend/to.frontend/
COPY src/to.backlogrepo/to.backlogrepo/to.backlogrepo.csproj src/to.backlogrepo/to.backlogrepo/
COPY src/to.contracts/to.contracts/to.contracts.csproj src/to.contracts/to.contracts/
COPY src/to.totalorder/to.totalorder/to.totalorder.csproj src/to.totalorder/to.totalorder/
COPY src/to.requesthandler/to.requesthandler/to.requesthandler.csproj src/to.requesthandler/to.requesthandler/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/src/to.frontend/to.frontend
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "to.frontend.dll"]
