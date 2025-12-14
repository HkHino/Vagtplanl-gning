# ================
# Build stage
# ================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiér projektet
COPY "Vagtplanlægning/" "Vagtplanlægning/"

# Restore
RUN dotnet restore "Vagtplanlægning/Vagtplanlægning.csproj"

# Build + publish
RUN dotnet publish "Vagtplanlægning/Vagtplanlægning.csproj" -c Release -o /app/publish

# ==================
# Runtime stage
# ==================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Vagtplanlægning.dll"]