#define LEOECSLITE_FILTER_EVENTS

using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace Submodules.EcsLite
{
#if LEOECSLITE_FILTER_EVENTS
	public abstract class ReactiveSystem : IEcsRunSystem, IEcsInitializeSystem, IEcsDestroySystem, IEcsFilterEventListener
	{
		private readonly HashSet<EcsFilterMonitor> _listeningFilters = new(4);
		private readonly HashSet<int> _entities = new(10);
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
			if (_monitoringType is MonitoringType.Added or MonitoringType.AddedOrRemoved)
			{
				_entities.Add(entity);
			}
		}

		public void OnEntityRemoved(int entity)
		{
			if (_monitoringType is MonitoringType.Removed or MonitoringType.AddedOrRemoved)
			{
				_entities.Add(entity);
			}
		}

		public void Run()
		{
			if (_entities.Count == 0)
			{
				return;
			}

			_cachedEntities.Clear();
			foreach (var monitor in _listeningFilters)
			{
				foreach (var filteredEntity in monitor.Filter)
				{
					_cachedEntities.Add(filteredEntity);
				}
			}

			Process(_cachedEntities);
			
			_entities.Clear();
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
				_monitoringType = MonitoringType.AddedOrRemoved;
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
			_listeningFilters.AddRange(Subscribe());

			foreach (var monitor in _listeningFilters)
			{
				if (monitor.Filter == null)
				{
					Debug.LogError($"Filter in reactive system {GetType()} cannot be null");
					continue;
				}
				
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
			_entities.Clear();
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