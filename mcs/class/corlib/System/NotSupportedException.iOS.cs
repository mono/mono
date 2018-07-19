namespace System {

	public partial class NotSupportedException {

		// Avoid having the linker generate this method for every linked build
		// It also fix #30075 where --linkskip=mscorlib means that method could not be added
		// but still referenced from other assemblies
		internal static Exception LinkedAway ()
		{
			return new NotSupportedException ("Linked Away");
		}
	}
}