### ToyBox DarkHeresy - Ver 0.1.5 (built for 0.0.3.480)
- (***ADDB***) Fix ToyBox crashing Main Menu UI if feature init failed. Whoopsie. Something something fail early.

### ToyBox DarkHeresy - Ver 0.1.4 (built for 0.0.3.480)
- (***ADDB***) Fix Stats Editor not working.
- (***ADDB***) Fix Alignment Editor being weird.

### ToyBox DarkHeresy - Ver 0.1.3 (built for 0.0.3.477)
- (***ADDB***) Fix Etude Editor using the wrong guid as root etude, making Etude Editor not working.
- (***ADDB***) Slightly change the Mod UI so that Mod UI crashes are less likely to soft lock the Mod UI.

### ToyBox DarkHeresy - Ver 0.1.2 (built for 0.0.3.475)
- (***ADDB***) Swap to custom UMM because Owlcat disabled official
- (***ADDB***) Add Modify Money thingy

### ToyBox DarkHeresy - Ver 0.1.1 (built for 0.0.3.467)
- (***ADDB***) Remove AbilityResource related things (AddBA, RemoveBA, Type in IdCache) because there is not a single bp with that type in DH.

### ToyBox DarkHeresy - Ver 0.1.0 (built for 0.0.3.467)
- (***ADDB***) Initial port for the game
- Find pre-port changelogs [here](https://raw.githubusercontent.com/xADDBx/ToyBox-RogueTrader/refs/heads/main/ToyBox/ReadMe.md)
- This port is based on TB2, see the relevant changes [here](https://raw.githubusercontent.com/xADDBx/ToyBox-ToyBox-DarkHeresy/refs/heads/main/Porting/Changes.txt)
- See Credits for the various versions of ToyBox [here](https://raw.githubusercontent.com/xADDBx/ToyBox-ToyBox-DarkHeresy/refs/heads/main/Credits.md)

### Install & Use

1. ***Important:*** You need to replace the shipped UnityModManager once with the custom one that is provided in the release. Just replace `C:\Users\<YourUser>\AppData\LocalLow\Owlcat Games\WHDH\UnityModManager\UnityModManager.dll` with the file of the same name that is provided.
2. Install the by manually extracting the archive to your game's mod folder (e.g. one of the final paths could be: `C:\Users\<YourUser>\AppData\LocalLow\Owlcat Games\WHDH\UnityModManager\ToyBox\ToyBox.dll`).
3. Start the game and load a save or start a new save (most of the mod's functions can't access from the main menu).
4. Open the Unity Mod Manager﻿ by pressing CTRL + F10.
5. Adjust the settings in the mod's menu.

### Development Setup

1. Clone the git repo
2. Build the solution and it will automatically build and install into the mod folder in the game

### Acknowledgments

* **ArcaneTrixter**for many awesome improvements and bug fixes
* **fire**& **m0nster**for lots of awesome code from bag of tricks
* [Truinto](https://github.com/cabarius/ToyBox/issues?q=is%3Apr+author%3ATruinto),**Delth, Aphelion, fire** for great
  contributions to the ToyBox project
* **Owlcat Games** - for making fun and amazing games
* **Paizo** - for carrying the D20 3.5 torch
* Pathfinder Wrath of The Righteous Discord channel members
    * **@Spacehamster** - awesome tutorials and taking time to teach me modding WoTR, and letting me port stuff from
      Kingdom Resolution Mod
    * **@m0nster** - for giving me permission to port stuff from Back of Tricks
    * **@Vek17, @Bubbles, @Balkoth, @swizzlewizzle** and the rest of our great Discord modding community - help, moral
      support and just general awesomeness
    * **@m0nster, @Hsinyu, @fireundubh** for Bag of Tricks which inspired me to get into modding WoTR because I missed
      this mod so much
* PS: Learn to mod Kingmaker Games at
  Spacehamster's [Modding Wiki](https://github.com/spacehamster/OwlcatModdingWiki/wiki/Beginner-Guide )
* Come visit the authors on the [Owlcat Discord](https://discord.gg/owlcat)﻿


### License: MIT

Copyright <2025> ADDB
Copyright <2021> Narria (github user Cabarius)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
