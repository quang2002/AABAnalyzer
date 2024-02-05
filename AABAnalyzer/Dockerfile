FROM quang2002/t1-worker AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AABAnalyzer/AABAnalyzer.csproj", "AABAnalyzer/"]
RUN dotnet restore "AABAnalyzer/AABAnalyzer.csproj"
COPY . .
WORKDIR "/src/AABAnalyzer"
RUN dotnet build "AABAnalyzer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AABAnalyzer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AABAnalyzer.dll"]
