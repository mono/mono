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
//		[MonoTODO]
//		//FIXME
//		protected void MainMenu(Menu m)
//		{
//			throw new NotImplementedException();
//		}

		[MonoTODO]
		public override ObjRef CreateObjRef(Type requestedType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException();
		}


		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}		
		//inherited
		//public override void Dispose() 
		//{
		//	throw new NotImplementedException();
		//}
		//
		//protected override void Dispose(bool disposing) 
		//{
		//	throw new NotImplementedException();
		//}
		//public override bool Equals(object o, object o)
		//{
		//	throw new NotImplementedException();
		//}
		//public ContextMenu GetContextMenu()
		//{
		//	throw new NotImplementedException();
		//}
		//public object GetLifetimeService()
		//{
		//	throw new NotImplementedException();
		//}
		//public MainMenu GetMainMenu() {
		//	throw new NotImplementedException();
		//}
		//
		//public override Type GetType() {
		//	throw new NotImplementedException();
		//}
		//
		//public virtual object InitializeLifetimeService() {
		//	throw new NotImplementedException();
		//}
		//
		//public virtual void MergeMenu(Menu menuSrc) {
		//	throw new NotImplementedException();
		//}

		[MonoTODO]
		public Form GetForm()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException();
		}

		//
		// -- Protected Methods
		//

		//protected void CloneMenu(Menu menuSrc) 
		//{
		//	throw new NotImplementedException();
		//}
		//protected override void Dispose(bool disposing) 
		//{
		//	throw new NotImplementedException();
		//}
		//public void Dispose() 
		//{
		//	throw new NotImplementedException();
		//}
		//protected object MemberwiseClone() 
		//{
		//	throw new NotImplementedException();
		//}

		[MonoTODO]
		protected virtual object GetService() 
		{
			throw new NotImplementedException();
		}

		//
		// -- DeConstructor
		//

		[MonoTODO]
		~MainMenu() 
		{
			throw new NotImplementedException();
		}

		//
		// -- Public Events
		//
		//inherited
		//public event EventHandler Disposed;

		//
		// -- Public Properties
		//
		//inherited
		//public IContainer Container  {
		//	get 
		//	{
		//		throw new NotImplementedException(); 
		//	}
		//}
		//public IntPtr Handle  {
		//	get
		//	{
		//		throw new NotImplementedException();
		//	}
		//}
		//public MenuItem MdiListItem  {
		//	get 
		//	{
		//		throw new NotImplementedException();
		//	}
		//}
		//public Menu.MenuItemCollection MenuItems  {
		//	get
		//	{
		//		throw new NotImplementedException();
		//	}
		//}
		//public override ISite Site  {
		//	get 
		//	{
		//		throw new NotImplementedException();
		//	}
		//	set
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		[MonoTODO]
		public override bool IsParent  {
			get 
			{
				throw new NotImplementedException();
			}
		}
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
		//inherited
		//protected bool DesignMode  {
		//	get 
		//	{
		//		throw new NotImplementedException();
		//	}
		//}
		//protected EventHandler Events  {
		//	get 
		//	{
		//		throw new NotImplementedException();
		//	}
		//}
	}
}
