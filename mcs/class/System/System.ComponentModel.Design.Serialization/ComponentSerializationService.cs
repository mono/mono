//
// System.ComponentModel.Design.Serialization.ComponentSerializationService
//
// Authors:	 
//	  Ivan N. Zlatev (contact@i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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
using System.ComponentModel;
using System.Collections;
using System.IO;

namespace System.ComponentModel.Design.Serialization
{
	public abstract class ComponentSerializationService
	{

		protected ComponentSerializationService ()
		{
		}

		public abstract SerializationStore CreateStore ();
		public abstract ICollection Deserialize (SerializationStore store);
		public abstract ICollection Deserialize (SerializationStore store, IContainer container);
		public abstract SerializationStore LoadStore (Stream stream);
		public abstract void Serialize (SerializationStore store, object value);
		public abstract void SerializeAbsolute (SerializationStore store, object value);
		public abstract void SerializeMember (SerializationStore store, object owningObject, MemberDescriptor member);
		public abstract void SerializeMemberAbsolute (SerializationStore store, object owningObject, MemberDescriptor member);

		public void DeserializeTo (SerializationStore store, IContainer container)
		{
			DeserializeTo (store, container, true);
		}

		public void DeserializeTo (SerializationStore store, IContainer container, bool validateRecycledTypes)
		{
			DeserializeTo (store, container, validateRecycledTypes, true);
		}

		public abstract void DeserializeTo (SerializationStore store, IContainer container, bool validateRecycledTypes, bool applyDefaults);
	}
}
