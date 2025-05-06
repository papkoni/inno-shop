FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ["APIGateway/APIGateway/APIGateway.csproj", "APIGateway/APIGateway/"]

RUN dotnet restore "APIGateway/APIGateway/APIGateway.csproj"

COPY . .

RUN dotnet publish "APIGateway/APIGateway/APIGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 80

RUN apk add --no-cache ca-certificates \
    && update-ca-certificates

COPY --from=build /app/publish . 

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "APIGateway.dll"]