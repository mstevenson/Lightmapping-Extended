Lightmapping Extended
=====================

**Lightmapping Extended** is an editor tool for the Unity game engine that exposes all compatible XML configuration options for the Autodesk Beast lightmapping tool built-in to Unity.

The most notable additions to Unity's lightmapping settings are **image-based lighting**, **Path Tracer GI**, and **Monte Carlo GI**.

A Cornell Box scene is included for testing. The scene was created by Kristen Schat of iKriz Media (http://www.ikriz.nl/2010/11/11/unity3d-cornell-box)

Usage
-----

To integrate Lightmapping Extended into an existing Unity project, copy the directory ***Lightmapping-Extended/Assets/Editor/*** into your project's Assets folder. The tool's editor window can be accessed from the menu ***Window > Lightmapping Extended***.

Configuration options are scene-specific. If the current scene does not include a lightmap configuration file, the Lightmapping Extended editor window will provide an option to create one. Lightmapping Extended's default settings will match Unity's "High Quality" preset.