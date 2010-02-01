//
// ScriptBehaviorDescriptor.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;

namespace System.Web.UI
{
	public class ScriptBehaviorDescriptor : ScriptComponentDescriptor
	{
		string _name;
		bool _nameSet;
		
		public ScriptBehaviorDescriptor (string type, string elementID)
			: base (type) {
			if (String.IsNullOrEmpty (elementID))
				throw new ArgumentException ("Value cannot be null or empty.", "elementID");
			ElementIDInternal = elementID;
		}

		public override string ClientID {
			get {
				string clientId = base.ClientID;
				if (String.IsNullOrEmpty (clientId))
					return String.Format ("{0}${1}", ElementID, Name);
				return clientId;
			}
		}

		public string ElementID {
			get {
				return ElementIDInternal;
			}
		}

		public string Name {
			get {
				if (String.IsNullOrEmpty (_name))
					_name = GetNameFromType (Type);
				return _name;
			}
			set {
				_name = value;
				_nameSet = true;
			}
		}

		static string GetNameFromType (string Type) {
			int lastIndex = Type.LastIndexOf ('.') + 1;
			if (lastIndex > 0 && lastIndex < Type.Length)
				return Type.Substring (lastIndex);

			return Type;
		}

		protected internal override string GetScript ()
		{
			if (_nameSet && !String.IsNullOrEmpty (_name))
				AddProperty ("name", _name);
			
			return base.GetScript ();
		}
	}
}
