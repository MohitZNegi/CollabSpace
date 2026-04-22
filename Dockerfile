# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the API project file first
# Docker caches this layer so restore only reruns when dependencies change
COPY CollabSpace.csproj .

# Restore only the API project — test project is never involved
RUN dotnet restore CollabSpace.csproj

# Copy the rest of the API source code
# Excludes anything in .dockerignore
COPY . .

# Publish the API project to the /app/out folder
RUN dotnet publish CollabSpace.csproj \
    --no-restore \
    -c Release \
    -o /app/out

# Stage 2: Runtime
# Use the smaller runtime image, not the full SDK
# This makes your deployed container significantly smaller
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy only the published output from the build stage
COPY --from=build /app/out .

# Railway injects the PORT environment variable
# ASP.NET Core reads ASPNETCORE_URLS to know where to listen
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080

ENTRYPOINT ["dotnet", "CollabSpace.dll"]