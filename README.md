# UnityLua #

[NLua](https://github.com/NLua/NLua) for [unity](http://www.unity3d.com/).

# Usage #
copy `example/Assets/UnityLua` and `example/Assets/Plugins` to you project

The binaries in Plugings can be built from source. The solution files are in lua. For example, lua.dll can be built using /lua/proj.win

# Change list #
1. Remove KopiLua from NLua.
1. Make KeraLua support unity ios. 
1. Remove Emit, because unity ios not support.
1. Remove some luanet_* functions. such as luanet_loadbuffer...
1. String use custom encoding, such as UTF8.

# Todo list #
1. Continue to support unity.
