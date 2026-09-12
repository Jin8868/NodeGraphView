# NodeGraphView 文档

NodeGraphView 是一套面向 Unity 的通用节点图编辑与运行时执行框架。它将编辑器资产、业务节点、运行时节点和业务 UI 分开，使不同业务可以拥有独立的图编辑窗口与运行时表现。

## 文档目录

1. [为什么需要 NodeGraphView](https://cyskdsn.top/posts/ca3fa1d1.html)
2. [编辑器基类与扩展点](https://cyskdsn.top/posts/80a055ae.html)
3. [通用运行时与序列化接口](https://cyskdsn.top/posts/7bb25447.html)
4. [创建新的业务节点图](https://cyskdsn.top/posts/d5e3da58.html)
5. [Task 节点图示例](https://cyskdsn.top/posts/83c1b684.html)

## 目录结构

```text
Assets/NodeGraphView/
├── Editor/                         编辑器框架
│   ├── Controller/                 图控制器与注册
│   └── View/                       图、节点和节点内容 View
├── Script/                         通用 Runtime 与数据结构
│   ├── Runtime/
│   └── Serialization/
├── Examples/Task/                  Task 业务示例
│   ├── Editor/
│   ├── Runtime/
│   └── View/
└── Editor Default Resources/
    └── GraphData/                  编辑器图资产
```

编辑器面板
![taskNodeWidnows](img/taskEditorWindow.png)

业务自动创建面板
![taskNodeWidnows](img/nodeGraphCreatePanel.png)

任务实例运行
![nodeRuntime](img/taskExampleGif.gif)

