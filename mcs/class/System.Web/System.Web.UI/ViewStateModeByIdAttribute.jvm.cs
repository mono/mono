#if NET_2_0
namespace System.Web.UI
{
	[AttributeUsageAttribute (AttributeTargets.Class)]
	public sealed class ViewStateModeByIdAttribute : Attribute
	{
		public ViewStateModeByIdAttribute ()
		{}
	}
}
#endif
