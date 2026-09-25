# syntax=docker/dockerfile:1

# ---- Stage 1: build & publish ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files first so NuGet restore is cached
# between builds as long as no .csproj changes.
COPY ["Web.API/Web.API.csproj", "Web.API/"]
COPY ["Web.Infrastructure/Web.Infrastructure.csproj", "Web.Infrastructure/"]
COPY ["Web.Application/Web.Application.csproj", "Web.Application/"]
COPY ["Web.Domain/Web.Domain.csproj", "Web.Domain/"]
RUN dotnet restore "Web.API/Web.API.csproj"

# Now copy the actual source and publish.
COPY Web.API/ Web.API/
COPY Web.Infrastructure/ Web.Infrastructure/
COPY Web.Application/ Web.Application/
COPY Web.Domain/ Web.Domain/

RUN dotnet publish "Web.API/Web.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# .NET 8+ images ship a built-in non-root "app" user — use it.
USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.API.dll"]
