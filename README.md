# Usage: #
Add lua scripting capability into unity, so that game can be updated without re-packaging.

# Comparison: #
There are 4 commonly used unity lua libraries:

- luainterface: old
- ulua: based on luainterface
- nlua: cross-platform version writen by luainterface author
- unilua: compatible with lua5.2, writen in c#

Concerning to bridging c#, nlua is the best, which adds special support for ios. For example, it supports delegate bridging. However ulua is better for its compatibility with unity, because parts of its apis are re-writen, such as loadfile, print, etc.

Unilua isn't very useful, so I won't comment on it.

# First version(2014.7) #
To sum up, I choose luainterface, and adds nlua capabilities on ios. For example, to support:

    UIEventListener.Get(label3).onClick = OnClick;

Here are the reasons:

1. Most of China's companies use ulua. So ulua is unlikely to have fatal bugs.
1. To be honest, I prefer nlua. But I don't have much time to make nlua work on unity.

# Second version(2015.2) #

....

# Remarks: #
    lua
    	Sources of lua
    example:
    	Examples to show how to use lua on unity
	
## Building Plugings ##
The binaries in Plugings can be built from source. The solution files are in lua. For example, lua.dll can be built using /lua/proj.win

Note: android NDK must be Platform(32-bit target)

Todo
1. Adusting sources that load resources, according to development requirement.
1. Rearrange the source to make it conform to our coding convensions
1. Port nlua code that supports bridging interface, class, etc.

---------- 

# 用途 #
为unity引入lua支持，解决“中国特色”的游戏自更新问题。

# 对比 #
目前常见的四种unity lua库，luainterface，ulua，nlua，unilua：

- luainterface: C#的lua桥接库
- ulua是luainterface增加了对unity的支持
- nlua是luainterface的作者为了跨平台，而写的升级版
- unilua是lua5.2的C#版

从桥接C#上来说，nlua最好，它对ios平台做了特殊处理，如支持了委托的桥接。
从兼容unity上来说，ulua最好，它重写了loadfile、print等api。

unilua 略显鸡肋，不评价了。

# 第一版本（2014.7） #

选择使用luainterface，并为其引入nlua对ios平台的特殊处理，如支持了如下代码：

    UIEventListener.Get(label3).onClick = OnClick;

选择原因如下：

1. 国内大部分的公司采用了ulua，其应当没有明显bug
1. 我更倾向于使用nlua，但工作紧，无法投入时间去做nlua的unity支持

# 第二版本（2015.2） #

选择使用 nlua+KeraLua，做了以下修改

1. 增加unity支持
1. 删除对KopiLua的支持
1. 删除动态创建程序集的代码
1. 删除多平台的代码

# 说明 #

    lua
        lua源代码
    example
        unity lua使用示例

## 使用方法 ##
1. 将example/Assets/unitylua 拷贝到你的工程中。
2. 将example/Assets/Plugins拷贝到你的工程的Assets/Plugins中

## Plugins构建 ##
Plugins中的二进制文件可以自己构建，工程文件在/lua-5.1.5中，如/lua/proj.win可以构建出lua.dll。

**注意：** android的NDK一定要选择 Platform(32-bit target)
