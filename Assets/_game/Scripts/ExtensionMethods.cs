using UnityEngine;

namespace _game.Scripts
{
    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2) { return (value - from1) / (to1 - from1) * (to2 - from2) + from2; }

        public static float RemapClamped(this float value, float from1, float to1, float from2, float to2)
        {
            value = Mathf.Clamp(value, from1, to1);
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
