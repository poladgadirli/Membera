# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BuildingBlocks/ BuildingBlocks/
COPY Services/Auth/ Services/Auth/

RUN dotnet restore "Services/Auth/Membera.Auth.Api/Membera.Auth.Api.csproj"
RUN dotnet publish "Services/Auth/Membera.Auth.Api/Membera.Auth.Api.csproj" -c Release -o /app/publish

# ---- Final stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Membera.Auth.Api.dll"]
