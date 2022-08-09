namespace Modules.EcsLite
{
#if DEBUG || LEOECSLITE_WORLD_EVENTS
	public interface IEcsWorldEventListener {
		void OnEntityCreated (int entity);
		void OnEntityChanged (int entity);
		void OnEntityDestroyed (int entity);
		void OnFilterCreated (EcsFilter filter);
		void OnWorldResized (int newSize);
		void OnWorldDestroyed (EcsWorld world);
	}
#endif
}