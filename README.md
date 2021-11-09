[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)
[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

# About SoundActor

This package adds **Audio Control Point Controller** -component which lets you to control how sounds are manipulated and played out from [FMOD](https://www.fmod.com) project or from any [OSC](https://en.wikipedia.org/wiki/Open_Sound_Control) controlled audio software/platform.

When used with humanoid rigged character you can use any bone in the rig to control selected attribute in FMOD to alter the sound or send the point's control data as OSC command to be used in a desired way. The control data is either absolute position on selected axis, velocity or distance between selected bone to another bone or to outside game object. 

When used on regular game object all the same applies with the obvious difference that humanoid bones are not available.


<img src="https://user-images.githubusercontent.com/16014157/97693161-69b1fe80-1aa9-11eb-8bfc-06569075480c.jpg" width="700">
<img src="https://user-images.githubusercontent.com/16014157/97710670-b9ea8a00-1ac4-11eb-998a-784fe8344fae.gif" width="700">

---


# Installing Sound Actor

## Step 1

Add needed registries and scopes into your project's package manifest (Packages/manifest.json). 

To the `scopedRegistries` section:

``` json
"scopedRegistries": [
    {
      "name": "Uniarts",
      "url": "https://registry.npmjs.com",
      "scopes": [ "fi.uniarts" ]
    },
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
```

To the `dependencies` section:

``` json
"fi.uniarts.soundactor": "1.1.1"
```

After changes, the manifest file should look like below:

``` json
{
  "scopedRegistries": [
    {
      "name": "Uniarts",
      "url": "https://registry.npmjs.com",
      "scopes": [ "fi.uniarts" ]
    },
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "fi.uniarts.soundactor": "1.1.1",
    ...
```

## Step 2
You also need to setup the FMOD Unity integration, which you can get from www.fmod.com (requires free account) or from [Asset Store](https://assetstore.unity.com/packages/tools/audio/fmod-for-unity-161631).

---

# Using SoundActor

Detailed docs and example video for the usage should be coming. In the meanwhile the basic gist is following;

1. Add _Audio Control Point Controller_ component to game object. Component tries to look if that game object has humanoid rig (simple lookup via animator) or is it regular game object. 
2. Setup the fmod event name in _Connection and output settings_
3. Start adding control points and use the parameter names you have in fmod (_fmod control parameter_).
4. Hit play and you should be able to control parameters in fmod side.

---

# Requirements

Currently SoundActor requires the following

* 2019.3 or later
* [FMODUnity integration package](https://assetstore.unity.com/packages/tools/audio/fmod-for-unity-161631)
* [FMOD Studio](https://www.fmod.com)
* [OscJack](https://github.com/keijiro/OscJack) (>= 1.0.3)

If you intend send out data as OSC messages you also need
* Some OSC capable sound source 


