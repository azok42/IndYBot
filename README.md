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

  Getters include, but are not limited to:
  
  - Subjects: get all available subjects
  
  - Hours: get all available IndY-Hours a user can make entries for
  
  - Special-IndY: get all currently active Special-IndY
  
  - Studentcount: get the current amount of students, which made entries for a specific teacher

  - ...
</details>

<details>
<summary>Making of all types of entries</summary>

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

## IndY 

## Installation

## Setup

## Developing

## Licence

Copyright © 2026 Anton Hackner. Licensed under the [GPL-3.0](LICENSE).
