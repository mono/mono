// 
// System.Xml.Serialization.SerializationSource.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System.Collections;
using System.Text;

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
		string attributeOverridesHash;
		Type type;
		string rootHash;
		
		public XmlTypeSerializationSource (Type type, XmlRootAttribute root, XmlAttributeOverrides attributeOverrides, string namspace, ArrayList includedTypes)
		: base (namspace, includedTypes)
		{
			if (attributeOverrides != null) {
				StringBuilder sb = new StringBuilder ();
				attributeOverrides.AddKeyHash (sb);
				attributeOverridesHash = sb.ToString ();
			}
			
			if (root != null) {
				StringBuilder sb = new StringBuilder ();
				root.AddKeyHash (sb);
				rootHash = sb.ToString ();
			}
				
			this.type = type;
		}
		
		public override bool Equals (object o)
		{
			XmlTypeSerializationSource other = o as XmlTypeSerializationSource;
			if (other == null) return false;
			
			if (!type.Equals(other.type)) return false;
			if (rootHash != other.rootHash) return false;
			if (attributeOverridesHash != other.attributeOverridesHash) return false;
			
			return base.Equals (o);
		}
		
		public override int GetHashCode ()
		{
			return type.GetHashCode ();
		}
	}
	
	internal class SoapTypeSerializationSource: SerializationSource
	{
		string attributeOverridesHash;
		Type type;
		
		public SoapTypeSerializationSource (Type type, SoapAttributeOverrides attributeOverrides, string namspace, ArrayList includedTypes)
		: base (namspace, includedTypes)
		{
			if (attributeOverrides != null) {
				StringBuilder sb = new StringBuilder ();
				attributeOverrides.AddKeyHash (sb);
				attributeOverridesHash = sb.ToString ();
			}
			
			this.type = type;
		}

		public override bool Equals (object o)
		{
			SoapTypeSerializationSource other = o as SoapTypeSerializationSource;
			if (other == null) return false;
			if (!type.Equals(other.type)) return false;
			if (attributeOverridesHash != other.attributeOverridesHash) return false;
			
			return base.Equals (o);
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
		string membersHash;
		bool writeAccessors;
		bool literalFormat;
		
		public MembersSerializationSource (string elementName, bool hasWrapperElement, XmlReflectionMember [] members, bool writeAccessors, 
										   bool literalFormat, string namspace, ArrayList includedTypes)
		: base (namspace, includedTypes)
		{
			this.elementName = elementName;
			this.hasWrapperElement = hasWrapperElement;
			this.writeAccessors = writeAccessors;
			this.literalFormat = literalFormat;
			
			StringBuilder sb = new StringBuilder ();
			sb.Append (members.Length.ToString());
			foreach (XmlReflectionMember mem in members)
				mem.AddKeyHash (sb);
				
			membersHash = sb.ToString();
		}
		
		
		public override bool Equals (object o)
		{
			MembersSerializationSource other = o as MembersSerializationSource;
			if (other == null) return false;
			if (literalFormat = other.literalFormat) return false;
			if (elementName != other.elementName) return false;
			if (hasWrapperElement != other.hasWrapperElement) return false;
			if (membersHash != other.membersHash) return false;
			
			return base.Equals (o);
		}
		
		public override int GetHashCode ()
		{
			return membersHash.GetHashCode ();
		}
	}
	
	internal class KeyHelper
	{
		public static void AddField (StringBuilder sb, int n, string val)
		{
			AddField (sb, n, val, null);
		}
		
		public static void AddField (StringBuilder sb, int n, string val, string def)
		{
			if (val != def) {
				sb.Append (n.ToString());
				sb.Append (val.Length.ToString());
				sb.Append (val);
			}
		}
		
		public static void AddField (StringBuilder sb, int n, bool val)
		{
			AddField (sb, n, val, false);
		}
		
		public static void AddField (StringBuilder sb, int n, bool val, bool def)
		{
			if (val != def)
				sb.Append (n.ToString());
		}
		
		public static void AddField (StringBuilder sb, int n, int val, int def)
		{
			if (val != def) {
				sb.Append (n.ToString());
				sb.Append (val.ToString());
			}
		}
		
		public static void AddField (StringBuilder sb, int n, Type val)
		{
			if (val != null) {
				sb.Append (n.ToString());
				sb.Append (val.ToString());
			}
		}
	}
}

