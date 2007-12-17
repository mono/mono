#if NET_2_0
namespace System.Web.UI
{
	[AttributeUsageAttribute (AttributeTargets.Class)]
	public sealed class FileLevelControlBuilderAttribute : Attribute
	{
		public FileLevelControlBuilderAttribute (Type builderType)
		{}

		public static readonly FileLevelControlBuilderAttribute Default;
		
		public Type BuilderType { get { throw new NotImplementedException (); } }
		
		public override bool Equals (Object obj)
		{
			throw new NotImplementedException ();
		}
		public static bool Equals (Object objA, Object objB)
		{
			throw new NotImplementedException ();
		}
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
		public override bool IsDefaultAttribute ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
