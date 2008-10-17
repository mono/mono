#if NET_2_0
namespace System.Web.UI
{
	public class ComplexPropertyEntry : BuilderPropertyEntry
	{
		public bool IsCollectionItem {
			get { throw new NotImplementedException (); }
		}

		public bool ReadOnly {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
#endif
