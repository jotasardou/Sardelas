using UnityEngine;
namespace NeuroRun
{
    public class BugEnemy : MonoBehaviour
    {
        public NeuroRunGame game;
        public int identity;
        public Vector2Int spawn;
        Rigidbody2D body;
        SpriteRenderer visual;
        Vector2Int cell, target, previous;
        float release;
        static readonly Color[] Colors = { new Color32(235,101,79,255), new Color32(109,184,182,255), new Color32(221,153,82,255), new Color32(174,142,188,255), new Color32(218,190,133,255) };
        void Awake() { body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); }
        public void Initialize() { cell = target = spawn; previous = new Vector2Int(-1,-1); release = 1 + identity * .65f; }
        public void ResetPatrol() { body.position = game.Maze.Position(spawn); Initialize(); release = 2; }
        void FixedUpdate()
        {
            if (!game.IsPlaying) return;
            visual.color = game.CoffeeTime > 0 ? new Color32(105,105,118,255) : Colors[identity];
            if (release > 0) { release -= Time.fixedDeltaTime; return; }
            if (game.CoffeeTime > 0) return;
            Vector2 destination = game.Maze.Position(target);
            if (Vector2.Distance(body.position, destination) < .015f)
            {
                body.position = destination; cell = target;
                var goal = game.Maze.Cell(game.Player.transform.position);
                // Vermelho segue; azul antecipa; os outros cercam por lados diferentes.
                if (identity == 1) goal += game.Player.GridDirection * 4;
                if (identity >= 2 && Vector2Int.Distance(cell, goal) < 6)
                    goal += (identity % 2 == 0 ? Vector2Int.right : Vector2Int.left) * 5;
                bool scatter = !game.Alarm && game.SectorTime % 14 < 3;
                if (scatter) goal = new Vector2Int(identity % 2 == 0 ? 1 : 29, identity < 2 ? 1 : 15);
                target = game.Maze.NextStep(cell, goal, previous, identity % 4); previous = cell;
                destination = game.Maze.Position(target);
            }
            float speed = 2.20f + game.Sector * .12f + (game.Alarm ? .42f : 0);
            body.MovePosition(Vector2.MoveTowards(body.position, destination, speed * Time.fixedDeltaTime));
        }
    }
}
