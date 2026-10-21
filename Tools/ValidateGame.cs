// Integração no Editor via Unity CLI eval_file. Não entra no build.
var game = UnityEngine.Object.FindFirstObjectByType<NeuroRun.NeuroRunGame>();
var report = new System.Collections.Generic.List<string>();
string path = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../Delivery/validation.txt");
System.Action<bool,string> check = (ok,label) => { report.Add((ok ? "PASS " : "FAIL ") + label); System.IO.File.WriteAllLines(path, report); };
System.Collections.IEnumerator Validate()
{
    int best = game.BestScore;
    for (int sector=0; sector<6; sector++)
    {
        var maze = new NeuroRun.MazeGrid(sector);
        var visited = new System.Collections.Generic.HashSet<UnityEngine.Vector2Int>();
        var queue = new System.Collections.Generic.Queue<UnityEngine.Vector2Int>();
        visited.Add(maze.Spawn); queue.Enqueue(maze.Spawn); bool loops=true;
        while(queue.Count>0)
        {
            var cell=queue.Dequeue(); int exits=0;
            foreach(var d in NeuroRun.MazeGrid.Directions)
            { var next=cell+d; if(!maze.Walkable(next)) continue; exits++; if(visited.Add(next)) queue.Enqueue(next); }
            if(exits<2) loops=false;
        }
        check(visited.Count==maze.cells.Count && loops,"Mapa "+(sector+1)+": todas as células acessíveis e sem becos");
    }
    game.StartMission(); yield return new UnityEngine.WaitForSeconds(.15f);
    check(game.Health==3 && game.BitsNeeded==125 && game.Data==0,"Início: 3 vidas, 6 disquetes e cota de 125 pontos");
    check(UnityEngine.Object.FindObjectsByType<NeuroRun.BugEnemy>(UnityEngine.FindObjectsSortMode.None).Length==3,"Três perseguidores desde o primeiro setor");
    var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick=UnityEngine.Vector2.left });
    yield return new UnityEngine.WaitForSeconds(.65f);
    check(game.Player.transform.position.x < -1.8f,"Analógico move o jogador no corredor");
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick=UnityEngine.Vector2.up });
    yield return new UnityEngine.WaitForSeconds(.65f);
    check(game.Player.transform.position.y > -4.0f && UnityEngine.Mathf.Abs(game.Player.transform.position.x+3.6f)<.1f,"Curva antecipada entra na esquina sem prender na parede");
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.B));
    yield return new UnityEngine.WaitForSeconds(.08f);
    check(game.Player.DashReady<.1f,"B aciona dash e inicia recarga de quatro segundos");
    UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);
    game.Pause(); float before=game.Remaining;
    yield return new UnityEngine.WaitForSeconds(.15f);
    check(game.Remaining==before,"Pausa congela tempo");game.Resume();
    game.DrinkCoffee();
    var enemy=UnityEngine.Object.FindFirstObjectByType<NeuroRun.BugEnemy>();var pos=enemy.transform.position;int health=game.Health;
    game.TakeDamage();yield return new UnityEngine.WaitForSeconds(.2f);
    check(UnityEngine.Vector2.Distance(pos,enemy.transform.position)<.01f && game.Health==health,"Café para inimigos e protege de contato");
    typeof(NeuroRun.NeuroRunGame).GetProperty("Remaining").SetValue(game,.01f);
    yield return new UnityEngine.WaitForSeconds(.1f);
    check(game.State==NeuroRun.NeuroRunGame.GameState.Lost,"Tempo zero causa derrota");
    game.RetrySector();yield return new UnityEngine.WaitForSeconds(.1f);
    check(game.Health==3 && game.Data==0 && game.Bits==0,"Reinício restaura vidas e todos os itens");
    game.Player.ResetPosition(game.Maze.Position(game.Maze.Spawn));yield return new UnityEngine.WaitForSeconds(2.1f);
    before=game.Remaining;game.TakeDamage();
    check(game.Health==2 && UnityEngine.Mathf.Abs(before-game.Remaining-6)<.01f,"Dano desconta uma vida e seis segundos");
    game.StartMission();yield return new UnityEngine.WaitForSeconds(.1f);
    for(int sector=0;sector<6;sector++)
    {
        var pickups=UnityEngine.Object.FindObjectsByType<NeuroRun.DataPickup>(UnityEngine.FindObjectsSortMode.None);
        int disks=0,cups=0;
        foreach(var item in pickups) {if(item.kind==NeuroRun.DataPickup.Kind.Disk)disks++;if(item.kind==NeuroRun.DataPickup.Kind.Coffee)cups++;}
        check(disks==6 && cups==2,"Setor "+(sector+1)+": seis disquetes e dois cafés");
        foreach(var item in pickups) if(item.kind==NeuroRun.DataPickup.Kind.Disk)
        {game.Player.ResetPosition(item.transform.position);yield return new UnityEngine.WaitForSeconds(.05f);}
        check(game.Data==6 && !game.TerminalReady && game.Alarm,"Disquetes ativam alarme mas não substituem cota de pontos");
        foreach(var item in pickups) if(item!=null && item.kind==NeuroRun.DataPickup.Kind.Bit)
        {game.Player.ResetPosition(item.transform.position);yield return new UnityEngine.WaitForSeconds(.025f);}
        check(game.Bits>=game.BitsNeeded && game.TerminalReady,"Coleta por trigger libera terminal");
        game.Player.ResetPosition(game.Maze.Position(game.Maze.Terminal));yield return new UnityEngine.WaitForSeconds(.1f);
        check(game.State==NeuroRun.NeuroRunGame.GameState.Quiz,"Terminal abre quiz após exploração");
        if(sector==0)
        {
            before=game.Remaining;yield return new UnityEngine.WaitForSeconds(.15f);check(before==game.Remaining,"Quiz pausa cronômetro");
            game.Answer(System.Array.FindIndex(game.OptionMap,value=>value!=game.CurrentQuestion.correct));
            check(game.State==NeuroRun.NeuroRunGame.GameState.Feedback && UnityEngine.Mathf.Abs(before-game.Remaining-8)<.01f,"Erro explica a resposta e custa oito segundos");
            game.ContinueFeedback();check(game.State==NeuroRun.NeuroRunGame.GameState.Quiz,"É possível corrigir a resposta");
        }
        for(int q=0;q<3;q++){game.Answer(System.Array.FindIndex(game.OptionMap,value=>value==game.CurrentQuestion.correct));game.ContinueFeedback();yield return null;}
        if(sector<5){check(game.State==NeuroRun.NeuroRunGame.GameState.SectorComplete,"Três acertos abrem próximo labirinto");game.NextSector();yield return new UnityEngine.WaitForSeconds(.1f);}
    }
    check(game.State==NeuroRun.NeuroRunGame.GameState.Won && game.Solved==18 && game.FirstTry==17,"Vitória com 18 respostas e contagem de tentativas correta");
    check(game.Elapsed>0 && game.Score>0,"Resultado final tem score e tempo");
    UnityEngine.PlayerPrefs.SetInt("NeuroRun.ArcadeBest",best);UnityEngine.PlayerPrefs.Save();game.Menu();
    check(game.State==NeuroRun.NeuroRunGame.GameState.Menu,"Retorno ao menu após vitória");
    report.Add("COMPLETE");System.IO.File.WriteAllLines(path,report);
}
game.StartCoroutine(Validate());return "Validação arcade iniciada";
