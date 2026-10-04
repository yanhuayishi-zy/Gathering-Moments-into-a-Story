using System.IO;
using RebuildHighSchool;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProjectSetup
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ScenePath = SceneFolder + "/Main.unity";

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.delayCall += EnsureProject;
    }

    [MenuItem("拾光成章/配置项目")]
    public static void EnsureProject()
    {
        ConfigurePlayer();
        EnsureMainScene();
    }

    private static void ConfigurePlayer()
    {
        PlayerSettings.companyName = "Independent Student Studio";
        PlayerSettings.productName = "拾光成章";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.student.rebuildhighschool");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        PlayerSettings.runInBackground = false;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
    }

    private static void EnsureMainScene()
    {
        if (!AssetDatabase.IsValidFolder(SceneFolder)) AssetDatabase.CreateFolder("Assets", "Scenes");

        if (!File.Exists(ScenePath))
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cameraObject.GetComponent<Camera>().backgroundColor = new Color(.04f, .05f, .06f);
            new GameObject("Game Bootstrap", typeof(MemoryPuzzleGame));
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[拾光成章] 项目配置完成，主场景：" + ScenePath);
    }

    [MenuItem("拾光成章/构建 Windows")]
    public static void BuildWindows()
    {
        EnsureProject();
        ProjectValidation.ValidateOrThrow();
        string output = Path.GetFullPath("Builds/Windows/ShiGuangChengZhang.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        ThrowIfBuildFailed(report);
    }

    [MenuItem("拾光成章/构建 Android")]
    public static void BuildAndroid()
    {
        EnsureProject();
        ProjectValidation.ValidateOrThrow();
        string output = Path.GetFullPath("Builds/Android/ShiGuangChengZhang.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        EditorUserBuildSettings.buildAppBundle = false;
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.None
        });
        ThrowIfBuildFailed(report);
    }

    private static void ThrowIfBuildFailed(BuildReport report)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[拾光成章] 构建完成：" + report.summary.outputPath);
            return;
        }
        throw new System.Exception("构建失败：" + report.summary.result);
    }
}
