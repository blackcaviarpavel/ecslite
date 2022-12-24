namespace Submodules.EcsLite
{
	public static class EcsFilterReactiveExtension
	{
		public static EcsFilterMonitor OnUpdated(this EcsFilter filter)
		{
			return new EcsFilterMonitor(filter, MonitoringType.Updated);
		}
		
		public static EcsFilterMonitor OnRemoved(this EcsFilter filter)
		{
			return new EcsFilterMonitor(filter, MonitoringType.Removed);
		}
		
		public static EcsFilterMonitor OnUpdatedOrRemoved(this EcsFilter filter)
		{
			return new EcsFilterMonitor(filter, MonitoringType.UpdatedOrRemoved);
		}
	}
}