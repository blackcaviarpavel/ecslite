#define LEOECSLITE_FILTER_EVENTS

using System.Collections.Generic;

namespace Submodules.EcsLite
{
#if LEOECSLITE_FILTER_EVENTS
	public abstract class ReactiveSystem : IEcsRunSystem, IEcsInitializeSystem, IEcsDestroySystem, IEcsFilterEventListener
	{
		private readonly List<EcsFilterMonitor> _listeningFilters = new(4);
		private readonly List<int> _entities = new();
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

			Process(_entities);
			
			_entities.Clear();
		}

		public void Destroy()
		{
			Deactivate();
			
			OnDestroy();
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
			
			foreach (var monitor in _listeningFilters)
			{
				monitor.Filter.RemoveEventListener(this);
			}
		}
	}
#endif
}