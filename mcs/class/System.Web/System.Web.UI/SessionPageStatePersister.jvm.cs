#if NET_2_0
using System.Web.UI;

namespace System.Web.UI
{
	public class SessionPageStatePersister : PageStatePersister
	{
		public SessionPageStatePersister (Page page):base(page)
		{
			throw new NotImplementedException ();
		}

		public override void Load ()
		{
			throw new NotImplementedException ();
		}

		public override void Save ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
