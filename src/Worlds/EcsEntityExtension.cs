using System.Runtime.CompilerServices;

namespace Submodules.EcsLite
{
	public static class EcsEntityExtension
	{
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ref ecsWorld.GetPool<T>().Get(entity);
		}
		
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ref T Add<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ref ecsWorld.GetPool<T>().Add(entity);
		}
		
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ref T Change<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ref ecsWorld.GetPool<T>().Change(entity);
		}
		
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void Set<T>(this int entity, EcsWorld ecsWorld, bool added) where T : struct
		{
			ecsWorld.GetPool<T>().Set(entity, added);
		}
		
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			ecsWorld.GetPool<T>().Remove(entity);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ecsWorld.GetPool<T>().Has(entity);
		}
	}
}