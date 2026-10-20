using UnityEngine;

namespace NeuroRun
{
    public class DataPickup : MonoBehaviour
    {
        public NeuroRunGame game;
        bool collected;
        Vector3 originalScale;
        void Start() { originalScale = transform.localScale; }
        void Update()
        {
            if (!game.IsPlaying) return;
            transform.localScale = originalScale * (1 + .09f * Mathf.Sin(Time.time * 4 + transform.position.x));
        }
        public void Collect()
        {
            if (collected) return; // Evita contar o mesmo trigger duas vezes.
            collected = true; game.CollectData(); Destroy(gameObject);
        }
    }
}
