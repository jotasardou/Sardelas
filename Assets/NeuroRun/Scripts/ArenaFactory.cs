using UnityEngine;

namespace NeuroRun
{
    public static class ArenaFactory
    {
        public static readonly Color Floor = new Color32(17, 25, 35, 255);
        public static readonly Color WallColor = new Color32(47, 86, 112, 255);
        public static GameObject Build(NeuroRunGame game, int sector, out RunnerController player, out Transform terminal)
        {
            var root = new GameObject("Mundo"); var maze = game.Maze;
            Sprite(game, root.transform, "Piso", Vector2.zero, new Vector2(18.7f, 10.3f), game.square, Floor, -10);
            for (int y = 0; y < MazeGrid.Height; y++) for (int x = 0; x < MazeGrid.Width; x++)
            {
                var cell = new Vector2Int(x, y); if (maze.Walkable(cell)) continue;
                var wall = Sprite(game, root.transform, "wall", maze.Position(cell), Vector2.one * MazeGrid.Tile, game.square, new Color32(23, 39, 53, 255), -5);
                wall.layer = 8; wall.AddComponent<BoxCollider2D>();
                // Contorno só na borda do corredor: blocos contínuos, sem uma grade de quadradinhos.
                foreach (var d in MazeGrid.Directions)
                {
                    if (!maze.Walkable(cell + d)) continue;
                    var pos = maze.Position(cell) + new Vector2(d.x, -d.y) * .28f;
                    Sprite(game, root.transform, "Contorno", pos, d.x == 0 ? new Vector2(.6f, .045f) : new Vector2(.045f, .6f), game.square, WallColor, -4);
                }
            }
            terminal = Sprite(game, root.transform, "Terminal", maze.Position(maze.Terminal), Vector2.one * .64f, game.ring, Color.gray, 2).transform;
            Vector2Int[] disks = { new Vector2Int(1,1), new Vector2Int(29,1), new Vector2Int(1,8), new Vector2Int(29,8), new Vector2Int(5,15), new Vector2Int(25,15) };
            Vector2Int[] coffee = { new Vector2Int(9,8), new Vector2Int(21,8) };
            int bits = 0;
            foreach (var cell in maze.cells)
            {
                if (cell == maze.Spawn || cell == maze.Terminal) continue;
                bool disk = System.Array.IndexOf(disks, cell) >= 0, cup = System.Array.IndexOf(coffee, cell) >= 0;
                var go = Object.Instantiate(game.dataPrefab, maze.Position(cell), Quaternion.identity, root.transform);
                var pickup = go.GetComponent<DataPickup>(); pickup.game = game;
                pickup.kind = disk ? DataPickup.Kind.Disk : cup ? DataPickup.Kind.Coffee : DataPickup.Kind.Bit;
                go.name = disk ? "Disquete" : cup ? "Cafe" : "Ponto";
                go.transform.localScale = Vector3.one * (disk ? .44f : cup ? .48f : .095f);
                var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = disk ? game.diamond : cup ? game.coffee : game.square;
                sr.color = disk || cup ? Color.white : new Color32(208, 190, 141, 255);
                go.GetComponent<CircleCollider2D>().radius = disk || cup ? .5f : 1.6f;
                if (!disk && !cup) bits++;
            }
            game.SetBitTarget(Mathf.CeilToInt(bits * .65f));
            var runner = Object.Instantiate(game.playerPrefab, maze.Position(maze.Spawn), Quaternion.identity, root.transform);
            runner.name = "Player"; player = runner.GetComponent<RunnerController>(); player.game = game; player.ResetPosition(maze.Position(maze.Spawn));
            Vector2Int[] spawns = { new Vector2Int(15,1), new Vector2Int(1,5), new Vector2Int(29,11), new Vector2Int(9,1), new Vector2Int(21,15) };
            for (int i = 0; i < 3 + sector / 2; i++)
            {
                var enemy = Object.Instantiate(game.bugPrefab, maze.Position(spawns[i]), Quaternion.identity, root.transform);
                enemy.name = "Bug_" + (i + 1); var ai = enemy.GetComponent<BugEnemy>(); ai.game = game;
                ai.identity = i; ai.spawn = spawns[i]; ai.Initialize();
            }
            return root;
        }
        public static GameObject Sprite(NeuroRunGame game, Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = color;
            sr.sharedMaterial = game.spriteMaterial; sr.sortingOrder = order; return go;
        }
    }
}
