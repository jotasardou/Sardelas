using UnityEngine;
using UnityEngine.InputSystem;
namespace NeuroRun
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RunnerController : MonoBehaviour
    {
        public NeuroRunGame game;
        public float speed = 3.65f;
        public float DashReady => Mathf.Clamp01(1 - cooldown / 4f);
        public bool Invulnerable => protection > 0 || dashTime > 0 || game.CoffeeTime > 0;
        public Vector2Int GridDirection { get; private set; } = Vector2Int.left;
        Rigidbody2D body;
        SpriteRenderer visual;
        Vector2Int target, requested;
        float cooldown, dashTime, protection;
        void Awake() { body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            if (!game.IsPlaying) return;
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime); protection = Mathf.Max(0, protection - Time.deltaTime); dashTime = Mathf.Max(0, dashTime - Time.deltaTime);
            Vector2 input = ReadMovement();
            if (input.sqrMagnitude > .01f)
                requested = Mathf.Abs(input.x) > Mathf.Abs(input.y) ? new Vector2Int(input.x > 0 ? 1 : -1, 0) : new Vector2Int(0, input.y > 0 ? -1 : 1);
            var key = Keyboard.current; var pad = Gamepad.current;
            bool dash = (key != null && (key.spaceKey.wasPressedThisFrame || key.leftShiftKey.wasPressedThisFrame)) || (pad != null && pad.buttonEast.wasPressedThisFrame);
            if (dash && cooldown <= 0) { dashTime = .22f; cooldown = 4; game.sound.Play(NeuroAudio.Cue.Dash); }
            visual.color = protection > 0 && Mathf.Sin(Time.time * 25) > 0 ? new Color(1,1,1,.3f) : Color.white;
            if (GridDirection.x != 0) visual.flipX = GridDirection.x < 0;
        }
        void FixedUpdate()
        {
            body.linearVelocity = Vector2.zero;
            if (!game.IsPlaying) return;
            var maze = game.Maze; var position = maze.Position(target);
            // Reversão imediata e curva antecipada: o comando fica guardado até a próxima esquina.
            if (requested == -GridDirection && Vector2.Distance(body.position, position) > .015f)
            { target -= GridDirection; GridDirection = requested; position = maze.Position(target); }
            if (Vector2.Distance(body.position, position) < .015f)
            {
                body.position = position;
                if (requested != Vector2Int.zero && maze.Walkable(target + requested)) GridDirection = requested;
                if (!maze.Walkable(target + GridDirection)) return;
                target += GridDirection; position = maze.Position(target);
            }
            body.MovePosition(Vector2.MoveTowards(body.position, position, speed * (dashTime > 0 ? 2.15f : 1) * Time.fixedDeltaTime));
        }
        public static Vector2 ReadMovement()
        {
            Vector2 move = Vector2.zero; var k = Keyboard.current;
            if (k != null)
            {
                move.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
                move.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            }
            var pad = Gamepad.current;
            if (pad != null) { if (pad.leftStick.ReadValue().magnitude > .2f) move = pad.leftStick.ReadValue(); if (pad.dpad.ReadValue().sqrMagnitude > 0) move = pad.dpad.ReadValue(); }
            return Vector2.ClampMagnitude(move, 1);
        }
        public void ResetPosition(Vector2 point)
        { target = game.Maze.Nearest(game.Maze.Cell(point)); body.position = game.Maze.Position(target); body.linearVelocity = Vector2.zero; protection = 2; dashTime = 0; requested = Vector2Int.zero; GridDirection = Vector2Int.zero; }
        void OnTriggerEnter2D(Collider2D other) { Touch(other); }
        void OnTriggerStay2D(Collider2D other) { Touch(other); }
        void Touch(Collider2D other)
        {
            if (!game.IsPlaying) return;
            var pickup = other.GetComponent<DataPickup>();
            if (pickup != null) pickup.Collect();
            else if (other.GetComponent<BugEnemy>() != null && !Invulnerable) game.TakeDamage();
        }
    }
}
