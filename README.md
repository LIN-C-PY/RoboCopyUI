# ROBOCOPY 快速复制工具

为 Windows 自带的 `robocopy` 打造的原生 **WPF 桌面程序**（Fluent Design 风格，类似 OpenMediaConvert / FastCopy）。

## 桌面程序（推荐）

- **位置**：桌面 `RoboCopyUI.exe`（单文件、无依赖，双击即用）
- **源码**：`RoboCopyWpf/`（WPF + XAML，`dotnet publish` 可重新编译）

### 界面特色

- 圆角无边框窗口 + 阴影，自定义标题栏（Fluent 图标、底部主题色细线）
- **深色渐变背景** + 卡片式布局，主题色 #0078D4，渐变主按钮、渐变进度条
- 左侧导航（选中项带主题色指示条）+ 四个一键预设
- **界面全部使用通俗中文**，无任何 `/xx` 技术参数字样，普通用户也能直接上手

### 功能

- 选择源/目标目录（支持网络路径），一键「浏览…」
- 7 种复制模式：标准 / 复制子目录（含/不含空目录）/ 镜像同步 / 移动文件 / 移动全部 / 清理多余文件
- 常用开关：多线程、重试、等待、限速、可重启、备份模式、优先备份、排除联接点、FAT 时间、安全设置、复制全部信息、目录时间戳、修复时间、仅试运行
- 文件筛选：仅复制 / 排除文件 / 排除目录；日志文件记录
- 实时输出复制过程（文件名+进度），错误红色高亮，进度条 + 退出码提示（0-7 成功，8+ 出错）
- 危险模式（镜像同步/清理/移动）执行前二次确认；互斥选项自动拦截
- 「开始 / 停止 / 复制命令 / 清空日志」按钮

### 使用方法

1. 双击桌面 `RoboCopyUI.exe`
2. 「快速复制」页填源目录、目标目录，选复制模式
3. 「高级选项」页按需勾选开关
4. 点「▶ 开始复制」，日志实时显示进度
5. 需要管理员权限的选项（如备份模式）请右键 → 以管理员身份运行

## 网页版（可选）

- `index.html`：浏览器打开即可用，适合生成命令或 .bat 脚本的场景（仍含技术参数说明）。

## 安全提醒

- 「镜像同步」「清理多余文件」「移动全部」会**删除目标中源没有的文件**，程序已内置二次确认，执行前建议先勾选「仅试运行」预览。

## 重新编译

```powershell
dotnet publish "RoboCopyWpf\RoboCopyWpf.csproj" -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:NuGetAudit=false -o RoboCopyWpf\publish
```

## 文件结构

```
ROBOCOPY UI/
├── RoboCopyUI.exe        # 桌面程序（已复制到桌面）
├── RoboCopyWpf/          # WPF 工程源码
│   ├── MainWindow.xaml(.cs)
│   ├── Themes/FluentStyles.xaml
│   ├── App.xaml(.cs)
│   └── publish/          # 发布产物（单文件 exe）
├── index.html            # 网页版工具
├── offscreen.png         # 界面预览图
└── README.md             # 本说明
```

## 版权与致谢

- 本工具代码为原创，不包含第三方开源库；运行环境使用 **Microsoft .NET 8 / WPF**（MIT License）与 Windows 自带 **robocopy.exe**。
- UI 视觉风格参考了本地项目 **OpenMediaConvert** 的 Fluent 深色主题设计（配色、卡片与导航布局），代码为独立编写。
- 详细第三方声明见 **[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)**。
