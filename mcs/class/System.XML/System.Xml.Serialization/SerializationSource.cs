// 
// System.Xml.Serialization.SerializationSource.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System.Collections;

namespace System.Xml.Serialization 
{
	internal class SerializationSource 
	{
		ArrayList includedTypes;
		string namspace;
		bool canBeGenerated = true;
		
		public SerializationSource (string namspace, ArrayList includedTypes)
		{
			this.namspace = namspace;
			this.includedTypes = includedTypes;
		}
		
		public override bool Equals (object o)
		{
			SerializationSource other = o as SerializationSource;
			if (other == null) return false;
			if (namspace != other.namspace) return false;
			if (canBeGenerated != other.canBeGenerated) return false;
			
			if (includedTypes == null)
				return other.includedTypes == null;
			
			if (includedTypes.Count != other.includedTypes.Count) return false;
			for (int n=0; n<includedTypes.Count; n++)
				if (!includedTypes[n].Equals (other.includedTypes[n])) return false;

			return true;
		}
		
		public virtual bool CanBeGenerated
		{
			get { return canBeGenerated; }
			set { canBeGenerated = value; }
		}
	}
	
	internal class XmlTypeSerializationSource: SerializationSource
	{
		XmlAttributeOverrides attributeOverrides;
		Type type;
		XmlRootAttribute root;
		
		public XmlTypeSerializationSource (Type type, XmlRootAttribute root, XmlAttributeOverrides attributeOverrides, string namspace, ArrayList includedTypes)
		: base (namspace, includedTypes)
		{
			this.attributeOverrides = attributeOverrides;
			this.type = type;
			this.root = root;
		}
		
		public override bool Equals (object o)
		{
			XmlTypeSerializationSource other = o as XmlTypeSerializationSource;
			if (other == null) return false;
			if (!type.Equals(other.type)) return false;
			
			if (root == null) {
				if (other.root != null) 
					return false;
			}
			else if (!root.InternalEquals (other.root))
				return false;
			
			if (!base.Equals (o))
				return false;
			
			if (attributeOverrides == null)
				return other.attributeOverrides == null;
			
			return attributeOverrides.InternalEquals (other.attributeOverrides);
		}
		
		public override int GetHashCode ()
		{
			return type.GetHashCode ();
		}
	}
	
	internal class SoapTypeSerializationSource: SerializationSource
	{
		SoapAttributeOverrides attributeOverrides;
		Type type;
		
		public SoapTypeSerializationSource (Type type, SoapAttributeOverrides attributeOverrides, string namspace, ArrayList includedTypes)
		: base (namspace, includedTypes)
		{
			this.attributeOverrides = attributeOverrides;
			this.type = type;
		}

		public override bool Equals (object o)
		{
			SoapTypeSerializationSource other = o as SoapTypeSerializationSource;
			if (other == null) return false;
			if (!type.Equals(other.type)) return false;
			
			if (!base.Equals (o))
				return false;
			
			if (attributeOverrides == null)
				return other.attributeOverrides == null;
			
			return attributeOverrides.InternalEquals (other.attributeOverrides);
		}
		
		public override int GetHashCode ()
		{
			return type.GetHashCode ();
		}
	}
	
	internal class MembersSerializationSource: SerializationSource
	{
		string elementName;
		bool hasWrapperElement;
		XmlReflectionMember [] members;
		bool writeAccessors;
		bool literalFormat;
		int hcode = -1;
		
		public MembersSerializationSource (string elementName, bool hasWrapperElement, XmlReflectionMember [] members, bool writeAccessors, 
										   bool literalFormat, string namspace, ArrayList includedTypes)
		: base (namspace, includedTypes)
		{
			this.elementName = elementName;
			this.hasWrapperElement = hasWrapperElement;
			this.members = members;
			this.writeAccessors = writeAccessors;
			this.literalFormat = literalFormat;
		}
		
		
		public override bool Equals (object o)
		{
			MembersSerializationSource other = o as MembersSerializationSource;
			if (other == null) return false;
			if (literalFormat = other.literalFormat) return false;
			if (elementName != other.elementName) return false;
			if (hasWrapperElement != other.hasWrapperElement) return false;
			if (members.Length != other.members.Length) return false;
			
			if (!base.Equals (o))
				return false;
			
			for (int n=0; n<members.Length; n++)
				if (!members[n].InternalEquals (other.members[n])) return false;

			return true;
		}
		
		public override int GetHashCode ()
		{
			if (hcode != -1) return hcode;
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			foreach (XmlReflectionMember mem in members)
				sb.Append (mem.MemberName);
			hcode = sb.ToString().GetHashCode ();
			return hcode;
		}
	}
}

