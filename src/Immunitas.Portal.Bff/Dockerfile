# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ------------------------------------------------------------
# BASE STAGE
# ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER root

# Установка Python
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        python3 \
        python3-dev \
        python3-venv \
        python3-pip \
        ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Проверка
RUN python3 --version && \
    python3 -c "import ssl; print(ssl.OPENSSL_VERSION)" && \
    find /usr -name "libpython*"

# Создаем виртуальное окружение
RUN python3 -m venv /opt/venv

# Используем venv по умолчанию
ENV PATH="/opt/venv/bin:${PATH}"

# Обновляем pip и устанавливаем библиотеки
RUN python -m pip install --upgrade pip && \
    python -m pip install --no-cache-dir \
        numpy \
        scipy \
        scikit-learn

# Если путь отличается — заменить после проверки
ENV PYTHONNET_PYDLL=/usr/lib/x86_64-linux-gnu/libpython3.12.so.1.0

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

USER $APP_UID

# ------------------------------------------------------------
# BUILD STAGE
# ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Immunitas.Portal.Bff/Immunitas.Portal.Bff.csproj", "Immunitas.Portal.Bff/"]
COPY ["Immunitas.Application/Immunitas.Application.csproj", "Immunitas.Application/"]
COPY ["Immunitas.Domain/Immunitas.Domain.csproj", "Immunitas.Domain/"]
COPY ["Immunitas.Generators/Immunitas.Generators.csproj", "Immunitas.Generators/"]
COPY ["Immunitas.Generators.Attributes/Immunitas.Generators.Attributes.csproj", "Immunitas.Generators.Attributes/"]
COPY ["Immunitas.Persistence/Immunitas.Persistence.csproj", "Immunitas.Persistence/"]
COPY ["Immunitas.Portal.UI/Immunitas.Portal.UI.csproj", "Immunitas.Portal.UI/"]
COPY ["Immunitas.Portal.UI.ApiProxies/Immunitas.Portal.UI.ApiProxies.csproj", "Immunitas.Portal.UI.ApiProxies/"]

RUN dotnet restore "./Immunitas.Portal.Bff/Immunitas.Portal.Bff.csproj"

COPY . .

WORKDIR "/src/Immunitas.Portal.Bff"

RUN dotnet build "./Immunitas.Portal.Bff.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# ------------------------------------------------------------
# PUBLISH STAGE
# ------------------------------------------------------------
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Immunitas.Portal.Bff.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ------------------------------------------------------------
# FINAL STAGE
# ------------------------------------------------------------
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Immunitas.Portal.Bff.dll"]
