# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BuildingBlocks/ BuildingBlocks/
COPY Services/Auth/Membera.Auth.Domain/ Services/Auth/Membera.Auth.Domain/
COPY Services/Auth/Membera.Auth.Application/ Services/Auth/Membera.Auth.Application/
COPY Services/Merchant/ Services/Merchant/

RUN dotnet restore "Services/Merchant/Membera.Merchant.Api/Membera.Merchant.Api.csproj"
RUN dotnet publish "Services/Merchant/Membera.Merchant.Api/Membera.Merchant.Api.csproj" -c Release -o /app/publish

# ---- Final stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Membera.Merchant.Api.dll"]
