// 
// System.Xml.Serialization.XmlMembersMapping
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

using System.Reflection;

namespace System.Xml.Serialization {
	public class XmlMembersMapping : XmlMapping {

		bool _hasWrapperElement;
		XmlMemberMapping[] _mapping;

		internal XmlMembersMapping ()
		{
		}

		internal XmlMembersMapping (XmlMemberMapping[] mapping): this ("", null, false, false, mapping)
		{
		}

		internal XmlMembersMapping (string elementName, string ns, XmlMemberMapping[] mapping): this (elementName, ns, true, false, mapping)
		{
		}

		internal XmlMembersMapping (string elementName, string ns, bool hasWrapperElement, bool writeAccessors, XmlMemberMapping[] mapping)
		: base (elementName, ns)
		{
			_hasWrapperElement = hasWrapperElement;
			_mapping = mapping;

			ClassMap map = new ClassMap ();
			map.IgnoreMemberNamespace = writeAccessors;
			foreach (XmlMemberMapping mm in mapping)
				map.AddMember (mm.TypeMapMember);
			ObjectMap = map;
		}

		#region Properties

		public int Count {	
			get { return _mapping.Length; }
		}

#if !NET_2_0
		public string ElementName {	
			get { return _elementName; }
		}

		public string Namespace {
			get { return _namespace; }
		}
#endif

		public XmlMemberMapping this [int index] {	
			get { return _mapping[index]; }
		}

		public string TypeName {
			[MonoTODO]
			get { return null; } // when does it return non-null string?
		}

		public string TypeNamespace {
			[MonoTODO]
			get { return null; } // when does it return non-null string?
		}

		internal bool HasWrapperElement {
			get { return _hasWrapperElement; }
		}

		#endregion // Properties
	}
}
