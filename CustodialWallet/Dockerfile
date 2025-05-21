# Используйте официальный образ .NET SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Установите текущий рабочий каталог
WORKDIR /app

# Скопируйте csproj и восстановите зависимости
COPY . ./
RUN dotnet restore CustodialWallet.Web

# Скопируйте все остальные файлы в контейнер
COPY . ./

# Соберите приложение
RUN dotnet publish CustodialWallet.Web -c Release -o out

# Используйте официальный образ .NET Runtime для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Установите рабочий каталог
WORKDIR /app

# Скопируйте собранные файлы из build образа
COPY --from=build /app/out .

# Укажите команду для запуска приложения
ENTRYPOINT ["dotnet", "CustodialWallet.Web.dll"]