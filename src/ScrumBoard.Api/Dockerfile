# Construccion desde la raiz del repositorio:
#   docker build -f src/ScrumBoard.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
# QuestPDF renderiza via SkiaSharp y necesita fontconfig en la imagen de runtime.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Los csproj se copian primero para que la capa de restore se reutilice mientras
# no cambien las dependencias.
COPY ["src/ScrumBoard.Domain/ScrumBoard.Domain.csproj", "src/ScrumBoard.Domain/"]
COPY ["src/ScrumBoard.Application/ScrumBoard.Application.csproj", "src/ScrumBoard.Application/"]
COPY ["src/ScrumBoard.Infrastructure/ScrumBoard.Infrastructure.csproj", "src/ScrumBoard.Infrastructure/"]
COPY ["src/ScrumBoard.Api/ScrumBoard.Api.csproj", "src/ScrumBoard.Api/"]
RUN dotnet restore "src/ScrumBoard.Api/ScrumBoard.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/ScrumBoard.Api/ScrumBoard.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ScrumBoard.Api.dll"]
