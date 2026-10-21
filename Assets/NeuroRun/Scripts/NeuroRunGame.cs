using UnityEngine;
using UnityEngine.InputSystem;

namespace NeuroRun
{
    // Controlador da partida. Estados explícitos evitam ações durante menus e leituras.
    public class NeuroRunGame : MonoBehaviour
    {
        public enum GameState { Menu, Playing, Quiz, Feedback, SectorComplete, Paused, Won, Lost, Guide }
        public Sprite square, diamond, ring, robot, bug, coffee;
        public Material spriteMaterial;
        public GameObject playerPrefab, bugPrefab, dataPrefab;
        public Font regularFont, boldFont;
        public NeuroAudio sound;
        public Camera worldCamera;
        public NeuroUI ui;
        public GameState State { get; private set; }
        public RunnerController Player { get; private set; }
        public bool IsPlaying => State == GameState.Playing;
        public int Sector { get; private set; }
        public int Score { get; private set; }
        public int Health { get; private set; }
        public int Data { get; private set; }
        public int Bits { get; private set; }
        public int BitsNeeded { get; private set; }
        public MazeGrid Maze { get; private set; }
        public float SectorTime { get; private set; }
        public float CoffeeTime { get; private set; }
        public bool TerminalReady => Data >= 6 && Bits >= BitsNeeded;
        public bool Alarm => Data >= 5 || Remaining <= 25;
        public float TimeLimit => 95 - Sector * 3;
        public void SetBitTarget(int count) { BitsNeeded = count; }
        public int Solved { get; private set; }
        public int FirstTry { get; private set; }
        public float Remaining { get; private set; }
        public float Elapsed { get; private set; }
        public string Toast { get; private set; }
        public Lesson Lesson => LessonData.All[Sector];
        public Question CurrentQuestion => Lesson.questions[questionIndex];
        public int[] OptionMap { get; private set; } = { 0, 1, 2 };
        public int BestScore => PlayerPrefs.GetInt("NeuroRun.ArcadeBest", 0);
        GameObject world;
        Transform terminal;
        int questionIndex, checkpointScore, checkpointFirstTry, guidePage;
        bool missedQuestion, feedbackCorrect;
        float toastUntil;

