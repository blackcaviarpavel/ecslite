// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Collections.Generic;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Submodules.EcsLite {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public class EcsSystems : IEcsSystems {
        readonly EcsWorld _defaultWorld;
        readonly Dictionary<string, EcsWorld> _worlds;
        readonly List<IEcsSystem> _allSystems;
        readonly List<IEcsRunSystem> _runSystems;
        readonly List<IEcsLateRunSystem> _lateRunSystems;
        
        private bool _inited;

        public EcsSystems (EcsWorld defaultWorld, object shared = null) {
            _defaultWorld = defaultWorld;
            _worlds = new Dictionary<string, EcsWorld> (8);
            _allSystems = new List<IEcsSystem> (128);
            _runSystems = new List<IEcsRunSystem> (128);
            _lateRunSystems = new List<IEcsLateRunSystem> (128);
        }

        public virtual IEcsSystems AddWorld (EcsWorld world, string name) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (_inited) { throw new System.Exception ("Cant add world after initialization."); }
            if (world == null) { throw new System.Exception ("World cant be null."); }
            if (string.IsNullOrEmpty (name)) { throw new System.Exception ("World name cant be null or empty."); }
            if (_worlds.ContainsKey (name)) { throw new System.Exception ($"World with name \"{name}\" already added."); }
#endif
            _worlds[name] = world;
            return this;
        }

        public virtual EcsWorld GetWorld (string name = null) {
            if (name == null) {
                return _defaultWorld;
            }
            _worlds.TryGetValue (name, out var world);
            return world;
        }

        public virtual IReadOnlyDictionary<string, EcsWorld> GetAllNamedWorlds () {
            return _worlds;
        }

        public virtual IEcsSystems Add (IEcsSystem system) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (_inited) { throw new System.Exception ("Cant add system after initialization."); }
#endif
            _allSystems.Add (system);
            if (system is IEcsRunSystem runSystem) {
                _runSystems.Add (runSystem);
            }
            if (system is IEcsLateRunSystem lateRunSystem) {
                _lateRunSystems.Add (lateRunSystem);
            }
            return this;
        }

        public virtual IReadOnlyList<IEcsSystem> GetAllSystems () {
            return _allSystems;
        }

        public bool IsActive() {
            return _inited;
        }

        public virtual void Init () {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (_inited) { throw new System.Exception ("Already initialized."); }
#endif
            foreach (var system in _allSystems) {
                if (system is IEcsPreInitSystem initSystem) {
                    initSystem.PreInit (this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities (this);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType ().Name}.PreInit()."); }
#endif
                }
            }
            foreach (var system in _allSystems) {
                if (system is IEcsInitializeSystem initSystem) {
                    initSystem.Initialize ();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities (this);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType ().Name}.Init()."); }
#endif
                }
            }
            
            _inited = true;
        }

        public virtual void Run () {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_inited) { throw new System.Exception ("Cant run without initialization."); }
#endif
            for (int i = 0, iMax = _runSystems.Count; i < iMax; i++) {
                _runSystems[i].Run ();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                var worldName = CheckForLeakedEntities (this);
                if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {_runSystems[i].GetType ().Name}.Run()."); }
#endif
            }
        }

        public virtual void LateRun () {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_inited) { throw new System.Exception ("Cant run without initialization."); }
#endif
            for (int i = 0, iMax = _lateRunSystems.Count; i < iMax; i++) {
                _lateRunSystems[i].LateRun();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                var worldName = CheckForLeakedEntities (this);
                if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {_lateRunSystems[i].GetType ().Name}.LateRun()."); }
#endif
            }
        }

        public virtual void Destroy () {
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                if (_allSystems[i] is IEcsDestroySystem destroySystem) {
                    destroySystem.Destroy ();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities (this);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {destroySystem.GetType ().Name}.Destroy()."); }
#endif
                }
            }
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                if (_allSystems[i] is IEcsPostDestroySystem postDestroySystem) {
                    postDestroySystem.PostDestroy ();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities (this);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {postDestroySystem.GetType ().Name}.PostDestroy()."); }
#endif
                }
            }
            _worlds.Clear ();
            _allSystems.Clear ();
            _runSystems.Clear ();
            _lateRunSystems.Clear ();
            
            _inited = false;
        }

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
        public static string CheckForLeakedEntities (IEcsSystems systems) {
            if (systems.GetWorld ().CheckForLeakedEntities ()) { return "default"; }
            foreach (var pair in systems.GetAllNamedWorlds ()) {
                if (pair.Value.CheckForLeakedEntities ()) {
                    return pair.Key;
                }
            }
            return null;
        }
#endif
    }
}