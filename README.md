Simple plugin to allow limiting which assets get loaded.

# Asset List file

Location:
* `U3DS\Servers\[Server ID]\Better Loading\Asset Whitelist.dat`
* `Unturned\Better Loading\Asset Whitelist.dat`

Format:
Each line is either a GUID or an asset type followed by an ID:

Any text after a '#' character is ignored as a comment.

Load the 'Ace' by GUID (recommended):
```
92b49222958d4c6fbeca1bd00987b0fd
```

Load the 'Eaglefire' by ID:
```
Item 4
```

Example file:
```
# Whitelist of assets to load
# Text after a '#' is considered as a comment.

92b49222958d4c6fbeca1bd00987b0fd	# Ace
Item 4								# Eaglefire
```

Asset type can be any of the following (case insensitive):
* Item
* Effect
* Object
* Resource
* Vehicle
* Animal
* Mythic
* Skin
* Spawn
* NPC