//
// UpdatePanelTriggerCollection.cs
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
using System.Collections.ObjectModel;

namespace System.Web.UI
{
	public class UpdatePanelTriggerCollection : Collection<UpdatePanelTrigger>
	{
		UpdatePanel _owner;
		bool initialized;
		
		public UpdatePanelTriggerCollection (UpdatePanel owner)
		{
			_owner = owner;
		}

		public UpdatePanel Owner {
			get {
				return _owner;
			}
		}

		protected override void ClearItems ()
		{
			base.ClearItems ();
		}

		protected override void InsertItem (int index, UpdatePanelTrigger item)
		{
			base.InsertItem (index, item);
			item.Owner = Owner;

			if (!initialized || item == null)
				return;

			item.Initialize ();
		}

		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
		}
		
		protected override void SetItem (int index, UpdatePanelTrigger item)
		{
			base.SetItem (index, item);
			item.Owner = Owner;
		}

		internal void Initialize ()
		{
			if (initialized)
				return;

			for (int i = 0; i < Count; i++)
				this [i].Initialize ();

			initialized = true;
		}
	}
}
