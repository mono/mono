//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc (http://novell.com/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Util;

namespace Microsoft.Web.Infrastructure.DynamicValidationHelper
{
	sealed class LazyWebROCollection : WebROCollection
	{
		WebROCollection wrapped;
		RequestValidationSource validationSource;
		
		public LazyWebROCollection (RequestValidationSource validationSource, WebROCollection wrapped)
		{
			if (wrapped == null)
				throw new ArgumentNullException ("wrapped");
			
			this.validationSource = validationSource;
			this.wrapped = wrapped;
		}
		
		public new string this [int index] {
			get { return Get (index); }
		}

		public new string this [string name] {
			get { return Get (name); }
			set{ Set (name,value); }
		}
		
		public override string[] AllKeys {
			get { return wrapped.AllKeys; }
		}

		public override int Count {
			get { return wrapped.Count; }
		}
		
		public override NameObjectCollectionBase.KeysCollection Keys {
			get { return wrapped.Keys; }
		}

		public new void Add (NameValueCollection c)
		{
			wrapped.Add (c);
		}
			
		public override void Add (string name, string val)
		{
			wrapped.Add (name, val);
		}
		
		public override void Clear ()
		{
			wrapped.Clear ();
		}

		public override string Get (string name)
		{
			return Validate (name, wrapped.Get (name));
		}

		public override string Get (int index)
		{
			return Validate (wrapped.GetKey (index), wrapped.Get (index));
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			wrapped.GetObjectData (info, context);
		}
		
		public override IEnumerator GetEnumerator ()
		{
			return wrapped.GetEnumerator ();
		}

		public override string GetKey (int index)
		{
			return wrapped.GetKey (index);
		}

		public override string[] GetValues (int index)
		{
			return wrapped.GetValues (index);
		}

		public override string[] GetValues (string name)
		{
			return wrapped.GetValues (name);
		}

		public override void OnDeserialization (object sender)
		{
			wrapped.OnDeserialization (sender);
		}
		
		public override void Set (string name, string value)
		{
			wrapped.Set (name, value);
		}
		
		string Validate (string key, string value)
		{
			if (String.IsNullOrEmpty (value))
				return value;

			HttpRequest.ValidateString (key, value, validationSource);
			return value;
		}
	}
}
