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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms 
{
	public abstract class ListControl : Control
	{
		string display_member;
		
		protected ListControl ()
		{
			display_member = string.Empty;
		}

		#region Events		
		public event EventHandler DataSourceChanged;		
		public event EventHandler DisplayMemberChanged;
		public event EventHandler SelectedValueChanged;
		public event EventHandler ValueMemberChanged;
		#endregion // Events

		#region Public Properties
		//protected CurrencyManager DataManager {
		//get {throw new NotImplementedException (); };
		//}
	
		public object DataSource {
			get {throw new NotImplementedException (); }
			set {throw new NotImplementedException (); }
		}
		string DisplayMember {
			get { return display_member; } 
			set { display_member = value; }
		}
				
		public abstract int SelectedIndex {
			get; 
			set;
		}

		
		public object SelectedValue {
			get {throw new NotImplementedException (); }                     
			set {throw new NotImplementedException (); }
		}

		public string ValueMember  {
			get {throw new NotImplementedException (); }
			set {throw new NotImplementedException (); }
		}
		
		#endregion Public Properties

		#region Public Methods

		protected object FilterItemOnProperty (object item)
		{
			throw new NotImplementedException ();
		}

		protected object FilterItemOnProperty (object item, string field)
		{
			throw new NotImplementedException (); 
		}

		public string GetItemText (object item)
		{
			 throw new NotImplementedException ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			 throw new NotImplementedException ();
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{

		}

		protected virtual void OnDisplayMemberChanged (EventArgs e)
		{

		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
		
		}

		protected virtual void OnSelectedValueChanged (EventArgs e)
		{

		}

		protected virtual void OnValueMemberChanged (EventArgs e)
		{

		}

		protected abstract void RefreshItem (int index);

		protected virtual void SetItemCore (int index,  object value)
		{

		}

		protected abstract void SetItemsCore (IList items);

		
		#endregion Public Methods
		
	}	

}

