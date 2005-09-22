#if NET_2_0
namespace System.Runtime.Serialization
{
	public class ExportOptions
	{
		IDataContractSurrogate surrogate;
		KnownTypeCollection known_types;

		public ExportOptions ()
		{
		}

		public IDataContractSurrogate DataContractSurrogate {
			get { return surrogate; }
			set { surrogate = value; }
		}

		public KnownTypeCollection KnownTypes {
			get { return known_types; }
		}
	}
}
#endif
