# NodeGraphView 文档

NodeGraphView 是一套面向 Unity 的通用节点图编辑与运行时执行框架。它将编辑器资产、业务节点、运行时节点和业务 UI 分开，使不同业务可以拥有独立的图编辑窗口与运行时表现。

## 文档目录

1. [与 xNode 的区别](01-xnode-compare.md)
2. [编辑器基类与扩展点](02-editor-base.md)
3. [通用运行时与序列化接口](03-runtime.md)
4. [创建新的业务节点图](04-create-business.md)
5. [Task 节点图示例](05-task-example.md)

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

## 建议补充的截图

文档中的截图占位符可按以下顺序补充：

1. `01-xnode-compare.md`：Task Graph 编辑窗口全图。
2. `02-editor-base.md`：一个节点的标题、ID、复制按钮、端口和内容区。
3. `03-runtime.md`：运行时调试时当前节点高亮。
4. `04-create-business.md`：`Tools/Node Graph/Create Graph Scripts` 面板。
5. `05-task-example.md`：TaskPanel 显示任务名、描述和完成/失败按钮。

建议将截图放在本目录的 `images/` 下，并以文档编号命名，例如 `images/04-create-business.png`。
