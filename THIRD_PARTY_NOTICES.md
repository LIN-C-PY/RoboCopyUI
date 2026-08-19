# 第三方声明（THIRD-PARTY NOTICES）

本工具由原创代码构成（C# / XAML / HTML / CSS / JS），不包含任何第三方开源库、组件或素材文件。
以下为运行与界面中所涉及的系统组件及设计参考的如实说明：

## 1. Microsoft .NET 8 / WPF（框架，MIT 许可证）
- 程序基于 **.NET 8 + Windows Presentation Foundation (WPF)** 开发。
- .NET（含 WPF）为微软开源项目，采用 **MIT License**。
- 属于开发框架引用（SDK 隐式引用），未修改其源码，亦未随项目捆绑其源码。

## 2. Microsoft.NET.ILLink.Tasks（构建期依赖，MIT License）
- 仅用于「单文件发布」时的构建环节，由 .NET SDK 自动带入，**不随程序分发**。

## 3. robocopy.exe（Windows 系统自带工具，微软专有软件）
- 程序运行时通过命令行调用 Windows 自带的 `robocopy.exe` 完成文件复制。
- 该工具属于 Windows 操作系统组件（非开源），本程序**仅调用其命令行接口**，未包含/分发其代码。

## 4. Segoe Fluent Icons / Segoe MDL2 Assets（微软系统字体，专有）
- 界面图标使用 Windows 自带的 Segoe Fluent Icons / Segoe MDL2 Assets 字体字形（如标题栏、导航图标）。
- 该字体为微软专有字体，随 Windows 系统提供；程序仅按字体名称引用，未随程序分发字体文件。

## 5. UI 视觉风格参考：OpenMediaConvert（本地项目，许可情况未知）
- 应作者要求，界面视觉风格参考了本地项目 **OpenMediaConvert**（`D:\软件\codex转换\src\OpenMediaConvert`），
  借鉴其 Fluent 深色主题的**设计语言**：配色（背景 / 卡片 / 主题色 #0078D4）、圆角窗口、
  卡片布局、左侧导航、自定义标题栏等。
- 本程序**未复制其源代码**，所有 XAML / C# 代码均为独立编写；仅风格上有意保持一致。
- 该项目为本地生成/作者自有项目，**公开许可情况不明**。如该风格涉及第三方版权或授权要求，
  请确认其许可后再行分发，并按需补充署名或调整视觉方案。

## 6. 其余内容
- 应用图标（app.ico）由程序内代码绘制（“RC”文字图标），非第三方素材。
- 界面预览图（offscreen.png）为本程序离屏渲染截图，非第三方素材。
- HTML 网页版（index.html）为纯原生 HTML/CSS/JS，无任何 CDN 或外部资源引用。
