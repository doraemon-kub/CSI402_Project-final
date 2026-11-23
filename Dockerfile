# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy all source files and build release
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Set default port to 8080 (standard for .NET container & Render) and Thailand Timezone
ENV ASPNETCORE_HTTP_PORTS=8080
ENV TZ=Asia/Bangkok
EXPOSE 8080

ENTRYPOINT ["dotnet", "CSI402_Project-final.dll"]
