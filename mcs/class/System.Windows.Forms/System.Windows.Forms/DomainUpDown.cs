//
// System.Windows.Forms.DomainUpDown
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Collections;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template. Nothing is implemented yet.
	//	Represents a Windows up-down control that displays string values.
	// </summary>
	public class DomainUpDown : UpDownBase {

		
		//  --- Constructors/Destructors
		
		[MonoTODO]
		public DomainUpDown() : base()
		{
			throw new NotImplementedException ();
		}

		
		//  --- Public Methods
		
		[MonoTODO]
		public override void DownButton()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override void UpButton()
		{
			throw new NotImplementedException ();
		}

		
		//  --- Protected Methods
		
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			throw new NotImplementedException ();
		}
		//[MonoTODO]
		//protected override void OnChanged(object source, EventArgs e)
		//{
		//	//This method is internal to the .NET framework.
		//	if (Changed != null) {
		//
		//		Changed(this, e);
		//	}
		//}
		[MonoTODO]
		protected void OnSelectedItemChanged(object source, EventArgs e)
		{
			if (SelectedItemChanged != null) {

				SelectedItemChanged(this, e);
			}
		}
		[MonoTODO]
		protected override void OnTextBoxKeyDown(object source, KeyEventArgs e)
		{
			throw new NotImplementedException ();
			//if (TextBoxKeyDown != null) {
			//	TextBoxKeyDown(this, e);
			//}
		}
		[MonoTODO]
		protected override void UpdateEditText()
		{
			throw new NotImplementedException ();
		}

		//inherited
		//[MonoTODO]
		//protected override void WndProc(Message m) // override changed to virtual in .NET V.1. Beta
		//{
		//	throw new NotImplementedException ();
		//}

		
		//  --- Public Events
		
		public event EventHandler SelectedItemChanged;

		
		//  --- Public Properties
		
		[MonoTODO]
		public DomainUpDown.DomainUpDownItemCollection Items {

			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public int SelectedIndex{// default -1 {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public object SelectedItem{ // default null {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool Sorted{ // default false {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool Wrap{ // default false {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		//System.Windows.Forms.DomainUpDown.DomainUpDownItemCollection
		//
		//Author:
		//stubbed out by Richard Baumann (biochem333@nyc.rr.com)
		//
		//(C) Ximian, Inc., 2002
		//
		//<summary>
		//This is only a template. Nothing is implemented yet.
		//Encapsulates a collection of objects for use by the DomainUpDown class.
		//</summary>
		public class DomainUpDownItemCollection : ArrayList {

			//
			//  --- Constructors/Destructors
			//
			[MonoTODO]
			internal DomainUpDownItemCollection(DomainUpDown owner) : base()
			{
				throw new NotImplementedException ();
			}

			
			//  --- Public Methods
			
			[MonoTODO]
			public override int Add(object item)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override void Insert(int index, object item)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override void Remove(object item)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override void RemoveAt(int item)
			{
				throw new NotImplementedException ();
			}

			
			//  --- Public Properties
					
			public override object this[int index] {

				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
		}
	}
}
