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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Andreia Gaita	(avidigal@novell.com)
//

#if NET_2_0
using System;
using System.Runtime.Serialization;

namespace System.Resources
{
	[SerializableAttribute]
	public sealed class ResXDataNode : ISerializable
	{
		private string name;
		private object value;
		private Type type;
		private ResXFileRef fileRef;
		private string comment;

		public string Comment {
			get { return this.comment; }
			set { this.comment = value; }
		}
		
		public ResXFileRef FileRef {
			get { return this.fileRef; }
		}

		public string Name {
			get { return this.name; }
			set { this.name = value; }
		}

		internal object Value {
			get { return this.value; }
		}

		public ResXDataNode (string name, object value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("name");

			Type type = (value == null) ? typeof (object) : value.GetType ();
			if ((value != null) && !type.IsSerializable) {
				throw new InvalidOperationException (String.Format ("'{0}' of type '{1}' cannot be added because it is not serializable", name, type));
			}

			this.type = type;
			this.name = name;
			this.value = value;
		}

		public ResXDataNode (string name, ResXFileRef fileRef)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (fileRef == null)
				throw new ArgumentNullException ("fileRef");

			if (name.Length == 0)
				throw new ArgumentException ("name");

			this.name = name;
			this.fileRef = fileRef;
		}

		#region ISerializable Members

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("Name", this.Name);
			info.AddValue ("Comment", this.Comment);
		}

		#endregion
	}
}
#endif