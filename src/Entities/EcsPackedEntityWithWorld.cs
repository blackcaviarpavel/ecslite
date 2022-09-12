namespace Submodules.EcsLite
{
	public struct EcsPackedEntityWithWorld {
		internal int Id;
		internal int Gen;
		internal EcsWorld World;
#if DEBUG
		// For using in IDE debugger.
		internal object[] DebugComponentsView {
			get {
				object[] list = null;
				if (World != null && World.IsEntityAliveInternal (Id) && World.GetEntityGen (Id) == Gen) {
					World.GetComponents (Id, ref list);
				}
				return list;
			}
		}
		// For using in IDE debugger.
		internal int DebugComponentsCount {
			get {
				if (World != null && World.IsEntityAliveInternal (Id) && World.GetEntityGen (Id) == Gen) {
					return World.GetComponentsCount (Id);
				}
				return 0;
			}
		}

		// For using in IDE debugger.
		public override string ToString () {
			if (Id == 0 && Gen == 0) { return "Entity-Null"; }
			if (World == null || !World.IsEntityAliveInternal (Id) || World.GetEntityGen (Id) != Gen) { return "Entity-NonAlive"; }
			System.Type[] types = null;
			var count = World.GetComponentTypes (Id, ref types);
			System.Text.StringBuilder sb = null;
			if (count > 0) {
				sb = new System.Text.StringBuilder (512);
				for (var i = 0; i < count; i++) {
					if (sb.Length > 0) { sb.Append (","); }
					sb.Append (types[i].Name);
				}
			}
			return $"Entity-{Id}:{Gen} [{sb}]";
		}
#endif
	}
}