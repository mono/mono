//
// System.Management.ManagementNamedValueCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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

using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Management
{
	public class ManagementNamedValueCollection : NameObjectCollectionBase
	{
		public ManagementNamedValueCollection ()
		{
		}

#if NET_2_0
		protected
#else
		public
#endif
		ManagementNamedValueCollection (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public void Add (string name, object value)
		{
			if (BaseGet (name) != null)
				BaseRemove (name);

			BaseAdd (name, value);
		}

		public ManagementNamedValueCollection Clone ()
		{
			ManagementNamedValueCollection result = new ManagementNamedValueCollection ();
			foreach (string key in Keys) {
				object value = BaseGet (key);
				if (value == null) {
					result.Add (key, value);
					continue;
				}

				if (value is ICloneable) {
					result.Add (key, ((ICloneable) value).Clone ());
				} else {
					result.Add (key, value);
				}
			}

			return result;
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAll ()
		{
			BaseClear ();
		}

		public object this [string name] {
			get { return BaseGet (name); }
		}
	}
}

