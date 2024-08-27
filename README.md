![](logo.png)
# KindredSchematics for V Rising
KindredSchematics is a build mod meant for use on offline maps, with capabilities to load into live maps for new creative opportunities.

This mod will allow you to take building to another level- literally.
Save out castles or designs, and load them into other maps. Share your castle designs with friends.
This mod is meant for most use in a local server, with the ideal being that you use it to only load on a live server. Safest action is to practice and design off-server. It will cause some lag when loading in, varying depending on the complexity of the build and if you're deleting tiles to load in the schematic.

A few notes: 
- DO NOT use .build free on a live map, as it will allow all players to build without cost.
- DO NOT use .build restrictions on a live map, as it will allow all players to build off plot.
- Set a fallback heart for building off plots/restrictions off. If you don't, it will default to the first heart you placed on the map.
- Loading in schematics outside of a territory with a heart will tie the buildings to your fallback heart, wherever it is. I advise admins set this to the dev island territory.
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

## Commands

### Build Commands
- `.build free` 
  - turns on debug mode free building. Building/Crafting will have no cost. DO NOT USE THIS ON A LIVE SERVER. There is a config file to disable this command for your live server.
- `.build restrictions`
  - toggles building placement restrictions. Also disables all respawns. DO NOT USE THIS ON A LIVE SERVER. There is a config file to disable this command for your live server.
  - Shortcut: `.build r`
- `.build disablefreebuild`
  - disables freebuild and restrictionless build modes. You will need to edit the config to turn them back on. (useful for live servers)
- `.build clearradius (radius)` 
  - Deletes out everything in a radius centered on you. 
- `.build setcorner` 
  - sets corner coordinates for rectangular work area
- `.build clearbox`
  - clears a box with your current coordinates and the coordinates from setcorner.
- `.build clearterritory (territoryIndex)` 
  - clears out a territory of all tiles except a heart.
  - Shortcut: `.build ct (territoryIndex)`
- `.build spawn (tile)`
  - spawns in the specified tile at your aimed position. This is ueful for Tile Models NOT included in the build menu.
- `.build search (searchterm)`
  - searches through tile prefabs for a match to the search term.
  - Shortcut: `.build s (searchterm)`
- `.build check`
  - checks the tile you are looking at for a prefab name. This will only fetch an *entities' prefab*, if it is not a placeable entity you won't get a read on it. (invalid models)
- `.build delete` 
  - deletes the tile model you are looking at.
- `.build rotate` 
  - rotates a tile at your aimed position.
- `.build immortal`
 - makes the tile you are looking at immortal. (can't be broken)
- `.build immortalrange (radius)`
  - makes all tiles in a radius immortal. (can't be broken)
- `.build mortal`
  - makes the tile you are looking at mortal. (can be broken)
- `.build mortalrange (radius)`
  - makes all tiles in a radius mortal. (can be broken)
- `.build changeheart`
  - changes the heart of the tile you are looking at to your fallback heart.
- `.build lookupheart`
  - looks up the heart of the tile you are looking at.
- `.build setfallbackheart (useOwnerDoors=true) (useOwnerChests=true)`
  - Sets the fallback castle heart for loading or building without restrictions to the nearby heart. You can also specify whether to use Owner or Neutral for doors and chests. Defaults to owner.
- `.build neutraldoors`
  - Any doors you place will be neutral and will not belong to a heart. (useable by all)
- `.build ownerdoors`
  - Any doors you place will belong to the your fallback heart. (normal door behavior)
- `.build neutralchests`
  - Any chests you place will be neutral and will not belong to a heart. (useable by all)
- `.build ownerchests`
  - Any chests you place will belong to the your fallback heart. (normal chest behavior)
- `.build settings`
  - Shows the current settings for the .build commands. Current heart, and what doors/chests will be placed as.
- `.build lock`
  - Prevents the tile from being dismantled.
- `.build unlock`
  - Allows the tile to be dismantled.

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

### Misc Commands
- `.floorup (NumberofFloors)`
  - Teleports you up a floor or specified number of floors.
  - Shortcut: `.fu (NumberofFloors)`
- `.floordown (NumberofFloors)`
  - Teleports you down a floor or specified number of floors.
  - Shortcut: `.fd (NumberofFloors)`

	

  
  
## Eventual To-Do/Possible features
- Come find out in the V Rising Modding Discord!

This mod is licensed under the AGPL-3.0 license.