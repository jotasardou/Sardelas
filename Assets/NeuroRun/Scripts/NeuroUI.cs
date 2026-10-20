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
        static readonly Color Ink = Hex("081320"), Panel = Hex("102538"), Border = Hex("24485C");
        static readonly Color White = Hex("E8F3F5"), Muted = Hex("9CB5C5"), Mint = Hex("93F5C0");
        static readonly Color Gold = Hex("FBD16D"), Pink = Hex("FF6F91");
        NeuroRunGame game;
        RectTransform canvas, hud, overlay;
        Text score, timer, health, data, elapsed, toast, objective, dash, soundLabel;
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
            var outline = image.gameObject.AddComponent<Outline>(); outline.effectColor = primary ? Mint : Border; outline.effectDistance = Vector2.one;
            var text = Label(image.transform, value, 16, 0, w - 32, h, 19, primary ? Ink : White, true);
            text.alignment = TextAnchor.MiddleLeft;
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
            Box(root, 40, 30, 12, 12, Mint);
            Label(root, "NEURORUN", 65, 21, 220, 35, 24, White, true);
            Label(root, "LABORATÓRIO DE APRENDIZADO", 290, 29, 390, 25, 14, Muted);
        }
        public void ShowMenu()
        {
            hud.gameObject.SetActive(false); OpenOverlay(true); Brand(overlay);
            for (int x = 720; x < 1240; x += 40) Box(overlay, x, 110, 1, 460, new Color(.08f, .17f, .23f));
            for (int y = 110; y < 590; y += 40) Box(overlay, 720, y, 520, 1, new Color(.08f, .17f, .23f));
            Label(overlay, "MISSÃO 01  /  RECUPERAR O CONHECIMENTO", 64, 114, 630, 30, 15, Mint, true);
            Label(overlay, "NEURO\nRUN", 58, 160, 650, 200, 86, White, true);
            Label(overlay, "Aprenda. Desvie. Evolua.", 65, 375, 600, 48, 29, Mint, true);
            Label(overlay, "Recupere dados, escape dos bugs e use Machine Learning\npara reativar seis setores de um laboratório digital.", 65, 435, 615, 70, 20, Muted);
            var start = Button(overlay, "INICIAR MISSÃO  →", 65, 532, 300, 62, game.StartMission, true);
            Button(overlay, "GUIA DE CAMPO", 385, 532, 260, 62, () => game.Guide());
            Label(overlay, "06 SETORES    /    18 DESAFIOS    /    TECLADO OU CONTROLE", 65, 617, 730, 25, 14, Muted);
            Label(overlay, "João Pedro Sardou  ·  Insper  ·  Projeto Introdução ao Unity", 65, 677, 900, 24, 14, Muted);
            Label(overlay, "RECORDE  " + game.BestScore.ToString("0000"), 1010, 677, 230, 25, 15, Gold, true);
            DrawNetwork(); Focus(start);
        }
        void DrawNetwork()
        {
            Vector2 center = new Vector2(972, 333);
            string[] names = { "DADOS", "HISTÓRIA", "CÓDIGO", "MATEMÁTICA", "MODELOS", "CRITÉRIO" };
            Vector2[] positions = { new Vector2(972, 144), new Vector2(1160, 238), new Vector2(1160, 428), new Vector2(972, 523), new Vector2(784, 428), new Vector2(784, 238) };
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 p = positions[i], delta = p - center;
                var connector = Box(overlay, center.x, center.y, delta.magnitude, 2, Border).rectTransform;
                connector.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                var node = Box(overlay, p.x - 29, p.y - 29, 58, 58, Panel);
                node.gameObject.AddComponent<Outline>().effectColor = Mint;
                var number = Label(overlay, (i + 1).ToString("00"), p.x - 22, p.y - 21, 44, 42, 24, Mint, true);
                number.alignment = TextAnchor.MiddleCenter;
                var name = Label(overlay, names[i], p.x - 72, p.y + 37, 144, 30, 13, Muted); name.alignment = TextAnchor.UpperCenter;
            }
            var ring = Box(overlay, 896, 257, 152, 152, Mint); ring.sprite = game.ring;
            var robot = Box(overlay, 931, 292, 82, 82, White); robot.sprite = game.robot;
            Label(overlay, "AGENTE N-01", 914, 432, 180, 28, 16, Mint, true);
        }
        public void ShowHUD()
        {
            Clear(hud); hud.gameObject.SetActive(true); overlay.gameObject.SetActive(false);
            Box(hud, 0, 0, 1280, 136, Ink); Box(hud, 0, 626, 1280, 94, Ink);
            Box(hud, 0, 136, 32, 490, Ink); Box(hud, 928, 136, 352, 490, Ink);
            Brand(hud);
            var pause = Button(hud, "PAUSA", 1040, 22, 100, 37, game.Pause);
            pause.navigation = new Navigation { mode = Navigation.Mode.None };
            var mute = Button(hud, "SOM", 1152, 22, 90, 37, game.ToggleSound);
            mute.navigation = new Navigation { mode = Navigation.Mode.None }; soundLabel = mute.GetComponentInChildren<Text>(); RefreshSound();
            Label(hud, "SETOR " + (game.Sector + 1).ToString("00") + " / 06", 40, 75, 210, 25, 14, Mint, true);
            Label(hud, game.Lesson.title, 40, 98, 275, 30, 18, White, true);
            Label(hud, "SCORE", 340, 74, 120, 20, 13, Muted);
            score = Label(hud, "", 340, 94, 155, 35, 25, Gold, true);
            Label(hud, "INTEGRIDADE", 525, 74, 165, 20, 13, Muted);
            health = Label(hud, "", 525, 94, 160, 35, 25, Pink, true);
            Label(hud, "TEMPO DO SETOR", 730, 74, 170, 20, 13, Muted);
            timer = Label(hud, "", 730, 94, 155, 35, 25, Mint, true);
            Box(hud, 918, 136, 2, 490, Border);
            Box(hud, 948, 150, 292, 470, Panel);
            Label(hud, "OBJETIVO ATUAL", 972, 174, 245, 30, 14, Mint, true);
            objective = Label(hud, "", 972, 214, 245, 90, 24, White, true);
            data = Label(hud, "", 972, 322, 245, 35, 20, Gold, true);
            Label(hud, "AMOSTRAS → CONHECIMENTO", 972, 366, 245, 25, 12, Muted);
            Label(hud, "Dourado: colete\nVerde: núcleo de perguntas\nRosa: bugs, evite contato", 972, 407, 245, 95, 17, Muted);
            Label(hud, "ERRO NO QUIZ: −8s\nACERTO: libera conhecimento", 972, 528, 245, 65, 15, White);
            Box(hud, 34, 635, 886, 3, Border);
            timeFill = Box(hud, 34, 635, 886, 3, Mint);
            toast = Label(hud, "", 52, 589, 840, 35, 17, Mint, true);
            Label(hud, "WASD / SETAS / ANALÓGICO   mover", 40, 662, 410, 28, 15, Muted);
            dash = Label(hud, "", 470, 662, 420, 28, 15, Mint);
            Box(hud, 470, 694, 210, 4, Border); dashFill = Box(hud, 470, 694, 210, 4, Mint);
            elapsed = Label(hud, "", 964, 660, 280, 30, 16, Muted);
            EventSystem.current.SetSelectedGameObject(null); UpdateHUD();
        }
        public void UpdateHUD()
        {
            if (!hud.gameObject.activeSelf || score == null) return;
            score.text = game.Score.ToString("0000"); timer.text = NeuroRunGame.TimeText(game.Remaining);
            timer.color = game.Remaining < 15 ? Pink : Mint;
            health.text = new string('♥', Mathf.Max(0, game.Health)) + new string('♡', Mathf.Max(0, 3 - game.Health));
            data.text = "DADOS   " + game.Data + " / 3";
            objective.text = game.Data < 3 ? "Recupere os\n3 dados dourados" : "Entre no núcleo\nverde central";
            elapsed.text = "MISSÃO  " + NeuroRunGame.TimeText(game.Elapsed);
            toast.text = game.Toast;
            timeFill.rectTransform.sizeDelta = new Vector2(886 * Mathf.Clamp01(game.Remaining / 79f), 3);
            float ready = game.Player == null ? 1 : game.Player.DashReady;
            dash.text = ready >= 1 ? "ESPAÇO / SHIFT / B   dash pronto" : "DASH   recarregando...";
            dashFill.rectTransform.sizeDelta = new Vector2(210 * ready, 4);
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
            Modal("SETOR " + (game.Sector + 1) + "  /  " + game.Lesson.subtitle + "  /  DESAFIO " + number + "/3", "Valide seu conhecimento");
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
            Modal(correct ? "CONHECIMENTO VALIDADO" : "TENTATIVA FAZ PARTE DO APRENDIZADO", correct ? "Boa! Conexão estabelecida." : "Vamos ajustar o modelo.");
            Label(overlay, correct ? "+ pontos de conhecimento" : "−8 segundos no setor · leia e tente novamente", 210, 211, 850, 40, 21, correct ? Mint : Gold, true);
            Label(overlay, "RESPOSTA: " + question.answers[question.correct], 210, 275, 850, 90, 24, White, true);
            Label(overlay, question.explanation, 210, 377, 850, 150, 23, Muted);
            var b = Button(overlay, correct ? "CONTINUAR  →" : "TENTAR DE NOVO  →", 210, 555, 400, 62, game.ContinueFeedback, true);
            Label(overlay, "A leitura não consome tempo.", 650, 573, 390, 35, 17, Muted); Focus(b);
        }
        public void ShowSectorComplete()
        {
            Modal("SETOR REATIVADO", game.Lesson.title + " ✓");
            Label(overlay, "3 conceitos validados. O tempo restante virou bônus de score.", 210, 223, 850, 75, 25, Mint, true);
            Label(overlay, "PRÓXIMO SETOR", 210, 332, 850, 28, 15, Muted);
            Label(overlay, LessonData.All[game.Sector + 1].title, 210, 380, 850, 70, 34, White, true);
            Label(overlay, "Nova arena, novos bugs. Você começa com 3 vidas e 70 segundos.", 210, 461, 850, 70, 20, Muted);
            var b = Button(overlay, "AVANÇAR PARA O SETOR " + (game.Sector + 2), 210, 555, 540, 62, game.NextSector, true); Focus(b);
        }
        public void ShowPause()
        {
            Modal("MISSÃO PAUSADA", "Um respiro para pensar.");
            Label(overlay, game.Lesson.briefing, 210, 225, 850, 150, 24, Muted);
            Label(overlay, "WASD / setas / analógico: mover\nEspaço / Shift / B: dash com proteção\nEsc / Start: pausar · A: confirmar menus", 210, 397, 850, 110, 19, White);
            var b = Button(overlay, "CONTINUAR", 210, 555, 280, 62, game.Resume, true);
            Button(overlay, "REINICIAR SETOR", 510, 555, 300, 62, game.RetrySector);
            Button(overlay, "MENU", 830, 555, 230, 62, game.Menu); Focus(b);
        }
        public void ShowResult(bool won, string reason)
        {
            Modal(won ? "MISSÃO CONCLUÍDA / LABORATÓRIO ONLINE" : "MISSÃO INTERROMPIDA / VOCÊ PODE TENTAR DE NOVO", won ? "Conhecimento é poder." : "Uma falha não é o fim.");
            Label(overlay, reason, 210, 215, 850, 65, 23, Muted);
            Label(overlay, game.Score.ToString("0000"), 210, 310, 280, 75, 54, Gold, true);
            Label(overlay, "SCORE FINAL", 210, 391, 280, 28, 14, Muted);
            Label(overlay, NeuroRunGame.TimeText(game.Elapsed), 520, 310, 270, 75, 54, Mint, true);
            Label(overlay, "TEMPO DE EXPLORAÇÃO", 520, 391, 300, 28, 14, Muted);
            Label(overlay, game.Solved + " / 18", 820, 310, 270, 75, 44, White, true);
            Label(overlay, "CONCEITOS VALIDADOS", 820, 391, 280, 28, 14, Muted);
            Label(overlay, "Acertos de primeira: " + game.FirstTry + "  ·  Recorde: " + game.BestScore + "\nLeituras e pausas não entram no tempo de exploração.", 210, 457, 850, 65, 18, Muted);
            var b = Button(overlay, won ? "JOGAR NOVAMENTE" : "REINICIAR ESTE SETOR", 210, 555, 450, 62, won ? (Action)game.StartMission : game.RetrySector, true);
            Button(overlay, "VOLTAR AO MENU", 690, 555, 370, 62, game.Menu); Focus(b);
        }
        public void ShowGuide(int page)
        {
            OpenOverlay(true); Brand(overlay);
            Label(overlay, "GUIA DE CAMPO", 65, 112, 600, 60, 42, White, true);
            Label(overlay, "COMO JOGAR", 65, 202, 480, 30, 16, Mint, true);
            Label(overlay, "1. Colete os 3 dados dourados.\n2. Evite os bugs e use o dash.\n3. Entre no núcleo verde central.\n4. Acerte os 3 desafios para avançar.", 65, 251, 490, 180, 23, White);
            Label(overlay, "Mover: WASD, setas ou analógico\nDash: Espaço, Shift ou B (Xbox)\nMenus: mouse, Tab/Enter ou direcional/A\nQuiz: 1, 2, 3 ou navegação de menus\nPausa: Esc ou Start", 65, 458, 540, 150, 18, Muted);
            Box(overlay, 640, 112, 575, 490, Panel);
            var lesson = LessonData.All[page];
            Label(overlay, "CADERNO " + (page + 1).ToString("00") + " / 06", 675, 148, 490, 30, 15, Mint, true);
            Label(overlay, lesson.title, 675, 203, 490, 90, 32, White, true);
            Label(overlay, lesson.briefing, 675, 317, 490, 210, 23, Muted);
            Button(overlay, "PRÓXIMA PÁGINA  →", 675, 532, 490, 50, game.NextGuide);
            var b = Button(overlay, "VOLTAR AO MENU", 65, 635, 300, 56, game.Menu, true);
            Label(overlay, "Dados: +3s  ·  Erro no quiz: −8s  ·  Cada setor começa com 70s e 3 vidas.", 400, 649, 820, 44, 17, Muted);
            Focus(b);
        }
    }
}
