//
// System.Windows.Forms.DomainUpDown
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//   implemented by Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) Ximian, Inc., 2002
//
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms {

	// <summary>
	//	Represents a Windows up-down control that displays string values.
	// </summary>
	public class DomainUpDown : UpDownBase {

		
		DomainUpDownItemCollection items;
		int selectedIndex;
		bool sorted;
		bool wrap;

		[MonoTODO]
		public DomainUpDown() : base()
		{
			selectedIndex = -1;
			sorted = false;
			wrap = false;
			TextChanged += new EventHandler ( this.BuddyTextChanged );
		}

		
		public override void DownButton()
		{
			int newIndex = SelectedIndex + 1;
			if (  newIndex < Items.Count )
				SelectedIndex = newIndex;
			else if ( Wrap && Items.Count > 0)
				SelectedIndex = 0;
				
		}

		public override string ToString()
		{
			return GetType( ).FullName.ToString( ) + ", Items.Count: " + Items.Count.ToString ( ) + 
				", SelectedIndex: " + SelectedIndex;
		}

		public override void UpButton()
		{
			int newIndex = SelectedIndex - 1;
			if ( newIndex > -1 && newIndex < Items.Count )
				SelectedIndex = newIndex;
			else if ( Wrap && Items.Count > 0 )
				SelectedIndex = Items.Count - 1;
		}

		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			//FIXME:
			return base.CreateAccessibilityInstance();
		}

		protected void OnSelectedItemChanged(object source, EventArgs e)
		{
			if (SelectedItemChanged != null) 
				SelectedItemChanged(this, e);
		}

		[MonoTODO]
		protected override void OnTextBoxKeyDown(object source, KeyEventArgs e)
		{
			if ( ReadOnly ) {
				char symbol = System.Convert.ToChar( (int)e.KeyCode );
				
				if ( Char.IsLetterOrDigit ( symbol ) ) {
					string lower = Char.ToLower ( symbol ).ToString ( );
					string upper = Char.ToUpper ( symbol ).ToString ( );

					foreach ( object item in Items ) {
						string sitem = item.ToString ( );
						if ( sitem.StartsWith ( upper ) || sitem.StartsWith ( lower ) ) {
							SelectedItem = item;
							break;
						}
					}
					e.Handled = true;
				}
			}
			base.OnTextBoxKeyDown ( source, e );
		}

		protected override void UpdateEditText ( )
		{
			if ( SelectedIndex != -1 )
				Text = Items [ SelectedIndex ].ToString ( );
			else
				Text = String.Empty;
		}

		public event EventHandler SelectedItemChanged;

		public DomainUpDown.DomainUpDownItemCollection Items {
			get {
				if ( items == null )
					items = new DomainUpDownItemCollection ( this );
				return items; 
			}
		}

		[MonoTODO]
		public int SelectedIndex {
			get { return selectedIndex; }
			set {
				if ( value < -1 || value >= Items.Count )
					throw new ArgumentException ( ); // FIXME: message

				if ( selectedIndex != value ) {
					selectedIndex = value;
					UpdateEditText ( );
				}
			}
		}

		[MonoTODO]
		public object SelectedItem {
			get { 
				if ( SelectedIndex == -1 )
					return null;
				return Items[ SelectedIndex ];
			}
			set {
				SelectedIndex = Items.IndexOf ( value );
			}
		}

		[MonoTODO]
		public bool Sorted {
			get { return sorted; }
			set { 
				if ( sorted != value ) {
					object selectedItem = SelectedItem;
					Items.Sort ( );
					SelectedItem = selectedItem;
				}
			}
		}

		public bool Wrap { 
			get { return wrap; }
			set { wrap = value; }
		}

		private void itemAdded ( object item )
		{
		}

		private void itemInserted ( int index, object item )
		{
		}

		private void itemRemoved ( object item )
		{
		}

		private void itemRemoved ( int index )
		{
		}

		private void itemChanged ( int index )
		{
			if ( index == SelectedIndex )
				UpdateEditText ( );
		}

		private void BuddyTextChanged ( object sender, EventArgs e )
		{
			OnSelectedItemChanged ( this, EventArgs.Empty );
		}

		//System.Windows.Forms.DomainUpDown.DomainUpDownItemCollection
		//
		//Author:
		//stubbed out by Richard Baumann (biochem333@nyc.rr.com)
		//
		//(C) Ximian, Inc., 2002
		//
		//<summary>
		//Encapsulates a collection of objects for use by the DomainUpDown class.
		//</summary>
		public class DomainUpDownItemCollection : ArrayList {

			DomainUpDown owner;

			internal DomainUpDownItemCollection( DomainUpDown owner )
			{
				this.owner = owner;
			}

			public override int Add( object value )
			{
				int index =  base.Add ( value );
				owner.itemAdded ( value );
				return index;
			}

			public override void Insert( int index, object value )
			{
				base.Insert ( index, value );
				owner.itemInserted ( index, value );
			}

			public override void Remove( object obj )
			{
				base.Remove ( obj );
				owner.itemRemoved ( obj );
			}

			public override void RemoveAt( int index )
			{
				base.RemoveAt ( index );
				owner.itemRemoved ( index );
			}

			public override object this[ int index ]
			{
				get {
					return base[index];
				}
				set {
					base[index] = value;
					owner.itemChanged ( index );
				}
			}
		}
	}
}
