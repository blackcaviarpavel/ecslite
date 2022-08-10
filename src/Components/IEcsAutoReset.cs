namespace Submodules.EcsLite
{
	public interface IEcsAutoReset<T> where T : struct {
		void AutoReset (ref T c);
	}
}