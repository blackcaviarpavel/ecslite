namespace Modules.EcsLite
{
	public interface IEcsRunSystem : IEcsSystem {
		void Run (IEcsSystems systems);
	}
}