// 
// System.Xml.Serialization.XmlMembersMapping
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Reflection;

namespace System.Xml.Serialization {
	public class XmlMembersMapping : XmlMapping {

		string _elementName;
		string _namespace;
		bool _hasWrapperElement;
		XmlMemberMapping[] _mapping;

		internal XmlMembersMapping ()
		{
		}

		internal XmlMembersMapping (string elementName, string ns, bool hasWrapperElement, XmlMemberMapping[] mapping)
		{
			_elementName = elementName;
			_namespace = ns;
			_hasWrapperElement = hasWrapperElement;
			_mapping = mapping;

			ClassMap map = new ClassMap ();
			foreach (XmlMemberMapping mm in mapping)
				map.AddMember (mm.TypeMapMember);
			ObjectMap = map;
		}

		#region Properties

		public int Count {	
			get { return _mapping.Length; }
		}

		public string ElementName {	
			get { return _elementName; }
		}

		public XmlMemberMapping this [int index] {	
			get { return _mapping[index]; }
		}

		public string Namespace {
			get { return _namespace; }
		}

		public string TypeName {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string TypeNamespace {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		internal bool HasWrapperElement {
			get { return _hasWrapperElement; }
		}

		#endregion // Properties
	}
}
