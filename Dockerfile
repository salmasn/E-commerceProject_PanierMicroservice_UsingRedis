# ====================================
# Stage 1: Build
# ====================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier le fichier .csproj et restaurer les dépendances
COPY ["PanierService.csproj", "./"]
RUN dotnet restore "PanierService.csproj"

# Copier tout le code source
COPY . .

# Build du projet en mode Release
RUN dotnet build "PanierService.csproj" -c Release -o /app/build

# ====================================
# Stage 2: Publish
# ====================================
FROM build AS publish
RUN dotnet publish "PanierService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ====================================
# Stage 3: Runtime
# ====================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Exposer le port (Railway utilise la variable $PORT)
EXPOSE 8080

# Copier les fichiers publiés
COPY --from=publish /app/publish .

# Variables d'environnement
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Point d'entrée
ENTRYPOINT ["dotnet", "PanierService.dll"]