        void Start()
        {
            Application.targetFrameRate = 60;
            ui = gameObject.AddComponent<NeuroUI>(); ui.Initialize(this);
            Menu();
        }
        void Update()
        {
            // Mantém a arena inteira visível mesmo em fullscreen com outra proporção.
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            float left = (Screen.width - 1280 * scale) / 2, bottom = (Screen.height - 720 * scale) / 2;
            worldCamera.rect = new Rect((left + 32 * scale) / Screen.width, (bottom + 82 * scale) / Screen.height,
                960 * scale / Screen.width, 544 * scale / Screen.height);
            float aspect = 960f / 544f;
            worldCamera.orthographicSize = Mathf.Max(5.4f, 9.8f / aspect);
            var k = Keyboard.current; var pad = Gamepad.current;
            bool pause = (k != null && k.escapeKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame);
            if (pause) { if (IsPlaying) Pause(); else if (State == GameState.Paused) Resume(); }
            if (State == GameState.Quiz && k != null)
            {
                if (k.digit1Key.wasPressedThisFrame) Answer(0);
                else if (k.digit2Key.wasPressedThisFrame) Answer(1);
                else if (k.digit3Key.wasPressedThisFrame) Answer(2);
            }
            if (IsPlaying)
            {
                Remaining -= Time.deltaTime; Elapsed += Time.deltaTime; SectorTime += Time.deltaTime; CoffeeTime = Mathf.Max(0, CoffeeTime - Time.deltaTime);
                if (Remaining <= 0) { Remaining = 0; Lose("O tempo deste setor acabou."); }
                else if (TerminalReady && Vector2.Distance(Player.transform.position, terminal.position) < .28f) OpenQuestion();
                if (Time.unscaledTime > toastUntil) Toast = "";
            }
            if (terminal != null)
                terminal.GetComponent<SpriteRenderer>().color = TerminalReady ? new Color(1, .88f, .51f) : new Color(.42f,.48f,.51f);
            ui.UpdateHUD();
        }
        public void StartMission()
        {
            sound.StartMusic(); sound.Play(NeuroAudio.Cue.Click);
            Sector = 0; Score = 0; Elapsed = 0; Solved = 0; FirstTry = 0;
            checkpointScore = 0; checkpointFirstTry = 0; StartSector();
        }
        void StartSector()
        {
            ClearWorld();
            Maze = new MazeGrid(Sector);
            world = ArenaFactory.Build(this, Sector, out var player, out terminal); Player = player;
            worldCamera.enabled = true;
            Health = 3; Data = 0; Bits = 0; CoffeeTime = 0; SectorTime = 0; Remaining = TimeLimit; questionIndex = 0; missedQuestion = false;
            State = GameState.Playing; ui.ShowHUD();
            Tell("6 disquetes + " + BitsNeeded + " pontos. Depois, terminal no centro.");
        }
        void ClearWorld()
        { if (world != null) { world.SetActive(false); Destroy(world); } }
        public void CollectData()
        {
            if (!IsPlaying) return;
            Data++; Score += 100; Remaining += 2; sound.Play(NeuroAudio.Cue.Collect);
            Tell(TerminalReady ? "TERMINAL ABERTO. Vá para o centro!" : Data == 5 ? "ALARME! Eles ficaram mais rápidos." : "Disquete " + Data + "/6. +100 pontos, +2s.");
        }
        public void CollectBit()
        {
            if (!IsPlaying) return;
            Bits++; Score += 5;
            if (Bits % 6 == 0) sound.Play(NeuroAudio.Cue.Collect);
            if (TerminalReady) Tell("TERMINAL ABERTO. Vá para o centro!");
        }
        public void DrinkCoffee()
        {
            CoffeeTime = 4; Score += 50; sound.Play(NeuroAudio.Cue.Dash);
            Tell("CAFÉ! Bugs parados por 4 segundos. Aproveite.");
        }
        public void TakeDamage()
        {
            if (!IsPlaying || Player.Invulnerable) return;
            Health--; Remaining = Mathf.Max(0, Remaining - 6); sound.Play(NeuroAudio.Cue.Damage);
            if (Health <= 0) Lose("Três vidas. Os bugs levaram as três.");
            else
            {
                Player.ResetPosition(Maze.Position(Maze.Spawn));
                foreach (var enemy in world.GetComponentsInChildren<BugEnemy>()) enemy.ResetPatrol();
                Tell("Pegaram você. −1 vida, −6s. Os itens coletados continuam salvos.");
            }
        }
        void OpenQuestion()
        {
            State = GameState.Quiz;
            // Embaralhar alternativas impede que a resposta certa tenha sempre a mesma tecla.
            OptionMap = new[] { 0, 1, 2 };
            for (int i = 2; i > 0; i--) { int j = Random.Range(0, i + 1); int a = OptionMap[i]; OptionMap[i] = OptionMap[j]; OptionMap[j] = a; }
            ui.ShowQuestion(questionIndex + 1);
        }
        public void Answer(int option)
        {
            if (State != GameState.Quiz || option < 0 || option > 2) return;
            feedbackCorrect = OptionMap[option] == CurrentQuestion.correct;
            State = GameState.Feedback;
            if (feedbackCorrect)
            {
                Score += missedQuestion ? 70 : 120; Solved++;
                if (!missedQuestion) FirstTry++;
                sound.Play(NeuroAudio.Cue.Correct);
            }
            else
            {
                Remaining = Mathf.Max(0, Remaining - 8); missedQuestion = true;
                sound.Play(NeuroAudio.Cue.Wrong);
            }
            ui.ShowFeedback(feedbackCorrect, CurrentQuestion);
        }
        public void ContinueFeedback()
        {
            if (State != GameState.Feedback) return;
            if (Remaining <= 0) { Lose("Acabou o tempo. As respostas erradas custaram caro."); return; }
            if (!feedbackCorrect) { OpenQuestion(); return; }
            questionIndex++; missedQuestion = false;
            if (questionIndex < Lesson.questions.Length) { OpenQuestion(); return; }
            Score += Mathf.FloorToInt(Remaining) * 2;
            if (Sector == LessonData.All.Length - 1)
            {
                State = GameState.Won; sound.Play(NeuroAudio.Cue.Win);
                if (Score > BestScore) { PlayerPrefs.SetInt("NeuroRun.ArcadeBest", Score); PlayerPrefs.Save(); }
                ui.ShowResult(true, "Seis labirintos. Dezoito respostas. Pode encerrar o plantão.");
            }
            else { State = GameState.SectorComplete; sound.Play(NeuroAudio.Cue.Win); ui.ShowSectorComplete(); }
        }
        public void NextSector()
        {
            if (State != GameState.SectorComplete) return;
            Sector++; checkpointScore = Score; checkpointFirstTry = FirstTry; StartSector();
        }
        public void RetrySector()
        {
            Score = checkpointScore; FirstTry = checkpointFirstTry; Solved = Sector * 3;
            // O tempo total continua contando a tentativa anterior, mas o setor reinicia completo.
            StartSector();
        }
        void Lose(string reason) { State = GameState.Lost; ui.ShowResult(false, reason); }
        public void Pause() { if (!IsPlaying) return; State = GameState.Paused; ui.ShowPause(); }
        public void Resume() { if (State != GameState.Paused) return; State = GameState.Playing; ui.ShowHUD(); }
        public void Menu()
        {
            ClearWorld(); Player = null; worldCamera.enabled = false;
            State = GameState.Menu; ui.ShowMenu();
        }
        public void Guide(int page = 0)
        { State = GameState.Guide; guidePage = (page + LessonData.All.Length) % LessonData.All.Length; ui.ShowGuide(guidePage); }
        public void NextGuide() { Guide(guidePage + 1); }
        public void Tell(string text) { Toast = text; toastUntil = Time.unscaledTime + 5; }
        public void ToggleSound() { sound.ToggleMute(); ui.RefreshSound(); }
        void OnApplicationFocus(bool focus) { if (!focus && IsPlaying) Pause(); }
        public static string TimeText(float time)
        { int seconds = Mathf.FloorToInt(time); return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00"); }
    }
}
