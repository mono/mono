﻿//
// WebManagementEvent.cs
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
using System.Security.Permissions;

namespace System.Web.Management {

	[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class WebManagementEvent : WebBaseEvent {
		public WebProcessInformation ProcessInformation
		{
			get
			{
				return WebProcessInformation.Current;
			}
		}

		static WebManagementEvent ()
		{
		}

		protected internal WebManagementEvent (string message, object eventSource, int eventCode)
			: base (message, eventSource, eventCode)
		{
		}

		protected internal WebManagementEvent (string message, object eventSource, int eventCode, int eventDetailCode)
			: base (message, eventSource, eventCode, eventDetailCode)
		{
		}
	}
}
