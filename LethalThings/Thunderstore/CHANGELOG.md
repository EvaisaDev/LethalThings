# LethalThings 0.10.7  
**Improvements**  
- Potential fix for ammo resetting upon loading save.
   - Thanks PlagueTR [PR #81](https://github.com/EvaisaDev/LethalThings/pull/81)
  
# LethalThings 0.10.6
**Improvements**
- Better compatibility for Utility Belt, should cause less probably with some other mods probably.

# LethalThings 0.10.5
- Recompiled for latest version.

# LethalThings 0.10.4
**Bugfixes**
- Maybe fixed issue with remote radar by making the code way worse. (idc)

# LethalThings 0.10.3
**Bugfixes**
- Mod works on v50 now i think.

# LethalThings 0.10.2  
**Bugfixes**    
- Unbroke the remote radar.  
  
# LethalThings 0.10.1  
**Bugfixes**  
- Maxwell can no longer explode when loading a save.  
- Fixed null reference error when other mods register network prefabs incorrectly or something. (Thanks DBJ)  
- Fixed error when dying from gremlin energy.  
  
(probably other things i forgor about)  
  
# LethalThings 0.10.0
**Bugfixes**
- Zapgun laser is no longer permanently active.
- Radar camera is only active if any player is holding Remote Radar and said radar is turned on.

# LethalThings 0.9.4
**Bugfixes**  
- Pinger was breaking the escape menu while turned on.  
- Pinger ping animation was not playing.  
  
# Lethal Things 0.9.3  
- Updated dependency version of LethalLib to 0.11.2  
	- If you want to LethalThings to work please update LethalLib to that version.  
  
# Lethal Things 0.9.2
- Updated used NetcodePatcher version to v3.3.3  
- Removed unfinished content from config, accidental leak moment lmao  

# Lethal Things 0.9.1
**Bugfixes**
- Fixed crash to desktop on launch, caused by NetcodePatcher update.

# Lethal Things 0.9.0
**Content**
- Added Shop Item: Pinger
- Added Scrap: Gnarpy Plush

**Systems**
- Completely rewrite of Content Loader and Config system.
- Config files are now synced between host and client.  
- Changed how utility belt adds slots for potential compatibility improvement.  

**Bugfixes**
- Lethal Things content should now spawn on modded moons  
- Boomba improvements.  
- Power outlet stun damage config is now working.  
- Added null check to tracking point which should fix one of the dartboard errors.  
- Fixed flare desync  
- Rocket launcher laser turns off when pocketed.  
- Evil maxwell desync.  
- Flare gun client aim issue.  
- Remote radar was able to switch target while turned off.  
- Toy hammer was broken because of lethal company v47 update.  
- Darts are no longer pingable while held.
- Maxwell becomes evil sometimes when loading a save :(

**Changes**
- Toyhammer is now a scrap item by default  
	- Added config option to add Toyhammer to store.  
	- Added config option to make Toyhammer spawn as scrap.  
- Cookie is now throwable.  
	- Note: Batteries may spontaneously combust  

# Lethal Things 0.8.8
- Glizzy is no longer called "Training Manual" (idk how the heck that happened because it was called glizzy before.)
- Custom items / scraps no longer receive decals from spraypaint, in order to fix a transparency bug.
- Added version check for Reserved Item Slots compatibility, if it is installed, please make sure you are running version 1.6.6 or newer.

# Lethal Things 0.8.0 - 0.8.7
**Features**  
- Crimas vibes!!!   
- Dirty stinky arsons can be showered.    
  
**Items**  
- Added Scrap: Gremlin Energy Drink  
- Added Scrap: Revolver  
- Added Item: Flaregun  
	- Attracts certain enemies.  
	- Flares are automatically pinged by nearby players.  
	- Ammo can be bought from store.  

**Decor**  
- Added Decor: Dartboard  
	- Usable dartboard for passing time.  

**Improvements**  
- Utility belt slot distance and positioning was improved  
- Rocket launcher now has a tooltip for "Fire : [LMB]"  
- Maxwell no longer weights 42 lbs  
- Removed conductiveness of various items  
- Improved weight of various items  
- Removed red tint from rocket launcher lights.  
- Custom items should no longer phase through the floor when ship takes off or lands.  
- Added version check for InputUtils, in case it is installed and the wrong version is present.   
  
**Bugfixes**  
- Hämis plush not longer glitches out price screen due to ä character.  
- Rocket launcher lights no longer break when multiple rocket launchers are present.  
- Rocket launcher ammo is now saved properly with the game and synced between players.  
- Arson no longer kills items in other player's hotbars when she munchy.  
- Fakewell is not synchronized between clients  
- Fakewell can no longer be activated multiple times.  
- Electrocution config setting now works.  
- Rocket launcher aim direction is no longer wrong when shot from clients.  
- Fatalities sign now shows the correct value, and is saved properly between games.  
- Decor items now show up in shop decor rotation and are no longer always buyable.  
- Fixed bug with Hacking Tool not initializing properly, caused by a change for preventing item switching.  

**Compatibility**  
- Quickswap mods not longer switch items when using hacking tool.  
- Utility belt should now work properly alongside the reserved item slot mods.  
- Added support for InputUtils, which when installed allows you to bind belt slots to hotkeys.