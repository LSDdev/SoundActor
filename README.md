[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)
[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

# About SoundActor

This package adds **Audio Control Point Controller** -component which you would use on any Humanoid rigged character or in any regular game object in your Unity project. 

When used with humanoid rigged character you can use any bone in the rig to control selected attribute in FMOD to alter the sound or send the point's control data as OSC command to be used in a desired way. The control data is either absolute position on selected axis, velocity or distance between selected bone to another bone or to outside game object. 

When used on regular game object all the same applies with the obvious difference that humanoid bones are not available.

![Example](https://media.giphy.com/media/kXqf9xoJ1lnfzR6gd9/giphy.gif)

# Installing Core

Add this into your project's package manifest 

```` json
"fi.uniarts.soundactor": "https://github.com/LSDdev/SoundActor.git"
````

You also need to setup the FMOD Unity integration, which you can get from www.fmod.com (requires free account) or from [Asset Store](https://assetstore.unity.com/packages/tools/audio/fmod-for-unity-161631).

Once you have imported the integration into your project you need to create Unity's assembly definitions for FMOD in order to use this package. Go to your assets and locate the FMOD plugin and locate the Runtime folder under src. Create asmdef with a name of **FMODUnity** under Runtime and add Timeline as dependancy. That's all.

![Example](https://media.giphy.com/media/43bsre4ylQirSsXUmf/giphy.gif)

# Using Core

Docs for the usage coming.

# Technical details
## Requirements

Currently SoundActor requires the following

* 2019.3 or later
* [FMODUnity integration package](https://assetstore.unity.com/packages/tools/audio/fmod-for-unity-161631)
* [FMOD Studio](https://www.fmod.com)

If you intend send out data as OSC messages you also need
* [OscJack](https://github.com/keijiro/OscJack) 
* Some OSC capable sound source 


