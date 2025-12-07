# ===========================
# 1. Build Stage
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# copy csproj and restore dependencies
COPY *.sln ./
COPY */*.csproj ./
RUN for file in *.csproj; do mkdir -p ${file%.*} && mv $file ${file%.*}/; done
RUN dotnet restore

# copy the rest of the project files
COPY . .

# build the project
RUN dotnet publish -c Release -o /app/publish


# ===========================
# 2. Runtime Stage
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# expose port (change if needed)
EXPOSE 8080

# run application
ENTRYPOINT ["dotnet", "Vagtplanlægning.dll"]
