﻿//
// WebProcessInformation.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;


namespace System.Web.Management {
	public sealed class WebProcessInformation {
		private int processID;
		private string processName;
		private string accountName;

		public int ProcessID
		{
			get
			{
				return this.processID;
			}
		}

		public string ProcessName
		{
			get
			{
				return this.processName;
			}
		}

		public string AccountName
		{
			get
			{
				return this.accountName ?? string.Empty;
			}
		}
		private static WebProcessInformation current = new WebProcessInformation();
		internal static WebProcessInformation Current
		{
			get { return current; }
		}
		internal WebProcessInformation ()
		{
			var proc = System.Diagnostics.Process.GetCurrentProcess ();
			this.processName = proc.MainModule.FileName;
			this.processID = proc.Id;
			this.accountName = WindowsIdentity.GetCurrent ().Name;
		}

		[MonoTODO]
		public void FormatToString (WebEventFormatter formatter)
		{
			throw new NotImplementedException ();
		}
	}
}
