# unitylua #

为了支持游戏自更新，引入了lua支持。

自更新方案：表现逻辑部分使用C#开发为组件，游戏逻辑部分使用lua开发为脚本。

目前常见的三种unity lua库，ulua，nlua，unilua：

- ulua是luainterface的扩展了unity功能
- nlua是luainterface的作者为了跨平台，而写的升级版
- unilua是lua5.2的C#版

从桥接C#上来说，nlua最好，它对ios平台做了特殊处理，如支持了委托的桥接。
从兼容unity上来说，ulua最好，它重写了loadfile、print等api。

综合两者而言，我选择使用ulua，并为其引入nlua对ios平台的特殊处理。代码示例如下：

    UIEventListener.Get(label3).onClick = OnClick;

