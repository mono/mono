//
// RegisteredScript.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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

namespace System.Web.UI
{
	public sealed class RegisteredScript
	{
		readonly Control _control;
		readonly bool _addScriptTags;
		readonly string _key;
		readonly string _script;
		readonly RegisteredScriptType _scriptType;
		readonly Type _type;
		readonly string _url;

		internal RegisteredScript (Control control, Type type, string key, string script, string url, bool addScriptTag, RegisteredScriptType scriptType) {
			_control = control;
			_type = type;
			_script = script;
			_url = url;
			_addScriptTags = addScriptTag;
			_scriptType = scriptType;
			_key = key;
		}

		public bool AddScriptTags {
			get { return _addScriptTags; }
		}

		public Control Control {
			get { return _control; }
		}

		public string Key {
			get { return _key; }
		}

		public string Script {
			get { return _script; }
		}

		public RegisteredScriptType ScriptType {
			get { return _scriptType; }
		}

		public Type Type {
			get { return _type; }
		}

		public string Url {
			get { return _url; }
		}
	}
}
