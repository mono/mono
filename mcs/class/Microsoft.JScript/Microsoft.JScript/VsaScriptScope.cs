//
// VsaScriptScope.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
using System.Collections;
using Microsoft.Vsa;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	internal class VsaScriptScope : VsaItem, IVsaScriptScope {
	
		internal VsaScriptScope parent;
		internal ArrayList items;
		internal GlobalScope scope;

		internal VsaScriptScope (VsaEngine engine, string item_name, VsaScriptScope parent)
			: base (engine, item_name, (VsaItemType) 0, VsaItemFlag.None)
		{
			this.parent = parent;
			items = new ArrayList (8);
		}

		public IVsaScriptScope Parent {
			get { return (IVsaScriptScope) parent; }
		}

		public IVsaItem AddItem (string item_name, VsaItemType type)
		{
			throw new NotImplementedException ();
		}

		public IVsaItem GetItem (string item_name)
		{
			throw new NotImplementedException ();
		}

		public void RemoveItem (string item_name)
		{
			throw new NotImplementedException ();
		}

		public void RemoveItem (IVsaItem item)
		{
			throw new NotImplementedException ();
		}

		public int GetItemCount ()
		{
			return items.Count;
		}

		public IVsaItem GetItemAtIndex (int index)
		{
			return (IVsaItem) items [index];
		}

		public void RemoveItemAtIndex (int index)
		{
			throw new NotImplementedException ();
		}

		public object GetObject ()
		{
			if (scope == null)
				if (parent == null) {
					scope = new GlobalScope (null, engine);
				} else {
					scope = new GlobalScope ((GlobalScope) parent.GetObject (), 
								 engine, false);
				}
			return scope;			
		}

		public IVsaItem CreateDynamicItem (string item_name, VsaItemType type)
		{
			throw new NotImplementedException ();
		}
	}
}
