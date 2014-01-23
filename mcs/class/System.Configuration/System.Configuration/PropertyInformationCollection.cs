//
// System.Configuration.PropertyInformationCollection.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Configuration
{
	[Serializable]
	public sealed class PropertyInformationCollection: NameObjectCollectionBase
	{
		internal PropertyInformationCollection ()
			: base (StringComparer.Ordinal)
		{
		}
		
		public void CopyTo (PropertyInformation[] array, int index)
		{
			((ICollection)this).CopyTo (array, index);
		}
		
		public PropertyInformation this [string propertyName] {
			get { return (PropertyInformation) BaseGet (propertyName); }
		}
		
		public override IEnumerator GetEnumerator ()
		{
			return new PropertyInformationEnumerator (this);
		}
		
		internal void Add (PropertyInformation pi)
		{
			BaseAdd (pi.Name, pi);
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		class PropertyInformationEnumerator : IEnumerator
		{
			private PropertyInformationCollection collection;
			private int position;
			
			public PropertyInformationEnumerator (PropertyInformationCollection collection)
			{
				this.collection = collection;
				position = -1;
			}
			
			public object Current 
			{
				get {
					if ((position < collection.Count) && (position >= 0))
						return collection.BaseGet (position);
					else 
						throw new InvalidOperationException();
				}
				
			}
			
			public bool MoveNext ()
			{
				return (++position < collection.Count) ? true : false;
			}
			
			public void Reset ()
			{
				position = -1;
			}
		}
	}
}

