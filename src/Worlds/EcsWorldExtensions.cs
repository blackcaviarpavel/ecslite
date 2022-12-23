using System;
using System.Collections.Generic;

namespace Submodules.EcsLite
{
	public static class EcsWorldExtensions
	{
		/// <summary>
		/// Warmup ecs world pools by reflection
		/// </summary>
		/// <param name="ecsWorld">Target ecs world</param>
		/// <param name="types">Types to bind</param>
		public static void WarmupPoolsByComponentTypes(this EcsWorld ecsWorld, IEnumerable<Type> types)
		{
			foreach (var ecsComponentType in types)
			{
				if (!ecsComponentType.IsValueType) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
					throw new Exception("Component can't be not a valueType");
#endif
					continue;
				}
				
				var warmupPoolName = nameof(EcsWorld.WarmupPool);
				var warmupPoolMethod = typeof(EcsWorld).GetMethod(warmupPoolName);
				var warmupPoolGenericMethod = warmupPoolMethod.MakeGenericMethod(ecsComponentType);
				warmupPoolGenericMethod.Invoke(ecsWorld, null);
			}
		}
		
		[Obsolete]
		public static int GetSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
		{
			var filter = ecsWorld.Filter<T>().End();
			return filter.GetSingleEntity();
		}

		[Obsolete]
		public static ref T ChangeSingleEntity<T>(this EcsWorld ecsWorld) where T : struct
		{
			var entity = GetSingleEntity<T>(ecsWorld);
			var pool = ecsWorld.GetPool<T>();
			if (entity == EcsWorld.NullEntityIndex)
			{
				entity = ecsWorld.NewEntity();

				pool.Add(entity);
			}

			return ref pool.Change(entity);
		}

		[Obsolete]
		public static void SetSingleEntity<T>(this EcsWorld ecsWorld, bool added) where T : struct
		{
			var entity = GetSingleEntity<T>(ecsWorld);
			var pool = ecsWorld.GetPool<T>();
			if (entity == EcsWorld.NullEntityIndex && added)
			{
				entity = ecsWorld.NewEntity();

				pool.Add(entity);
			}
			else if (entity != EcsWorld.NullEntityIndex && !added)
			{
				pool.Remove(entity);
			}
		}

		[Obsolete]
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