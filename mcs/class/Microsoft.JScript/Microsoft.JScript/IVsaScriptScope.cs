//
// IVsaScriptScope.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using Microsoft.Vsa;

	public interface IVsaScriptScope : IVsaItem
	{
		IVsaScriptScope Parent { 
			get; 
		}

		IVsaItem AddItem (string itemName, VsaItemType type);

		IVsaItem GetItem (string itemName);
	
		void RemoveItem (string itemName);

		void RemoveItem (IVsaItem item);

		int GetItemCount ();
		
		IVsaItem GetItemAtIndex (int index);

		void RemoveItemAtIndex (int index);

		Object GetObject ();

		IVsaItem CreateDynamicItem (string itemName, VsaItemType type);
	}
}