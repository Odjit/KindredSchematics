![](logo.png)
# KindredSchematics for V Rising
KindredSchematics is a build mod meant for use on offline maps, with capabilities to load into live maps for new creative opportunities.

This mod will allow you to take building to another level- literally.
Save out castles or designs, and load them into other maps. Share your castle designs with friends.
This mod is meant for most use in a local server, with the ideal being that you use it to only load on a live server. Safest action is to practice and design off-server. It will cause some lag when loading in, varying depending on the complexity of the build and if you're deleting tiles to load in the schematic.

A few notes: 
- DO NOT have a glow library up on a live map. That is hundreds of buffs going off at once and will lag out the server. The buffs aren't all garaunteed to not cause problems, they are filtered to what won't immediately crash a server being applied only.
- DO NOT use .build free on a live map, as it will allow all players to build without cost.
- DO NOT use .build restrictions on a live map, as it will allow all players to build off plot.
- Set a fallback heart for building off plots/restrictions off. If you don't, it will default to the first heart you placed on the map.
- Loading in schematics outside of a territory with a heart will tie the buildings to your fallback heart, wherever it is. I advise admins set this to the dev island territory (territory 0).
- On castle territories, first place down a heart before you load in a territory save. 
- Complicated builds will lag out the server for a few moments. Loading a schematic into a place with existing tiles (thus triggering a delete and then a load) will do the same.
- Be very certain where you want to load things in. Cleanup is annoying, and clearing will permanently delete respawning nodes and the like, and if you want them back, you'll need to use .build spawn to put them back in one by one. 
- If you paste in a floating building, any roof will stop batform from working below. 
- build height ends at 50 (total of 10 floors). Flying above that (with KindredCommands) will result in wonky behavior as you can technically go up to 150. Buildings will not work above 50, don't do it. Same applies to trying to build "below" the map. While it may show there, you cannot move properly as a player.
- Neutral tiles will be "locked" on placement and unable to be dismantled. This is to prevent griefing. If you need to remove them, use .build unlock. Or just delete.
- Neutral walls will show as being in decay. You also cannot put wallpapers on them. To remove this issue, link them to a castle heart. (you won't see decay if build free is enabled but it will be there)

---
Thanks to the V Rising modding and server communities for ideas and requests!
Feel free to reach out to me on discord (odjit) if you have any questions or need help with the mod.

[V Rising Modding Discord](https://vrisingmods.com/discord)

## Build Mode & Palette System (Preview for Testing)

Sneak Peek for **Build Mode** and **Palette System** aimed at making construction more intuitive and efficient. These features allow you to quickly browse through prefabs, place tiles down, and manage your build palette. Try them out and [share your feedback](https://discord.gg/Tp4yBzhKVs)!
Again, DO NOT use this on a live server! Some tilemodels will not work properly and can cause issues. This is a preview for testing purposes only. D:<

### Build Mode

- **Toggle Build Mode**: Use `.build mode` to turn Build Mode on or off.
- **Entity Highlighting**: Build Mode will highlight certain tiles that you are aiming at (more tiles will be supported over time).
- **Grab Tiles**: Use **Left Click** to grab a tile.
- **Rotate Tiles**: Use **Q** to rotate counterclockwise and **E** to rotate clockwise any item you have grabbed.
- **Delete or Copy Tiles**: Press **Space** to delete a grabbed item, or to grab a *copy* of what you last grabbed.
- **Copy Hovered Tiles**: Press **R** to make a copy of whatever tile you are currently hovering over.

### Palette System

The Palette System offers a powerful toolset for managing prefabs with ease:
- **Adding Prefabs to Palette** (`.palette add`) - Search for and add prefabs to your palette for easy access.
- **Removing Prefabs from Palette** (`.palette remove`) - Remove unwanted prefabs from your palette.
- **Listing Palette Contents** (`.palette list`) - Display all prefabs currently stored in your palette.
- **Clearing Palette** (`.palette clear`) - Wipe the palette clean and start fresh.
- **Cycling Through Palette**: While in Build Mode, use **C** and **T** to cycle backwards and forwards through your current palette respectively.


## Commands

### Build Commands

- `.build free`  
  - Turns on debug mode free building. Building/Crafting will have no cost.  
  - DO NOT USE THIS ON A LIVE SERVER. There is a config file to disable this command for your live server.

- `.build restrictions`  
  - Toggles building placement restrictions. Also disables all respawns.  
  - DO NOT USE THIS ON A LIVE SERVER. There is a config file to disable this command for your live server.  
  - Shortcut: `.build r`

- `.build disablefreebuild`  
  - Disables freebuild and restrictionless build modes. Also disables spawning in the glow library. You will need to edit the config to turn them back on (useful for live servers).

- `.build mode`  
  - Toggles build mode.

- `.build setcursor`  
  - Sets a cursor prefab to place tiles.

- `.build clearradius (radius)`  
  - Deletes out everything in a radius centered on you.

- `.build setcorner`  
  - Sets corner coordinates for rectangular work area.

- `.build clearbox`  
  - Clears a box with your current coordinates and the coordinates from `setcorner`.

- `.build clearterritory (territoryIndex)`  
  - Clears out a territory of all tiles except a heart.  
  - Shortcut: `.build ct (territoryIndex)`

- `.build spawn (tile)`  
  - Spawns in the specified tile at your aimed position.  
  - Useful for Tile Models NOT included in the build menu.

- `.build search (searchterm)`  
  - Searches through tile prefabs for a match to the search term.  
  - Shortcut: `.build s (searchterm)`

- `.build check`  
  - Checks the tile you are looking at for a prefab name.  
  - This will only fetch an entity's prefab-if it is not a placeable entity you won't get a read on it. (invalid models)

- `.build delete`  
  - Deletes the tile model you are looking at.

- `.build rotate`  
  - Rotates a tile at your aimed position.

- `.build changeheart`  
  - Changes the heart of the tile you are looking at to your fallback heart.

- `.build changeheartrange (radius)`  
  - Changes the heart of all tiles in a radius to your fallback heart.

- `.build lookupheart`  
  - Looks up the heart of the tile you are looking at.

- `.build setfallbackheart (useOwnerDoors=true) (useOwnerChests=true)`  
  - Sets the fallback castle heart for loading or building without restrictions to the nearby heart.  
  - You can also specify whether to use Owner or Neutral for doors and chests. Defaults to Owner.

- `.build neutraldoors`  
  - Any doors you place will be neutral and will not belong to a heart (useable by all).

- `.build ownerdoors`  
  - Any doors you place will belong to your fallback heart (normal door behavior).

- `.build neutralchests`  
  - Any chests you place will be neutral and will not belong to a heart (useable by all).

- `.build ownerchests`  
  - Any chests you place will belong to your fallback heart (normal chest behavior).

- `.build settings`  
  - Shows the current settings for the `.build` commands - current heart, and what doors/chests will be placed as.

- `.build teleporters`  
  - Makes all teleporters able to traverse any distance across the world (WIP not optimal how it works currently)

### Palette Commands

- `.palette add (searchterm)`  
  - Adds a specified prefab search to the build palette.  
  - Shortcut: `.pal a (searchterm)`

- `.palette remove (searchterm)`  
  - Removes a specified prefab search from the build palette.  
  - Shortcut: `.pal r (seachterm)`

- `.palette list`  
  - Lists all prefabs in the build palette.  
  - Shortcut: `.pal l`

- `.palette clear`  
  - Clears the build palette.  
  - Shortcut: `.pal c`

### Modifytile Commands

- `.modifytile lock`  
  - Prevents the tile you are looking at from being dismantled.  
  - Shortcut: `.modt lock`

- `.modifytile unlock`  
  - Allows the tile you are looking at to be dismantled.  
  - Shortcut: `.modt unlock`

- `.modifytile lockrange (radius)`  
  - Prevents all tiles in a radius from being dismantled.  
  - Shortcut: `.modt lockrange (radius)`

- `.modifytile unlockrange (radius)`  
  - Allows all tiles in a radius to be dismantled.  
  - Shortcut: `.modt unlockrange (radius)`

- `.modifytile lockterritory (territoryIndex)`  
  - Prevents all tiles in a territory from being dismantled.  
  - Shortcut: `.modt lockterritory (territoryIndex)`

- `.modifytile unlockterritory (territoryIndex)`  
  - Allows all tiles in a territory to be dismantled.  
  - Shortcut: `.modt unlockterritory (territoryIndex)`

- `.modifytile movelock`  
  - Prevents the tile you are looking at from being moved.  
  - Shortcut: `.modt movelock`

- `.modifytile moveunlock`  
  - Allows the tile you are looking at to be moved.  
  - Shortcut: `.modt moveunlock`

- `.modifytile movelockterritory (territoryIndex)`  
  - Prevents all tiles in a territory from being moved.  
  - Shortcut: `.modt movelockterritory (territoryIndex)`

- `.modifytile moveunlockterritory (territoryIndex)`  
  - Allows all tiles in a territory from being moved.  
  - Shortcut: `.modt moveunlockterritory (territoryIndex)`

- `.modifytile immortal`  
  - Makes the tile you are looking at immortal (can't be broken).  
  - Shortcut: `.modt immortal`

- `.modifytile immortalrange (radius)`  
  - Makes all tiles in a radius immortal (can't be broken).  
  - Shortcut: `.modt immortalrange (radius)`  
  - Alias: `.modt ir (radius)`

- `.modifytile mortal`  
  - Makes the tile you are looking at mortal (can be broken).  
  - Shortcut: `.modt mortal`

- `.modifytile mortalrange (radius)`  
  - Makes all tiles in a radius mortal (can be broken).  
  - Shortcut: `.modt mortalrange (radius)`  
  - Alias: `.modt mr (radius)`

- `.modifytile pavementspeed (speed) (territoryIndex)`  
  - Changes the PavementBonusSource movement speed for all tiles in the specified territory.  
  - Shortcut: `.modt pavementspeed (speed) (territoryIndex)`

### Schematic Commands
- `.schematic list`
  - Shows a list of schematics
  - Shortcut: `.sc l`
- `.schematic save (schematicName) (Radius)`
  - saves a schematic of anything within or attached to tiles within the radius. 
	- Shortcut: `.sc s (schematicName) (Radius)`
- `.schematic setcorner`
  - sets corner coordinates for rectangular schematic save at your current position
  - Shortcut: `.sc sc`
- `.schematic savebox (schematicName)`
  - saves out a box schematic using the coordinates from setcorner and your new position
  - Shortcut: `.sc sb (schematicName)`
- `.schematic saveterritory (schematicName) (territoryIndex)`
  - saves out a schematic of the territory you are standing in, or a specified territory
  - Shortcut: `.sc st (schematicName) (territoryIndex)`
- `.schematic load (schematicname) (expandClear=0)`
  - pastes in a schematic at the same coordinates it was saved from.
  - Shortcut: `.sc l (schematicname) (expandClear=0)`
- `.schematic loadatpos (schematicName) (expandclear=1) (heightoffset)`
  - pastes in a schematic at your current position. Heightoffset will send it that high up. 
  - expandclear range 0 if direct in, otherwise it will clear out a radius around the load in.
  - Shortcut: `.sc lp (schematicName) (expandclear=1) (heightoffset)`
- `.schematic loadat (schematicName) (x) (y) (z) (expandclear=1)`
  - pastes in a schematic at specified coordinates.
  - Shortcut: `.sc la (schematicName) (x) (y) (z) (expandclear=1)`
- `.schematic toggleplacegrid`
  - on = will attempt to paste in any schematic along grid lines or off grid will allow for offgrid (and thus, misaligned) placement.
- `.schematic remap (sourcePrefabName) (targetPrefabName)`
  - Remaps one prefab to another for loading schematics.
- `.schematic removeremap (remapPrefabName)`
  - Removes a single specified remap.
- `.schematic clearremaps`
  - Clears all prefab remappings.
- `.schematic listremaps`
  - Lists all prefab remappings.
- `.schematic deleteallschematicentities`
  - Deletes all schematic spawned entities.
- `.schematic removeschematicrange (radius)`
  - Removes all schematic spawned entities in a radius.`

### HeartLimit Commands
- `.heartlimit floorcount (#)`
  - change the floorcount of the heart you are next to.
- `.heartlimit blockrelocate`
  - blocks the heart from being relocated.
- `.heartlimit tombcount (#)`
  - change the tomb count of the heart you are next to.
- `.heartlimit nestcount (#)`
  - change the nest count of the heart you are next to.
- `.heartlimit safetyboxcount (#)`
  - change the safety box count of the heart you are next to.
- `.heartlimit eyestructurecount (#)`
  - change the eye structure count of the heart you are next to.
- `.heartlimit prisoncellcount (#)`
  - change the prison cell count of the heart you are next to.
- `.heartlimit servantcount (#)`
  - change the servant count of the heart you are next to.
- `.heartlimit nethergatecount (#)`
  - change the nether gate count of the heart you are next to.
- `heartlimit throneofdarknesscount (#)`
  - change the throne of darkness count of the heart you are next to.
- `.heartlimit musicplayercount (#)`
  - change the music player count of the heart you are next to.

### Glow Commands
- `.glow add (glow)`
  - adds a specified glow to the tile you are looking at.
- `.glow remove (glow)`
  - removes a specified glow from the tile you are looking at.
- `.glow list`
  - lists all available glows.
- `.glow new (buff) (name)`
  - creates a new glow with the specified buff and name.
- `.glow delete (glow)`
  - deletes the specified glow from the glow list.
- `.glow check`
  - checks the glow of the tile you are looking at.
- `.glow library`
  - Spawns a library of all glows for you to look at. Will teleport you to the library if a library is already spawned. Spawns in off the coast of south west Silverlight.
  - This is very resource consuming. Use this on an empty map to pick out your glows and save them to the config with `.glow new`. Do not use this on any map you intend to keep!

### Misc Commands
- `.floorup (NumberofFloors)`
  - Teleports you up a floor or specified number of floors.
  - Shortcut: `.fu (NumberofFloors)`
- `.floordown (NumberofFloors)`
  - Teleports you down a floor or specified number of floors.
  - Shortcut: `.fd (NumberofFloors)`

  
## Eventual To-Do/Possible features
- Come find out in the V Rising Modding Discord!
- 
## Credits
- Thanks to [Mfoltz](https://github.com/mfoltz) for the original build mode idea in VCreate and for his consultation on ability replacement/spatial lookups.
- Thanks to Rendy for the consult on snap.


This mod is licensed under the AGPL-3.0 license.