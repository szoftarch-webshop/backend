# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build-env
WORKDIR /App

# Copy everything
COPY . ./

# Install Entity Framework CLI
RUN dotnet tool install --global dotnet-ef

# Ensure PATH includes dotnet tools
ENV PATH="$PATH:/root/.dotnet/tools"

# Restore dependencies
RUN dotnet restore

# Add initial migration
RUN dotnet ef migrations add Init

# Build and publish a release
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff
WORKDIR /App

# Copy the built application from the build stage
COPY --from=build-env /App/out .

# Run the application
ENTRYPOINT ["dotnet", "DotNet.Docker.dll"]
