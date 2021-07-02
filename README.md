# Snkrx-Save-Parser

Provides a library and test suite (soon to also include executable) for editing the save data of a SNKRX game.

The save data isn't terribly hard to parse, but I decided I wanted to go overboard and wrote a fully fledged parsing library for it, assuming that it uses some sort of standardized Lua serializer which might be worth having around.

Thus, this project is far more verbose than necessary-- a simple python script with a few regexes, namely:

```py
itemRegex = r'(\[(?(?="[^"]+")")((?(?=[^"]+"])[^"]+)|\d+)(?(?=")")] = ("[^"]+"|\d+|true|false|\{(?:(?1)(?:, )?)*\}))'
objRegex = r'{(.*)}'
```

is good enough for a simple way of parsing the save data.

This project, however, should serve as a practical launching point for creating applications that modify the save data in any way shape or form, as well as provide modular functionality for deserializing (and eventually serializing) the save data.
