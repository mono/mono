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
