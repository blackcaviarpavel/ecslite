#define LEOECSLITE_FILTER_EVENTS

using System.Collections.Generic;
using UnityEngine;

namespace Submodules.EcsLite
{
#if LEOECSLITE_FILTER_EVENTS
	public abstract class ReactiveSystem : IEcsPreInitSystem, IEcsRunSystem, IEcsInitializeSystem, IEcsDestroySystem, IEcsFilterEventListener
	{
		private readonly HashSet<EcsFilterMonitor> _listeningFilters = new(4);
		private readonly HashSet<EcsPackedEntity> _triggeredEntities = new(10);
		private readonly HashSet<int> _cachedEntities = new(10);
		private MonitoringType _monitoringType = MonitoringType.Unknown;
		private bool _isActive;

		public void Initialize()
		{
			Activate();
			
			OnInitialize();
		}
		
		public void OnEntityAdded(int entity)
		{
			if (_monitoringType is MonitoringType.Updated or MonitoringType.UpdatedOrRemoved)
			{
				_triggeredEntities.Add(EcsWorld.PackEntity(entity));
			}
		}

		public void OnEntityRemoved(int entity)
		{
			if (_monitoringType is MonitoringType.Removed or MonitoringType.UpdatedOrRemoved)
			{
				_triggeredEntities.Add(EcsWorld.PackEntity(entity));
			}
		}

		public void Run()
		{
			if (_triggeredEntities.Count == 0)
			{
				return;
			}

			_cachedEntities.Clear();
			foreach (var packedEntity in _triggeredEntities)
			{
				if (!packedEntity.Unpack(EcsWorld, out var unpackedEntity))
				{
					continue;
				}
				
				if (_monitoringType == MonitoringType.Updated)
				{
					foreach (var monitor in _listeningFilters)
					{
						if (monitor.Filter.HasEntity(unpackedEntity))
						{
							_cachedEntities.Add(unpackedEntity);
							break;
						}
					}
						
					continue;
				}
					
				_cachedEntities.Add(unpackedEntity);
			}

			Process(_cachedEntities);
			
			_triggeredEntities.Clear();
		}

		public void Destroy()
		{
			Deactivate();
			
			OnDestroy();
		}

		protected void ForceProcess()
		{
			_cachedEntities.Clear();
			foreach (var monitor in _listeningFilters)
			{
				foreach (var entity in monitor.Filter)
				{
					_cachedEntities.Add(entity);
				}
			}
			Process(_cachedEntities);
		}

		protected virtual void OnInitialize() { }
		
		protected virtual void OnDestroy() { }

		protected abstract IEnumerable<EcsFilterMonitor> Subscribe();

		protected abstract void Process(IEnumerable<int> entities);

		private void UpdateMonitoringType(EcsFilterMonitor filterMonitor)
		{
			if (_monitoringType == MonitoringType.Unknown)
			{
				_monitoringType = filterMonitor.MonitoringType;
			}
			else if (_monitoringType != filterMonitor.MonitoringType)
			{
				_monitoringType = MonitoringType.UpdatedOrRemoved;
			}
		}
		
		private void Activate()
		{
			if (_isActive)
			{
				return;
			}

			_isActive = true;
			
			_listeningFilters.Clear();
			foreach (var monitor in Subscribe())
			{
				if (monitor.Filter == null)
				{
					Debug.LogError($"Filter in reactive system {GetType()} cannot be null");
					continue;
				}
				_listeningFilters.Add(monitor);
				
				UpdateMonitoringType(monitor);
				
				monitor.Filter.AddEventListener(this);
			}
		}
		
		private void Deactivate()
		{
			if (!_isActive)
			{
				return;
			}

			_isActive = false;
			_triggeredEntities.Clear();
			_listeningFilters.Clear();
			_monitoringType = MonitoringType.Unknown;
			
			foreach (var monitor in _listeningFilters)
			{
				monitor.Filter.RemoveEventListener(this);
			}
		}
	}
#endif
}