using System;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public abstract class LoadableAsset<T> where T : UnityEngine.Object
    {
        private T _cached;
        private bool _loaded;

        public T Get()
        {
            if (_loaded && _cached != null) return _cached;
            _loaded = true;
            try
            {
                _cached = Load();
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("LoadableAsset<" + typeof(T).Name + ">.Load failed: " + e);
                _cached = null;
            }
            return _cached;
        }

        public void Invalidate()
        {
            _loaded = false;
            _cached = null;
        }

        protected abstract T Load();
    }

    public class LoadableSprite : LoadableAsset<Sprite>
    {
        private readonly System.Reflection.Assembly _assembly;
        private readonly string _resourceName;
        private readonly float _pixelsPerUnit;
        private readonly bool _resizeToStandardIconSize;

        public LoadableSprite(System.Reflection.Assembly assembly, string resourceName, float pixelsPerUnit = 100f, bool resizeToStandardIconSize = false)
        {
            _assembly = assembly;
            _resourceName = resourceName;
            _pixelsPerUnit = pixelsPerUnit;
            _resizeToStandardIconSize = resizeToStandardIconSize;
        }

        protected override Sprite Load() =>
            AssetUtils.LoadSpriteFromEmbeddedResource(_assembly, _resourceName, _pixelsPerUnit,
                _resizeToStandardIconSize ? AssetUtils.StandardIconSize : null);
    }

    public class LoadableBundleAsset<T> : LoadableAsset<T> where T : UnityEngine.Object
    {
        private readonly string _bundleKey;
        private readonly string _assetName;

        public LoadableBundleAsset(string bundleKey, string assetName)
        {
            _bundleKey = bundleKey;
            _assetName = assetName;
        }

        protected override T Load() => AssetBundleManager.LoadAsset<T>(_bundleKey, _assetName);
    }

    public class LoadableEmbeddedBundleAsset<T> : LoadableAsset<T> where T : UnityEngine.Object
    {
        private readonly System.Reflection.Assembly _assembly;
        private readonly string _resourceName;
        private readonly string _assetName;

        public LoadableEmbeddedBundleAsset(System.Reflection.Assembly assembly, string resourceName, string assetName)
        {
            _assembly = assembly;
            _resourceName = resourceName;
            _assetName = assetName;
        }

        protected override T Load()
        {
            string bundleKey = _assembly.FullName + "::" + _resourceName;
            var bundle = AssetBundleManager.LoadFromEmbeddedResource(_assembly, _resourceName, bundleKey);
            return bundle == null ? null : AssetBundleManager.LoadAsset<T>(bundleKey, _assetName);
        }
    }
}
