using System;
using System.Collections.Generic;
using System.IO;
using RebuildHighSchool;
using UnityEditor;
using UnityEngine;

public static class ProjectValidation
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    private static readonly string[] PuzzleArtNames =
    {
        "01-new-friends", "02-good-exam-result", "03-gold-medal-cheer",
        "04-rainy-farewell", "05-moonlit-dorm-walk", "06-graduation-photo"
    };

    private static readonly int[] ExpectedPieceCounts = { 4, 6, 9, 12, 16, 20 };

    [MenuItem("拾光成章/运行项目校验")]
    public static void RunFromMenu()
    {
        ValidateOrThrow();
        EditorUtility.DisplayDialog("拾光成章", "项目校验通过。", "确定");
    }

    // Command line: -executeMethod ProjectValidation.RunBatch
    public static void RunBatch()
    {
        ValidateOrThrow();
        Debug.Log("[拾光成章] BATCH_VALIDATION_PASSED");
    }

    public static void ValidateOrThrow()
    {
        var failures = new List<string>();
        ValidateEditor(failures);
        ValidateScene(failures);
        ValidateLevels(failures);
        ValidatePuzzleArt(failures);
        ValidatePlayerSettings(failures);

        if (failures.Count > 0)
            throw new InvalidOperationException("[拾光成章] 项目校验失败：\n- " + string.Join("\n- ", failures));

        Debug.Log("[拾光成章] 项目校验通过：6 章、54 个调查步骤、6 张拼图图和横屏设置均有效。");
    }

    private static void ValidateEditor(List<string> failures)
    {
        if (!Application.unityVersion.StartsWith("2022.3.62", StringComparison.Ordinal))
            failures.Add("当前 Unity 版本为 " + Application.unityVersion + "，预期 2022.3.62f3c1。");
    }

    private static void ValidateScene(List<string> failures)
    {
        if (!File.Exists(ScenePath)) failures.Add("缺少主场景 " + ScenePath + "。");

        bool sceneEnabled = false;
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled && scene.path == ScenePath) sceneEnabled = true;
        if (!sceneEnabled) failures.Add("主场景未启用在 Build Settings。");
    }

    private static void ValidateLevels(List<string> failures)
    {
        LevelDefinition[] levels = LevelDefinition.CreateAll();
        if (levels == null || levels.Length != 6)
        {
            failures.Add("章节数量不是 6。");
            return;
        }

        for (int levelIndex = 0; levelIndex < levels.Length; levelIndex++)
        {
            LevelDefinition level = levels[levelIndex];
            if (level.Index != levelIndex) failures.Add("第 " + (levelIndex + 1) + " 章索引不连续。");
            if (level.PuzzlePieceCount != ExpectedPieceCounts[levelIndex])
                failures.Add("第 " + (levelIndex + 1) + " 章拼图数量应为 " + ExpectedPieceCounts[levelIndex] + "，实际为 " + level.PuzzlePieceCount + "。");
            if (level.ChainTitles == null || level.ChainTitles.Length != 3)
                failures.Add("第 " + (levelIndex + 1) + " 章调查链数量不是 3。");
            if (level.Investigations == null || level.Investigations.Length != 9)
            {
                failures.Add("第 " + (levelIndex + 1) + " 章调查步骤数量不是 9。");
                continue;
            }

            var seen = new bool[3, 3];
            foreach (InvestigationStep step in level.Investigations)
            {
                if (step.Chain < 0 || step.Chain >= 3 || step.Step < 0 || step.Step >= 3)
                {
                    failures.Add("第 " + (levelIndex + 1) + " 章存在越界的调查步骤。");
                    continue;
                }
                if (seen[step.Chain, step.Step]) failures.Add("第 " + (levelIndex + 1) + " 章存在重复的调查步骤。");
                seen[step.Chain, step.Step] = true;
            }
        }
    }

    private static void ValidatePuzzleArt(List<string> failures)
    {
        foreach (string name in PuzzleArtNames)
        {
            string path = "Assets/Resources/PuzzleArt/" + name + ".png";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                failures.Add("无法导入拼图图 " + path + "。");
                continue;
            }
            if (texture.width != 1920 || texture.height != 1200)
                failures.Add(name + " 尺寸为 " + texture.width + "x" + texture.height + "，预期 1920x1200。");
        }
    }

    private static void ValidatePlayerSettings(List<string> failures)
    {
        if (PlayerSettings.defaultScreenWidth != 1920 || PlayerSettings.defaultScreenHeight != 1080)
            failures.Add("桌面参考分辨率不是 1920x1080。");
        if (PlayerSettings.allowedAutorotateToPortrait || PlayerSettings.allowedAutorotateToPortraitUpsideDown)
            failures.Add("移动端仍允许竖屏旋转。");
        if (!PlayerSettings.allowedAutorotateToLandscapeLeft || !PlayerSettings.allowedAutorotateToLandscapeRight)
            failures.Add("移动端没有同时允许左右横屏。");
    }
}
