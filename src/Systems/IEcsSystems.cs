using System.Collections.Generic;

namespace Submodules.EcsLite
{
	public interface IEcsSystems {
		IEcsSystems AddWorld (EcsWorld world, string name);
		EcsWorld GetWorld (string name = null);
		IReadOnlyDictionary<string, EcsWorld> GetAllNamedWorlds ();
		IEcsSystems Add (IEcsSystem system);
		IReadOnlyList<IEcsSystem> GetAllSystems ();
		void Init ();
		void Run ();
		void LateRun ();
		void Destroy ();
	}
}