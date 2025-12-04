FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8888

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Devisions.Web/Devisions.Web.csproj", "src/Devisions.Web/"]
COPY ["src/Devisions.Application/Devisions.Application.csproj", "src/Devisions.Application/"]
COPY ["src/Devisions.Contracts/Devisions.Contracts.csproj", "src/Devisions.Contracts/"]
COPY ["src/Devisions.Domain/Devisions.Domain.csproj", "src/Devisions.Domain/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/Devisions.Infrastructure.Postgres/Devisions.Infrastructure.Postgres.csproj", "src/Devisions.Infrastructure.Postgres/"]

COPY ["Directory.Build.props", "./"]

RUN dotnet restore "src/Devisions.Web/Devisions.Web.csproj"
COPY . .
WORKDIR "/src/src/Devisions.Web"
RUN dotnet build "./Devisions.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Devisions.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Devisions.Web.dll"]
