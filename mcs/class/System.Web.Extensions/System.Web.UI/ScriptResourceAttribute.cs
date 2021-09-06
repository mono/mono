//
// ScriptResourceAttribute.cs
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
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ScriptResourceAttribute : Attribute
	{
		string _scriptName;
		string _stringResourceName;
		string _stringResourceClientTypeName;

		public ScriptResourceAttribute (string scriptName, string stringResourceName, string stringResourceClientTypeName) {
			_scriptName = scriptName;
			_stringResourceName = stringResourceName;
			_stringResourceClientTypeName = stringResourceClientTypeName;
		}

		public string ScriptName {
			get { return _scriptName; }
		}

		[Obsolete("This property is obsolete. Use StringResourceName instead.")]
		public string ScriptResourceName {
			get {
				return StringResourceName;
			}
		}

		public string StringResourceClientTypeName {
			get {
				return _stringResourceClientTypeName;
			}
		}

		public string StringResourceName {
			get {
				return _stringResourceName;
			}
		}

		[Obsolete("This property is obsolete. Use StringResourceClientTypeName instead.")]
		public string TypeName {
			get {
				return StringResourceClientTypeName;
			}
		}
	}
}
