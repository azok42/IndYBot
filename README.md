<div align="center">

# IndYBot
[![badge](https://hackatime.hackclub.com/api/v1/badge/U091Q6Y0MM4/azok42/IndYBot)](https://hackatime.hackclub.com/@Azok) [![badge](https://shieldcn.dev/group/github/stars/azok42/IndYBot+github/forks/azok42/IndYBot+github/license/azok42/IndYBot.svg?variant=secondary&size=xs&theme=zinc)](https://hackatime.hackclub.com/@Azok) :(

IndY-Bot is a Discord bot for interacting with the IndY-API used in my school.

[Features](#features) •
[IndY](#indy) •
[Installation](#installation) •
[Setup](#setup) •
[Developing](#developing) •
[Licence](#licence)

</div>

## Features

<details>
<summary>Getter for various IndY info</summary>
<br />

  Getters include, but are not limited to:
  
  - Subjects: get all available subjects
  
  - Hours: get all available IndY-Hours a user can make entries for
  
  - Special-IndY: get all currently active Special-IndY
  
  - Studentcount: get the current amount of students, which made entries for a specific teacher

  - ...
</details>

<details>
<summary>Entry making</summary>

  - Manual, specific entries for each day

  - Normal, school-event and absence types

  - Individual hour making

  - Setting standards for quickly making entries

  - No need to specify the data when using the standard entry
</details>

<details>
<summary>Automatic Entries</summary>

  - Entries will be made at specified times

  - Time can be set and will execute at that time on the day before IndY

  - Standards will be used to get entry data like teacher, subject, ..

  - Errors will be written to the guilds auto-entry channel

</details>

<details>
<summary>Groups</summary>

  - Create, delete and add users to groups

  - Recognise groups by group role (*name*_group)

  - Not much usecases other than roles, but who am I to remove a useless feature I added
</details>

<details>
<summary>Group Entries</summary>

  - Create a public entry

  - Everyone in the channel can join

  - Provide all data and a reason

  - Provide a optional description of the entry (used in the actual entry)

  - Override the provided description, when making entries

  - Ping a role (usecase for groups maybe??)
</details>

<details>
<summary>Absence Rank</summary>

  - Get absence entry count of everyone

  - Everyone even includes pleople from the school, who don't use IndYBot

  - Includes a rank of the student compared to all other students in the school
</details>

<details>
<summary>WhereIs</summary>

  - Get the next entry location of a user

  - OptIn: Users have to enable it
</details>

<details>
<summary>Admin</summary>

  - Set the guild's needed channels or fall back to default channel

  - Enable and Disable logging

  - Only for *bot owner*: Send global message to all known guilds
</details>

<details>
<summary>Other</summary>

  - Nickname: set your nickname to your real name (by IndY)
</details>

## Installation

Prerequisites

- MariaDB / MySQL

- .Net 10, if cloned

> [!IMPORTANT]
> I only used XAMPP's provided database when testing

Clone the repository or download a release

```bash
git clone https://github.com/azok42/IndYBot     # Clone
```

If you clone, you need to rename the 'appsettings.json.local' file for further setup

## Setup

Edit your appsettings.json and add:

- **Database connection string** Example: "Server=localhost;Port=3306;Database=database;Uid=user;Pwd=password"

- **Encryption Key** Used for encrypting the passwords, has to be a 32 Byte long Base64 string

You can create such a key without problems with script:

```cs
using var rng = RandomNumberGenerator.Create();
byte[] keyBytes = new byte[32];
rng.GetBytes(keyBytes);
Console.WriteLine(Convert.ToBase64String(keyBytes)); // Save this string!
```

- **Token** Paste your Discord bot token, retreived from [Discord Developer Portal](https://discord.com/developers/applications)

- **Debug things** You can ignore them, if you just want to use IndY-Bot

---

After adding everything, start the bot with:

- **Cloned**: ```dotnet run``` while in the project directory

- **Downloaded**: Simply execute the binary in the directory

## Developing

## IndY 

IndY is a education system in my school in Austria.

### Concept

The first 6 hours on each day are getting cut by 10 minutes. This time is being used on Monday, Wednesday and Friday in what is now the 3rd and 4th hour. For these hours students have to make entries, at least the day before the actuall IndY-Day. If enough entries are missing, the student's behaviour mark is made worse.

Teachers have a set schedule of where and when they have a IndY-Hour. Also teachers can host 'Special-IndY' hours. These are *special* hours where their normal schedule is being altered, for the duration of the Special-IndY. Examples are lectures not tied directly to school.

There are also other types of entries students can make:

| Type | Description |
| -------------- | --------------- |
| Normal | Normal or Special-IndY entries |
| Absence | Entries marking absence in IndY-Hours |
| School-Event| Events like field trips |

## Licence

Copyright © 2026 Anton Hackner. Licensed under the [GPL-3.0](LICENSE).
