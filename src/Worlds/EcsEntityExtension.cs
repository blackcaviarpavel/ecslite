using System.Runtime.CompilerServices;
using System;

namespace Submodules.EcsLite
{
	public static class EcsEntityExtension
	{
		/// <summary>
		/// If you want just get and read component values, use TryRead or Read methods.
		/// If you want get and modify component values, use Add or Change methods.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <typeparam name="T">Component type</typeparam>
		/// <returns>Component from entity</returns>
		[Obsolete("usages of this method with change values may cause unexpected behavior in Reactive Systems")]
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ref ecsWorld.GetPool<T>().Get(entity);
		}
		
		/// <summary>
		/// Get readonly component from entity.
		/// Use Change if you want to change values in component.
		/// Method will throw exception if entity doesn't have component.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <typeparam name="T">Component type</typeparam>
		/// <returns>Readonly component from entity</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ecsWorld.GetPool<T>().Read(entity);
		}
		
		/// <summary>
		/// Try get readonly component from entity.
		/// Changes in component will not be saved.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <param name="component">Return component if target entity has component. Return default if target entity doesn't have component</param>
		/// <typeparam name="T">Component type</typeparam>
		/// <returns>True if target entity returned component, false if it doesn't returned</returns>
		public static bool TryRead<T>(this int entity, EcsWorld ecsWorld, out T component) where T : struct
		{
			return ecsWorld.GetPool<T>().TryRead(entity, out component);
		}
		
		/// <summary>
		/// Add component on entity.
		/// Method will throw exception if entity already has component.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <typeparam name="T">Component type</typeparam>
		/// <returns>Added component from target entity, which can be modified</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ref T Add<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ref ecsWorld.GetPool<T>().Add(entity);
		}
		
		/// <summary>
		/// Change component on entity.
		/// If it doesn't exist - component will be automatically add on target entity. 
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <typeparam name="T">Component type</typeparam>
		/// <returns>Component from target entity, which can be modified</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ref T Change<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ref ecsWorld.GetPool<T>().Change(entity);
		}
		
		/// <summary>
		/// Add or remove target entity component.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <param name="value">Add if true, remove if false</param>
		/// <typeparam name="T">Component type</typeparam>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void Set<T>(this int entity, EcsWorld ecsWorld, bool value) where T : struct
		{
			ecsWorld.GetPool<T>().Set(entity, value);
		}

		/// <summary>
		/// Remove component from target entity.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <typeparam name="T">Component type</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			ecsWorld.GetPool<T>().Remove(entity);
		}
		
		/// <summary>
		/// Check on target entity has component.
		/// </summary>
		/// <param name="entity">Target entity</param>
		/// <param name="ecsWorld">World which contains target entity</param>
		/// <typeparam name="T">Component type</typeparam>
		/// <returns>True if target entity has component, false if it doesn't have</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this int entity, EcsWorld ecsWorld) where T : struct
		{
			return ecsWorld.GetPool<T>().Has(entity);
		}
	}
}