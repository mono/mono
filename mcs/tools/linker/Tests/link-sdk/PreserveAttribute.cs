namespace System
{
	// TODO: this is just here to make the code compile but it's not yet integrated into mono linker (use LinkerDescriptor xml for now)
	[AttributeUsage (
		AttributeTargets.Assembly
		| AttributeTargets.Class
		| AttributeTargets.Struct
		| AttributeTargets.Enum
		| AttributeTargets.Constructor
		| AttributeTargets.Method
		| AttributeTargets.Property
		| AttributeTargets.Field
		| AttributeTargets.Event
		| AttributeTargets.Interface
		| AttributeTargets.Delegate,
		AllowMultiple = true)]
	public sealed class PreserveAttribute : Attribute {

		public bool AllMembers;
		public bool Conditional;

		public PreserveAttribute ()
		{
		}

		public PreserveAttribute (Type type)
		{
		}
	}
}