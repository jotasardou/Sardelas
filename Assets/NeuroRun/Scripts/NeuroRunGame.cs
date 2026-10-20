using UnityEngine;
using UnityEngine.InputSystem;

namespace NeuroRun
{
    // Controlador da partida. Estados explícitos evitam ações durante menus e leituras.
    public class NeuroRunGame : MonoBehaviour
    {
        public enum GameState { Menu, Playing, Quiz, Feedback, SectorComplete, Paused, Won, Lost, Guide }
        public Sprite square, diamond, ring, robot, bug;
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
        public int Solved { get; private set; }
        public int FirstTry { get; private set; }
        public float Remaining { get; private set; }
        public float Elapsed { get; private set; }
        public string Toast { get; private set; }
        public Lesson Lesson => LessonData.All[Sector];
        public Question CurrentQuestion => Lesson.questions[questionIndex];
        public int[] OptionMap { get; private set; } = { 0, 1, 2 };
        public int BestScore => PlayerPrefs.GetInt("NeuroRun.Best", 0);
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
            worldCamera.rect = new Rect((left + 32 * scale) / Screen.width, (bottom + 93.6f * scale) / Screen.height,
                896 * scale / Screen.width, 489.6f * scale / Screen.height);
            float aspect = 896f / 489.6f;
            worldCamera.orthographicSize = Mathf.Max(5.2f, 10.2f / aspect);
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
                Remaining -= Time.deltaTime; Elapsed += Time.deltaTime;
                if (Remaining <= 0) { Remaining = 0; Lose("O tempo deste setor acabou."); }
                else if (Data >= 3 && Vector2.Distance(Player.transform.position, terminal.position) < .85f) OpenQuestion();
                if (Time.unscaledTime > toastUntil) Toast = "";
            }
            if (terminal != null && Data >= 3)
            {
                terminal.GetComponent<SpriteRenderer>().color = new Color(.45f, 1, .76f);
                terminal.Rotate(0, 0, 18 * Time.deltaTime);
            }
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
            world = ArenaFactory.Build(this, Sector, out var player, out terminal); Player = player;
            worldCamera.enabled = true;
            Health = 3; Data = 0; Remaining = 70; questionIndex = 0; missedQuestion = false;
            State = GameState.Playing; ui.ShowHUD();
            Tell("Colete os 3 dados dourados. Depois, entre no núcleo central.");
        }
        void ClearWorld()
        { if (world != null) { world.SetActive(false); Destroy(world); } }
        public void CollectData()
        {
            if (!IsPlaying) return;
            Data++; Score += 25; Remaining += 3; sound.Play(NeuroAudio.Cue.Collect);
            Tell(Data == 3 ? "NÚCLEO LIBERADO! Entre no círculo verde central." : "+25 pontos · +3 segundos · dado recuperado");
        }
        public void TakeDamage()
        {
            if (!IsPlaying || Player.Invulnerable) return;
            Health--; sound.Play(NeuroAudio.Cue.Damage);
            if (Health <= 0) Lose("Os bugs esgotaram sua integridade.");
            else { Player.ResetPosition(new Vector2(0, -3.6f)); Tell("Integridade perdida. Você tem 2 segundos de proteção."); }
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
            if (Remaining <= 0) { Lose("As penalidades esgotaram o tempo do setor."); return; }
            if (!feedbackCorrect) { OpenQuestion(); return; }
            questionIndex++; missedQuestion = false;
            if (questionIndex < Lesson.questions.Length) { OpenQuestion(); return; }
            Score += Mathf.FloorToInt(Remaining) * 2;
            if (Sector == LessonData.All.Length - 1)
            {
                State = GameState.Won; sound.Play(NeuroAudio.Cue.Win);
                if (Score > BestScore) { PlayerPrefs.SetInt("NeuroRun.Best", Score); PlayerPrefs.Save(); }
                ui.ShowResult(true, "Você recuperou os seis setores do laboratório.");
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
