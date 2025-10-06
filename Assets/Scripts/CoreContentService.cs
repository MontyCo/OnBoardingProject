using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace com.ksgames.services.content
{
    public enum BundleType
    {
        MAINMENUBUTTONS,
    }

    public class CoreContentService
    {
        private static CoreContentService _instance;
        public static CoreContentService Instance => _instance ??= new CoreContentService();

        public string Version { get; }

        private Dictionary<BundleType, AssetLabelReference> _bundlesMap;
        private Dictionary<string, SpriteAtlas> _cache = new Dictionary<string, SpriteAtlas>();

        // Commented out since GameResourceEnum is missing
        // private Dictionary<GameResourceEnum, Sprite> _gameResourceSprites = new Dictionary<GameResourceEnum, Sprite>();

        private const string TOKENS_PATH = "TOKENS";

        private CoreContentService() { }

        #region TASK_RELATED_FUNCTIONS

        /// <summary>
        /// Load a SpriteAtlas from Addressables.
        /// On success: OnNext(SpriteAtlas) → OnCompleted().
        /// On error: returns null → OnCompleted().
        /// Returns cached value immediately if available.
        /// </summary>
        public Observable<SpriteAtlas> LoadAtlas(string address)
        {
            if (_cache.TryGetValue(address, out var cachedAtlas))
            {
                Debug.Log($"[ContentService] Cache hit: {address}");
                return Observable.Return(cachedAtlas);
            }

            return Observable.Create<SpriteAtlas>(observer =>
            {
                Debug.Log($"[ContentService] Start loading atlas: {address}");
                var handle = Addressables.LoadAssetAsync<SpriteAtlas>(address);
                bool completed = false;

                handle.Completed += op =>
                {
                    completed = true;
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _cache[address] = op.Result;
                        observer.OnNext(op.Result);
                        observer.OnCompleted();
                        Debug.Log($"[ContentService] Successfully loaded atlas: {address}");
                    }
                    else
                    {
                        Debug.Log($"[ContentService] Error loading atlas: {address}");
                        observer.OnNext(null); // R3 workaround
                        observer.OnCompleted();
                    }
                };

                return Disposable.Create(() =>
                {
                    if (!completed && handle.IsValid())
                    {
                        Addressables.Release(handle);
                        Debug.Log($"[ContentService] Handle auto-release (loading in progress): {address}");
                    }
                    else if (completed && handle.IsValid())
                    {
                        Addressables.Release(handle);
                        Debug.Log($"[ContentService] Handle released after completion: {address}");
                    }
                });
            });
        }

        /// <summary>
        /// Explicitly unload a cached atlas.
        /// Returns true if successful, false if atlas was not cached.
        /// </summary>
        public bool UnloadAtlas(string address)
        {
            if (_cache.TryGetValue(address, out var atlas))
            {
                _cache.Remove(address);
                if (atlas != null)
                    Addressables.Release(atlas);

                Debug.Log($"[ContentService] Explicit unload: {address}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Load multiple atlases asynchronously and return a list of loaded SpriteAtlases.
        /// Works with R3.Observable.
        /// </summary>
        public Observable<List<SpriteAtlas>> LoadAtlases(IEnumerable<string> addresses)
        {
            return Observable.Create<List<SpriteAtlas>>(observer =>
            {
                var results = new List<SpriteAtlas>();
                var remaining = new List<string>(addresses);

                foreach (var addr in addresses)
                {
                    LoadAtlas(addr).Subscribe(atlas =>
                    {
                        results.Add(atlas);
                        remaining.Remove(addr);
                        if (remaining.Count == 0)
                        {
                            observer.OnNext(results);
                            observer.OnCompleted();
                        }
                    });
                }

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Returns a cached SpriteAtlas if it exists, otherwise null.
        /// </summary>
        public SpriteAtlas GetAtlas(string address)
        {
            _cache.TryGetValue(address, out var atlas);
            return atlas;
        }

        /// <summary>
        /// Returns true if the atlas is cached (loaded), false otherwise.
        /// </summary>
        public bool IsAtlasLoaded(string address)
        {
            return _cache.ContainsKey(address);
        }

        /// <summary>
        /// Unloads all cached atlases and clears the cache.
        /// </summary>
        public void ClearAllAtlases()
        {
            foreach (var atlas in _cache.Values)
            {
                if (atlas != null)
                    Addressables.Release(atlas);
            }

            _cache.Clear();
            Debug.Log("[ContentService] All cached atlases cleared.");
        }

        #endregion

        public Sprite GetTokenSpriteByType(string entityType, int level)
        {
            if (!_cache.TryGetValue(TOKENS_PATH, out var spriteAtlas))
                return null;

            return spriteAtlas.GetSprite($"{entityType}_{level}");
        }

        // Commented out until GameResourceEnum exists
        // public Sprite GetGameResourceSprite(GameResourceEnum commandGameResourceEnum)
        // {
        //     return _gameResourceSprites.ContainsKey(commandGameResourceEnum) ? _gameResourceSprites[commandGameResourceEnum] : null;
        // }

        private void setBattleBtnSprite(AsyncOperationHandle<SpriteAtlas> obj)
        {
            // Original stub
        }

        private void UnloadSpritesAtlas(AsyncOperationHandle<SpriteAtlas> obj)
        {
            Addressables.Release(obj);
        }

        // Example of old LoadDataByAddress, kept commented
        /*
        public Observable<bool> LoadDataByAddress(string address)
        {
            if (_cache.ContainsKey(address))
            {
                return Observable.Return(true);
            }

            return Observable.Create<bool>(observer =>
            {
                var handle = Addressables.LoadAssetAsync<SpriteAtlas>(address);

                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        _cache[address] = operation.Result;
                        observer.OnNext(true);
                        observer.OnCompleted();
                    }
                    else
                    {
                        observer.OnNext(false);
                    }
                };

                return Disposable.Create(() =>
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                });
            });
        }
        */

        // todo replace to Addressables
        /*
        public Object GetPrefab(string getID)
        {
            var allWaveItems = Resources.LoadAll<WaveItemView>("");
            foreach (var waveItem in allWaveItems)
            {
                if (waveItem.name == getID)
            }
            return Resources.Load(getID);
        }
        */
    }
}
