using System;

namespace Submodules.EcsLite
{
	public static class EcsWorldExtensions
	{
		public static int GetSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
		{
			var filter = ecsWorld.Filter<T>().End();
			return filter.GetSingleEntity();
		}

		public static ref T ChangeSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
		{
			var entity = GetSingleEntity<T>(ecsWorld);
			var pool = ecsWorld.GetPool<T>();
			if (entity == EcsWorld.NullEntityIndex)
			{
				entity = ecsWorld.NewEntity();

				pool.Add(entity);
			}

			return ref pool.Get(entity);
		}

		public static void RemoveSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
		{
			var entity = GetSingleEntity<T>(ecsWorld);
			if (entity != EcsWorld.NullEntityIndex)
			{
				ecsWorld.DestroyEntity(entity);
			}
		}
	}
}