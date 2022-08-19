using System;

namespace Submodules.EcsLite
{
	public static class EcsWorldExtensions
	{
		public static int GetSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
		{
			var filter = ecsWorld.Filter<T>().End();
			if (filter.GetEntitiesCount() == 0)
			{
				return EcsWorld.NullEntityIndex;
			}
			if (filter.GetEntitiesCount() == 1)
			{
				return filter.GetEntities()[0];
			}

			throw new Exception($"Cannot get the single entity from type {typeof(T)}. Group contains {filter.GetEntitiesCount()} entities. ");
		}

		public static ref T SetSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
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