﻿namespace Submodules.EcsLite
{
	public interface IEcsPostDestroySystem : IEcsSystem {
		void PostDestroy (IEcsSystems systems);
	}
}