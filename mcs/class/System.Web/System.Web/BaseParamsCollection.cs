//
// System.Web.BaseParamsCollection
//
// Authors:
//   Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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

namespace System.Web
{
	abstract class BaseParamsCollection : WebROCollection
	{
		protected HttpRequest _request;
		protected bool _loaded = false;

		public BaseParamsCollection (HttpRequest request)
		{		
			_request = request;
			IsReadOnly = true;
		}

		void LoadInfo ()
		{
			if (_loaded)
				return;
			IsReadOnly = false;

			InsertInfo ();
	
			IsReadOnly = true;
			_loaded = true;

		}

		protected abstract void InsertInfo ();

		public override string Get (int index)
		{
			LoadInfo ();
			return base.Get (index); 
		}

		protected abstract string InternalGet (string name);

		public override string Get (string name)
		{
			if (!_loaded) {
#if TARGET_JVM
				return InternalGet (name);
#else
				string s = InternalGet (name);
				if (s != null && s.Length > 0)
					return s;

				LoadInfo ();
#endif
			}
				
			return base.Get (name);		
		}

		public override string GetKey (int index)
		{
			LoadInfo ();
			return base.GetKey (index); 
		}
 
		public override string[] GetValues (int index)
		{
			string text1;
			string[] array1;
			text1 = Get (index);
			if (text1 == null) 
				return null; 

			array1 = new string[1];
			array1[0] = text1;
			return array1; 
		}
 
		public override string[] GetValues (string name)
		{
			string text1;
			string[] array1;
			text1 = Get (name);
			if (text1 == null) 
				return null; 

			array1 = new string[1];
			array1[0] = text1;
			return array1; 
		}
 
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException (); 
		}

		public override string[] AllKeys 
		{
			get {
				LoadInfo ();
				return base.AllKeys;
			}
		}

		public override int Count 
		{
			get {
				LoadInfo ();
				return base.Count;
			}
		}

		public override NameObjectCollectionBase.KeysCollection Keys {
			get {
				LoadInfo ();
				return base.Keys;
			}
		}

#if NET_2_0
		public override System.Collections.IEnumerator GetEnumerator () {
			LoadInfo ();
			return base.GetEnumerator ();
		}
#endif
	}
}
