## Go into cpp dir
cd godot-cpp

## Get the absolute path for scons compiler
## Build c++ bindings for windows and use the json, 10 workers set here
C:\msys64\mingw64\bin\scons.exe platform=windows custom_api_file=../extension_api.json bits=64 -j10

## Wait, so we can check output
pause
