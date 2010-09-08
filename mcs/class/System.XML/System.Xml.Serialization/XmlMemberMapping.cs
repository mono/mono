// 
// System.Xml.Serialization.XmlMemberMapping
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

using System.Xml.Schema;

namespace System.Xml.Serialization 
{
	public class XmlMemberMapping {

		XmlTypeMapMember _mapMember;
		string _elementName;
		string _memberName;
		string _namespace;
		string _typeNamespace;
		XmlSchemaForm _form;

		internal XmlMemberMapping (string memberName, string defaultNamespace, XmlTypeMapMember mapMem, bool encodedFormat)
		{
			_mapMember = mapMem;
			_memberName = memberName;

			if (mapMem is XmlTypeMapMemberAnyElement)
			{
				XmlTypeMapMemberAnyElement anyelem = (XmlTypeMapMemberAnyElement) mapMem;
				XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) anyelem.ElementInfo[anyelem.ElementInfo.Count-1];
				_elementName = info.ElementName;
				_namespace = info.Namespace;
				if (info.MappedType != null) _typeNamespace = info.MappedType.Namespace;
				else _typeNamespace = "";
			}
			else if (mapMem is XmlTypeMapMemberElement)
			{
				XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) ((XmlTypeMapMemberElement)mapMem).ElementInfo[0];
				_elementName = info.ElementName;
				if (encodedFormat)
				{
					_namespace = defaultNamespace;
					if (info.MappedType != null) _typeNamespace = "";
					else _typeNamespace = info.DataTypeNamespace;
				}
				else
				{
					_namespace = info.Namespace;
					if (info.MappedType != null) _typeNamespace = info.MappedType.Namespace;
					else _typeNamespace = "";
					_form = info.Form;
				}
			}
			else
			{
				_elementName = _memberName;
				_namespace = "";
			}
			
			if (_form == XmlSchemaForm.None) 
				_form = XmlSchemaForm.Qualified;
		}

		#region Properties

		public bool Any {
			get { return _mapMember is XmlTypeMapMemberAnyElement; }
		}

		public string ElementName {	
			get { return _elementName; }
		}

		public string MemberName {	
			get { return _memberName; }
		}

		public string Namespace {
			get { return _namespace; }
		}

		public string TypeFullName {
			get { return _mapMember.TypeData.FullTypeName; }
		}

		public string TypeName {
			get { return _mapMember.TypeData.XmlType; }
		}

		public string TypeNamespace {
			get { return _typeNamespace; }
		}

		internal XmlTypeMapMember TypeMapMember {
			get { return _mapMember; }
		}
		
		internal XmlSchemaForm Form {
			get { return _form; }
		}
		
#if NET_2_0
		public string XsdElementName
		{
			get { return _mapMember.Name; }
		}
#if !TARGET_JVM	&& !MOBILE
		public string GenerateTypeName (System.CodeDom.Compiler.CodeDomProvider codeProvider)
		{
			string ret = codeProvider.CreateValidIdentifier (_mapMember.TypeData.FullTypeName);
			return _mapMember.TypeData.IsValueType && _mapMember.TypeData.IsNullable ? 
				"System.Nullable`1[" + ret + "]" : ret;
		}
#endif
#endif

#if NET_1_1
		public bool CheckSpecified
		{
			get { return _mapMember.IsOptionalValueType; }
		}
#endif

		#endregion // Properties
	}
}
