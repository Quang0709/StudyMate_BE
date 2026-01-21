# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["StudyMate_BE.sln", "./"]
COPY ["StudyMate_BE/StudyMate_BE.csproj", "StudyMate_BE/"]
COPY ["Repository/Repository.csproj", "Repository/"]
COPY ["Service/Service.csproj", "Service/"]

# Restore dependencies
RUN dotnet restore "StudyMate_BE.sln"

# Copy everything else and build
COPY . .
WORKDIR "/src/StudyMate_BE"
RUN dotnet build "StudyMate_BE.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "StudyMate_BE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Set environment to use Docker settings
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StudyMate_BE.dll"]
