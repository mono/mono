//
// ScriptingProfileServiceSection.cs
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
using System.Configuration;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace System.Web.Configuration
{
	public sealed class ScriptingProfileServiceSection : ConfigurationSection
	{
		[ConfigurationPropertyAttribute ("enabled", DefaultValue = false)]
		public bool Enabled {
			get {
				return (bool) this ["enabled"];
			}
			set {
				this ["enabled"] = value;
			}
		}

		[ConfigurationPropertyAttribute ("readAccessProperties", DefaultValue = null)]
		[TypeConverter(typeof(StringArrayConverter))]
		public string [] ReadAccessProperties {
			get {
				string [] data = ReadAccessPropertiesNoCopy;
				return (string []) data.Clone ();
			}
			set {
				if (value != null)
					value = (string []) value.Clone ();
				ReadAccessPropertiesNoCopy = value;
			}
		}

		internal string [] ReadAccessPropertiesNoCopy {
			get {
				return (string []) this ["readAccessProperties"];
			}
			set {
				this ["readAccessProperties"] = value;
			}
		}

		[ConfigurationPropertyAttribute ("writeAccessProperties", DefaultValue = null)]
		[TypeConverter (typeof (StringArrayConverter))]
		public string [] WriteAccessProperties {
			get {
				string [] data = WriteAccessPropertiesNoCopy;
				return (string []) data.Clone ();
			}
			set {
				if (value != null)
					value = (string []) value.Clone ();
				WriteAccessPropertiesNoCopy = value;
			}
		}

		internal string [] WriteAccessPropertiesNoCopy {
			get {
				return (string []) this ["writeAccessProperties"];
			}
			set {
				this ["writeAccessProperties"] = value;
			}
		}
	}
}
