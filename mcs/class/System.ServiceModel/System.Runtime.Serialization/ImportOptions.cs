using System.Collections.Generic;

namespace System.Runtime.Serialization
{
	public class ImportOptions
	{
		IDataContractSurrogate surrogate;
		ICollection<Type> referencedCollectionTypes = new List<Type> ();
		ICollection<Type> referencedTypes = new List<Type> ();
		bool enableDataBinding;
		bool generateInternal;
		bool generateSerializable;
		bool importXmlType;
		IDictionary<string, string> namespaces =
			new Dictionary<string, string> ();

		public ImportOptions ()
		{
		}

		public IDataContractSurrogate DataContractSurrogate {
			get { return surrogate; }
			set { surrogate = value; }
		}

		public bool EnableDataBinding {
			get { return enableDataBinding; }
			set { enableDataBinding = value; }
		}

		public bool GenerateInternal {
			get { return generateInternal; }
			set { generateInternal = value; }
		}

		public bool GenerateSerializable {
			get { return generateSerializable; }
			set { generateSerializable = value; }
		}

		public bool ImportXmlType {
			get { return importXmlType; }
			set { importXmlType = value; }
		}

		public IDictionary<string, string> Namespaces {
			get { return namespaces; }
		}

		public ICollection<Type> ReferencedCollectionTypes {
			get { return referencedCollectionTypes; }
		}

		public ICollection<Type> ReferencedTypes {
			get { return referencedTypes; }
		}
	}
}
