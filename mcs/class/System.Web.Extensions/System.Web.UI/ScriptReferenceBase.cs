//
// JsonDeserializer.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2009 Novell, Inc.  http://novell.com/
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

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace System.Web.UI
{
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class ScriptReferenceBase
	{
		string _path;
		
		public bool NotifyScriptLoaded {
			get; set;
		}

		public string Path {
			get { return _path != null ? _path : String.Empty; }
			set { _path = value; }
		}
		
		[TypeConverterAttribute(typeof(StringArrayConverter))]
		public string[] ResourceUICultures {
			get; set;
		}

		public ScriptMode ScriptMode {
			get; set;
		}

		protected ScriptReferenceBase ()
		{
			this.NotifyScriptLoaded = true;
			this.ScriptMode = ScriptMode.Auto;
		}

		protected internal abstract string GetUrl (ScriptManager scriptManager, bool zip);
		protected internal abstract bool IsFromSystemWebExtensions ();

		// This method is an example of particularily bad coding - .NET performs NO checks
		// on pathOrName!
		protected static string ReplaceExtension (string pathOrName)
		{
			// emulate .NET behavior
			if (pathOrName == null)
				throw new NullReferenceException ();
			
			// We should check the length, but since .NET doesn't do that, we won't
			// either. Ugh.
			return pathOrName.Substring (0, pathOrName.Length - 2) + "debug.js";
		}
	}
}
