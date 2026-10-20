using UnityEngine;

namespace NeuroRun
{
    // Dois comportamentos legíveis: patrulha entre pontos e perseguição de curta duração.
    public class BugEnemy : MonoBehaviour
    {
        public NeuroRunGame game;
        public Vector2 pointA, pointB;
        public float speed = 2;
        public bool canChase;
        Rigidbody2D body;
        bool towardB = true;
        float seed;
        void Awake() { body = GetComponent<Rigidbody2D>(); seed = Random.Range(0f, 6f); }
        void FixedUpdate()
        {
            if (!game.IsPlaying) return;
            Vector2 target = towardB ? pointB : pointA;
            // A perseguição alterna com a patrulha, deixando janelas para escapar.
            bool chase = canChase && Mathf.Sin(Time.time * .8f + seed) > 0
                && Vector2.Distance(body.position, game.Player.transform.position) < 5;
            if (chase) target = game.Player.transform.position;
            else if (Vector2.Distance(body.position, target) < .3f) towardB = !towardB;
            Vector2 direction = (target - body.position).normalized;
            float step = speed * Time.fixedDeltaTime;
            // Impede que os inimigos atravessem as paredes da arena.
            if (Physics2D.CircleCast(body.position, .30f, direction, step + .1f, 1 << 8))
            {
                Vector2 tangent = new Vector2(-direction.y, direction.x);
                if (Physics2D.CircleCast(body.position, .30f, tangent, step + .1f, 1 << 8)) tangent = -tangent;
                direction = tangent;
                towardB = !towardB;
            }
            if (!Physics2D.CircleCast(body.position, .30f, direction, step + .02f, 1 << 8))
                body.MovePosition(body.position + direction * step);
        }
    }
}
