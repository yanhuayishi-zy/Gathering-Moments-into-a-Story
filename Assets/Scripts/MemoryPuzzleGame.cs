using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RebuildHighSchool
{
    public sealed class MemoryPuzzleGame : MonoBehaviour
    {
        private const string SaveKey = "RebuildHighSchool.MaxUnlocked";
        private const string InvestigationSaveKey = "RebuildHighSchool.InvestigationCleared";
        private static MemoryPuzzleGame instance;

        private readonly List<Texture2D> generatedTextures = new List<Texture2D>();
        private readonly List<Button> hotspotButtons = new List<Button>();
        private readonly Text[] chainProgressTexts = new Text[3];
        private readonly Text[] chainTargetTexts = new Text[3];
        private LevelDefinition[] levels;
        private Canvas canvas;
        private RectTransform screenRoot;
        private AudioSource audioSource;
        private AudioSource musicSource;
        private AudioSource ambienceSource;
        private AudioClip tickClip;
        private AudioClip successClip;
        private AudioClip musicClip;
        private AudioClip clockLoopClip;
        private AudioClip nightLoopClip;
        private AudioClip celebrationClip;
        private int currentLevel;
        private int foundCount;
        private int[] chainProgress;
        private int hintLevel;
        private int placedCount;
        private Text statusText;
        private Text countText;
        private Button startPuzzleButton;
        private Button rotateButton;
        private PuzzlePieceView selectedPiece;
        private GameObject investigationOverlay;
        private Text investigationFeedback;
        private Texture2D currentMemoryTexture;
        private bool completionSequenceRunning;
        private Coroutine completionRoutine;
        private Font graduationMemoryFont;

        private static readonly string[] GraduationMemoryLines =
        {
            "你好，新同学。",
            "晨光落在新课桌上，也照见了初见的我们。",
            "三年的故事，从那句自我介绍开始。",
            "别紧张，我们都在。",
            "第一次月考后，批注比分数更让人难忘。",
            "试卷会泛黄，围在桌边的笑声不会。",
            "接力棒递到手里，我们只管向前跑。",
            "金牌高高举起，欢呼落满整条跑道。",
            "风吹起班旗，也吹亮了那年的秋天。",
            "教室会换，我们还是我们。",
            "雨后的长廊很长，一次回头就望了很久。",
            "没说出口的告别，后来我们都懂了。",
            "晚自习的灯，陪我们熬过一个又一个夜晚。",
            "最后一道题写完，月亮正停在窗边。",
            "四个人并肩回宿舍，影子被路灯拉得很长。",
            "那时最平常的归途，后来成了最想念的一段路。",
            "黑板上的倒计时，终于走到了最后一天。",
            "这一次，拍照的人也要站进照片里。",
            "愿这张照片，替我们留住十八岁的盛夏。",
            "我们毕业了。"
        };

        private static readonly string[][] DossierHints =
        {
            new[]
            {
                "晨光被布影拦在左侧窗边", "被擦去的编号藏在黑板下沿", "右上方的时间快了五分钟",
                "黑板右侧有一张被墨迹盖住的名单", "左侧课桌旁垂着一抹蓝色", "窗边第三排有处没有合严",
                "靠窗桌面留着空白扉页", "讲台上那摞本子少了一册", "右侧高柜门上贴着两片纸角"
            },
            new[]
            {
                "考场右上方的指针停在7:55", "前排桌面压着一张折起的证件", "讲台下方的封条已经松开",
                "中央桌面有张纸被碎屑盖住", "座位图上压着一道透明刻度", "第三列第二排的桌缝露出纸角",
                "试卷旁的演算痕迹反复改写", "试卷背面一角被白色方块压住", "桌角透明盒的底部夹着字"
            },
            new[]
            {
                "看台上方传来断续的人名", "终点旁三块布片的背面有数字", "旗杆套筒上留着三个缺口",
                "第五道的鞋印在交接区中断", "红色胶带缠在一根短棒上", "领奖台第二级留着圆形压痕",
                "看台空位上放着一张饮料单", "补给箱底压着一张未显影相纸", "场边高柜里传来背带碰撞声"
            },
            new[]
            {
                "左侧玻璃上的雨雾遮住公告栏", "滴水的伞柄缠着一张便签", "右侧公告栏只剩蓝色名单清晰",
                "窗台湿纸朝走廊方向晕开", "长椅下面压着星期三课程", "走廊17号柜的锁舌已经生锈",
                "透明黏条上留着倒写的字", "一张课表背面缺了半个偏旁", "走廊尽头的班牌背面夹着纸"
            },
            new[]
            {
                "月光在仍有余温的杯壁上反射", "讲台没有合严的地方藏着微光", "微弱光束同时扫过窗边和黑板",
                "右侧窗边的拉绳打了特殊的结", "桌上的厚本子缺少最后一道条件", "墙上的倒数牌空了一个数字",
                "半透明纸页边缘印着异地校名", "旁边桌斗露出一只未封口信封", "墙角箱子顶层空出信封大小"
            },
            new[]
            {
                "左侧旧课桌上散着五件旧物", "旧物中有一页褪色的分数", "布片、留言和厚本子写着不同年份",
                "讲台上留着一页定时拍摄说明", "黑板上的名字旁画着小相机", "花束丝带里卷着一张取件单",
                "一张旧留言的折痕像保护袋", "厚本子夹着六张右侧留白的照片", "最后一页只剩快门指印和空白"
            }
        };

        private static readonly string PlayerLetter =
            "<b>亲爱的玩家：</b>也许你还没有走进高中，也许正在其中，也许才刚刚离开，或已经工作了许多年。愿这场关于三年时光的故事，能让你想起某个清晨、某条放学后的路，或一个很久没有提起的名字。我们总以为要等到下一次考试结束、等到毕业、等到生活安稳下来，才有空好好感受眼前的一切，可真正值得珍惜的，往往就是那些当时看起来最普通的日子。请认真对待今天，也温柔地理解过去的自己：那时的你或许犹豫、冲动，也可能做过后来不再认同的选择，但不必因此反复责怪自己，因为每一步，都是当时的你在有限的答案里尽力作出的决定。人生不会因为一次选择就被彻底写定，走得慢一点、绕一些路，甚至停下来重新出发，都不代表失败。愿你珍惜身边仍在的人，勇敢说出喜欢、感谢和道歉；也愿你不被遗憾困住，不因未知退缩，带着已经走过的路，坦然迎接尚未发生的明天。过去值得怀念，当下更值得好好生活，未来也依然值得期待。与诸位共勉。";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<MemoryPuzzleGame>() != null) return;
            new GameObject("MemoryPuzzleGame").AddComponent<MemoryPuzzleGame>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            if (FindObjectOfType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            levels = LevelDefinition.CreateAll();
            ConfigureScreen();
            CreateAudio();
            canvas = UiKit.CreateCanvas();
            DontDestroyOnLoad(canvas.gameObject);
            screenRoot = UiKit.Stretch("SafeArea", canvas.transform);
            screenRoot.gameObject.AddComponent<SafeAreaFitter>();
            ShowTitle();
        }

        private void ConfigureScreen()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Application.targetFrameRate = 60;
        }

        private void CreateAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            tickClip = CreateTone("Tick", 520f, .07f, .18f);
            successClip = CreateTone("Success", 740f, .24f, .24f);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = .22f;
            musicClip = CreateMusicLoop();
            musicSource.clip = musicClip;
            musicSource.Play();

            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            ambienceSource.volume = .34f;
            clockLoopClip = CreateClockLoop();
            nightLoopClip = CreateNightLoop();
            celebrationClip = CreateCelebration();
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int length = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float envelope = 1f - i / (float)length;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * volume * envelope;
            }
            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateMusicLoop()
        {
            const int sampleRate = 22050;
            const float duration = 12f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[length];
            float[] notes = { 261.63f, 329.63f, 392f, 440f, 392f, 329.63f, 293.66f, 261.63f };
            float noteLength = duration / notes.Length;
            for (int i = 0; i < length; i++)
            {
                float time = i / (float)sampleRate;
                int noteIndex = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(time / noteLength));
                float local = (time - noteIndex * noteLength) / noteLength;
                float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(local * 8f)) * Mathf.Clamp01((1f - local) * 3.4f);
                float note = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * time) * .055f * envelope;
                float pad = (Mathf.Sin(2f * Mathf.PI * 130.81f * time) + Mathf.Sin(2f * Mathf.PI * 196f * time) * .55f) * .018f;
                float loopFade = Mathf.Clamp01(Mathf.Min(time, duration - time) * 3f);
                samples[i] = (note + pad) * loopFade;
            }
            var clip = AudioClip.Create("MemoryTheme", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateClockLoop()
        {
            const int sampleRate = 22050;
            const float duration = 4f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float time = i / (float)sampleRate;
                float local = time - Mathf.Floor(time);
                if (local < .055f)
                {
                    float envelope = Mathf.Exp(-local * 72f);
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * 1150f * local) * .13f + Mathf.Sin(2f * Mathf.PI * 760f * local) * .08f) * envelope;
                }
            }
            var clip = AudioClip.Create("ExamClock", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateNightLoop()
        {
            const int sampleRate = 22050;
            const float duration = 10f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[length];
            var random = new System.Random(7305);
            float filteredNoise = 0f;
            for (int i = 0; i < length; i++)
            {
                float time = i / (float)sampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filteredNoise = Mathf.Lerp(filteredNoise, noise, .006f);
                float wind = filteredNoise * (.07f + .025f * Mathf.Sin(time * .73f));
                float chirp = 0f;
                float[] chirpStarts = { 1.1f, 1.28f, 3.8f, 3.97f, 6.4f, 6.56f, 8.65f };
                for (int c = 0; c < chirpStarts.Length; c++)
                {
                    float local = time - chirpStarts[c];
                    if (local >= 0f && local < .12f)
                        chirp += Mathf.Sin(2f * Mathf.PI * (2850f + c * 90f) * local) * Mathf.Sin(local / .12f * Mathf.PI) * .055f;
                }
                float loopFade = Mathf.Clamp01(Mathf.Min(time, duration - time) * 2f);
                samples[i] = (wind + chirp) * loopFade;
            }
            var clip = AudioClip.Create("SnowNightWind", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateCelebration()
        {
            const int sampleRate = 22050;
            const float duration = 2.6f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[length];
            float[] notes = { 392f, 523.25f, 659.25f, 783.99f };
            for (int i = 0; i < length; i++)
            {
                float time = i / (float)sampleRate;
                float value = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float local = time - n * .28f;
                    if (local >= 0f && local < 1.1f)
                        value += Mathf.Sin(2f * Mathf.PI * notes[n] * local) * Mathf.Exp(-local * 2.7f) * .11f;
                }
                float finalChord = Mathf.Max(0f, time - 1.15f);
                if (time >= 1.15f)
                    value += (Mathf.Sin(2f * Mathf.PI * 523.25f * finalChord) + Mathf.Sin(2f * Mathf.PI * 659.25f * finalChord)) * Mathf.Exp(-finalChord * 2f) * .055f;
                samples[i] = value;
            }
            var clip = AudioClip.Create("ResultJoy", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void SetChapterAmbience(bool enabled)
        {
            if (!enabled)
            {
                ambienceSource.Stop();
                ambienceSource.clip = null;
                return;
            }

            AudioClip desired = currentLevel == 1 ? clockLoopClip : currentLevel == 4 ? nightLoopClip : null;
            if (ambienceSource.clip == desired && ambienceSource.isPlaying) return;
            ambienceSource.Stop();
            ambienceSource.clip = desired;
            if (desired != null) ambienceSource.Play();
        }

        private void ClearScreen()
        {
            if (completionRoutine != null)
            {
                StopCoroutine(completionRoutine);
                completionRoutine = null;
            }
            completionSequenceRunning = false;
            selectedPiece = null;
            investigationOverlay = null;
            currentMemoryTexture = null;
            hotspotButtons.Clear();
            for (int i = screenRoot.childCount - 1; i >= 0; i--) Destroy(screenRoot.GetChild(i).gameObject);
            foreach (Texture2D texture in generatedTextures) if (texture != null) Destroy(texture);
            generatedTextures.Clear();
        }

        private void ShowTitle()
        {
            ClearScreen();
            SetChapterAmbience(false);
            Texture2D background = ProceduralArt.Create(levels[5], 1600, 900, false);
            generatedTextures.Add(background);
            var image = UiKit.TexturePanel("Background", screenRoot, Vector2.zero, new Vector2(1920, 1080), background);
            image.uvRect = new Rect(0, 0, 1, 1);
            UiKit.Panel("Shade", screenRoot, new Vector2(-510, 0), new Vector2(1020, 1080), new Color(.025f, .045f, .052f, .78f));
            UiKit.Panel("AccentLine", screenRoot, new Vector2(-852, 96), new Vector2(8, 350), levels[5].Accent);

            UiKit.Label("Eyebrow", screenRoot, "2019 — 2022  ·  青春记忆档案", new Vector2(-480, 310), new Vector2(660, 54),
                25, new Color(.94f, .78f, .48f), TextAnchor.MiddleLeft, FontStyle.Bold);
            Text title = UiKit.Label("Title", screenRoot, "拾光\n成章", new Vector2(-480, 120), new Vector2(680, 260),
                96, Color.white, TextAnchor.MiddleLeft, FontStyle.Normal);
            title.font = UiKit.DisplayFont;
            UiKit.Label("Subtitle", screenRoot, "寻找散落在校园里的碎片\n把从入学到毕业的故事重新拼完整", new Vector2(-480, -50), new Vector2(670, 100),
                30, new Color(.91f, .93f, .90f), TextAnchor.MiddleLeft);

            int unlocked = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 0), 0, levels.Length - 1);
            string primary = unlocked > 0 ? "继续游戏" : "开始游戏";
            UiKit.Button("Primary", screenRoot, primary, new Vector2(-625, -205), new Vector2(370, 82),
                levels[5].Accent, Color.white, () => StartLevel(unlocked), 32);
            UiKit.Button("Chapters", screenRoot, "章节选择", new Vector2(-625, -305), new Vector2(370, 70),
                new Color(.94f, .91f, .84f), new Color(.11f, .18f, .22f), ShowChapterSelect, 28);
            UiKit.Label("Controls", screenRoot, "鼠标 / 触控均可游玩  ·  移动端横屏", new Vector2(-580, -430), new Vector2(580, 42),
                20, new Color(1f, 1f, 1f, .60f), TextAnchor.MiddleLeft);
        }

        private void ShowChapterSelect()
        {
            ClearScreen();
            SetChapterAmbience(false);
            UiKit.Panel("Background", screenRoot, Vector2.zero, new Vector2(1920, 1080), new Color(.90f, .89f, .84f));
            AddTopBar("章节选择", ShowTitle);
            int unlocked = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 0), 0, levels.Length - 1);

            for (int i = 0; i < levels.Length; i++)
            {
                int captured = i;
                int col = i % 3;
                int row = i / 3;
                Vector2 position = new Vector2(-570 + col * 570, 180 - row * 310);
                Color cardColor = i <= unlocked ? new Color(.96f, .95f, .91f) : new Color(.69f, .70f, .69f);
                var card = UiKit.Button("Chapter" + i, screenRoot, string.Empty, position, new Vector2(480, 245),
                    cardColor, Color.white, () => StartLevel(captured));
                card.interactable = i <= unlocked;
                Texture2D thumbnail = ProceduralArt.Create(levels[i], 480, 160, false);
                generatedTextures.Add(thumbnail);
                var preview = UiKit.TexturePanel("Preview", card.transform, new Vector2(0, 43), new Vector2(472, 151), thumbnail);
                preview.raycastTarget = false;
                UiKit.Panel("Tint", card.transform, new Vector2(0, 43), new Vector2(472, 151), new Color(.02f, .04f, .05f, i <= unlocked ? .23f : .62f)).raycastTarget = false;
                UiKit.Panel("Accent", card.transform, new Vector2(-232, -79), new Vector2(8, 80), i <= unlocked ? levels[i].Accent : new Color(.44f, .45f, .44f)).raycastTarget = false;
                UiKit.Label("Chapter", card.transform, levels[i].Chapter + "  ·  " + levels[i].Time, new Vector2(0, 88), new Vector2(420, 36), 20,
                    i <= unlocked ? levels[i].Accent : new Color(.45f, .45f, .45f), TextAnchor.MiddleLeft, FontStyle.Bold);
                Text chapterTitle = UiKit.Label("Title", card.transform, i <= unlocked ? levels[i].Title : "尚未解锁", new Vector2(8, -79), new Vector2(405, 58), 32,
                    new Color(.10f, .15f, .17f), TextAnchor.MiddleLeft, FontStyle.Normal);
                chapterTitle.font = UiKit.DisplayFont;
            }
        }

        private void StartLevel(int levelIndex)
        {
            currentLevel = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
            ShowExplore();
        }

        private void ShowExplore()
        {
            ClearScreen();
            foundCount = 0;
            hintLevel = 0;
            chainProgress = new[] { 0, 0, 0 };
            LevelDefinition level = levels[currentLevel];
            bool canSkipInvestigation = HasClearedInvestigation(currentLevel);
            Texture2D background = ProceduralArt.Create(level, 1600, 760, false);
            generatedTextures.Add(background);

            UiKit.Panel("Backdrop", screenRoot, Vector2.zero, new Vector2(1920, 1080), new Color(.035f, .045f, .047f));
            RawImage scene = UiKit.TexturePanel("Scene", screenRoot, new Vector2(-170, -5), new Vector2(1480, 850), background);
            UiKit.Panel("SceneTint", screenRoot, new Vector2(-170, -5), new Vector2(1480, 850), new Color(.02f, .025f, .028f, .10f));
            UiKit.Panel("SceneEdge", screenRoot, new Vector2(574, -5), new Vector2(8, 850), new Color(level.Accent.r, level.Accent.g, level.Accent.b, .82f));
            AddTopBar(level.Chapter + "  ·  " + level.Title, ShowChapterSelect);

            Image dossier = UiKit.Panel("Dossier", screenRoot, new Vector2(770, -5), new Vector2(380, 850), new Color(.94f, .925f, .865f));
            UiKit.Label("DossierEyebrow", dossier.transform, "MEMORY ARCHIVE", new Vector2(0, 360), new Vector2(310, 32), 16,
                new Color(.38f, .38f, .34f), TextAnchor.MiddleLeft, FontStyle.Bold);
            Text dossierTitle = UiKit.Label("DossierTitle", dossier.transform, "调查档案", new Vector2(0, 315), new Vector2(310, 58), 34,
                new Color(.10f, .15f, .16f), TextAnchor.MiddleLeft, FontStyle.Normal);
            dossierTitle.font = UiKit.DisplayFont;
            UiKit.Label("DossierObjective", dossier.transform, level.Objective, new Vector2(0, 244), new Vector2(310, 88), 20,
                new Color(.20f, .24f, .24f), TextAnchor.UpperLeft);
            UiKit.Panel("DossierRule", dossier.transform, new Vector2(0, 188), new Vector2(310, 3), level.Accent);
            for (int chain = 0; chain < 3; chain++)
            {
                float y = 116 - chain * 145;
                UiKit.Label("ChainNumber" + chain, dossier.transform, "0" + (chain + 1), new Vector2(-128, y + 30), new Vector2(52, 34), 18,
                    level.Accent, TextAnchor.MiddleLeft, FontStyle.Bold);
                UiKit.Label("ChainTitle" + chain, dossier.transform, level.ChainTitles[chain], new Vector2(24, y + 30), new Vector2(250, 34), 21,
                    new Color(.11f, .16f, .17f), TextAnchor.MiddleLeft, FontStyle.Bold);
                chainTargetTexts[chain] = UiKit.Label("ChainTarget" + chain, dossier.transform, string.Empty,
                    new Vector2(12, y - 6), new Vector2(272, 44), 16,
                    new Color(.32f, .39f, .39f), TextAnchor.MiddleLeft, FontStyle.Normal);
                chainProgressTexts[chain] = UiKit.Label("ChainProgress" + chain, dossier.transform, "○  ○  ○", new Vector2(20, y - 42), new Vector2(255, 24), 20,
                    new Color(.42f, .43f, .40f), TextAnchor.MiddleLeft);
            }
            countText = UiKit.Label("Count", dossier.transform, "拼图碎片  0 / " + level.PuzzlePieceCount, new Vector2(0, -354), new Vector2(310, 44), 23,
                new Color(.16f, .20f, .20f), TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Panel("BottomBar", screenRoot, new Vector2(0, -455), new Vector2(1920, 110), new Color(.025f, .045f, .052f, .94f));
            string openingHelp = canSkipInvestigation
                ? "本章解密已经通过。你可以直接跳过解密开始拼图，也可以重新调查。"
                : currentLevel == 0
                    ? "新手引导 1/3：查看右侧档案的场景描述，判断对应的调查位置。"
                    : "根据右侧档案的描述，在场景中判断下一处调查位置。";
            statusText = UiKit.Label("Objective", screenRoot, openingHelp, new Vector2(-70, -455), new Vector2(1080, 52), 23,
                Color.white, TextAnchor.MiddleCenter);

            startPuzzleButton = UiKit.Button("StartPuzzle", screenRoot, canSkipInvestigation ? "跳过解密" : "开始拼图",
                new Vector2(700, -448), new Vector2(250, 64),
                level.Accent, Color.white, ShowPuzzle, 26);
            startPuzzleButton.interactable = canSkipInvestigation;
            UiKit.Button("Hint", screenRoot, "提示", new Vector2(-790, -448), new Vector2(170, 64),
                new Color(.94f, .91f, .80f), new Color(.15f, .18f, .18f), RevealHint, 25);

            Vector2[] positions = GetHotspotPositions(currentLevel);
            Vector2[] glowSizes = GetHotspotGlowSizes(currentLevel);
            for (int i = 0; i < level.Hotspots.Length; i++)
            {
                int captured = i;
                InvestigationStep step = level.Investigations[i];
                Color chainColor = GetChainColor(step.Chain, level.Accent);
                // Hotspot coordinates are authored directly in the displayed 1480x850 scene.
                Vector2 position = new Vector2(positions[i].x - 170f, positions[i].y - 5f);
                Button button = UiKit.MarkerButton("Hotspot_" + i, screenRoot, position, glowSizes[i],
                    chainColor, () => OpenInvestigation(captured));
                hotspotButtons.Add(button);
            }
            RefreshHotspots();
            SetChapterAmbience(true);
        }

        private static Vector2[] GetHotspotPositions(int levelIndex)
        {
            Vector2[][] positions =
            {
                new[]
                {
                    new Vector2(-416, 208), new Vector2(0, 18), new Vector2(481, 321),
                    new Vector2(361, 142), new Vector2(-411, -165), new Vector2(-425, -90),
                    new Vector2(-393, -66), new Vector2(227, 56), new Vector2(620, 132)
                },
                new[]
                {
                    new Vector2(500, 370), new Vector2(65, -330), new Vector2(509, 75),
                    new Vector2(74, -132), new Vector2(296, -165), new Vector2(-139, -278),
                    new Vector2(-287, -113), new Vector2(-287, -132), new Vector2(-430, -38)
                },
                new[]
                {
                    new Vector2(-278, 236), new Vector2(657, -321), new Vector2(-583, 217),
                    new Vector2(0, -161), new Vector2(111, -283), new Vector2(305, 19),
                    new Vector2(-629, -189), new Vector2(518, -151), new Vector2(657, 56)
                },
                new[]
                {
                    new Vector2(-509, 189), new Vector2(-379, -189), new Vector2(398, 142),
                    new Vector2(-546, -93), new Vector2(-254, -161), new Vector2(185, 47),
                    new Vector2(370, -142), new Vector2(407, 0), new Vector2(425, 345)
                },
                new[]
                {
                    new Vector2(153, -55), new Vector2(-120, 130), new Vector2(-572, -15),
                    new Vector2(552, 265), new Vector2(-325, -125), new Vector2(-400, 265),
                    new Vector2(-65, -145), new Vector2(420, -25), new Vector2(610, -235)
                },
                new[]
                {
                    new Vector2(-499, -47), new Vector2(-583, -132), new Vector2(-523, -24),
                    new Vector2(259, 104), new Vector2(278, 274), new Vector2(444, -90),
                    new Vector2(-389, 66), new Vector2(-278, -132), new Vector2(490, -260)
                }
            };
            return positions[Mathf.Clamp(levelIndex, 0, positions.Length - 1)];
        }

        private static Vector2[] GetHotspotGlowSizes(int levelIndex)
        {
            Vector2[][] sizes =
            {
                new[] { new Vector2(145, 330), new Vector2(680, 38), new Vector2(96, 96), new Vector2(92, 145), new Vector2(112, 205), new Vector2(285, 125), new Vector2(125, 42), new Vector2(135, 58), new Vector2(145, 345) },
                new[] { new Vector2(104, 104), new Vector2(185, 92), new Vector2(158, 82), new Vector2(435, 180), new Vector2(175, 58), new Vector2(420, 108), new Vector2(285, 138), new Vector2(112, 72), new Vector2(255, 104) },
                new[] { new Vector2(225, 165), new Vector2(145, 92), new Vector2(155, 275), new Vector2(610, 245), new Vector2(175, 44), new Vector2(240, 105), new Vector2(222, 250), new Vector2(205, 105), new Vector2(145, 330) },
                new[] { new Vector2(360, 330), new Vector2(118, 255), new Vector2(150, 190), new Vector2(225, 72), new Vector2(175, 265), new Vector2(210, 375), new Vector2(108, 68), new Vector2(145, 182), new Vector2(155, 82) },
                new[] { new Vector2(105, 180), new Vector2(155, 82), new Vector2(190, 82), new Vector2(135, 290), new Vector2(350, 130), new Vector2(120, 130), new Vector2(235, 130), new Vector2(285, 145), new Vector2(205, 190) },
                new[] { new Vector2(330, 135), new Vector2(210, 95), new Vector2(150, 180), new Vector2(245, 205), new Vector2(670, 245), new Vector2(300, 190), new Vector2(185, 72), new Vector2(225, 105), new Vector2(310, 185) }
            };
            return sizes[Mathf.Clamp(levelIndex, 0, sizes.Length - 1)];
        }

        private static Vector2[] Shape(params float[] coordinates)
        {
            var result = new Vector2[coordinates.Length / 2];
            for (int i = 0; i < result.Length; i++) result[i] = new Vector2(coordinates[i * 2], coordinates[i * 2 + 1]);
            return result;
        }

        private static Vector2[] Ellipse(int segments = 16)
        {
            var result = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                result[i] = new Vector2(Mathf.Cos(angle) * .46f, Mathf.Sin(angle) * .46f);
            }
            return result;
        }

        private static Vector2[] GetHotspotOutline(int levelIndex, int hotspotIndex)
        {
            int key = Mathf.Clamp(levelIndex, 0, 5) * 10 + Mathf.Clamp(hotspotIndex, 0, 8);
            switch (key)
            {
                // Chapter 1: curtain, chalk tray, clock, duty sheet, bag, desk, books, stack, cabinet.
                case 0: return Shape(-.40f,.48f, .18f,.48f, .34f,.28f, .26f,.08f, .42f,-.12f, .22f,-.48f, -.20f,-.44f, -.36f,-.10f);
                case 1: return Shape(-.49f,.18f, -.30f,.08f, -.05f,.12f, .22f,.06f, .49f,.14f, .48f,-.12f, .16f,-.18f, -.18f,-.12f, -.48f,-.18f);
                case 2: return Ellipse();
                case 3: return Shape(-.43f,.47f, .42f,.47f, .46f,-.45f, -.45f,-.45f);
                case 4: return Shape(-.22f,.47f, .20f,.43f, .39f,.20f, .34f,-.37f, .12f,-.49f, -.28f,-.40f, -.39f,.12f);
                case 5: return Shape(-.48f,.34f, .42f,.42f, .49f,.08f, .38f,-.42f, -.42f,-.46f, -.49f,-.05f);
                case 6: return Shape(-.47f,.27f, .38f,.43f, .48f,-.20f, -.36f,-.39f);
                case 7: return Shape(-.45f,.20f, -.28f,.44f, .40f,.38f, .48f,-.32f, -.38f,-.42f);
                case 8: return Shape(-.42f,.48f, .40f,.46f, .46f,.24f, .42f,-.48f, -.44f,-.47f, -.48f,.18f);

                // Chapter 2: clock and the exam materials on the foreground desks.
                case 10: return Ellipse();
                case 11: return Shape(-.46f,.35f, .31f,.46f, .48f,.08f, .36f,-.42f, -.43f,-.35f);
                case 12: return Shape(-.44f,.35f, .38f,.46f, .48f,.12f, .42f,-.40f, -.48f,-.44f);
                case 13: return Shape(-.46f,.39f, .41f,.46f, .48f,-.34f, -.41f,-.45f);
                case 14: return Shape(-.49f,.22f, .43f,.46f, .49f,-.20f, -.44f,-.46f);
                case 15: return Shape(-.48f,.38f, .44f,.42f, .49f,-.30f, .25f,-.48f, -.46f,-.39f);
                case 16: return Shape(-.48f,.35f, .26f,.46f, .48f,.11f, .35f,-.44f, -.43f,-.35f);
                case 17: return Shape(-.42f,.25f, -.20f,.45f, .35f,.39f, .47f,.05f, .28f,-.43f, -.35f,-.39f);
                case 18: return Shape(-.45f,.29f, -.34f,.45f, .39f,.39f, .48f,.12f, .37f,-.39f, -.43f,-.34f);

                // Chapter 3: broadcast booth, bib, flag, track, baton, podium, seat, crate, cabinet.
                case 20: return Shape(-.46f,.33f, -.24f,.48f, .35f,.44f, .48f,.06f, .39f,-.45f, -.39f,-.42f);
                case 21: return Shape(-.43f,.46f, .42f,.38f, .47f,-.40f, -.38f,-.47f);
                case 22: return Shape(-.30f,.48f, .33f,.35f, .46f,.02f, .23f,-.42f, -.36f,-.48f, -.47f,.10f);
                case 23: return Shape(-.49f,.35f, -.12f,.48f, .49f,-.10f, .20f,-.46f, -.46f,-.31f);
                case 24: return Shape(-.49f,.18f, .42f,.44f, .49f,.02f, -.42f,-.44f);
                case 25: return Shape(-.46f,.42f, .46f,.43f, .48f,-.40f, -.47f,-.42f);
                case 26: return Shape(-.46f,.44f, .40f,.45f, .48f,.12f, .36f,-.44f, -.43f,-.39f);
                case 27: return Shape(-.45f,.38f, .36f,.47f, .48f,.18f, .40f,-.43f, -.42f,-.46f);
                case 28: return Shape(-.43f,.48f, .44f,.47f, .48f,-.46f, -.46f,-.48f);

                // Chapter 4: window panes, umbrella, notices, paper, bench, locker, tape, timetable, door plate.
                case 30: return Shape(-.48f,.47f, .46f,.47f, .48f,-.45f, -.47f,-.46f);
                case 31: return Shape(-.08f,.48f, .15f,.44f, .42f,.18f, .29f,-.42f, .02f,-.49f, -.28f,-.34f, -.43f,.05f);
                case 32: return Shape(-.45f,.46f, .40f,.43f, .47f,-.43f, -.42f,-.47f);
                case 33: return Shape(-.46f,.31f, .30f,.46f, .48f,.04f, .28f,-.43f, -.44f,-.31f);
                case 34: return Shape(-.48f,.27f, .40f,.42f, .48f,.06f, .39f,-.42f, -.45f,-.38f);
                case 35: return Shape(-.43f,.47f, .42f,.46f, .48f,-.44f, -.46f,-.48f);
                case 36: return Shape(-.49f,.12f, .43f,.45f, .49f,-.10f, -.44f,-.45f);
                case 37: return Shape(-.43f,.47f, .43f,.43f, .47f,-.45f, -.46f,-.47f);
                case 38: return Shape(-.44f,.39f, -.32f,.47f, .42f,.43f, .48f,.20f, .38f,-.42f, -.43f,-.38f);

                // Chapter 5: flask, drawer, flashlight beam, curtain, notebook, board, paper, desk, book box.
                case 40: return Shape(-.25f,.48f, .23f,.45f, .38f,.25f, .34f,-.43f, -.31f,-.45f, -.38f,.24f);
                case 41: return Shape(-.45f,.36f, .42f,.43f, .47f,-.34f, -.42f,-.45f);
                case 42: return Shape(-.49f,.18f, .34f,.46f, .49f,.12f, .36f,-.43f, -.48f,-.18f);
                case 43: return Shape(-.38f,.48f, .20f,.44f, .42f,.18f, .31f,-.46f, -.22f,-.43f, -.46f,.02f);
                case 44: return Shape(-.48f,.30f, -.35f,.46f, .39f,.40f, .48f,-.25f, .32f,-.46f, -.43f,-.39f);
                case 45: return Shape(-.45f,.44f, .40f,.47f, .48f,-.40f, -.43f,-.46f);
                case 46: return Shape(-.47f,.35f, .36f,.46f, .48f,-.20f, -.38f,-.45f);
                case 47: return Shape(-.47f,.35f, .42f,.44f, .49f,-.34f, -.42f,-.45f);
                case 48: return Shape(-.43f,.45f, .40f,.48f, .48f,-.40f, .20f,-.48f, -.46f,-.37f);

                // Chapter 6: accumulated keepsakes, podium, blackboard, bouquet, notebook and yearbook.
                case 50: return Shape(-.47f,.35f, -.30f,.47f, .38f,.42f, .48f,.05f, .34f,-.43f, -.42f,-.46f);
                case 51: return Shape(-.47f,.34f, .31f,.46f, .48f,.08f, .35f,-.43f, -.43f,-.38f);
                case 52: return Shape(-.38f,.48f, .31f,.43f, .47f,.16f, .37f,-.43f, -.40f,-.46f, -.48f,.08f);
                case 53: return Shape(-.46f,.45f, .42f,.44f, .48f,-.43f, -.45f,-.46f);
                case 54: return Shape(-.48f,.45f, .47f,.46f, .49f,-.43f, -.47f,-.44f);
                case 55: return Shape(-.47f,.02f, -.34f,.30f, -.12f,.46f, .08f,.38f, .24f,.48f, .43f,.24f, .48f,-.10f, .28f,-.42f, -.35f,-.45f);
                case 56: return Shape(-.46f,.34f, .31f,.47f, .48f,.04f, .29f,-.44f, -.42f,-.36f);
                case 57: return Shape(-.47f,.31f, -.31f,.47f, .38f,.41f, .48f,-.24f, .30f,-.46f, -.43f,-.38f);
                case 58: return Shape(-.46f,.40f, -.26f,.48f, .41f,.37f, .48f,-.35f, -.36f,-.47f);
                default: return Shape(-.46f,.46f, .46f,.46f, .46f,-.46f, -.46f,-.46f);
            }
        }

        private static Color GetChainColor(int chain, Color accent)
        {
            if (chain == 0) return new Color(accent.r, accent.g, accent.b, .82f);
            if (chain == 1) return new Color(.30f, .58f, .55f, .82f);
            return new Color(.72f, .43f, .34f, .82f);
        }

        private void OpenInvestigation(int index)
        {
            if (investigationOverlay != null) Destroy(investigationOverlay);
            LevelDefinition level = levels[currentLevel];
            InvestigationStep step = level.Investigations[index];
            bool tutorialGate = currentLevel == 0 && step.Chain > 0 && chainProgress[step.Chain - 1] < 3;
            bool available = step.Step == chainProgress[step.Chain] && !tutorialGate;

            investigationOverlay = UiKit.Panel("InvestigationShade", screenRoot, Vector2.zero, new Vector2(1920, 1080),
                new Color(.015f, .022f, .024f, .76f)).gameObject;
            Image card = UiKit.Panel("InvestigationCard", investigationOverlay.transform, Vector2.zero, new Vector2(900, 620),
                new Color(.955f, .94f, .885f));
            UiKit.Panel("CardAccent", card.transform, new Vector2(-446, 0), new Vector2(8, 620), GetChainColor(step.Chain, level.Accent));
            UiKit.Label("Chain", card.transform, "线索 " + (step.Chain + 1).ToString("00") + "  /  " + level.ChainTitles[step.Chain],
                new Vector2(-20, 245), new Vector2(760, 36), 19, level.Accent, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Label("Object", card.transform, step.Name, new Vector2(-20, 188), new Vector2(760, 64), 37,
                new Color(.09f, .14f, .15f), TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Label("Observation", card.transform, step.Observation, new Vector2(-20, 98), new Vector2(760, 100), 25,
                new Color(.17f, .21f, .21f), TextAnchor.UpperLeft);
            UiKit.Panel("CardRule", card.transform, new Vector2(-20, 37), new Vector2(760, 2), new Color(.22f, .24f, .23f, .25f));
            investigationFeedback = UiKit.Label("Feedback", card.transform,
                available ? GetInvestigationPrompt(step) : GetLockedMessage(step),
                new Vector2(-20, -4), new Vector2(760, 52), 20,
                available ? new Color(.30f, .34f, .32f) : new Color(.67f, .35f, .25f), TextAnchor.MiddleLeft);

            if (available)
            {
                string[] actions = { step.CorrectAction, step.WrongActionA, step.WrongActionB };
                int rotation = (index + currentLevel) % 3;
                for (int slot = 0; slot < 3; slot++)
                {
                    int actionIndex = (slot + rotation) % 3;
                    bool correct = actionIndex == 0;
                    string label = actions[actionIndex];
                    int capturedIndex = index;
                    UiKit.Button("Action" + slot, card.transform, label, new Vector2(-20, -82 - slot * 72), new Vector2(760, 58),
                        new Color(.82f, .80f, .73f), new Color(.16f, .20f, .20f),
                        () => ChooseInvestigationAction(capturedIndex, correct), 22);
                }
            }
            UiKit.Button("Close", card.transform, "关闭", new Vector2(350, 268), new Vector2(120, 48),
                new Color(.78f, .76f, .70f), new Color(.16f, .19f, .19f), CloseInvestigation, 20);
        }

        private string GetInvestigationPrompt(InvestigationStep step)
        {
            if (currentLevel != 0) return "根据目前掌握的线索，你准备怎么做？";
            if (step.Chain == 0) return "新手引导 2/3：读出物件透露的信息，再选择最合理的行动。";
            return "新手引导：现在试着独立连接观察与行动。";
        }

        private string GetLockedMessage(InvestigationStep step)
        {
            LevelDefinition level = levels[currentLevel];
            if (currentLevel == 0 && step.Chain > 0 && chainProgress[step.Chain - 1] < 3)
                return "新手引导：先完成上一条调查链，理解线索如何一步步连接。";
            int requiredIndex = step.Chain * 3 + chainProgress[step.Chain];
            return "现在还无法判断。先调查“" + level.Investigations[requiredIndex].Name + "”。";
        }

        private void ChooseInvestigationAction(int index, bool correct)
        {
            InvestigationStep step = levels[currentLevel].Investigations[index];
            if (!correct)
            {
                audioSource.PlayOneShot(tickClip);
                investigationFeedback.text = "这个动作没有利用刚才观察到的细节。再看看物件之间的联系。";
                investigationFeedback.color = new Color(.69f, .31f, .23f);
                return;
            }

            chainProgress[step.Chain]++;
            hintLevel = 0;
            int reward = 0;
            if (step.Step == 2)
            {
                reward = GetChainPieceReward(step.Chain, levels[currentLevel].PuzzlePieceCount);
                foundCount += reward;
            }
            countText.text = "拼图碎片  " + foundCount + " / " + levels[currentLevel].PuzzlePieceCount;
            statusText.text = step.Discovery + (reward > 0 ? "  找回 " + reward + " 块拼图碎片。" : string.Empty);
            audioSource.PlayOneShot(step.Step == 2 ? successClip : tickClip);
            CloseInvestigation();
            RefreshHotspots();
            if (foundCount == levels[currentLevel].PuzzlePieceCount)
            {
                MarkInvestigationCleared(currentLevel);
                statusText.text = "三条调查链已经闭合。现在把完整画面拼回来。";
                startPuzzleButton.interactable = true;
                startPuzzleButton.GetComponentInChildren<Text>().text = "拼合记忆";
            }
        }

        private bool HasClearedInvestigation(int levelIndex)
        {
            int clearedMask = PlayerPrefs.GetInt(InvestigationSaveKey, 0);
            if ((clearedMask & (1 << levelIndex)) != 0) return true;

            // Existing saves only tracked chapter unlocks. Any chapter before the
            // highest unlocked chapter must already have passed its investigation.
            return levelIndex < PlayerPrefs.GetInt(SaveKey, 0);
        }

        private static void MarkInvestigationCleared(int levelIndex)
        {
            int clearedMask = PlayerPrefs.GetInt(InvestigationSaveKey, 0);
            int updatedMask = clearedMask | (1 << levelIndex);
            if (updatedMask == clearedMask) return;
            PlayerPrefs.SetInt(InvestigationSaveKey, updatedMask);
            PlayerPrefs.Save();
        }

        private void CloseInvestigation()
        {
            if (investigationOverlay != null) Destroy(investigationOverlay);
            investigationOverlay = null;
        }

        private static int GetChainPieceReward(int chain, int total)
        {
            int reward = total / 3;
            int remainder = total % 3;
            if (chain == 1 && remainder > 1) reward++;
            if (chain == 2 && remainder > 0) reward++;
            return reward;
        }

        private void RefreshHotspots()
        {
            LevelDefinition level = levels[currentLevel];
            for (int i = 0; i < hotspotButtons.Count; i++)
            {
                InvestigationStep step = level.Investigations[i];
                Button button = hotspotButtons[i];
                bool solved = step.Step < chainProgress[step.Chain];
                bool tutorialGate = currentLevel == 0 && step.Chain > 0 && chainProgress[step.Chain - 1] < 3;
                bool available = step.Step == chainProgress[step.Chain] && !tutorialGate;
                button.interactable = available;
                HotspotGlow glow = button.GetComponent<HotspotGlow>();
                if (glow != null) glow.SetState(solved, available, GetChainColor(step.Chain, level.Accent));
            }
            for (int chain = 0; chain < 3; chain++)
            {
                string progress = string.Empty;
                for (int step = 0; step < 3; step++) progress += step < chainProgress[chain] ? "●  " : "○  ";
                chainProgressTexts[chain].text = progress.TrimEnd();
                chainProgressTexts[chain].color = chainProgress[chain] == 3
                    ? new Color(.21f, .48f, .36f)
                    : new Color(.42f, .43f, .40f);

                if (chainProgress[chain] < 3)
                {
                    int investigationIndex = chain * 3 + chainProgress[chain];
                    bool tutorialGate = currentLevel == 0 && chain > 0 && chainProgress[chain - 1] < 3;
                    chainTargetTexts[chain].text = (tutorialGate ? "待续 · " : "档案 · ") +
                        DossierHints[currentLevel][investigationIndex];
                    chainTargetTexts[chain].color = tutorialGate
                        ? new Color(.48f, .48f, .44f)
                        : GetChainColor(chain, level.Accent);
                }
                else
                {
                    chainTargetTexts[chain].text = "已完成 · 线索闭合";
                    chainTargetTexts[chain].color = new Color(.21f, .48f, .36f);
                }
            }
        }

        private void RevealHint()
        {
            LevelDefinition level = levels[currentLevel];
            int chain = 0;
            while (chain < 3 && chainProgress[chain] >= 3) chain++;
            if (chain >= 3) return;
            int index = chain * 3 + chainProgress[chain];
            InvestigationStep step = level.Investigations[index];
            HotspotGlow glow = hotspotButtons[index].GetComponent<HotspotGlow>();
            if (glow != null) glow.Emphasize();
            hotspotButtons[index].transform.SetAsLastSibling();
            hintLevel++;
            if (hintLevel == 1)
                statusText.text = "环境提示：注意“" + step.Name + "”附近的变化。";
            else if (hintLevel == 2)
                statusText.text = "关系提示：“" + step.Observation + "”";
            else
                statusText.text = "行动提示：尝试“" + step.CorrectAction + "”。";
            audioSource.PlayOneShot(tickClip);
        }

        private void ShowPuzzle()
        {
            ClearScreen();
            SetChapterAmbience(true);
            placedCount = 0;
            LevelDefinition level = levels[currentLevel];
            int columns = level.PuzzleColumns;
            int rows = level.PuzzleRows;
            int pieceCount = level.PuzzlePieceCount;
            UiKit.Panel("Background", screenRoot, Vector2.zero, new Vector2(1920, 1080), new Color(.075f, .085f, .09f));
            AddTopBar(level.Chapter + "  ·  " + level.PuzzleTitle, ShowExplore);
            countText = UiKit.Label("Progress", screenRoot, "已归位  0 / " + pieceCount, new Vector2(0, 432), new Vector2(420, 48), 27,
                Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            string puzzleHelp = currentLevel == 0
                ? "新手引导 3/3：按住碎片拖到画面中的正确位置，靠近后会自动吸附。"
                : currentLevel == 1
                    ? "拖动碎片完成画面，这一关不需要旋转。"
                    : "拖动碎片靠近正确位置；方向不对时，选中碎片后点击旋转。";
            statusText = UiKit.Label("Help", screenRoot, puzzleHelp, new Vector2(0, -456), new Vector2(1120, 46), 22,
                new Color(1f, 1f, 1f, .78f));

            const float boardY = -5f;
            const float boardWidth = 960f;
            const float boardHeight = 600f;
            float cellWidth = boardWidth / columns;
            float cellHeight = boardHeight / rows;
            UiKit.Panel("BoardShadow", screenRoot, new Vector2(8, boardY - 10), new Vector2(boardWidth + 18, boardHeight + 18), new Color(0, 0, 0, .35f));
            UiKit.Panel("Board", screenRoot, new Vector2(0, boardY), new Vector2(boardWidth + 10, boardHeight + 10), new Color(.93f, .90f, .82f));
            for (int row = 0; row < rows; row++)
            for (int col = 0; col < columns; col++)
            {
                Color slotColor = (row + col) % 2 == 0 ? new Color(.18f, .20f, .20f, .16f) : new Color(.10f, .12f, .12f, .11f);
                Vector2 target = new Vector2(-boardWidth * .5f + cellWidth * (col + .5f),
                    boardY + boardHeight * .5f - cellHeight * (row + .5f));
                Image slot = UiKit.Panel("Slot", screenRoot, target, new Vector2(cellWidth - 6, cellHeight - 6), slotColor);
                UiKit.AddOutline(slot, new Color(.15f, .16f, .16f, .28f), new Vector2(2, -2));
            }

            Texture2D memory = ProceduralArt.Create(level, (int)boardWidth, (int)boardHeight, true);
            generatedTextures.Add(memory);
            currentMemoryTexture = memory;
            Vector2 previewSize;
            float tabSize = JigsawPieceFactory.GetTabSize(cellWidth, cellHeight);
            float pieceAspect = (cellWidth + tabSize * 2f) / (cellHeight + tabSize * 2f);
            Vector2[] traySlots = BuildTraySlots(pieceCount, pieceAspect, out previewSize);
            int[] shuffled = ShuffleIndices(pieceCount, currentLevel + 17);

            for (int index = 0; index < pieceCount; index++)
            {
                int row = index / columns;
                int col = index % columns;
                Vector2 placedSize;
                Texture2D pieceTexture = JigsawPieceFactory.Create(memory, columns, rows, row, col,
                    currentLevel + 101, out placedSize);
                generatedTextures.Add(pieceTexture);
                Sprite sprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceTexture.width, pieceTexture.height),
                    new Vector2(.5f, .5f), 100f);
                Image image = UiKit.Panel("Piece_" + index, screenRoot, traySlots[shuffled[index]], previewSize, Color.white);
                image.sprite = sprite;
                image.preserveAspect = true;
                image.alphaHitTestMinimumThreshold = .10f;
                image.gameObject.AddComponent<CanvasGroup>();
                PuzzlePieceView piece = image.gameObject.AddComponent<PuzzlePieceView>();
                Vector2 target = new Vector2(-boardWidth * .5f + cellWidth * (col + .5f),
                    boardY + boardHeight * .5f - cellHeight * (row + .5f));
                piece.Configure(this, canvas, target, placedSize);

                if (currentLevel >= 2 && (index + currentLevel) % 3 != 0)
                {
                    int turns = ((index * 7 + currentLevel * 3) % 3) + 1;
                    image.rectTransform.localEulerAngles = new Vector3(0, 0, turns * 90);
                }
            }

            rotateButton = UiKit.Button("Rotate", screenRoot, "↻ 旋转", new Vector2(700, -444), new Vector2(220, 64),
                level.Accent, Color.white, RotateSelected, 25);
            rotateButton.gameObject.SetActive(currentLevel >= 2);
        }

        private static Vector2[] BuildTraySlots(int count, float pieceAspect, out Vector2 previewSize)
        {
            var slots = new Vector2[count];
            int leftCount = (count + 1) / 2;
            int maxSideCount = leftCount;
            int maxColumns = maxSideCount > 5 ? 2 : 1;
            float maxWidth = maxColumns == 2 ? 138f : 238f;
            float maxHeight = maxColumns == 2 ? 108f : 150f;
            float previewWidth = Mathf.Min(maxWidth, maxHeight * pieceAspect);
            float previewHeight = previewWidth / pieceAspect;
            if (previewHeight > maxHeight)
            {
                previewHeight = maxHeight;
                previewWidth = previewHeight * pieceAspect;
            }
            previewSize = new Vector2(previewWidth, previewHeight);

            for (int i = 0; i < count; i++)
            {
                bool left = i < leftCount;
                int sideIndex = left ? i : i - leftCount;
                int sideCount = left ? leftCount : count - leftCount;
                int columns = sideCount > 5 ? 2 : 1;
                int rows = Mathf.CeilToInt(sideCount / (float)columns);
                int row = sideIndex / columns;
                int col = sideIndex % columns;
                float xCenter = left ? -720f : 720f;
                float x = xCenter + (col - (columns - 1) * .5f) * 152f;
                float gap = Mathf.Min(145f, 650f / Mathf.Max(1, rows));
                float y = ((rows - 1) * .5f - row) * gap;
                slots[i] = new Vector2(x, y);
            }
            return slots;
        }

        private static int[] ShuffleIndices(int count, int seed)
        {
            var values = new int[count];
            for (int i = 0; i < count; i++) values[i] = i;
            var random = new System.Random(seed);
            for (int i = values.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = values[i];
                values[i] = values[j];
                values[j] = temp;
            }
            return values;
        }

        public void SelectPiece(PuzzlePieceView piece)
        {
            if (selectedPiece != null && selectedPiece != piece) selectedPiece.SetSelected(false);
            selectedPiece = piece;
            if (selectedPiece != null) selectedPiece.SetSelected(true);
            if (rotateButton != null) rotateButton.interactable = currentLevel >= 2 && selectedPiece != null && !selectedPiece.Placed;
        }

        private void RotateSelected()
        {
            if (selectedPiece == null || selectedPiece.Placed) return;
            selectedPiece.RotateClockwise();
            audioSource.PlayOneShot(tickClip);
        }

        private void Update()
        {
            if (selectedPiece != null && !selectedPiece.Placed && Input.GetKeyDown(KeyCode.R)) RotateSelected();
        }

        public void TryPlace(PuzzlePieceView piece)
        {
            if (piece.Placed) return;
            float distance = Vector2.Distance(piece.Rect.anchoredPosition, piece.TargetPosition);
            float angle = Mathf.Abs(Mathf.DeltaAngle(piece.Rect.localEulerAngles.z, 0));
            float snapDistance = Mathf.Min(piece.TargetSize.x, piece.TargetSize.y) * .48f;
            if (distance <= snapDistance && angle <= 8f)
            {
                piece.Place();
                placedCount++;
                int pieceCount = levels[currentLevel].PuzzlePieceCount;
                countText.text = "已归位  " + placedCount + " / " + pieceCount;
                statusText.text = currentLevel == 0 && placedCount == 1
                    ? "很好，碎片会自动吸附。继续观察画面边缘，把另外三块也放回去。"
                    : "这一块找到了属于它的位置";
                audioSource.PlayOneShot(tickClip);
                if (currentLevel == levels.Length - 1) ShowGraduationMemoryLine(placedCount - 1);
                if (selectedPiece == piece) selectedPiece = null;
                if (placedCount == pieceCount && !completionSequenceRunning)
                {
                    completionSequenceRunning = true;
                    completionRoutine = StartCoroutine(PlayCompletionSequence());
                }
            }
            else if (distance <= snapDistance && angle > 8f)
            {
                statusText.text = "位置接近了，但碎片方向还不对";
            }
        }

        private void ShowGraduationMemoryLine(int index)
        {
            if (index < 0 || index >= GraduationMemoryLines.Length) return;
            if (index == GraduationMemoryLines.Length - 1)
            {
                ShowGraduationFinale();
                return;
            }

            RectTransform memoryLine = UiKit.Rect("GraduationMemoryLine_" + index, screenRoot,
                new Vector2(UnityEngine.Random.Range(-460f, 460f), UnityEngine.Random.Range(-340f, 340f)),
                new Vector2(960f, 190f));
            memoryLine.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(-3.2f, 3.2f));
            memoryLine.SetAsLastSibling();

            CanvasGroup group = memoryLine.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Text text = UiKit.Label("MemoryText", memoryLine, GraduationMemoryLines[index], Vector2.zero,
                new Vector2(940f, 180f), index == GraduationMemoryLines.Length - 1 ? 76 : 60,
                Color.white, TextAnchor.MiddleCenter, FontStyle.Normal);
            text.font = GraduationMemoryFont;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 46;
            text.resizeTextMaxSize = index == GraduationMemoryLines.Length - 1 ? 76 : 60;
            text.gameObject.AddComponent<GraduationTextGradient>();
            UiKit.AddOutline(text, new Color(1f, .72f, .16f, 1f), new Vector2(2.6f, -2.6f));
            UiKit.AddOutline(text, new Color(.10f, .035f, .018f, .96f), new Vector2(4.4f, -4.4f));
            Shadow depth = text.gameObject.AddComponent<Shadow>();
            depth.effectColor = new Color(.08f, .025f, .012f, .76f);
            depth.effectDistance = new Vector2(0f, -6f);
            depth.useGraphicAlpha = true;

            var sparks = new List<RectTransform>();
            for (int i = 0; i < 12; i++)
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float radius = UnityEngine.Random.Range(210f, 390f);
                float size = UnityEngine.Random.Range(4f, 10f);
                Image spark = UiKit.Panel("GoldSpark_" + i, memoryLine,
                    new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * 34f),
                    new Vector2(size, size), new Color(1f, UnityEngine.Random.Range(.62f, .88f), .18f, .92f));
                spark.sprite = UiKit.CircleSprite;
                spark.raycastTarget = false;
                sparks.Add(spark.rectTransform);
            }

            StartCoroutine(AnimateGraduationMemoryLine(memoryLine, group, sparks));
        }

        private void ShowGraduationFinale()
        {
            RectTransform finale = UiKit.Rect("GraduationFinale", screenRoot, Vector2.zero, new Vector2(720f, 620f));
            finale.SetAsLastSibling();

            CanvasGroup group = finale.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Image ringGlow = UiKit.Panel("FinaleRingGlow", finale, Vector2.zero, new Vector2(520f, 520f),
                new Color(1f, .62f, .16f, .24f));
            ringGlow.sprite = UiKit.GlowRingSprite;
            ringGlow.raycastTarget = false;

            Image ring = UiKit.Panel("FinaleRing", finale, Vector2.zero, new Vector2(452f, 452f),
                new Color(1f, .94f, .72f, .94f));
            ring.sprite = UiKit.GlowRingSprite;
            ring.raycastTarget = false;

            CreateGraduationFinaleGlyph(finale, "我", new Vector2(-72f, 88f), new Vector2(180f, 185f), 132, -6f);
            CreateGraduationFinaleGlyph(finale, "们", new Vector2(72f, 88f), new Vector2(180f, 185f), 132, 5f);
            CreateGraduationFinaleGlyph(finale, "毕", new Vector2(-120f, -76f), new Vector2(195f, 205f), 150, -4f);
            CreateGraduationFinaleGlyph(finale, "业", new Vector2(0f, -76f), new Vector2(195f, 205f), 150, 2f);
            CreateGraduationFinaleGlyph(finale, "了", new Vector2(120f, -76f), new Vector2(195f, 205f), 150, -4f);

            var sparks = new List<RectTransform>();
            for (int i = 0; i < 18; i++)
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float radius = UnityEngine.Random.Range(235f, 315f);
                float size = UnityEngine.Random.Range(4f, 11f);
                Image spark = UiKit.Panel("FinaleSpark_" + i, finale,
                    new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius),
                    new Vector2(size, size), new Color(1f, UnityEngine.Random.Range(.68f, .94f), .25f, .94f));
                spark.sprite = UiKit.CircleSprite;
                spark.raycastTarget = false;
                sparks.Add(spark.rectTransform);
            }

            StartCoroutine(AnimateGraduationMemoryLine(finale, group, sparks, true));
        }

        private void CreateGraduationFinaleGlyph(Transform parent, string glyph, Vector2 position,
            Vector2 size, int fontSize, float rotation)
        {
            Text text = UiKit.Label("FinaleGlyph_" + glyph, parent, glyph, position, size, fontSize,
                Color.white, TextAnchor.MiddleCenter, FontStyle.Normal);
            text.font = GraduationMemoryFont;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(64, fontSize - 36);
            text.resizeTextMaxSize = fontSize;
            text.gameObject.AddComponent<GraduationTextGradient>();
            UiKit.AddOutline(text, new Color(1f, .75f, .22f, 1f), new Vector2(3.4f, -3.4f));
            UiKit.AddOutline(text, new Color(.09f, .025f, .012f, .98f), new Vector2(6f, -6f));
            Shadow depth = text.gameObject.AddComponent<Shadow>();
            depth.effectColor = new Color(.06f, .015f, .008f, .84f);
            depth.effectDistance = new Vector2(0f, -8f);
            depth.useGraphicAlpha = true;
            text.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
        }

        private Font GraduationMemoryFont
        {
            get
            {
                if (graduationMemoryFont != null) return graduationMemoryFont;
                graduationMemoryFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "华文行楷", "STXingkai", "方正舒体", "FZShuTi", "KaiTi", "Microsoft YaHei" }, 52);
                if (graduationMemoryFont == null) graduationMemoryFont = UiKit.DisplayFont;
                return graduationMemoryFont;
            }
        }

        private IEnumerator AnimateGraduationMemoryLine(RectTransform root, CanvasGroup group,
            List<RectTransform> sparks, bool stayCentered = false)
        {
            const float fadeIn = .28f;
            const float hold = 1.55f;
            const float fadeOut = 1.15f;
            float duration = fadeIn + hold + fadeOut;
            Vector2 start = root.anchoredPosition;

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                if (root == null || group == null) yield break;
                if (elapsed < fadeIn) group.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeIn);
                else if (elapsed < fadeIn + hold) group.alpha = 1f;
                else group.alpha = 1f - Mathf.SmoothStep(0f, 1f, (elapsed - fadeIn - hold) / fadeOut);

                root.anchoredPosition = stayCentered ? start : start + Vector2.up * (elapsed * 10f);
                for (int i = 0; i < sparks.Count; i++)
                {
                    if (sparks[i] == null) continue;
                    sparks[i].anchoredPosition += new Vector2((i % 2 == 0 ? -1f : 1f) * 5f,
                        11f + i % 3 * 3f) * Time.unscaledDeltaTime;
                    float pulse = .72f + Mathf.Sin(elapsed * 7f + i) * .28f;
                    sparks[i].localScale = Vector3.one * pulse;
                }
                yield return null;
            }

            if (root != null) Destroy(root.gameObject);
        }

        private IEnumerator PlayCompletionSequence()
        {
            if (rotateButton != null) rotateButton.interactable = false;
            if (currentLevel != 4) SetChapterAmbience(false);
            audioSource.PlayOneShot(currentLevel == 1 ? celebrationClip : successClip);
            if (currentLevel == levels.Length - 1) yield return new WaitForSecondsRealtime(2.75f);

            RectTransform reveal = UiKit.Stretch("MemoryReveal", screenRoot);
            CanvasGroup group = reveal.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = true;
            group.blocksRaycasts = true;
            UiKit.Panel("RevealShade", reveal, Vector2.zero, new Vector2(1920, 1080), new Color(.018f, .025f, .028f, .96f));
            RawImage memoryView = UiKit.TexturePanel("LivingMemory", reveal, new Vector2(0, 12), new Vector2(1440, 900), currentMemoryTexture);
            memoryView.raycastTarget = false;
            UiKit.AddOutline(memoryView, new Color(levels[currentLevel].Accent.r, levels[currentLevel].Accent.g,
                levels[currentLevel].Accent.b, .72f), new Vector2(3f, -3f));
            AmbientMotionLayer.Attach(memoryView.transform, currentLevel, new Vector2(1440, 900), true);
            Text caption = UiKit.Label("RevealCaption", reveal, "这一刻，重新鲜活起来", new Vector2(0, -486), new Vector2(720, 44), 24,
                new Color(1f, 1f, 1f, .74f), TextAnchor.MiddleCenter, FontStyle.Normal);
            caption.font = UiKit.DisplayFont;

            const float fadeIn = .55f;
            for (float elapsed = 0f; elapsed < fadeIn; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / fadeIn);
                group.alpha = t * t * (3f - 2f * t);
                yield return null;
            }
            group.alpha = 1f;
            yield return new WaitForSecondsRealtime(4.6f);

            const float fadeOut = .40f;
            for (float elapsed = 0f; elapsed < fadeOut; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / fadeOut);
                group.alpha = 1f - t * t;
                yield return null;
            }

            Destroy(reveal.gameObject);
            completionRoutine = null;
            completionSequenceRunning = false;
            ShowCompletion();
        }

        private void ShowCompletion()
        {
            LevelDefinition level = levels[currentLevel];
            int nextUnlocked = Mathf.Min(currentLevel + 1, levels.Length - 1);
            int saved = PlayerPrefs.GetInt(SaveKey, 0);
            if (nextUnlocked > saved)
            {
                PlayerPrefs.SetInt(SaveKey, nextUnlocked);
                PlayerPrefs.Save();
            }

            UiKit.Panel("CompletionShade", screenRoot, Vector2.zero, new Vector2(1920, 1080), new Color(.025f, .035f, .04f, .78f));
            Image card = UiKit.Panel("CompletionCard", screenRoot, Vector2.zero, new Vector2(920, 530), new Color(.95f, .93f, .86f));
            UiKit.AddOutline(card, new Color(level.Accent.r, level.Accent.g, level.Accent.b, .95f), new Vector2(7, -7));
            UiKit.Panel("PaperRule", card.transform, new Vector2(0, 215), new Vector2(760, 2), new Color(level.Accent.r, level.Accent.g, level.Accent.b, .50f));
            Text complete = UiKit.Label("Complete", card.transform, currentLevel == levels.Length - 1 ? "三年 · 完整归位" : "记忆 · 已复原",
                new Vector2(0, 170), new Vector2(780, 70), 45, new Color(.11f, .17f, .19f), TextAnchor.MiddleCenter, FontStyle.Normal);
            complete.font = UiKit.DisplayFont;
            UiKit.Label("Story", card.transform, level.CompletionText, new Vector2(0, 40), new Vector2(760, 160), 29,
                new Color(.16f, .21f, .22f), TextAnchor.MiddleCenter);
            string buttonLabel = currentLevel == levels.Length - 1 ? "阅读来信" : "进入下一章";
            UnityEngine.Events.UnityAction action = currentLevel == levels.Length - 1
                ? (UnityEngine.Events.UnityAction)ShowPlayerLetter
                : () => StartLevel(currentLevel + 1);
            UiKit.Button("Continue", card.transform, buttonLabel, new Vector2(0, -160), new Vector2(330, 76),
                level.Accent, Color.white, action, 29);
        }

        private void ShowPlayerLetter()
        {
            ClearScreen();
            SetChapterAmbience(false);

            Color accent = levels[levels.Length - 1].Accent;
            Texture2D background = ProceduralArt.Create(levels[levels.Length - 1], 1600, 900, false);
            generatedTextures.Add(background);
            RawImage backdrop = UiKit.TexturePanel("LetterBackdrop", screenRoot, Vector2.zero, new Vector2(1920, 1080), background);
            backdrop.raycastTarget = false;
            UiKit.Panel("LetterShade", screenRoot, Vector2.zero, new Vector2(1920, 1080), new Color(.018f, .027f, .03f, .88f));

            Image paper = UiKit.Panel("PlayerLetter", screenRoot, Vector2.zero, new Vector2(1280, 900),
                new Color(.955f, .94f, .885f));
            UiKit.AddOutline(paper, new Color(accent.r, accent.g, accent.b, .92f), new Vector2(8f, -8f));
            UiKit.Panel("LetterAccent", paper.transform, new Vector2(-626f, 0f), new Vector2(10f, 900f), accent);

            UiKit.Label("LetterEyebrow", paper.transform, "A LETTER TO YOU", new Vector2(0f, 388f), new Vector2(1080f, 30f),
                16, new Color(accent.r, accent.g, accent.b, .92f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Text title = UiKit.Label("LetterTitle", paper.transform, "致玩家", new Vector2(0f, 340f), new Vector2(1080f, 64f),
                42, new Color(.10f, .16f, .18f), TextAnchor.MiddleCenter, FontStyle.Normal);
            title.font = UiKit.DisplayFont;
            UiKit.Label("LetterSubtitle", paper.transform, "写在故事结束以后", new Vector2(0f, 300f), new Vector2(1080f, 34f),
                19, new Color(.32f, .36f, .35f), TextAnchor.MiddleCenter, FontStyle.Normal);
            UiKit.Panel("LetterRule", paper.transform, new Vector2(0f, 272f), new Vector2(1080f, 2f),
                new Color(accent.r, accent.g, accent.b, .42f));

            Text body = UiKit.Label("LetterBody", paper.transform, PlayerLetter, new Vector2(0f, -5f), new Vector2(1080f, 520f),
                26, new Color(.13f, .18f, .19f), TextAnchor.UpperLeft, FontStyle.Normal);
            body.lineSpacing = 1.24f;
            body.resizeTextForBestFit = true;
            body.resizeTextMinSize = 22;
            body.resizeTextMaxSize = 26;

            UiKit.Button("AcceptLetter", paper.transform, "收下这封信", new Vector2(0f, -390f), new Vector2(330f, 68f),
                accent, Color.white, ShowTitle, 27);
        }

        private void AddTopBar(string title, UnityEngine.Events.UnityAction backAction)
        {
            UiKit.Panel("TopBar", screenRoot, new Vector2(0, 490), new Vector2(1920, 100), new Color(.035f, .055f, .062f, .94f));
            UiKit.Button("Back", screenRoot, "‹ 返回", new Vector2(-810, 490), new Vector2(190, 60),
                new Color(.16f, .20f, .21f), Color.white, backAction, 24);
            Text topTitle = UiKit.Label("TopTitle", screenRoot, title, new Vector2(0, 490), new Vector2(980, 64), 31,
                Color.white, TextAnchor.MiddleCenter, FontStyle.Normal);
            topTitle.font = UiKit.DisplayFont;
            UiKit.Label("Stage", screenRoot, (currentLevel + 1).ToString("00") + " / " + levels.Length.ToString("00"), new Vector2(810, 490), new Vector2(180, 50), 23,
                new Color(1f, 1f, 1f, .66f));
        }
    }

    public sealed class GraduationTextGradient : BaseMeshEffect
    {
        private static readonly Color[] Colors =
        {
            new Color(.84f, .23f, .38f),
            new Color(1f, .58f, .34f),
            new Color(1f, .94f, .72f)
        };

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive() || vertexHelper.currentVertCount == 0) return;

            UIVertex vertex = default(UIVertex);
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);
                minY = Mathf.Min(minY, vertex.position.y);
                maxY = Mathf.Max(maxY, vertex.position.y);
            }

            float height = Mathf.Max(1f, maxY - minY);
            for (int i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);
                float normalizedY = Mathf.Clamp01((vertex.position.y - minY) / height);
                float scaled = normalizedY * (Colors.Length - 1);
                int left = Mathf.Min(Mathf.FloorToInt(scaled), Colors.Length - 2);
                Color gradient = Color.Lerp(Colors[left], Colors[left + 1], scaled - left);
                gradient.a *= vertex.color.a / 255f;
                vertex.color = gradient;
                vertexHelper.SetUIVertex(vertex, i);
            }
        }
    }
}
