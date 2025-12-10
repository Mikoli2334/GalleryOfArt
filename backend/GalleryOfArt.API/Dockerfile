# -------- runtime image --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# -------- build image --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копируем весь репозиторий внутрь контейнера
COPY . .

# восстанавливаем зависимости по solution-файлу
RUN dotnet restore ./GalleryOfART.sln

# Публикуем именно API-проект (ВАЖНО: правильный путь и регистр букв!)
RUN dotnet publish ./backend/GalleryOfArt.API/GalleryOfArt.API.csproj -c Release -o /app/publish

# -------- final image --------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GalleryOfArt.API.dll"]