# Estágio base
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Msh.Api/Msh.Api.csproj", "src/Msh.Api/"]
RUN dotnet restore "src/Msh.Api/Msh.Api.csproj"

# Copia o restante dos arquivos
COPY . .
WORKDIR "/src/src/Msh.Api" 
RUN dotnet build "Msh.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Msh.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Msh.Api.dll"]
