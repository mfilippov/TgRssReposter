# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
WORKDIR /app

ARG TARGETPLATFORM

COPY TgRssReposter/TgRssReposter.csproj ./
COPY TgRssReposter/*.cs ./
RUN dotnet restore

RUN case "${TARGETPLATFORM}" in \
    "linux/arm64" ) dotnet publish -c Release -o out -r linux-arm64 --self-contained true;; \
    "linux/amd64" ) dotnet publish -c Release -o out -r linux-x64 --self-contained true;; \
    * ) echo "Unknown TARGETPLATFORM: '${TARGETPLATFORM}'" && exit 2;; \
    esac

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["TgRssReposter"]