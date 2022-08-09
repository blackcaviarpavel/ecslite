namespace Modules.EcsLite
{
	public interface IEcsFilterEventListener {
		void OnEntityAdded (int entity);
		void OnEntityRemoved (int entity);
	}
}