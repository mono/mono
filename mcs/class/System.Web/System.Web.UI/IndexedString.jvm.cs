#if NET_2_0
namespace System.Web.UI
{
	[SerializableAttribute]
	public sealed class IndexedString
	{
		public IndexedString (string s)
		{
			throw new NotImplementedException ();
		}
		
		public string Value { get { throw new NotImplementedException (); } }
	}
}
#endif
