# Dice Callback Plugin
This is a framework plugin to allow modders to invoke dice rolls and provide a callback based on the results.

## Install

Currently you need to either follow the build guide down below or use the R2ModMan. 

## Usage
This is a framework plugin used to handle dice rolls and callbacks from result.
This does not provide any direct user functionality but tool for modders.

## How to Compile / Modify

Open ```DiceCallbackPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
* UnityEngine.UI
* Unity.TextMeshPro
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```DiceCallbackPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```

## Changelog
- 2.0.0: Allow Groups to be rolled and deal with negatives
- 1.0.2: Allow object pass through
- 1.0.1: Returns Title and Formula of roll with the data
- 1.0.0: Initial release

## Shoutouts
Shoutout to my Patreons on https://www.patreon.com/HolloFox recognising your
mighty contribution to my caffeine addiciton:
- John Fuller
- [Tales Tavern](https://talestavern.com/) - MadWizard