Lightmapping Extended
=====================

**Lightmapping Extended** is an editor tool for the Unity game engine. It exposes all compatible XML configuration options for Unity's integrated Autodesk Beast lightmapper through a simple UI.

The most notable additions to Unity's built-in lightmapping settings are **image-based lighting**, **Path Tracer GI**, and **Monte Carlo GI**.

A Cornell Box scene is included for testing. The scene was created by Kristen Schat of iKriz Media (http://www.ikriz.nl/2010/11/11/unity3d-cornell-box)

Installation
------------

To add Lightmapping Extended to an existing Unity project, copy the directory ***Assets/Lightmapping Extended/*** into your project's Assets folder. Once installed, the Lightmapping Extended editor window can be accessed from the menu ***Window > Lightmapping Extended***.

Configuration
-------------

Lightmapping configuration options are unique to each scene. If the current scene does not include a configuration file, the Lightmapping Extended editor window will provide an option to create one. If a configuration file exists for the current scene, it will be automatically loaded.

Presets
-------

Configuration settings may be saved as presets. Presets are stored in the folder ***Lightmapping Extended/Presets*** and may be checked into source control. Presets are available per-project, making them easy to re-use across multiple scenes. Preset files may be organized into folders and will be displayed hierarchically in the Presets selection menu at the top of the Lightmapping Extended window.

Shaders
-------

A set of transmissive shaders are included with Lightmapping Extended. These shaders are identical to Unity's built-in transparent shaders, with the addition of a *Transmissive Color* property. This property defines which colors of light are able to pass through the material, producing colored shadows. Colored shadows are only supported in baked lightmaps and will not be displayed by Unity's real-time shadow system.