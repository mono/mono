namespace System.Runtime.Serialization
{
	public class ExportOptions
	{
		IDataContractSurrogate surrogate;
		KnownTypeCollection knownTypes;

		public ExportOptions ()
		{
		}

		public IDataContractSurrogate DataContractSurrogate {
			get { return surrogate; }
			set { surrogate = value; }
		}

		public KnownTypeCollection KnownTypes {
			get { return knownTypes; }
		}
	}
}
