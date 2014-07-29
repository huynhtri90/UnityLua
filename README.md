# 用途 #
为unity引入lua支持，解决“中国特色”的游戏自更新问题。

# 对比 #
目前常见的三种unity lua库，ulua，nlua，unilua：

- ulua是luainterface增加了对unity的支持
- nlua是luainterface的作者为了跨平台，而写的升级版
- unilua是lua5.2的C#版

从桥接C#上来说，nlua最好，它对ios平台做了特殊处理，如支持了委托的桥接。
从兼容unity上来说，ulua最好，它重写了loadfile、print等api。

unilua 略显鸡肋，不评价了。

综合而言，我选择使用ulua，并为其引入nlua对ios平台的特殊处理，如支持了如下代码：

    UIEventListener.Get(label3).onClick = OnClick;

选择原因如下：

1. 国内大部分的公司采用了ulua，其应当没有明显bug
1. 我更倾向于使用nlua，但工作紧，无法投入时间去做nlua的unity支持

# 说明 #

    src
        桥接源代码
    lua-5.1.5
        lua源代码
    example
        unity lua使用示例

## 使用方法 ##
1. 将example/Assets/Thirdparty/unitylua 拷贝到你的工程中。
2. 将example/Assets/Plugins拷贝到你的工程的Assets/Plugins中

## Plugins构建 ##
Plugins中的二进制文件可以自己构建，工程文件在/lua-5.1.5中，如/lua-5.1.5/proj.win可以构建出lua.dll。


# ToDo #
1. 根据开发需要，调整部分资源加载部分的代码
1. 代码规范整理，按照nlua的代码规范，重新梳理一遍代码
1. 将nlua对interface，class的桥接代码移植过来