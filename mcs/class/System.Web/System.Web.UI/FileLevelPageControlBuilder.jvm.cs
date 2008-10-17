#if NET_2_0
using System.Web.UI;

namespace System.Web.UI
{
	public class FileLevelPageControlBuilder : RootBuilder
	{
		public FileLevelPageControlBuilder ()
		{
			throw new NotImplementedException ();
		}
		public override void AppendLiteralString (string text)
		{
			throw new NotImplementedException ();
		}
		public override void AppendSubBuilder (ControlBuilder subBuilder)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif

