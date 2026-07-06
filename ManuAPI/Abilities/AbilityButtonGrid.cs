using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public static class AbilityButtonGrid
    {
        private const float BaseDistanceFromEdge = 1.4f;
        private const float Spacing = 0.9f;
        private const float DefaultY = 1f;

        private static int _nextSlot = 1;

        public static Vector3 ReserveNextSlot(float y = DefaultY)
        {
            int slot = _nextSlot++;
            return new Vector3(BaseDistanceFromEdge + slot * Spacing, y, 0f);
        }
    }
}
