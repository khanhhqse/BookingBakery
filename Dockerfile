# Sử dụng SDK .NET 8.0 để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép các file .csproj vào container trước để restore dependencies (tận dụng Docker caching)
COPY ["BookingBakery/BookingBakery.csproj", "BookingBakery/"]
COPY ["BookingBakery.Application/BookingBakery.Application.csproj", "BookingBakery.Application/"]
COPY ["BookingBakery.Domain/BookingBakery.Domain.csproj", "BookingBakery.Domain/"]
COPY ["BookingBakery.Infrastructure/BookingBakery.Infrastructure.csproj", "BookingBakery.Infrastructure/"]

# Restore các NuGet packages
RUN dotnet restore "BookingBakery/BookingBakery.csproj"

# Sao chép toàn bộ mã nguồn vào container và tiến hành build
COPY . .
WORKDIR "/src/BookingBakery"
RUN dotnet build "BookingBakery.csproj" -c Release -o /app/build

# Publish ứng dụng ra thư mục /app/publish
FROM build AS publish
RUN dotnet publish "BookingBakery.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Sử dụng runtime của ASP.NET Core 8.0 để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cổng mặc định của .NET 8.0 trong Docker container
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookingBakery.dll"]
