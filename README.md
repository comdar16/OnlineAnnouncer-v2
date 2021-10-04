# OnlineAnnouncer-v2
Features:

Broadcasts a custom message to all players when a player joins and/or leaves the server.
Players can add/change/remove their own messages (with the proper permissions).
"Mods" can add/change/remove anyone's messages.
The config includes the addition of blacklisted phrases that cannot be used in messages (unless you have the mod permission), along with a default broadcast color.

Commands:
/greet <message>: Sets the user's greeting announcement.
/readgreet <player>: Reads the user's greeting and leaving announcements.
/setgreet <player> <message>: Sets the specified user's greeting announcement.
Replace /greet with /leave for the same functions for leaving announcements.
Use /reload to reload the OnlineAnnounceConfig.json file and re-sync with the database.
