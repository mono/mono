//
// VsaScriptScope.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
