#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
ENV RABBITMQ_HOST localhost
ENV RABBITMQ_PORT 31672
ENV ASPNETCORE_URLS=http://+:5000


FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["cartapi.csproj", "."]
RUN dotnet restore "./cartapi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "cartapi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cartapi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cartapi.dll"]
