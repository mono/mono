#if NET_2_0

using System.Collections.Generic;

namespace System.Runtime.Serialization
{
	public class ImportOptions
	{
		IDataContractSurrogate surrogate;
		ICollection<Type> referenced_collection_types =
			new List<Type> ();
		ICollection<Type> referenced_types = new List<Type> ();
		bool enable_data_binding;
		bool generate_internal;
		bool generate_serializable;
		bool import_xml_type;
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
			get { return enable_data_binding; }
			set { enable_data_binding = value; }
		}

		public bool GenerateInternal {
			get { return generate_internal; }
			set { generate_internal = value; }
		}

		public bool GenerateSerializable {
			get { return generate_serializable; }
			set { generate_serializable = value; }
		}

		public bool ImportXmlType {
			get { return import_xml_type; }
			set { import_xml_type = value; }
		}

		public IDictionary<string, string> Namespaces {
			get { return namespaces; }
		}

		public ICollection<Type> ReferencedCollectionTypes {
			get { return referenced_collection_types; }
		}

		public ICollection<Type> ReferencedTypes {
			get { return referenced_types; }
		}
	}
}

#endif
