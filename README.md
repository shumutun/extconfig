# Библиотека разбора конфиг-файла в формате JSON с расширенными функциями

* Добавлена возможность подключать внешние конфиги
* Добавлена возможность использовать значения переменных окружения

Это все бывает нужно для защиты секрктов при выкладывании кода, например на github

Примеры:
Конфиг debug - секреты подгружаются из файла, который не включен в репозиторий, подключение происходит через поле _include. 
Подключаемых внешних конфигов может быть сколько угодно.

Config.Debug.json
```
{
  "_include": "..\\..\\..\\..\\..\\secrets-dev\\myapp.json",
  "DB": {
    "ConnectionString": "mongodb://myhost",
    "DbName": "AppDB"
  }
}
```
myapp.json
```
{
  "DB": {
    "User": "db_user",
    "Password": "db_password"
  }
}
```
В результате в памяти приложения соберется конфиг
```
{
  "DB": {
    "ConnectionString": "mongodb://myhost",
    "DbName": "AppDB"
    "User": "db_user",
    "Password": "db_password"
  }
}
```
Конфиг release - секреты подгружаются из переменных окружения.

Config.Release.json
```
{
  "DB": {
    "ConnectionString": "mongodb://myhost",
    "DbName": "AppDB",
    "User": "${MONGO_USERNAME}",
    "Password": "${MONGO_PASSWORD}"
  }
}
```
Вместо ключей вида ${MONGO_USERNAME} в памяти приложения будут подствлены значения переменных окружения с указанным именем, в даннм примере - значение переменной MONGO_USERNAME

Сама библиотка сразу десериализует конфиг в объект. Пример использования:
```
var config = JsonConfigBuilder.Build<AppConfig>("Config.Debug.json");
```


