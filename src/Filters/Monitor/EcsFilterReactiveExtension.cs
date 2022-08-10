namespace Submodules.EcsLite
{
	public static class EcsFilterReactiveExtension
	{
		public static EcsFilterMonitor OnAdded(this EcsFilter filter)
		{
			return new EcsFilterMonitor(filter, MonitoringType.Added);
		}
		
		public static EcsFilterMonitor OnRemoved(this EcsFilter filter)
		{
			return new EcsFilterMonitor(filter, MonitoringType.Removed);
		}
		
		public static EcsFilterMonitor OnAddedOrRemoved(this EcsFilter filter)
		{
			return new EcsFilterMonitor(filter, MonitoringType.AddedOrRemoved);
		}
	}
}