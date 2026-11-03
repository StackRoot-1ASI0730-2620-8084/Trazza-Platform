FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app
COPY Trazza.Platform/*.csproj Trazza.Platform/
RUN dotnet restore ./Trazza.Platform
COPY . .
RUN dotnet publish ./Trazza.Platform -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Trazza.Platform.dll"]
