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
	/// ToDo note:
	///  - Nothing is implemented
	/// </summary>

	public class MainMenu : Menu  {

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
			throw new NotImplementedException();
		}
		[MonoTODO]
		public override string ToString() 
		{
			//FIXME: Replace with real to string.
			return base.ToString();
		}

		//
		// -- Protected Methods
		//

		//
		// -- Public Events
		//

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
				throw new NotImplementedException();
			}
		}

		//
		// -- protected Properties
		//
		[MonoTODO]
		protected override void Dispose(bool disposing){
			throw new NotImplementedException();
		}
	}
}
