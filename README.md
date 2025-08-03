## Installation guide
This will outline how to install and contribute to the GodotTest repo for Windows 11

1. Install the latest version of Godot
   (03/08/2025 Godot Engine 4.4.1)
<img width="669" height="300" alt="image" src="https://github.com/user-attachments/assets/80cc1695-bf14-4c6c-93ff-7a37fae3f59b" />

https://godotengine.org/download/windows/

2. Extract files to desired location
3. Launch Godot_v4.4.1-stable_win64
4. Go back to GitHub and clone the GodotTest repo
   (Easiest would be use GitHub Desktop)
5. In Godot, press 'Import' or 'Import Existing Project'
<img width="380" height="134" alt="image" src="https://github.com/user-attachments/assets/bb38d217-ffe5-4ad3-8418-3f8a82f6189a" />
<img width="691" height="157" alt="image" src="https://github.com/user-attachments/assets/f40f012e-a281-404c-9d29-8f2768f4dde3" />

6. Select and open 'project.godot'

<img width="256" height="161" alt="image" src="https://github.com/user-attachments/assets/af8f4c9b-af49-4090-ad42-617909e020b6" />

8. Press 'Import'

<img width="473" height="115" alt="image" src="https://github.com/user-attachments/assets/0a748868-e020-4724-8adc-342b2d5c6fc0" />

You will then be launched into the Godot Editor where you can start building GodotTest!

## Using C++ with Godot
C++ can be used in Godot in two ways, with GDExtentions or custom C++ modules.

The godot cpp headers have been added as a submodule to this project. If you are freshly cloning it make sure to add `--recurse-submodules` when cloning like so:

```c++
git clone --recurse-submodules
```

And then do a 

```c++
git submodule update --init
```

To check the submodule out to the right version.

### GDExtentions
GDExtentions are used in Godot to be able to make Godot's engine run C or C++ code and interact with native shared libs without the recomplilation of the engine.

https://docs.godotengine.org/en/4.4/tutorials/scripting/gdextension/gdextension_cpp_example.html
TODO: How to write and compile a GDExtention to run within Godot on windows

### C++ Modules
Modules are written within the core of the engine and require you to rebuild the whole engine.
This should only be done for building with external libraries, optimizing critical sections, adding new functionality to the engine.
