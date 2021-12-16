FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY src/Pavlok.Bridge/Pavlok.Bridge.csproj /source/src/Pavlok.Bridge/
WORKDIR /source/src/Pavlok.Bridge/
RUN dotnet restore

COPY . /source/
ARG BUILD_VERSION=0.0.1
RUN dotnet publish -c Release -o /app -p:Version=${BUILD_VERSION}

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pavlok.Bridge.dll"]