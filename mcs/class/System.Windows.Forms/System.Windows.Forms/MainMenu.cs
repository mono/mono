//
// System.Windows.Forms.MainMenu.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002 Ximian, Inc
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
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;
using System.Runtime.Remoting;
using System.ComponentModel;
namespace System.Windows.Forms {

	/// <summary>
	/// </summary>

	public class MainMenu : Menu  {
		Form form_;
		//
		//  --- Constructors
		//

		[MonoTODO]
		public MainMenu() : base(null)
		{
		}

		[MonoTODO]
		public MainMenu(MenuItem[] items) : base(items)
		{
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public virtual MainMenu CloneMenu()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public Form GetForm()
		{
			return form_;
		}

		[MonoTODO]
		public override string ToString() 
		{
			//FIXME: Replace with real to string.
			return base.ToString();
		}

		internal void setForm ( Form form ) {
			form_ = form;
		}

		//
		//  --- protected Methods
		//

		[MonoTODO]
		protected override void Dispose(bool disposing){
			base.Dispose(disposing);
		}


		//
		// -- Public Properties
		//

		[MonoTODO]
		public virtual RightToLeft RightToLeft  {
			get 
			{
				throw new NotImplementedException();
			}
			set
			{
				//FIXME:
			}
		}
	}
}
