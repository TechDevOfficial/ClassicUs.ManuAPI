using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public static class SpatialAudio
    {
        public const float DefaultMaxDistance = 8f;
        public const float DefaultMinDistance = 1f;

        public static void PlayAt(AudioClip clip, Vector2 position, float maxDistance = DefaultMaxDistance, float volume = 1f)
        {
            if (clip == null) return;

            var go = new GameObject("ManuAPISpatialAudio");
            go.transform.position = new Vector3(position.x, position.y, 0f);

            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = DefaultMinDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 0f;
            source.volume = volume;
            source.Play();

            Object.Destroy(go, clip.length + 0.1f);
        }

        public static void PlayAttachedTo(AudioClip clip, Transform target, float maxDistance = DefaultMaxDistance, float volume = 1f)
        {
            if (clip == null || target == null) return;

            var go = new GameObject("ManuAPISpatialAudio");
            go.transform.SetParent(target, false);

            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = DefaultMinDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 0f;
            source.volume = volume;
            source.Play();

            Object.Destroy(go, clip.length + 0.1f);
        }

        public static bool IsAudibleFrom(Vector2 listenerPosition, Vector2 sourcePosition, float maxDistance = DefaultMaxDistance) =>
            Vector2.Distance(listenerPosition, sourcePosition) <= maxDistance;
    }
}
