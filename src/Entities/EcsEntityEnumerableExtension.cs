using System.Collections.Generic;

namespace Submodules.EcsLite
{
	public static class EcsEntityEnumerableExtension
	{
		public static bool Unpack (this IEnumerable<EcsPackedEntity> packed, EcsWorld world, in List<int> cache)
		{
			cache.Clear();
			foreach (var packedEntity in packed)
			{
				if (packedEntity.Unpack(world, out var unpackedEntity))
				{
					cache.Add(unpackedEntity);
				}
			}

			return cache.Count != 0;
		}
	}
}