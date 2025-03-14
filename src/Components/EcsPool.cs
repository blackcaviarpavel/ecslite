// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Submodules.EcsLite {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class EcsPool<T> : IEcsPool where T : struct {
        readonly Type _type;
        readonly EcsWorld _world;
        readonly int _id;
        readonly AutoResetHandler _autoReset;
        // 1-based index.
        T[] _denseItems;
        int[] _sparseItems;
        int _denseItemsCount;
        int[] _recycledItems;
        int _recycledItemsCount;
#if ENABLE_IL2CPP && !UNITY_EDITOR
        T _autoresetFakeInstance;
#endif

        internal EcsPool (EcsWorld world, int id, int denseCapacity, int sparseCapacity, int recycledCapacity) {
            _type = typeof (T);
            _world = world;
            _id = id;
            _denseItems = new T[denseCapacity + 1];
            _sparseItems = new int[sparseCapacity];
            _denseItemsCount = 1;
            _recycledItems = new int[recycledCapacity];
            _recycledItemsCount = 0;
            var isAutoReset = typeof (IEcsAutoReset<T>).IsAssignableFrom (_type);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!isAutoReset && _type.GetInterface ("IEcsAutoReset`1") != null) {
                throw new Exception ($"IEcsAutoReset should have <{typeof (T).Name}> constraint for component \"{typeof (T).Name}\".");
            }
#endif
            if (isAutoReset) {
                var autoResetMethod = typeof (T).GetMethod (nameof (IEcsAutoReset<T>.AutoReset));
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (autoResetMethod == null) {
                    throw new Exception (
                        $"IEcsAutoReset<{typeof (T).Name}> explicit implementation not supported, use implicit instead.");
                }
#endif
                _autoReset = (AutoResetHandler) Delegate.CreateDelegate (
                    typeof (AutoResetHandler),
#if ENABLE_IL2CPP && !UNITY_EDITOR
                    _autoresetFakeInstance,
#else
                    null,
#endif
                    autoResetMethod);
            }
        }

#if UNITY_2020_3_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        void ReflectionSupportHack () {
            _world.WarmupPool<T> ();
            _world.GetWarmedPoolInternal<T> ();
            _world.GetPool<T> ();
            _world.Filter<T> ().Exc<T> ().End ();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld () {
            return _world;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetId () {
            return _id;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Type GetComponentType () {
            return _type;
        }

        void IEcsPool.Resize (int capacity) {
            Array.Resize (ref _sparseItems, capacity);
        }

        object IEcsPool.GetRaw (int entity) {
            return Get (entity);
        }

        void IEcsPool.SetRaw (int entity, object dataRaw) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (dataRaw == null || dataRaw.GetType () != _type) { throw new Exception ("Invalid component data, valid \"{typeof (T).Name}\" instance required."); }
            if (_sparseItems[entity] <= 0) { throw new Exception ($"Component \"{typeof (T).Name}\" not attached to entity."); }
#endif
            _denseItems[_sparseItems[entity]] = (T) dataRaw;
        }

        void IEcsPool.AddRaw (int entity, object dataRaw) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (dataRaw == null || dataRaw.GetType () != _type) { throw new Exception ("Invalid component data, valid \"{typeof (T).Name}\" instance required."); }
#endif
            ref var data = ref Add (entity);
            data = (T) dataRaw;
        }

        public T[] GetRawDenseItems () {
            return _denseItems;
        }

        public ref int GetRawDenseItemsCount () {
            return ref _denseItemsCount;
        }

        public int[] GetRawSparseItems () {
            return _sparseItems;
        }

        public int[] GetRawRecycledItems () {
            return _recycledItems;
        }

        public ref int GetRawRecycledItemsCount () {
            return ref _recycledItemsCount;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Set (int entity, bool added)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
            if (_sparseItems[entity] == 0 && added) {
                Add(entity);
            }
            else if (!added) {
                Remove(entity);
            }
        }

        public ref T Add (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
            if (_sparseItems[entity] > 0) { throw new Exception ($"Component \"{typeof (T).Name}\" already attached to entity."); }
#endif
            int idx;
            if (_recycledItemsCount > 0) {
                idx = _recycledItems[--_recycledItemsCount];
            } else {
                idx = _denseItemsCount;
                if (_denseItemsCount == _denseItems.Length) {
                    Array.Resize (ref _denseItems, _denseItemsCount << 1);
                }
                _denseItemsCount++;
                _autoReset?.Invoke (ref _denseItems[idx]);
            }
            _sparseItems[entity] = idx;
            _world.OnEntityChangeInternal (entity, _id, EntityUpdateType.Added);
            _world.Entities[entity].ComponentsCount++;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            _world.RaiseEntityChangeEvent (entity);
#endif
            return ref _denseItems[idx];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Change (int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
            if (_sparseItems[entity] == 0) {
                return ref Add(entity);
            }
            
            _world.OnEntityChangeInternal (entity, _id, EntityUpdateType.Changed);
            return ref Get(entity);
        }

        [Obsolete, MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Get (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
            if (_sparseItems[entity] == 0) { throw new Exception ($"Cant get \"{typeof (T).Name}\" component - not attached."); }
#endif
            return ref _denseItems[_sparseItems[entity]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public T Read (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
            if (_sparseItems[entity] == 0) { throw new Exception ($"Cant get \"{typeof (T).Name}\" component - not attached."); }
#endif
            return _denseItems[_sparseItems[entity]];
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool TryRead (int entity, out T component) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
            if (_sparseItems[entity] > 0) {
                component = Read (entity);
                return true;
            }

            component = default;
            return false;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
            return _sparseItems[entity] > 0;
        }

        public void Remove (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity == EcsWorld.NullEntityIndex) { throw new Exception ("Null reference entity."); }
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
            ref var sparseData = ref _sparseItems[entity];
            if (sparseData > 0) {
                _world.OnEntityChangeInternal (entity, _id, EntityUpdateType.Removed);
                if (_recycledItemsCount == _recycledItems.Length) {
                    Array.Resize (ref _recycledItems, _recycledItemsCount << 1);
                }
                _recycledItems[_recycledItemsCount++] = sparseData;
                if (_autoReset != null) {
                    _autoReset.Invoke (ref _denseItems[sparseData]);
                } else {
                    _denseItems[sparseData] = default;
                }
                sparseData = 0;
                ref var entityData = ref _world.Entities[entity];
                entityData.ComponentsCount--;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                _world.RaiseEntityChangeEvent (entity);
#endif
                if (entityData.ComponentsCount == 0) {
                    _world.DestroyEntity (entity);
                }
            }
        }

        delegate void AutoResetHandler (ref T component);
    }
}