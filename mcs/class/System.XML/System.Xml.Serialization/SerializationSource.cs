// 
// System.Xml.Serialization.SerializationSource.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
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

using System.Collections;
using System.Globalization;
using System.Text;

namespace System.Xml.Serialization 
{
#if !MOONLIGHT
	internal abstract class SerializationSource 
	{
		Type[] includedTypes;
		string namspace;
		bool canBeGenerated = true;
		
		public SerializationSource (string namspace, Type[] includedTypes)
		{
			this.namspace = namspace;
			this.includedTypes = includedTypes;
		}
		
		protected bool BaseEquals (SerializationSource other)
		{
			if (namspace != other.namspace) return false;
			if (canBeGenerated != other.canBeGenerated) return false;
			
			if (includedTypes == null)
				return other.includedTypes == null;
			
			if (other.includedTypes == null || includedTypes.Length != other.includedTypes.Length) return false;
			for (int n=0; n<includedTypes.Length; n++)
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
		
		public XmlTypeSerializationSource (Type type, XmlRootAttribute root, XmlAttributeOverrides attributeOverrides, string namspace, Type[] includedTypes)
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
			
			return base.BaseEquals (other);
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
		
		public SoapTypeSerializationSource (Type type, SoapAttributeOverrides attributeOverrides, string namspace, Type[] includedTypes)
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
			
			return base.BaseEquals (other);
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
										   bool literalFormat, string namspace, Type[] includedTypes)
		: base (namspace, includedTypes)
		{
			this.elementName = elementName;
			this.hasWrapperElement = hasWrapperElement;
			this.writeAccessors = writeAccessors;
			this.literalFormat = literalFormat;
			
			StringBuilder sb = new StringBuilder ();
			sb.Append (members.Length.ToString(CultureInfo.InvariantCulture));
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
			
			return base.BaseEquals (other);
		}
		
		public override int GetHashCode ()
		{
			return membersHash.GetHashCode ();
		}
	}
#endif
	
}

