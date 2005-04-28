//
// IVsaScriptScope.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using Microsoft.Vsa;
using System.Runtime.InteropServices;

namespace Microsoft.JScript {

	[GuidAttribute ("ED4BAE22-2F3C-419a-B487-CF869E716B95")]
	[ComVisibleAttribute (true)]
	public interface IVsaScriptScope : IVsaItem {

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
