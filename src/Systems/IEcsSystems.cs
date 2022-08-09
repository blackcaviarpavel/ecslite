using System.Collections.Generic;

namespace Modules.EcsLite
{
	public interface IEcsSystems {
		T GetShared<T> () where T : class;
		IEcsSystems AddWorld (EcsWorld world, string name);
		EcsWorld GetWorld (string name = null);
		IReadOnlyDictionary<string, EcsWorld> GetAllNamedWorlds ();
		IEcsSystems Add (IEcsSystem system);
		IReadOnlyList<IEcsSystem> GetAllSystems ();
		void Init ();
		void Run ();
		void Destroy ();
	}
}