FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .

RUN dotnet publish "PetClinics.csproj" -c Debug -o /app/Debug

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/Debug .

ENTRYPOINT ["dotnet", "PetClinics.dll"]
