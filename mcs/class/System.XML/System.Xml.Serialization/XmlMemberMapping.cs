// 
// System.Xml.Serialization.XmlMemberMapping
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Xml.Serialization 
{
	public class XmlMemberMapping {

		XmlTypeMapMember _mapMember;
		string _elementName;
		string _memberName;
		string _namespace;
		string _typeNamespace;

		internal XmlMemberMapping (XmlReflectionMember rmember, XmlTypeMapMember mapMem)
		{
			_mapMember = mapMem;
			_memberName = rmember.MemberName;

			if (mapMem is XmlTypeMapMemberElement)
			{
				XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) ((XmlTypeMapMemberElement)mapMem).ElementInfo[0];
				_elementName = info.ElementName;
				_namespace = info.Namespace;
				if (info.MappedType != null) _typeNamespace = info.MappedType.Namespace;
				else _namespace = "";
			}
			else
			{
				_elementName = _memberName;
				_namespace = "";
			}

			if (_typeNamespace == null) _typeNamespace = _namespace;
		}

		#region Properties

		public bool Any {	
			[MonoTODO]
			get { return false; }
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

		#endregion // Properties
	}
}
