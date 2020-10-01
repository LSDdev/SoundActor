[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)
[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

# About SoundActor

Something about the package

# Installing Core

Add this into your project's package manifest 

```` json
"fi.uniarts.soundactor": "https://github.com/LSDdev/SoundActor.git"
````

You also need to setup the FMOD Unity integration with Unity's assemblt definitoins in order this to work. After you have installed FMOD's Unity packe, go to your assets and locate the FMOD plugin and locate the Runtime folder under src. Create asmdef with a name of **FMODUnity** under Runtime and add Timeline as dependancy.

![Example](https://media.giphy.com/media/43bsre4ylQirSsXUmf/giphy.gif)

# Using Core

Docs for the usage

# Technical details
## Requirements

Currently SoundActor is dependant on few other packages and

* 2019.3 and later
* FMODUnity integration package
* (optional) Some OSC capable sound source 
