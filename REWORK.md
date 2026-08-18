# REWORK

Current project is a mess

## Problems

- weird directory setup

  - more seperated setup would be nice

- not re-using functions

- big files with too much functions/things

- ...

## New things

### Directories

Top level files: Bot.cs, InteractionHandler.cs

- Modules: Commands

  - AutoCompleteHandlers

  - Preconditions

  - Modals

- Extensions

- Services

- Helpers

- Types: Entry, ...

### Commands

#### Getter (GetterModule)

- /get ...

  - student

  - teachers filter:??

  - teacher absences

  - subjects

  - indy_days month:int

  - hours filter:?

  - special_indy filter:?

  - studentcount

    - plot

    - list

#### Auth (AuthModule)

- /login

- /logoff

- /auth ...

  - save

  - remove

#### Entry (EntryModule)

- /entry ...

  - normal

  - event

  - absence

  - view

  - ?auto ...?

    - set

    - list

    - status

    - toggle / enable, disable

#### Groups (GroupModule)

- /group ...

  - user user:@User

  - list

  - info

  - members

  - create

  - delete

  - invite

  - kick

  - join

  - leave

  - entry ??

#### Group entry (GroupEntryModule)

- /groupentry ...

  - create (same as /group entry)

  - close

  - view

  - join

  - edit

  - duplicate
  
  - poll ??

  - template (used for creating common group entries)

    - create

    - remove

    - list

    - edit ??

#### Autoentries (AutoEntryModule)

- /autoentry ...

  - set

  - enable

  - disable

  - toggle

  - reset

  - status

  - history

  - notifications type:{disabled, on_failure, always}

#### Where-Is (InfoModule)

- /whereis ...

  - user

  - group

  - status

  - enable visibility:{Group, Server}

  - disable

  - toggle

  - admin-only

- /absence-rank

- /nick

#### Admin (AdminModule)

- /admin ...

  - setup

  - status

  - timezone (why?? but idk)

  - version

  - feature

    - enable feature:{features...}

    - disable feature:{features...}

    - toggle feature:{features...}

    - list

  - group ...

    - delete

    - repair

    - prune

  - user ...

    - info

    - disable

    - enable

    - reset

    - logout

  - autoentry

  - announce
