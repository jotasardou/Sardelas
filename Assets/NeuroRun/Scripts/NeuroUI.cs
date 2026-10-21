using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace NeuroRun
{
    // UI em Canvas: posições usam uma resolução de referência, adaptada pelo CanvasScaler.
    public class NeuroUI : MonoBehaviour
    {
        static readonly Color Ink = Hex("111923"), Panel = Hex("1C2C3B"), Border = Hex("3F6072");
        static readonly Color White = Hex("E9E0CB"), Muted = Hex("A6B1AF"), Mint = Hex("8DB9B3");
        static readonly Color Gold = Hex("F2C66D"), Pink = Hex("E77860");
        NeuroRunGame game;
        RectTransform canvas, hud, overlay;
        Text score, timer, health, data, elapsed, toast, objective, dash, soundLabel, bitCount, pursuit;
        Image timeFill, dashFill;
        public void Initialize(NeuroRunGame owner)
        {
            game = owner;
            var go = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            // Uma moldura central mantém os mesmos espaços em 960x600 e em fullscreen.
            var frame = new GameObject("Moldura", typeof(RectTransform)); frame.transform.SetParent(go.transform, false);
            canvas = frame.GetComponent<RectTransform>(); canvas.anchorMin = canvas.anchorMax = new Vector2(.5f, .5f);
            canvas.pivot = new Vector2(.5f, .5f); canvas.sizeDelta = new Vector2(1280, 720);
            if (EventSystem.current == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            hud = Group(canvas, "HUD"); overlay = Group(canvas, "Telas");
        }
        static Color Hex(string code) { ColorUtility.TryParseHtmlString("#" + code, out var c); return c; }
        RectTransform Group(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero; return r;
        }
        RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
        }
        Image Box(Transform parent, float x, float y, float w, float h, Color color)
        {
            var r = Rect(parent, "Panel", x, y, w, h); var image = r.gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false; return image;
        }
        Text Label(Transform parent, string value, float x, float y, float w, float h, int size = 20, Color? color = null, bool bold = false)
        {
            var r = Rect(parent, value.Length > 22 ? value.Substring(0, 22) : value, x, y, w, h);
            var text = r.gameObject.AddComponent<Text>(); text.text = value;
            text.font = bold ? game.boldFont : game.regularFont; text.fontSize = size;
            text.color = color ?? White; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
        Button Button(Transform parent, string value, float x, float y, float w, float h, Action action, bool primary = false)
        {
            var image = Box(parent, x, y, w, h, primary ? Mint : Panel); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.80f, 1, .91f); colors.selectedColor = new Color(.68f, .9f, .80f);
            colors.pressedColor = new Color(.52f, .76f, .67f); button.colors = colors;
            Box(image.transform, 0, h - 3, w, 3, primary ? new Color(.32f,.49f,.47f) : Border);
            // Botões pequenos do HUD precisam de menos margem para caber em 960x600.
            bool compact = h < 40;
            float padding = compact ? 8 : 16;
            var text = Label(image.transform, value, padding, 0, w - padding * 2, h, compact ? 16 : 19, primary ? Ink : White, true);
            text.alignment = compact ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            button.onClick.AddListener(() => { game.sound.Play(NeuroAudio.Cue.Click); action(); });
            return button;
        }
        void Focus(Button button) { EventSystem.current.SetSelectedGameObject(button.gameObject); }
        void Clear(RectTransform root)
        { foreach (Transform child in root) { child.gameObject.SetActive(false); Destroy(child.gameObject); } }
        void OpenOverlay(bool opaque = false)
        {
            Clear(overlay); overlay.gameObject.SetActive(true);
            Box(overlay, 0, 0, 1280, 720, opaque ? Ink : new Color(.02f, .05f, .09f, .96f));
        }
        void Brand(Transform root)
        {
            Label(root, "NEURORUN", 38, 23, 245, 35, 26, White, true);
            Label(root, "PLANTÃO NO LABORATÓRIO", 285, 31, 510, 25, 14, Muted);
        }
        public void ShowMenu()
        {
            hud.gameObject.SetActive(false); OpenOverlay(true);
            Box(overlay, 0, 0, 1280, 720, White);
            Box(overlay, 38, 37, 1204, 3, Ink);
            Label(overlay, "J. SARDOU     /     JOGOS DIGITAIS", 40, 52, 650, 26, 15, Ink, true);
            Label(overlay, "1 JOGADOR  /  6 ANDARES", 922, 52, 330, 26, 15, Ink);
            Label(overlay, "PLANTÃO NO LABORATÓRIO", 42, 132, 550, 28, 17, Ink, true);
            Label(overlay, "NEURO\nRUN", 33, 174, 550, 218, 92, Ink, true);
            Box(overlay, 42, 405, 76, 7, Pink);
            Label(overlay, "O laboratório fechou.\nOs bugs ficaram.", 42, 438, 540, 85, 28, Ink, true);
            Label(overlay, "Pegue os disquetes. Despiste os bugs.\nChegue ao terminal para responder o quiz.", 42, 540, 540, 62, 18, Ink);
            var start = Button(overlay, "JOGAR", 42, 628, 240, 54, game.StartMission, true);
            Button(overlay, "COMO JOGAR", 300, 628, 255, 54, () => game.Guide());
            DrawAttractMode(); Focus(start);
        }
        void DrawAttractMode()
        {
            Box(overlay, 607, 112, 635, 570, Ink);
            Label(overlay, "LAB. 01     [ CONEXÃO INSTÁVEL ]", 632, 136, 585, 32, 17, Gold, true);
            var maze = new MazeGrid(0); const float tile = 18;
            float ox = 645, oy = 205;
            for (int y = 0; y < MazeGrid.Height; y++) for (int x = 0; x < MazeGrid.Width; x++)
            {
                var cell = new Vector2Int(x,y);
                if (!maze.Walkable(cell)) Box(overlay, ox+x*tile, oy+y*tile, tile-1, tile-1, Hex("294858"));
                else if ((x+y)%2 == 0) Box(overlay, ox+x*tile+7, oy+y*tile+7, 4, 4, Gold);
            }
            Icon(overlay, game.robot, ox+14*tile, oy+14*tile, 35, Color.white);
            Icon(overlay, game.bug, ox+8*tile, oy+4*tile, 33, Pink);
            Icon(overlay, game.bug, ox+24*tile, oy+10*tile, 33, Mint);
            Icon(overlay, game.diamond, ox, oy, 31, Color.white);
            Label(overlay, "RECORDE  " + game.BestScore.ToString("000000"), 635, 556, 570, 40, 26, Gold, true);
            Label(overlay, "WASD / SETAS    ·    ESPAÇO: DASH", 635, 613, 575, 28, 16, Muted);
            Label(overlay, "João Pedro Sardou  ·  Insper, 2026", 635, 648, 580, 22, 12, Muted);
        }
        void Icon(Transform parent, Sprite sprite, float x, float y, float size, Color color)
        { var image = Box(parent, x, y, size, size, color); image.sprite = sprite; image.preserveAspect = true; }
        public void ShowHUD()
        {
            Clear(hud); hud.gameObject.SetActive(true); overlay.gameObject.SetActive(false);
            Box(hud, 0, 0, 1280, 94, Ink); Box(hud, 0, 638, 1280, 82, Ink);
            Box(hud, 0, 94, 32, 544, Ink); Box(hud, 992, 94, 288, 544, Ink);
            Label(hud, "NEURORUN", 35, 20, 220, 35, 25, White, true);
            Label(hud, "LAB. " + (game.Sector+1).ToString("00") + " / " + game.Lesson.title, 35, 60, 480, 27, 15, Muted);
            Label(hud, "SCORE", 492, 18, 150, 22, 12, Muted);
            score = Label(hud, "", 492, 43, 170, 37, 27, Gold, true);
            health = Label(hud, "", 692, 43, 170, 37, 24, Pink, true);
            Label(hud, "VIDAS", 692, 18, 130, 22, 12, Muted);
            timer = Label(hud, "", 867, 40, 130, 40, 30, White, true);
            Label(hud, "TEMPO", 867, 18, 130, 22, 12, Muted);
            var pause = Button(hud, "PAUSA", 1030, 26, 105, 37, game.Pause);
            pause.navigation = new Navigation { mode = Navigation.Mode.None };
            var mute = Button(hud, "SOM", 1145, 26, 100, 37, game.ToggleSound);
            mute.navigation = new Navigation { mode = Navigation.Mode.None }; soundLabel = mute.GetComponentInChildren<Text>(); RefreshSound();
            Label(hud, "PARA ABRIR O TERMINAL", 1020, 112, 230, 50, 16, White, true);
            Box(hud, 1020, 176, 226, 2, Border);
            Icon(hud, game.diamond, 1020, 201, 35, Color.white);
            data = Label(hud, "", 1070, 204, 190, 35, 25, Gold, true);
            Label(hud, "DISQUETES", 1020, 253, 226, 26, 13, Muted);
            bitCount = Label(hud, "", 1020, 304, 226, 38, 25, Gold, true);
            Label(hud, "PONTOS DE REDE", 1020, 351, 226, 26, 13, Muted);
            Box(hud, 1020, 399, 226, 2, Border);
            objective = Label(hud, "", 1020, 426, 226, 90, 18, White, true);
            pursuit = Label(hud, "", 1020, 544, 226, 62, 18, Pink, true);
            timeFill = Box(hud, 32, 638, 960, 3, Gold);
            toast = Label(hud, "", 35, 650, 960, 29, 16, Gold);
            dash = Label(hud, "", 35, 687, 450, 25, 14, Muted);
            dashFill = Box(hud, 500, 695, 150, 5, Mint);
            elapsed = Label(hud, "", 1020, 657, 230, 34, 14, Muted);
            EventSystem.current.SetSelectedGameObject(null); UpdateHUD();
        }
        public void UpdateHUD()
        {
            if (!hud.gameObject.activeSelf || score == null) return;
            score.text = game.Score.ToString("000000"); timer.text = NeuroRunGame.TimeText(game.Remaining);
            timer.color = game.Remaining <= 25 ? Pink : White;
            health.text = new string('♥', Mathf.Max(0, game.Health));
            data.text = game.Data + " / 6";
            bitCount.text = Mathf.Min(game.Bits, game.BitsNeeded) + " / " + game.BitsNeeded;
            objective.text = game.TerminalReady ? "LIBERADO!\nTerminal no centro." : "Faltam itens.\nContinue a coleta.";
            pursuit.text = game.CoffeeTime > 0 ? "CAFÉ  " + game.CoffeeTime.ToString("0.0") + "s\nBugs parados." : game.Alarm ? "! ALARME\nEles aceleraram." : "Antecipe a curva.\nNão fique cercado.";
            pursuit.color = game.CoffeeTime > 0 ? Mint : Pink;
            elapsed.text = "PARTIDA " + NeuroRunGame.TimeText(game.Elapsed);
            toast.text = game.Toast;
            timeFill.rectTransform.sizeDelta = new Vector2(960 * Mathf.Clamp01(game.Remaining / (game.TimeLimit+12)), 3);
            float ready = game.Player == null ? 1 : game.Player.DashReady;
            dash.text = ready >= 1 ? "ESPAÇO / SHIFT / B: DASH PRONTO" : "DASH: " + ((1-ready)*4).ToString("0.0") + "s";
            dashFill.rectTransform.sizeDelta = new Vector2(150 * ready, 5);
        }
        public void RefreshSound() { if (soundLabel != null) soundLabel.text = game.sound.Muted ? "MUDO" : "SOM"; }
        void Modal(string eyebrow, string title)
        {
            OpenOverlay(); Box(overlay, 170, 55, 940, 610, Panel);
            Box(overlay, 170, 55, 6, 610, Mint);
            Label(overlay, eyebrow, 210, 85, 790, 26, 15, Mint, true);
            Label(overlay, title, 210, 126, 850, 70, 34, White, true);
        }
        public void ShowQuestion(int number)
        {
            Modal("SETOR " + (game.Sector + 1) + "  /  " + game.Lesson.subtitle + "  /  DESAFIO " + number + "/3", "Terminal de avaliação");
            Label(overlay, game.CurrentQuestion.prompt, 210, 211, 850, 100, 24, White, true);
            Button first = null;
            for (int i = 0; i < 3; i++)
            {
                int choice = i;
                var b = Button(overlay, (i + 1) + "   " + game.CurrentQuestion.answers[game.OptionMap[i]], 210, 330 + i * 81, 850, 68, () => game.Answer(choice));
                if (i == 0) first = b;
            }
            Label(overlay, "1 / 2 / 3 ou clique  ·  Controle: direcional + A  ·  Tempo pausado para pensar", 210, 603, 860, 30, 15, Muted);
            Focus(first);
        }
        public void ShowFeedback(bool correct, Question question)
        {
            Modal(correct ? "RESPOSTA CERTA" : "RESPOSTA ERRADA", correct ? "Isso mesmo." : "Não foi dessa vez.");
            Label(overlay, correct ? "Resposta registrada. Próxima pergunta." : "−8 segundos no setor · leia e tente novamente", 210, 211, 850, 40, 21, correct ? Mint : Gold, true);
            Label(overlay, "RESPOSTA: " + question.answers[question.correct], 210, 275, 850, 90, 24, White, true);
            Label(overlay, question.explanation, 210, 377, 850, 150, 23, Muted);
            var b = Button(overlay, correct ? "CONTINUAR  →" : "TENTAR DE NOVO  →", 210, 555, 400, 62, game.ContinueFeedback, true);
            Label(overlay, "A leitura não consome tempo.", 650, 573, 390, 35, 17, Muted); Focus(b);
        }
        public void ShowSectorComplete()
        {
            Modal("ACESSO AUTORIZADO", game.Lesson.title + " ✓");
            Label(overlay, "Três respostas certas. O tempo que sobrou vale pontos.", 210, 223, 850, 75, 25, Mint, true);
            Label(overlay, "PRÓXIMO SETOR", 210, 332, 850, 28, 15, Muted);
            Label(overlay, LessonData.All[game.Sector + 1].title, 210, 380, 850, 70, 34, White, true);
            Label(overlay, "O próximo labirinto tem menos tempo. Vidas e cafés são repostos.", 210, 461, 850, 70, 20, Muted);
            var b = Button(overlay, "AVANÇAR PARA O SETOR " + (game.Sector + 2), 210, 555, 540, 62, game.NextSector, true); Focus(b);
        }
        public void ShowPause()
        {
            Modal("PAUSA", "Respira. Eles esperam.");
            Label(overlay, game.Lesson.briefing, 210, 225, 850, 150, 24, Muted);
            Label(overlay, "WASD / setas / analógico: virar na próxima esquina\nEspaço / Shift / B: dash (recarga de 4s)\nCafé: paralisa os bugs por 4s · Esc / Start: pausa", 210, 397, 850, 110, 19, White);
            var b = Button(overlay, "CONTINUAR", 210, 555, 280, 62, game.Resume, true);
            Button(overlay, "REINICIAR SETOR", 510, 555, 300, 62, game.RetrySector);
            Button(overlay, "MENU", 830, 555, 230, 62, game.Menu); Focus(b);
        }
        public void ShowResult(bool won, string reason)
        {
            Modal(won ? "FIM DO PLANTÃO" : "FIM DE JOGO", won ? "Pode ir para casa." : "Os bugs venceram.");
            Label(overlay, reason, 210, 215, 850, 65, 23, Muted);
            Label(overlay, game.Score.ToString("0000"), 210, 310, 280, 75, 54, Gold, true);
            Label(overlay, "SCORE FINAL", 210, 391, 280, 28, 14, Muted);
            Label(overlay, NeuroRunGame.TimeText(game.Elapsed), 520, 310, 270, 75, 54, Mint, true);
            Label(overlay, "TEMPO DE EXPLORAÇÃO", 520, 391, 300, 28, 14, Muted);
            Label(overlay, game.Solved + " / 18", 820, 310, 270, 75, 44, White, true);
            Label(overlay, "RESPOSTAS CERTAS", 820, 391, 280, 28, 14, Muted);
            Label(overlay, "Acertos de primeira: " + game.FirstTry + "  ·  Recorde: " + game.BestScore + "\nLeituras e pausas não entram no tempo de exploração.", 210, 457, 850, 65, 18, Muted);
            var b = Button(overlay, won ? "JOGAR NOVAMENTE" : "REINICIAR ESTE SETOR", 210, 555, 450, 62, won ? (Action)game.StartMission : game.RetrySector, true);
            Button(overlay, "VOLTAR AO MENU", 690, 555, 370, 62, game.Menu); Focus(b);
        }
        public void ShowGuide(int page)
        {
            OpenOverlay(true); Brand(overlay);
            Label(overlay, "COMO JOGAR", 65, 112, 600, 60, 42, White, true);
            Label(overlay, "ANTES DE SAIR CORRENDO", 65, 202, 480, 30, 16, Mint, true);
            Label(overlay, "1. Pegue os 6 disquetes e a cota de pontos.\n2. Use as esquinas para despistar os bugs.\n3. Volte ao terminal no centro.\n4. Responda 3 perguntas para passar.", 65, 251, 490, 180, 20, White);
            Label(overlay, "O personagem anda sozinho após a saída.\nWASD / setas / analógico: escolha a curva.\nEspaço / Shift / B: dash.\nCafé: 4s sem perigo. Há só dois por fase.\nEsc / Start: pausa. Quiz: 1, 2, 3 ou clique.", 65, 458, 540, 150, 18, Muted);
            Box(overlay, 640, 112, 575, 490, Panel);
            var lesson = LessonData.All[page];
            Label(overlay, "CADERNO " + (page + 1).ToString("00") + " / 06", 675, 148, 490, 30, 15, Mint, true);
            Label(overlay, lesson.title, 675, 203, 490, 90, 32, White, true);
            Label(overlay, lesson.briefing, 675, 317, 490, 210, 20, Muted);
            Button(overlay, "PRÓXIMA PÁGINA  →", 675, 532, 490, 50, game.NextGuide);
            var b = Button(overlay, "VOLTAR AO MENU", 65, 635, 300, 56, game.Menu, true);
            Label(overlay, "Café: bugs parados por 4s. Colisão: −1 vida e −6s. Erro no quiz: −8s.", 400, 649, 820, 44, 17, Muted);
            Focus(b);
        }
    }
}
