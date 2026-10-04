# 拾光成章

《拾光成章》是一款使用 Unity 2022.3 开发、面向 Windows 与 Android 的横屏叙事解谜游戏。玩家将在六个高中场景中完成调查与推理，找回逐关增多的记忆碎片，再拼出每章独有的剧情画面，逐步重温从入学到毕业的青春故事。

## 已实现

- 六个章节：入学、月考、运动会、分班、晚自习、毕业
- 每章三条调查链：观察物件、判断行动、验证线索后获得碎片
- 六章共 54 组独立的物件描述、行动选项、错误反馈和剧情发现
- 右侧调查档案、链路进度、前置线索与三级递进提示
- 4、6、9、12、16、20 块递进式拼图，前两关免旋转，后四关加入方向判断
- 真实互锁拼图轮廓：相邻碎片使用配对的凸榫与凹口，并支持异形点击判定
- 第一章三阶段新手引导：发现发光物件、根据观察推理、拖拽并自动吸附
- 鼠标与单指触控共用交互逻辑
- 1920x1080 参考分辨率、Safe Area、移动端横屏锁定
- 章节解锁、继续游戏和本地存档
- 电影感程序化场景插画：透视教室、考场、运动场、雨廊、晚自习与毕业教室
- 相册式标题页、章节场景缩略图和不遮挡画面的编号寻物标记
- 每章探索图与完成后的剧情拼图分离；图片缺失时仍可回退到程序化插画

## 剧情拼图

六张拼图原画位于 `Assets/Resources/PuzzleArt`，采用统一的温暖手绘动画电影风格：

- 入学：第一次认识新朋友
- 月考：成绩比预想更好
- 运动会：获得金牌，全班欢呼
- 分班：雨廊中挥手告别
- 晚自习：月色下并肩走回宿舍
- 毕业：全班毕业合影

游戏只在进入拼图阶段后加载对应的回忆画面。

## 打开方式

1. 在 Unity Hub 中添加本目录，使用 `2022.3.62f3c1` 打开。
2. 首次导入后，编辑器会自动创建 `Assets/Scenes/Main.unity` 并加入 Build Settings。
3. 打开 Main 场景，点击 Play。

如自动配置未触发，可使用菜单 `拾光成章/配置项目`。

## 构建

- Windows：菜单 `拾光成章/构建 Windows`
- Android：先在 Hub 为 2022.3.62f3c1 安装 Android Build Support，再使用菜单 `拾光成章/构建 Android`
- 项目校验：菜单 `拾光成章/运行项目校验`

Windows 与 Android 构建会先自动校验 Unity 版本、主场景、六关调查数据、拼图数量、六张 1920x1200 拼图原画和横屏设置；校验失败时会中止构建并给出具体原因。命令行可使用：

```powershell
Unity.exe -batchmode -quit -projectPath <项目路径> -executeMethod ProjectValidation.RunBatch -logFile <日志路径>
```

Android 已配置为 Landscape Left/Right 自动旋转，不允许竖屏。

首次打开会进行资源导入，请等待 Unity 右下角进度完成后再进入 Play Mode。

首次导出 APK 前，请通过 Unity Hub 安装 Android Build Support、SDK/NDK Tools 与 OpenJDK。

## 主要代码

- `Assets/Scripts/MemoryPuzzleGame.cs`：游戏流程、六关数据与 UI
- `Assets/Scripts/PuzzlePieceView.cs`：鼠标/触控拖拽、选择和吸附
- `Assets/Scripts/JigsawPieceFactory.cs`：生成配对凸榫、凹口、透明点击区域与边缘描线
- `Assets/Scripts/ProceduralArt.cs`：六个场景和拼图画面的程序化插画
- `Assets/Editor/ProjectSetup.cs`：项目配置与双端构建
- `Assets/Editor/ProjectValidation.cs`：资源、章节数据、场景和横屏设置的构建前校验
