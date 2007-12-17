#if NET_2_0
using System.Web.UI;

namespace System.Web.UI
{
	public abstract class BuilderPropertyEntry : PropertyEntry
	{
		public ControlBuilder Builder {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

	}
}
#endif
