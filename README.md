# SehatMand .NET CORE BACKEND


## Connect to MYSQL DB
To connect to database create appsettings.Development.json.
Then add the MYSQL Connection string to it.

```shell
cd SehatMand.API/ 
```

```shell
cp appsettings.json appsettings.Development.json
```
Make a copy of appsettings.json

```json
{
  "ConnectionStrings": {
    "SmDb": "CONNECTION STRING HERE" 
  }
}
```
```json
{
  "JWT": {
    "Key": "JWT KEY HERE"
  }
}
```
edit appsettings.Development.json. Add Connection String & JWT Key.

Format of connection String is as follows:
```json
"Server=<IP>;Database=<DATABASE_NAME>;Uid=<DATABASE_USER>;Pwd=<DATABASE_PASSWORD>;"
```

## Run the API
```shell
dotnet run
```
Run the API using dotnet run command.