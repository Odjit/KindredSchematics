![](logo.png)
# KindredSchematics for V Rising
KindredSchematics is a build mod meant for use on offline maps, with capabilities to load into live maps for new creative opportunities.

This mod will allow you to take building to another level- literally.
Save out castles or designs, and load them into other maps. Share your castle designs with friends.
This mod is meant for most use in a local server, with the ideal being that you use it to only load on a live server. Safest action is to practice and design off-server. It will cause some lag when loading in, varying depending on the complexity of the build and if you're deleting tiles to load in the schematic.

A few notes: 
- DO NOT use .build free on a live map, or suffer the consequences listed below. Preferred method is to build on a trash map, save out your schematic, and load it into where you want it on the map you care about. 
  - Side effects of freebuild on a map are the following: 
  - Grow nodes (trees, ruins, ores) regrow through tiles in loaded chunks,
  - wildlife will respawn in all chunks
  - attempts to place a castle heart in free build will cresh the server immediately 
  - you can place "off grid" (you can use stairs to orient, they stay on grid always). (wonkly floor/wall alisgnments result)
- Loading in schematics outside of a territory with a heart will tie the buildings to your first castle heart, wherever it is. I advise admins set this to the dev island territory by placing their first heart of the map there.
- On castle territories,lease place down a heart on a territory before you load in a territory save. 
- If you want working waygates, you will need to place the tile down, and utilize KindredCommands' RevealMap command to have it for all players- or have your server settings to allow all waygates to be unlocked at start.
- Complicated builds will lag out the server for a few moments. Loading a schematic into a place with existing tiles (thus triggering a delete and then a load) will do the same.
- Be very certain where you want to load things in. Cleanup is annoying, and clearing will permanently delete respawning nodes and the like, and if you want them back, you'll need to use .build spawn to put them back in one by one. 
- If you paste in a floating building, any roof will stop batform from working below. 
- build height ends at 50 (total of 10 floors). Flying above that (with kindredCommands) will result in wonky behavior as you can technically go up to 150. Buildings will not work above 50, don't do it.

---
Also, thanks to the V Rising modding and server communities for ideas and requests!
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
  - disables freebuild and restrictionless build modes. You will need to edit the config to turn them back on.
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
- `.build delete` 
  - deletes the tile model you are looking at.
- `.build rotate` 
  - rotates a tile at your aimed position.
- `.build immortal`
 - makes the tile you are looking at immortal. (can't be broken)
- `.build mortal`
  - makes the tile you are looking at mortal. (can be broken)
- `.build search (searchterm)`
  - searches through tile prefabs for a match to the search term.
  - Shortcut: `.build s (searchterm)`


### Schematic Commands
- `.schematic list`
  - Shows a list of schematics
  - Shortcut: `.v l`
- `.schematic save (schematicName) (Radius)`
  - saves a schematic of anything within or attached to tiles within the radius. 
	- Shortcut: `.v s (schematicName) (Radius)`
- `.schematic setcorner`
  - sets corner coordinates for rectangular schematic save at your current position
  - Shortcut: `.v sc`
- `.schematic savebox (schematicName)`
  - saves out a box schematic using the coordinates from setcorner and your new position
  - Shortcut: `.v sb (schematicName)`
- `.schematic saveterritory (schematicName) (territoryIndex)`
  - saves out a schematic of the territory you are standing in, or a specified territory
  - Shortcut: `.v st (schematicName) (territoryIndex)`
- `.schematic load (schematicname) (expandClear=0)`
  - pastes in a schematic at the same coordinates it was saved from.
  - Shortcut: `.v l (schematicname) (expandClear=0)`
- `.schematic loadatpos (schematicName) (expandclear=1) (heightoffset)`
  - pastes in a schematic at your current position. Heightoffset will send it that high up. 
  - expandclear range 0 if direct in, otherwise it will clear out a radius around the load in.
  - Shortcut: `.v lp (schematicName) (expandclear=1) (heightoffset)`
- `.schematic loadat (schematicName) (x) (y) (z) (expandclear=1)`
  - pastes in a schematic at specified coordinates.
  - Shortcut: `.v la (schematicName) (x) (y) (z) (expandclear=1)`
- `.schematic toggleplacegrid`
  - on = will attempt to paste in any schematic along grid lines or off grid will allow for offgrid (and thus, misaligned) placement.

### Misc Commands
- `.floorup (NumberofFloors)`
  - Teleports you up a floor or specified number of floors.
  - Shortcut: `.fu (NumberofFloors)`
- `.floordown (NumberofFloors)`
  - Teleports you down a floor or specified number of floors.
  - Shortcut: `.fd (NumberofFloors)`

	

  
  
## Eventual To-Do/Possible features
- idk lots of stuff.
- Ease of use improvements for single tile build operations.
- Servants gear doesn't copy.
