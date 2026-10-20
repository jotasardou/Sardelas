using UnityEngine;
using UnityEngine.InputSystem;

namespace NeuroRun
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RunnerController : MonoBehaviour
    {
        public NeuroRunGame game;
        public float speed = 5.7f;
        public float DashReady => Mathf.Clamp01(1f - cooldown / 1.5f);
        public bool Invulnerable => protection > 0 || dashTime > 0;
        Rigidbody2D body;
        SpriteRenderer visual;
        Vector2 input, facing = Vector2.up;
        float cooldown, dashTime, protection;

        void Awake() { body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            if (!game.IsPlaying) { input = Vector2.zero; return; }
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
            protection = Mathf.Max(0, protection - Time.deltaTime);
            dashTime = Mathf.Max(0, dashTime - Time.deltaTime);
            input = ReadMovement();
            if (input.sqrMagnitude > .01f) facing = input.normalized;
            var key = Keyboard.current;
            var pad = Gamepad.current;
            bool dash = (key != null && (key.spaceKey.wasPressedThisFrame || key.leftShiftKey.wasPressedThisFrame))
                || (pad != null && pad.buttonEast.wasPressedThisFrame);
            if (dash && cooldown <= 0)
            { dashTime = .18f; cooldown = 1.5f; game.sound.Play(NeuroAudio.Cue.Dash); }
            // Piscar comunica claramente a proteção depois de receber dano.
            visual.color = protection > 0 && Mathf.Sin(Time.time * 25) > 0 ? new Color(1, 1, 1, .35f) : Color.white;
        }
        void FixedUpdate()
        {
            if (!game.IsPlaying) { body.linearVelocity = Vector2.zero; return; }
            Vector2 velocity = dashTime > 0 ? facing * speed * 2.6f : input * speed;
            body.linearVelocity = velocity;
        }
        public static Vector2 ReadMovement()
        {
            Vector2 move = Vector2.zero;
            var k = Keyboard.current;
            if (k != null)
            {
                move.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
                move.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (stick.magnitude > .15f) move = stick;
                if (pad.dpad.ReadValue().sqrMagnitude > 0) move = pad.dpad.ReadValue();
            }
            return Vector2.ClampMagnitude(move, 1);
        }
        public void ResetPosition(Vector2 point)
        { body.position = point; body.linearVelocity = Vector2.zero; protection = 2f; dashTime = 0; }
        void OnTriggerEnter2D(Collider2D other) { Touch(other); }
        void OnTriggerStay2D(Collider2D other) { Touch(other); }
        void Touch(Collider2D other)
        {
            if (!game.IsPlaying) return;
            var pickup = other.GetComponent<DataPickup>();
            if (pickup != null) { pickup.Collect(); return; }
            if (other.GetComponent<BugEnemy>() != null && !Invulnerable) game.TakeDamage();
        }
    }
}
