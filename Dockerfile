# =============================
# BUILD STAGE
# =============================
FROM mcr.microsoft.com/dotnet/sdk:2.1 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore
RUN dotnet restore Elite.Task.Microservice/Elite.Task.Microservice.csproj

# Publish
RUN dotnet publish Elite.Task.Microservice/Elite.Task.Microservice.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================
# RUNTIME STAGE
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:2.1
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://0.0.0.0:80

EXPOSE 80

ENTRYPOINT ["dotnet", "Elite.Task.Microservice.dll"]
