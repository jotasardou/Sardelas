using UnityEngine;
namespace NeuroRun
{
    public class DataPickup : MonoBehaviour
    {
        public enum Kind { Disk, Bit, Coffee }
        public NeuroRunGame game;
        public Kind kind;
        bool collected;
        public void Collect()
        {
            if (collected || !game.IsPlaying) return;
            collected = true;
            if (kind == Kind.Disk) game.CollectData();
            else if (kind == Kind.Bit) game.CollectBit();
            else game.DrinkCoffee();
            gameObject.SetActive(false); Destroy(gameObject);
        }
    }
}
