using UnityEngine;

namespace NeuroRun
{
    // A arena usa somente sprites próprios e colliders 2D. Cada setor altera os obstáculos.
    public static class ArenaFactory
    {
        static readonly Color floor = new Color(.045f, .09f, .14f);
        static readonly Color line = new Color(.08f, .18f, .24f);

        public static GameObject Build(NeuroRunGame game, int sector, out RunnerController player, out Transform terminal)
        {
            var root = new GameObject("Mundo");
            var ground = Sprite(game, root.transform, "Piso", Vector2.zero, new Vector2(19.4f, 9.8f), game.square, floor, -10);
            for (int x = -9; x <= 9; x++) Sprite(game, root.transform, "Grade", new Vector2(x, 0), new Vector2(.018f, 9.4f), game.square, line, -9);
            for (int y = -4; y <= 4; y++) Sprite(game, root.transform, "Grade", new Vector2(0, y), new Vector2(19, .018f), game.square, line, -9);
            Wall(game, root.transform, new Vector2(0, 4.8f), new Vector2(19.6f, .4f));
            Wall(game, root.transform, new Vector2(0, -4.8f), new Vector2(19.6f, .4f));
            Wall(game, root.transform, new Vector2(-9.6f, 0), new Vector2(.4f, 9.6f));
            Wall(game, root.transform, new Vector2(9.6f, 0), new Vector2(.4f, 9.6f));

            // Nunca fechamos completamente um corredor: sempre existe uma rota até cada dado.
            float obstacleY = sector % 2 == 0 ? 1.1f : -.8f;
            Wall(game, root.transform, new Vector2(-3.5f, obstacleY), new Vector2(.6f, 3.2f));
            Wall(game, root.transform, new Vector2(3.5f, -obstacleY), new Vector2(.6f, 3.2f));
            if (sector >= 2)
            {
                Wall(game, root.transform, new Vector2(-6.5f, -.4f), new Vector2(2.4f, .5f));
                Wall(game, root.transform, new Vector2(6.5f, .4f), new Vector2(2.4f, .5f));
            }
            if (sector >= 4) Wall(game, root.transform, new Vector2(0, 2.7f), new Vector2(2.8f, .45f));

            terminal = Sprite(game, root.transform, "Nucleo", Vector2.zero, Vector2.one * 1.8f, game.ring, new Color(.22f, .46f, .5f), 0).transform;
            Sprite(game, terminal, "Centro", Vector2.zero, Vector2.one * .34f, game.diamond, new Color(.3f, .7f, .7f), 1);
            Vector2[] data = sector % 2 == 0
                ? new[] { new Vector2(-7.5f, 3.3f), new Vector2(7.5f, 3.3f), new Vector2(7.5f, -3.4f) }
                : new[] { new Vector2(-7.5f, 3.3f), new Vector2(7.5f, -3.4f), new Vector2(-7.5f, -3.4f) };
            foreach (Vector2 p in data)
            {
                var go = Object.Instantiate(game.dataPrefab, p, Quaternion.identity, root.transform);
                go.name = "Dado"; go.GetComponent<DataPickup>().game = game;
            }
            var robot = Object.Instantiate(game.playerPrefab, new Vector3(0, -3.6f, 0), Quaternion.identity, root.transform);
            robot.name = "Player"; player = robot.GetComponent<RunnerController>(); player.game = game;

            int count = 2 + sector / 2;
            for (int i = 0; i < count; i++)
            {
                float enemyY = i % 2 == 0 ? 2f : -2f;
                float enemyX = i < 2 ? -6f : 6f;
                var enemy = Object.Instantiate(game.bugPrefab, new Vector3(enemyX, enemyY, 0), Quaternion.identity, root.transform);
                enemy.name = "Bug_" + i; var ai = enemy.GetComponent<BugEnemy>(); ai.game = game;
                ai.pointA = new Vector2(enemyX, -3.5f); ai.pointB = new Vector2(enemyX, 3.5f);
                ai.speed = 1.6f + sector * .18f; ai.canChase = sector >= 1;
            }
            return root;
        }
        static void Wall(NeuroRunGame game, Transform root, Vector2 position, Vector2 size)
        {
            var wall = Sprite(game, root, "wall", position, size, game.square, new Color(.16f, .3f, .37f), 1);
            wall.layer = 8; wall.AddComponent<BoxCollider2D>();
            Sprite(game, wall.transform, "Borda", Vector2.zero, new Vector2(.86f, .86f), game.square, new Color(.09f, .17f, .24f), 2);
        }
        public static GameObject Sprite(NeuroRunGame game, Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = color;
            sr.sharedMaterial = game.spriteMaterial; sr.sortingOrder = order;
            return go;
        }
    }
}
