FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["AspireHost/AspireHost/AspireHost.csproj", "AspireHost/"]
COPY ["Ecom.Users/src/Ecom.Users.API/Ecom.Users.API.csproj", "Ecom.Users/src/Ecom.Users.API/"]
COPY ["EcomCore/src/EcomCore.API/EcomCore.API.csproj", "EcomCore/src/EcomCore.API/"]
RUN dotnet restore "AspireHost/AspireHost.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AspireHost/AspireHost"
RUN dotnet build "AspireHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspireHost.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspireHost.dll"]