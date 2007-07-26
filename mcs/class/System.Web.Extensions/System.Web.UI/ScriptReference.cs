//
// ScriptReference.cs
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
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace System.Web.UI
{
	[DefaultProperty ("Path")]
	public class ScriptReference
	{
		string _path;
		string _name;
		string _assembly;
		ScriptMode _scriptMode = ScriptMode.Auto;
		bool _notifyScriptLoaded = true;
		bool _ignoreScriptPath;
		string [] _resourceUICultures;

		public ScriptReference ()
		{
		}

		public ScriptReference (string path)
		{
			_path = path;
		}

		public ScriptReference (string name, string assembly)
		{
			_name = name;
			_assembly = assembly;
		}

		public string Assembly {
			get {
				return _assembly;
			}
			set {
				_assembly = value;
			}
		}

		public bool IgnoreScriptPath {
			get {
				return _ignoreScriptPath;
			}
			set {
				_ignoreScriptPath = value;
			}
		}

		public string Name {
			get {
				return _name != null ? _name : String.Empty;
			}
			set {
				_name = value;
			}
		}

		public bool NotifyScriptLoaded {
			get {
				return _notifyScriptLoaded;
			}
			set {
				_notifyScriptLoaded = value;
			}
		}

		public string Path {
			get {
				return _path != null ? _path : String.Empty;
			}
			set {
				_path = value;
			}
		}

		[TypeConverter (typeof (StringArrayConverter))]
		public string [] ResourceUICultures {
			get {
				return _resourceUICultures;
			}
			set {
				_resourceUICultures = value;
			}
		}

		public ScriptMode ScriptMode {
			get {
				return _scriptMode;
			}
			set {
				_scriptMode = value;
			}
		}

		internal ScriptMode ScriptModeInternal {
			get {
				if (ScriptMode == ScriptMode.Auto) {
					if (!String.IsNullOrEmpty (Name))
						return ScriptMode.Inherit;
					else
						return ScriptMode.Release;
				}
				else
					return ScriptMode;
			}
		}

		public override string ToString ()
		{
			return Name.Length > 0 ? Name : Path;
		}
	}
}