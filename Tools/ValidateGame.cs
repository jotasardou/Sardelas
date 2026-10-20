// Script de integração executado pelo Unity CLI via eval_file, apenas no Editor.
// Não faz parte do build e não oferece atalhos de teste ao jogador.
var game = UnityEngine.Object.FindFirstObjectByType<NeuroRun.NeuroRunGame>();
var report = new System.Collections.Generic.List<string>();
string reportPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../Delivery/validation.txt");
System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(reportPath));
System.Action<bool, string> check = (ok, label) => {
    report.Add((ok ? "PASS " : "FAIL ") + label);
    System.IO.File.WriteAllLines(reportPath, report);
};
System.Collections.IEnumerator Validate()
{
    int previousBest = game.BestScore;
    game.StartMission();
    yield return new UnityEngine.WaitForSeconds(.1f);
    check(game.IsPlaying && game.Health == 3, "Partida começa com 3 vidas");
    var pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
    var initial = game.Player.transform.position;
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = UnityEngine.Vector2.right });
    yield return new UnityEngine.WaitForSeconds(.3f);
    check(game.Player.transform.position.x > initial.x + .5f, "Gamepad virtual move jogador com analógico");
    game.Player.ResetPosition(new UnityEngine.Vector2(8.6f, -3.6f));
    yield return new UnityEngine.WaitForSeconds(.6f);
    check(game.Player.transform.position.x < 9.2f, "Collider impede atravessar parede externa");
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
    yield return null;
    UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);

    game.Pause(); float remaining = game.Remaining;
    yield return new UnityEngine.WaitForSeconds(.2f);
    check(game.Remaining == remaining && game.State == NeuroRun.NeuroRunGame.GameState.Paused, "Pausa congela o cronômetro");
    game.Resume();
    typeof(NeuroRun.NeuroRunGame).GetProperty("Remaining").SetValue(game, .01f);
    yield return new UnityEngine.WaitForSeconds(.1f);
    check(game.State == NeuroRun.NeuroRunGame.GameState.Lost, "Tempo zero leva à derrota");
    game.RetrySector(); yield return new UnityEngine.WaitForSeconds(.1f);
    check(game.IsPlaying && game.Health == 3 && game.Data == 0, "Reiniciar restaura vidas, dados e estado");
    for (int hit = 0; hit < 3; hit++)
    {
        var enemy = UnityEngine.Object.FindFirstObjectByType<NeuroRun.BugEnemy>();
        game.Player.GetComponent<UnityEngine.Rigidbody2D>().position = enemy.transform.position;
        yield return new UnityEngine.WaitForSeconds(.12f);
        check(game.Health == 2 - hit, "Trigger de inimigo aplica dano " + (hit + 1));
        if (hit < 2) yield return new UnityEngine.WaitForSeconds(2.05f);
    }
    check(game.State == NeuroRun.NeuroRunGame.GameState.Lost, "Vida zero leva à derrota");
    game.StartMission(); yield return new UnityEngine.WaitForSeconds(.1f);
    for (int sector = 0; sector < 6; sector++)
    {
        check(game.Sector == sector, "Setor " + (sector + 1) + " carregado");
        var pickups = UnityEngine.Object.FindObjectsByType<NeuroRun.DataPickup>(UnityEngine.FindObjectsSortMode.None);
        check(pickups.Length == 3, "Setor tem 3 coletáveis");
        foreach (var pickup in pickups)
        {
            game.Player.ResetPosition(pickup.transform.position);
            yield return new UnityEngine.WaitForSeconds(.08f);
        }
        check(game.Data == 3, "Coleta por trigger libera núcleo");
        game.Player.ResetPosition(UnityEngine.Vector2.zero);
        yield return new UnityEngine.WaitForSeconds(.1f);
        check(game.State == NeuroRun.NeuroRunGame.GameState.Quiz, "Entrada no núcleo abre desafio");
        if (sector == 0)
        {
            float before = game.Remaining;
            yield return new UnityEngine.WaitForSeconds(.2f);
            check(game.Remaining == before, "Leitura do quiz não consome tempo");
            int wrong = System.Array.FindIndex(game.OptionMap, value => value != game.CurrentQuestion.correct);
            game.Answer(wrong);
            check(game.State == NeuroRun.NeuroRunGame.GameState.Feedback && UnityEngine.Mathf.Abs(game.Remaining - (before - 8)) < .01f, "Erro mostra explicação e desconta 8s");
            game.ContinueFeedback();
            check(game.State == NeuroRun.NeuroRunGame.GameState.Quiz, "Erro permite tentar novamente sem softlock");
        }
        for (int question = 0; question < 3; question++)
        {
            int correct = System.Array.FindIndex(game.OptionMap, value => value == game.CurrentQuestion.correct);
            game.Answer(correct); game.ContinueFeedback(); yield return null;
        }
        if (sector < 5)
        {
            check(game.State == NeuroRun.NeuroRunGame.GameState.SectorComplete, "3 acertos liberam próximo setor");
            game.NextSector(); yield return new UnityEngine.WaitForSeconds(.1f);
        }
    }
    check(game.State == NeuroRun.NeuroRunGame.GameState.Won && game.Solved == 18, "Vitória após seis setores e 18 conceitos");
    check(game.FirstTry == 17, "Contagem distingue acerto de primeira de correção após erro");
    check(game.Score > 0 && game.Elapsed > 0, "Resultado contém score e tempo final");
    game.Menu(); yield return null;
    check(game.State == NeuroRun.NeuroRunGame.GameState.Menu, "É possível voltar ao menu após vencer");
    UnityEngine.PlayerPrefs.SetInt("NeuroRun.Best", previousBest); UnityEngine.PlayerPrefs.Save(); game.Menu();
    report.Add("COMPLETE"); System.IO.File.WriteAllLines(reportPath, report);
}
game.StartCoroutine(Validate());
return "Validação em execução; relatório em Delivery/validation.txt";
