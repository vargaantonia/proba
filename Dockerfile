FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IngatlanokBackend.csproj", "."]
RUN dotnet restore "./IngatlanokBackend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./IngatlanokBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IngatlanokBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IngatlanokBackend.dll"]


# .NET SDK a buildhez
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Az összes fájl másolása a konténerbe
COPY . .

# Restore és build
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# .NET runtime a futtatáshoz
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Alkalmazás indítása
ENTRYPOINT ["dotnet", "ingatlanberlesiplatform.dll"]
