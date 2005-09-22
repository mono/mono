#if NET_2_0
namespace System.Xml
{
	public class XmlDictionaryReaderQuotas
	{
		public XmlDictionaryReaderQuotas ()
		{
		}

		[MonoTODO]
		public const int DefaultMaxArrayLength = int.MaxValue;

		[MonoTODO]
		public const int DefaultMaxBytesPerRead = int.MaxValue;

		[MonoTODO]
		public const int DefaultMaxDepth = int.MaxValue;

		[MonoTODO]
		public const int DefaultMaxNameTableCharCount = int.MaxValue;

		[MonoTODO]
		public const int DefaultMaxStringContentLength = int.MaxValue;

		public static XmlDictionaryReaderQuotas Default {
			get { throw new NotImplementedException (); }
		}

		public static XmlDictionaryReaderQuotas Max {
			get { throw new NotImplementedException (); }
		}

		public int MaxArrayLength {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxBytesPerRead {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxDepth {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxNameTableCharCount {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxStringContentLength {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public void CopyTo (XmlDictionaryReaderQuotas quota)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
