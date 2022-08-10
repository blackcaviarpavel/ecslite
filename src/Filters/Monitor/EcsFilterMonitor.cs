namespace Submodules.EcsLite
{
	public struct EcsFilterMonitor
	{
		public EcsFilter Filter { get; }
		public MonitoringType MonitoringType { get; }
		
		public EcsFilterMonitor(EcsFilter filter, MonitoringType monitoringType)
		{
			Filter = filter;
			MonitoringType = monitoringType;
		}
	}
}