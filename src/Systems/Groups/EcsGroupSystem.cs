#if ENABLE_IL2CPP
using System;
using Unity.IL2CPP.CompilerServices;
#endif

namespace Submodules.EcsLite.ExtendedSystems {
    public struct EcsGroupSystemState {
        public string Name;
        public bool State;
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class Extensions {
        public static IEcsSystems DelHere<T> (this IEcsSystems systems, string worldName = null) where T : struct {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (systems.GetWorld (worldName) == null) { throw new System.Exception ($"Requested world \"{(string.IsNullOrEmpty (worldName) ? "[default]" : worldName)}\" not found."); }
#endif
            return systems.Add (new DelHereSystem<T> (systems.GetWorld (worldName)));
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class DelHereSystem<T> : IEcsRunSystem where T : struct {
        readonly EcsFilter _filter;
        readonly EcsPool<T> _pool;

        public DelHereSystem (EcsWorld world) {
            _filter = world.Filter<T> ().End ();
            _pool = world.GetPool<T> ();
        }

        public void Run () {
            foreach (var entity in _filter) {
                _pool.Remove(entity);
            }
        }
    }
    
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public class DestroyEntityHereSystem<T> : IEcsRunSystem where T : struct {
        readonly EcsWorld _world;
        readonly EcsFilter _filter;
        readonly EcsPool<T> _pool;

        public DestroyEntityHereSystem (EcsWorld world) {
            _filter = world.Filter<T> ().End ();
            _pool = world.GetPool<T> ();
        }

        public void Run () {
            foreach (var entity in _filter) {
                _world.DestroyEntity(entity);
            }
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public class EcsGroupSystem :
        IEcsPreInitSystem,
        IEcsInitializeSystem,
        IEcsRunSystem,
        IEcsLateRunSystem,
        IEcsDestroySystem,
        IEcsPostDestroySystem {
        readonly IEcsSystem[] _nestedSystems;
        readonly IEcsRunSystem[] _runSystems;
        readonly IEcsLateRunSystem[] _lateRunSystems;
        readonly int _runSystemsCount;
        readonly int _lateRunSystemsCount;
        readonly string _eventsWorldName;
        readonly string _name;
        readonly IEcsSystems _systems;
        EcsFilter _filter;
        EcsPool<EcsGroupSystemState> _pool;
        bool _state;

        protected IEcsSystem[] GetNestedSystems () {
            return _nestedSystems;
        }

        public EcsGroupSystem (IEcsSystems systems, string name, bool defaultState, string eventsWorldName, params IEcsSystem[] nestedSystems) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (string.IsNullOrEmpty (name)) { throw new System.Exception ("Group name cant be null or empty."); }
            if (nestedSystems == null || nestedSystems.Length == 0) { throw new System.Exception ("Systems list cant be null or empty."); }
#endif
            _systems = systems;
            _name = name;
            _state = defaultState;
            _eventsWorldName = eventsWorldName;
            _nestedSystems = nestedSystems;
            _runSystemsCount = 0;
            _runSystems = new IEcsRunSystem[_nestedSystems.Length];
            _lateRunSystemsCount = 0;
            _lateRunSystems = new IEcsLateRunSystem[_nestedSystems.Length];
            for (var i = 0; i < _nestedSystems.Length; i++) {
                if (_nestedSystems[i] is IEcsRunSystem runSystem) {
                    _runSystems[_runSystemsCount++] = runSystem;
                }
                if (_nestedSystems[i] is IEcsLateRunSystem lateRunSystem) {
                    _lateRunSystems[_lateRunSystemsCount++] = lateRunSystem;
                }
            }
        }

        public void PreInit (IEcsSystems systems) {
            var world = _systems.GetWorld (_eventsWorldName);
            _pool = world.GetPool<EcsGroupSystemState> ();
            _filter = world.Filter<EcsGroupSystemState> ().End ();
            for (var i = 0; i < _nestedSystems.Length; i++) {
                if (_nestedSystems[i] is IEcsPreInitSystem preInitSystem) {
                    preInitSystem.PreInit (systems);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {preInitSystem.GetType ().Name}.PreInit()."); }
#endif
                }
            }
        }

        public void Initialize () {
            for (var i = 0; i < _nestedSystems.Length; i++) {
                if (_nestedSystems[i] is IEcsInitializeSystem initSystem) {
                    initSystem.Initialize();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType ().Name}.Init()."); }
#endif
                }
            }
        }

        public void Run () {
            foreach (var entity in _filter) {
                ref var evt = ref _pool.Get (entity);
                if (evt.Name == _name) {
                    _state = evt.State;
                    _pool.Remove(entity);
                }
            }
            if (_state) {
                for (var i = 0; i < _runSystemsCount; i++) {
                    _runSystems[i].Run();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {_runSystems[i].GetType ().Name}.Run()."); }
#endif
                }
            }
        }

        public void LateRun()
        {
            if (_state) {
                for (var i = 0; i < _lateRunSystemsCount; i++) {
                    _lateRunSystems[i].LateRun();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {_lateRunSystems[i].GetType ().Name}.LateRun()."); }
#endif
                }
            }
        }

        public void Destroy () {
            for (var i = _nestedSystems.Length - 1; i >= 0; i--) {
                if (_nestedSystems[i] is IEcsDestroySystem destroySystem) {
                    destroySystem.Destroy();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {destroySystem.GetType ().Name}.Destroy()."); }
#endif
                }
            }
        }

        public void PostDestroy () {
            for (var i = _nestedSystems.Length - 1; i >= 0; i--) {
                if (_nestedSystems[i] is IEcsPostDestroySystem postDestroySystem) {
                    postDestroySystem.PostDestroy();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {postDestroySystem.GetType ().Name}.PostDestroy()."); }
#endif
                }
            }
        }
    }
}