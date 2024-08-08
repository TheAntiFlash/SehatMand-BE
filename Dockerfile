FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 7247

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
ARG PASSWORD_ENV_SEEDED
WORKDIR /src
COPY /SehatMand.API/SehatMand.API.csproj SehatMand.API/
COPY /SehatMand.Application/SehatMand.Application.csproj SehatMand.Application/
COPY /SehatMand.Domain/SehatMand.Domain.csproj SehatMand.Domain/
COPY /SehatMand.Infrastructure/SehatMand.Infrastructure.csproj SehatMand.Infrastructure/
#generate the cert, define the path to store it and password to use
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p ${PASSWORD_ENV_SEEDED}

RUN dotnet restore "SehatMand.API/SehatMand.API.csproj" -a $TARGETARCH


COPY . .
WORKDIR "/src/SehatMand.API"
RUN dotnet build "SehatMand.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
#COPY --from=build /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/

RUN dotnet publish "SehatMand.API.csproj" -a $TARGETARCH -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
COPY --chmod=0755 --from=build /https/* /https/
EXPOSE 8080
EXPOSE 7247
COPY --from=publish /app/publish .
#https://+:7247;
ENTRYPOINT ["dotnet", "SehatMand.API.dll", "--environment=Development", "--urls=http://localhost:8080;https://localhost:8081"]
