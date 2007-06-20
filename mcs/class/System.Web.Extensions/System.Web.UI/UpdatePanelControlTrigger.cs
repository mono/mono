//
// UpdatePanelControlTrigger.cs
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

namespace System.Web.UI
{
	public abstract class UpdatePanelControlTrigger : UpdatePanelTrigger
	{
		string _controlID;

		protected UpdatePanelControlTrigger () { }

		[IDReferenceProperty]
		[DefaultValue ("")]
		[Category ("Behavior")]
		public string ControlID {
			get {
				if(_controlID==null)
					return String.Empty;
				return _controlID;
			}
			set {
				_controlID = value;
			}
		}

		protected Control FindTargetControl (bool searchNamingContainers) {
			if (String.IsNullOrEmpty (ControlID))
				throw new InvalidOperationException ();

			Control nc = Owner.NamingContainer;
			Control c = null;
			do {
				c = nc.FindControl (ControlID);
				nc = nc.NamingContainer;
			}
			while (searchNamingContainers && c == null && nc != null);

			if (c == null)
				throw new InvalidOperationException ();

			return c;
		}
	}
}
