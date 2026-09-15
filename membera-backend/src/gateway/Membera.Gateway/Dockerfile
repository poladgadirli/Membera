# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY gateway/Membera.Gateway/ gateway/Membera.Gateway/

RUN dotnet restore "gateway/Membera.Gateway/Membera.Gateway.csproj"
RUN dotnet publish "gateway/Membera.Gateway/Membera.Gateway.csproj" -c Release -o /app/publish

# ---- Final stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Membera.Gateway.dll"]
