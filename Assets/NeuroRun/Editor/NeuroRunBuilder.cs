using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeuroRun.Editor
{
    // Ferramenta de montagem, disponível apenas no Editor. Não entra no build.
    public static class NeuroRunBuilder
    {
        const string Root = "Assets/NeuroRun/";
        public const string ScenePath = Root + "Scenes/NeuroRun.unity";

        [MenuItem("NeuroRun/Criar cena do jogo")]
        public static void CreateGame()
        {
            Directory.CreateDirectory(Root + "Art"); Directory.CreateDirectory(Root + "Scenes");
            var square = MakeSprite("Square", 0); var diamond = MakeSprite("Data", 1);
            var ring = MakeSprite("Ring", 2); var robot = MakeSprite("Robot", 3); var bug = MakeSprite("Bug", 4);
            AssetDatabase.Refresh();
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            string matPath = Root + "Art/UnlitSprite.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, matPath); }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var background = new GameObject("Fundo").AddComponent<Camera>();
            background.depth = -1; background.cullingMask = 0; background.clearFlags = CameraClearFlags.SolidColor;
            background.backgroundColor = new Color(.03f, .07f, .12f);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true;
            camera.orthographicSize = 5.5f; camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.03f, .07f, .12f);
            camera.rect = new Rect(.025f, .13f, .70f, .68f); cameraObject.AddComponent<AudioListener>();
            var manager = new GameObject("NeuroRun");
            var sound = manager.AddComponent<NeuroAudio>();
            sound.music = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/LabLoop.wav");
            string[] names = { "Collect", "Correct", "Wrong", "Damage", "Dash", "Win", "Click" };
            sound.effects = new AudioClip[names.Length];
            for (int i = 0; i < names.Length; i++) sound.effects[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/" + names[i] + ".wav");
            var game = manager.AddComponent<NeuroRunGame>();
            game.square = square; game.diamond = diamond; game.ring = ring; game.robot = robot; game.bug = bug;
            game.spriteMaterial = material; game.worldCamera = camera; game.sound = sound;
            game.playerPrefab = MakePrefab("Player", robot, material, 0);
            game.bugPrefab = MakePrefab("Bug", bug, material, 1);
            game.dataPrefab = MakePrefab("Dado", diamond, material, 2);
            game.regularFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/DejaVuSans.ttf");
            game.boldFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/DejaVuSans-Bold.ttf");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true), new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false) };
            PlayerSettings.companyName = "João Pedro Sardou"; PlayerSettings.productName = "NeuroRun";
            PlayerSettings.defaultScreenWidth = 960; PlayerSettings.defaultScreenHeight = 600;
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
            Debug.Log("NeuroRun: cena criada, tutorial preservado e assets próprios atribuídos.");
        }

        static GameObject MakePrefab(string name, Sprite sprite, Material material, int kind)
        {
            Directory.CreateDirectory(Root + "Prefabs");
            var go = new GameObject(name); var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sharedMaterial = material; sr.sortingOrder = kind == 0 ? 8 : 4;
            go.transform.localScale = Vector3.one * (kind == 0 ? .9f : kind == 1 ? .8f : .62f);
            var collider = go.AddComponent<CircleCollider2D>(); collider.radius = .36f;
            if (kind == 2)
            {
                collider.isTrigger = true; sr.color = new Color(1, .85f, .32f);
                go.AddComponent<DataPickup>();
            }
            else
            {
                var body = go.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.freezeRotation = true;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                if (kind == 0) { body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; go.AddComponent<RunnerController>(); }
                else { body.bodyType = RigidbodyType2D.Kinematic; collider.isTrigger = true; go.AddComponent<BugEnemy>(); }
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefabs/" + name + ".prefab");
            Object.DestroyImmediate(go); return prefab;
        }

        [MenuItem("NeuroRun/Configurar WebGL conforme vídeo")]
        public static void ConfigureWebGL()
        {
            // Vídeo da disciplina, 01:44–02:07: Disabled, 960x600 e template Default.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.defaultWebScreenWidth = 960; PlayerSettings.defaultWebScreenHeight = 600;
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            EditorUserBuildSettings.development = false;
            AssetDatabase.SaveAssets();
            Debug.Log("WebGL configurado conforme vídeo: Disabled, Default, 960x600.");
        }

        [MenuItem("NeuroRun/Build and Run WebGL")]
        public static void BuildAndRun()
        {
            ConfigureWebGL(); Directory.CreateDirectory("Builds/WebGL"); Directory.CreateDirectory("Delivery");
            File.WriteAllText("Delivery/build-report.json", "{\"status\":\"building\"}");
            var options = new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/WebGL", target = BuildTarget.WebGL,
                options = BuildOptions.AutoRunPlayer | BuildOptions.DetailedBuildReport
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            File.WriteAllText("Delivery/build-report.json", JsonUtility.ToJson(new BuildResult {
                status = summary.result.ToString(), bytes = summary.totalSize,
                errors = summary.totalErrors, warnings = summary.totalWarnings,
                seconds = summary.totalTime.TotalSeconds
            }, true));
            Debug.Log("NeuroRun WebGL: " + summary.result);
        }
        [System.Serializable]
        class BuildResult { public string status; public ulong bytes; public int errors, warnings; public double seconds; }

        // Sprites geométricos originais: desenhados aqui, sem dependências externas.
        static Sprite MakeSprite(string name, int kind)
        {
            string path = Root + "Art/" + name + ".png";
            var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float u = (x - 63.5f) / 64, v = (y - 63.5f) / 64;
                float d = Mathf.Sqrt(u * u + v * v); Color color = Color.clear;
                if (kind == 0) color = Color.white;
                if (kind == 1 && Mathf.Abs(u) + Mathf.Abs(v) < .94f) color = Color.white;
                if (kind == 2 && d > .78f && d < .94f) color = Color.white;
                if (kind == 3)
                {
                    if (Mathf.Abs(u) < .67f && Mathf.Abs(v) < .8f) color = new Color(.88f, .97f, 1);
                    if (Mathf.Abs(u) < .53f && v > -.02f && v < .50f) color = new Color(.05f, .19f, .27f);
                    if ((Mathf.Abs(u - .28f) < .09f || Mathf.Abs(u + .28f) < .09f) && v > .13f && v < .32f) color = new Color(.45f, 1, .78f);
                    if (Mathf.Abs(u) < .28f && v < -.36f && v > -.51f) color = new Color(.13f, .54f, .65f);
                    if (Mathf.Abs(u) > .67f && Mathf.Abs(u) < .88f && Mathf.Abs(v) < .30f) color = new Color(.3f, .78f, .84f);
                }
                if (kind == 4)
                {
                    if (Mathf.Abs(u) + Mathf.Abs(v) < .95f) color = new Color(1, .35f, .55f);
                    if ((Mathf.Abs(u - .28f) < .09f || Mathf.Abs(u + .28f) < .09f) && Mathf.Abs(v - .1f) < .13f) color = new Color(.12f, .04f, .12f);
                    if (Mathf.Abs(u) < .25f && Mathf.Abs(v + .3f) < .05f) color = Color.white;
                }
                texture.SetPixel(x, y, color);
            }
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 128; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
