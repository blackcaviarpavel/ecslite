#define ENABLE_IL2CPP

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
#if !LEOECSLITE_DI
        public static IEcsSystems AddGroup (this IEcsSystems systems, string groupName, bool defaultState, string eventWorldName, params IEcsSystem[] nestedSystems) {
            return systems.Add (new EcsGroupSystem (systems, groupName, defaultState, eventWorldName, nestedSystems));
        }
#endif
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
    public sealed class DelHereSystem<T> : IEcsExecuteSystem where T : struct {
        readonly EcsFilter _filter;
        readonly EcsPool<T> _pool;

        public DelHereSystem (EcsWorld world) {
            _filter = world.Filter<T> ().End ();
            _pool = world.GetPool<T> ();
        }

        public void Execute () {
            foreach (var entity in _filter) {
                _pool.Remove(entity);
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
        IEcsExecuteSystem,
        IEcsDestroySystem,
        IEcsPostDestroySystem {
        readonly IEcsSystem[] _nestedSystems;
        readonly IEcsExecuteSystem[] _runSystems;
        readonly int _runSystemsCount;
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
            _runSystems = new IEcsExecuteSystem[_nestedSystems.Length];
            for (var i = 0; i < _nestedSystems.Length; i++) {
                if (_nestedSystems[i] is IEcsExecuteSystem runSystem) {
                    _runSystems[_runSystemsCount++] = runSystem;
                }
            }
        }

        public void PreInit () {
            var world = _systems.GetWorld (_eventsWorldName);
            _pool = world.GetPool<EcsGroupSystemState> ();
            _filter = world.Filter<EcsGroupSystemState> ().End ();
            for (var i = 0; i < _nestedSystems.Length; i++) {
                if (_nestedSystems[i] is IEcsPreInitSystem preInitSystem) {
                    preInitSystem.PreInit ();
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

        public void Execute () {
            foreach (var entity in _filter) {
                ref var evt = ref _pool.Get (entity);
                if (evt.Name == _name) {
                    _state = evt.State;
                    _pool.Remove(entity);
                }
            }
            if (_state) {
                for (var i = 0; i < _runSystemsCount; i++) {
                    _runSystems[i].Execute();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = EcsSystems.CheckForLeakedEntities (_systems);
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {_runSystems[i].GetType ().Name}.Run()."); }
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

#if ENABLE_IL2CPP
// Unity IL2CPP performance optimization attribute.
namespace Unity.IL2CPP.CompilerServices {
    enum Option {
        NullChecks = 1,
        ArrayBoundsChecks = 2
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; private set; }
        public object Value { get; private set; }

        public Il2CppSetOptionAttribute (Option option, object value) { Option = option; Value = value; }
    }
}
#endif