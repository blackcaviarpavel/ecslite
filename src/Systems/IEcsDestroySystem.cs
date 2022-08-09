namespace Modules.EcsLite
{
	public interface IEcsDestroySystem : IEcsSystem {
		void Destroy (IEcsSystems systems);
	}
}