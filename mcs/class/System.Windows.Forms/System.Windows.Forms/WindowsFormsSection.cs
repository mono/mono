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
// Copyright (c) 2007 Novell, Inc.
//

using System;
using System.Configuration;

namespace System.Windows.Forms
{
	public sealed class WindowsFormsSection : ConfigurationSection
	{
		private ConfigurationPropertyCollection properties;
		private ConfigurationProperty jit_debugging;

		public WindowsFormsSection ()
		{
			properties = new ConfigurationPropertyCollection();
			jit_debugging = new ConfigurationProperty ("jitDebugging", typeof (bool), false);

			properties.Add (jit_debugging);
		}

		[ConfigurationProperty("jitDebugging", DefaultValue = "False")]
		public bool JitDebugging {
			get { return (bool)base[jit_debugging]; }
			set { base[jit_debugging] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
