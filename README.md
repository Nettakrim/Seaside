# Take a Photo!

A short, simple game where you explore a little area looking for things to take photos of, with some light puzzle elements for some of the goals.

Play at https://netal.itch.io/take-a-photo

## Notes to developers

It should be relatively easy to add a new area and set of goals along side the current ones, provided you dont need any moving objects that the player can ride on, or for the map to be a different size/shape, in which case it might be a bit awkward.

The ./Assets/World/ folder contains everything specific to an area, all the other folders should be more broad.

Save data is stored fragmented across every photo, this means deleting photos can delete progress, and it means no changes to game state can be permanent - winning the game means that across all of a players photos, all goals are complete, there is no "game beaten" flag written to a specific file.

Because of the lack of a central save file, playerprefs are used for settings.

Contributions are welcome. Everything should be under an MIT license unless otherwise specified as more permissive (for instance the font uses OFL). If you contribute, it's probably best practice to add your name to a copyright thing somewhere? mabye in the files you've contributed to? I'm not a lawyer idk ;3