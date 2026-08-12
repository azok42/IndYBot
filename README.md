[![badge](https://hackatime.hackclub.com/api/v1/badge/U091Q6Y0MM4/azok42/IndYBot)](https://hackatime.hackclub.com/@Azok) [![badge](https://shieldcn.dev/group/github/stars/azok42/IndYBot+github/forks/azok42/IndYBot+github/license/azok42/IndYBot.svg?variant=secondary&size=xs&theme=zinc)](https://hackatime.hackclub.com/@Azok) :(

# IndYBot

This is a Discord bot for interacting with the IndY-API used in my school.

## How to use

- Clone repo

- Rename the appsettings.json.local file to appsettings.json

- Create on [Discord's Developer website](https://discord.com/developers/home) a bot or get the **token** from an existing one.

- Inside the appsettings.json set the 'Bot:Token' key to your token

- Paste your connection string for the database into the 'Database:Connection' key

- Run bot

```bash
dotnet run
```

- Pray to Holy GabeN everything works while it starts

## Developing

- same things as above (especially the last one)

- Modify your appsettings.json file again to include:

  - A 'Debug:Enabled' key set to 'true'

  - A 'Debug:Channel' key set to your test channel's ID

  - A 'Debug:Guild' key set to your test guild's ID

- Again, because it's **really** important to do: Pray to Holy GabeN

---

## What is IndY?

IndY is a project in my school, where students can freely decide what, where and with who the they work in. There are 6 IndY hours in a week (2x Monday, 2x Wendsday, 2x Friday)

## Why?

Better question: Why **NOT**?

With the current system there isn't a way to automatically make entries. I want to provide such a feature. But this is only an example.
