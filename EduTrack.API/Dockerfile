# ═════════════════════════ EduTrack.API ═════════════════════════
# Multi-stage build — SDK build → ASP.NET runtime
# Dùng cho Render / Railway / Docker Hub.
# ════════════════════════════════════════════════════════════════

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để cache restore layer
COPY Base.Lib/*.csproj          Base.Lib/
COPY EduTrack.API/*.csproj      EduTrack.API/
RUN dotnet restore EduTrack.API/EduTrack.API.csproj

# Copy source + publish
COPY Base.Lib/      Base.Lib/
COPY EduTrack.API/  EduTrack.API/
RUN dotnet publish EduTrack.API/EduTrack.API.csproj \
    -c Release -o /app \
    --no-restore /p:UseAppHost=false

# ─── Runtime stage ───
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render/Railway sẽ set PORT env var lúc runtime — Program.cs bind theo PORT
# Default 8080 nếu chưa set
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "EduTrack.API.dll"]
