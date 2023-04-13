#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SQLSyncModule2/SQLSyncModule.csproj", "SQLSyncModule2/"]
RUN dotnet restore "SQLSyncModule2/SQLSyncModule.csproj"
COPY . .
WORKDIR "/src/SQLSyncModule2"
RUN dotnet build "SQLSyncModule.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SQLSyncModule.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SQLSyncModule.dll"]