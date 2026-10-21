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
            var ring = MakeSprite("Ring", 2); var robot = MakeSprite("Robot", 3); var bug = MakeSprite("Bug", 4); var coffee = MakeSprite("Coffee", 5);
            AssetDatabase.Refresh();
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            string matPath = Root + "Art/UnlitSprite.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, matPath); }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var background = new GameObject("Fundo").AddComponent<Camera>();
            background.depth = -1; background.cullingMask = 0; background.clearFlags = CameraClearFlags.SolidColor;
            background.backgroundColor = new Color32(17, 25, 35, 255);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true;
            camera.orthographicSize = 5.5f; camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(17, 25, 35, 255);
            camera.rect = new Rect(.025f, .13f, .70f, .68f); cameraObject.AddComponent<AudioListener>();
            var manager = new GameObject("NeuroRun");
            var sound = manager.AddComponent<NeuroAudio>();
            sound.music = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/LabLoop.wav");
            string[] names = { "Collect", "Correct", "Wrong", "Damage", "Dash", "Win", "Click" };
            sound.effects = new AudioClip[names.Length];
            for (int i = 0; i < names.Length; i++) sound.effects[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/" + names[i] + ".wav");
            var game = manager.AddComponent<NeuroRunGame>();
            game.square = square; game.diamond = diamond; game.ring = ring; game.robot = robot; game.bug = bug;
            game.coffee = coffee; game.spriteMaterial = material; game.worldCamera = camera; game.sound = sound;
            game.playerPrefab = MakePrefab("Player", robot, material, 0);
            game.bugPrefab = MakePrefab("Bug", bug, material, 1);
            game.dataPrefab = MakePrefab("Dado", diamond, material, 2);
            game.regularFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/DejaVuSansMono.ttf");
            game.boldFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/DejaVuSansMono-Bold.ttf");
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
            go.transform.localScale = Vector3.one * (kind == 0 ? .47f : kind == 1 ? .48f : .44f);
            var collider = go.AddComponent<CircleCollider2D>(); collider.radius = .36f;
            if (kind == 2)
            {
                collider.isTrigger = true; sr.color = Color.white;
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

        // Pixel art original, desenhada em uma matriz 16x16. Cada letra é uma cor.
        static Sprite MakeSprite(string name, int kind)
        {
            string[] disk = {
                "................", "..YYYYYYYYYYYY..", "..YCCCCCCCCCYY..", "..YCBBBBBCCCYYY.",
                "..YCBBBBBCCCYYY.", "..YCCCCCCCCCYYY.", "..YYYYYYYYYYYYY.", "..YYYYYYYYYYYYY.",
                "..YYYWWWWWWWYYY.", "..YYYWBBBBBWYYY.", "..YYYWWWWWWWYYY.", "..YYYWBBBBBWYYY.",
                "..YYYWWWWWWWYYY.", "..YYYYYYYYYYYYY.", "...YYYYYYYYYYY..", "................" };
            string[] monitor = {
                "................", "..CCCCCCCCCCCC..", ".CWWWWWWWWWWWWC.", ".CWBBBBBBBBBBWC.",
                ".CWBGGGGGGBBBWC.", ".CWBBBBBBBBBBWC.", ".CWBGGGGBBBBBWC.", ".CWBBBBBBBBBBWC.",
                ".CWBGGBBBBBBBWC.", ".CWWWWWWWWWWWWC.", ".CCCCCCCCCCCYCC.", "..CCCCCCCCCCCC..",
                "......CCCC......", "....CCCCCCCC....", "....WWWWWWWW....", "................" };
            string[] runner = {
                "................", "......RRRR......", ".....RRRRRRR....", "....RRRRRRRRR...",
                ".....CCCCCC.....", ".....CBBCCB.....", ".....CCCCCC.....", "......CCCC......",
                "....RRWWRRRR....", "...CRRWWRRRRC...", "...CRRRRRRRRC...", "....RRRRRRRR....",
                ".....BBBBBB.....", ".....BB..BB.....", "....WWW..WWW....", "................" };
            string[] enemy = {
                "................", "...W........W...", "....W......W....", "...WWWWWWWWWW...",
                "..WWWWWWWWWWWW..", ".WWWBBWWWWBBWWW.", ".WWWBCWWWWBCWWW.", "..WWWWWWWWWWWW..",
                ".WWWWWWWWWWWWWW.", ".WWWBWWWWWWBWWW.", "..WWWBBBBBBWWW..", "...WWWWWWWWWW...",
                "....WW.WW.WW....", "...WW..WW..WW...", "................", "................" };
            string[] cup = {
                "................", "....C...C.......", ".....C...C......", "....C...C.......",
                "..WWWWWWWWWW....", "..WYYYYYYYYWWWW.", "..WWWWWWWWWW..W.", "..WWWWWWWWWW..W.",
                "..WWWRRRRWWW..W.", "..WWWRRRRWWWWWW.", "..WWWWWWWWWW....", "...WWWWWWWW.....",
                "....WWWWWW......", "..CCCCCCCCCCCC..", "................", "................" };
            var pixels = kind == 1 ? disk : kind == 2 ? monitor : kind == 3 ? runner : kind == 4 ? enemy : cup;
            var texture = new Texture2D(16,16,TextureFormat.RGBA32,false);
            for (int y=0;y<16;y++) for(int x=0;x<16;x++)
            {
                char c = kind == 0 ? 'W' : pixels[15-y][x];
                Color color = Color.clear;
                if(c=='W') color=Color.white;
                if(c=='B') color=new Color32(22,30,39,255);
                if(c=='C') color=new Color32(209,188,148,255);
                if(c=='Y') color=new Color32(237,183,78,255);
                if(c=='R') color=new Color32(210,83,57,255);
                if(c=='G') color=new Color32(134,183,139,255);
                texture.SetPixel(x,y,color);
            }
            texture.Apply(); string path=Root+"Art/"+name+".png";
            File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.spritePixelsPerUnit=16;
            importer.mipmapEnabled=false; importer.alphaIsTransparency=true;
            importer.filterMode=FilterMode.Point; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
