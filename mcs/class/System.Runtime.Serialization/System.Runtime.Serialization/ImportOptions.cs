//
// ImportOptions.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_2_0

using System.CodeDom.Compiler;
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
		CodeDomProvider code_provider;

		public ImportOptions ()
		{
		}

		public CodeDomProvider CodeProvider {
			get { return code_provider; }
			set { code_provider = value; }
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
