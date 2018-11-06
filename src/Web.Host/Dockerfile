FROM microsoft/dotnet:2.1-sdk AS build

WORKDIR /build
COPY ./*.sln .
COPY ./src/Web.Host/*.csproj ./src/Web.Host/
RUN dotnet restore ./src/Web.Host

COPY ./src ./src
RUN dotnet build ./src/Web.Host -c Release

RUN dotnet publish -c Release -o /app --no-build ./src/Web.Host

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS app
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN mkdir /logs
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Web.Host.dll"]