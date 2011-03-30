//
// MessagePartDescription.cs
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace System.ServiceModel.Description
{
	[DebuggerDisplay ("Name={name}, Namespace={ns}, Type={Type}, Index={index}}")]
	public class MessagePartDescription
	{
		int index;
		MemberInfo member;
		bool multiple;
		Type type;
		string name, ns;
		bool has_protection_level;
		ProtectionLevel protection_level;

		private XmlQualifiedName xml_schema_type_name;
		private XmlTypeMapping xml_type_mapping;
		
		public MessagePartDescription (string name, string ns)
		{
			this.name = name;
			this.ns = ns;
		}

		public int Index {
			get { return index; }
			set { index = value; }
		}

		public MemberInfo MemberInfo {
			get { return member; }
			set { member = value; }
		}

		public string Name {
			get { return name; }
		}

		public string Namespace {
			get { return ns; }
		}

		public bool HasProtectionLevel {
			get { return has_protection_level; }
		}

		public ProtectionLevel ProtectionLevel {
			get { return protection_level; }
			set {
				protection_level = value;
				has_protection_level = true;
			}
		}

		public bool Multiple {
			get { return multiple; }
			set { multiple = value; }
		}

		public Type Type {
			get { return type; }
			set { type = value; }
		}

#if !NET_2_1
		internal XsdDataContractImporter Importer { get; set; }
		internal System.CodeDom.CodeTypeReference CodeTypeReference { get; set; }
#endif

		#region internals required for moonlight compatibility

		ICustomAttributeProvider additional_att_provider;

		internal ICustomAttributeProvider AdditionalAttributesProvider {
			get { return additional_att_provider ?? MemberInfo; }
			set { additional_att_provider = value; }
		}

		internal int SerializationPosition { get; set; }

		#endregion
	}
}
