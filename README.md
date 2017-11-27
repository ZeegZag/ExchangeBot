# ExchangeBot

# Before Running

Create `appsettings.json` with content:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_domain;Database=db_name;UId=username;Pwd=password;"
  },
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  }
}

```