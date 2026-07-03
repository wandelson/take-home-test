FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/src/src.sln ./
COPY backend/src/Fundo.Domain/Fundo.Domain.csproj Fundo.Domain/
COPY backend/src/Fundo.Application/Fundo.Application.csproj Fundo.Application/
COPY backend/src/Fundo.Infrastructure/Fundo.Infrastructure.csproj Fundo.Infrastructure/
COPY backend/src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj Fundo.Applications.WebApi/

RUN dotnet restore Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj

COPY backend/src/ ./
RUN dotnet publish Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]
