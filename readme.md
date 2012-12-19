Lightmapping Extended
=====================

**Lightmapping Extended** is an editor tool for the Unity game engine. It exposes all compatible XML configuration options for Unity's integrated Autodesk Beast lightmapper through a simple UI.

The most notable additions to Unity's built-in lightmapping settings are **image-based lighting**, **Path Tracer GI**, and **Monte Carlo GI**.

A Cornell Box scene is included for testing. The scene was created by Kristen Schat of iKriz Media (http://www.ikriz.nl/2010/11/11/unity3d-cornell-box)

Installation
------------

To integrate Lightmapping Extended into an existing Unity project, copy the directory ***Lightmapping-Extended/Assets/Lightmapping Extended/*** into your project's Assets folder. The Lightmapping Extended editor window can be accessed from the menu ***Window > Lightmapping Extended***.

Configuration
-------------

Configuration options are unique to each scene. If the current scene does not include a lightmapping configuration file, the Lightmapping Extended editor window will provide an option to create one.

Configuration settings may be saved as presets. Presets are stored in the folder ***Lightmapping Extended/Presets*** and may be checked into source control. Presets are available per-project, making them easy to re-use across multiple scenes. Preset files may be organized into folders and will be displayed hierarchically in the Presets selection menu.