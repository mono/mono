//
// System.Web.UI.WebControls.DataPagerFieldCollection
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
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
#if NET_3_5
using System;
using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DataPagerFieldCollection : StateManagedCollection
	{
		enum KnownTypeIndexes
		{
			NextPreviousPagerField = 0,
			NumericPagerField = 1,
			TemplatePagerField = 2
		}
		
		static readonly Type[] knownTypes = {
			typeof (NextPreviousPagerField),
			typeof (NumericPagerField),
			typeof (TemplatePagerField)
		};

		static readonly object FieldsChangedEvent = new object ();
		
		IList list;
		DataPager owner;
		EventHandlerList events;

		public event EventHandler FieldsChanged {
			add { AddEventHandler (FieldsChangedEvent, value); }
			remove { RemoveEventHandler (FieldsChangedEvent, value); }
		}
		
		public DataPagerFieldCollection (DataPager dataPager)
		{
			list = (IList)this;
			owner = dataPager;
		}

		public void Add (DataPagerField field)
		{
			list.Add (field);
		}

		public DataPagerFieldCollection CloneFields (DataPager pager)
		{
			throw new NotImplementedException ();
		}

		public bool Contains (DataPagerField field)
		{
			return list.Contains (field);
		}

		public void CopyTo (DataPagerField[] array, int index)
		{
		}

		protected override object CreateKnownType (int index)
		{
			if (!Enum.IsDefined (typeof (KnownTypeIndexes), index))
				throw new ArgumentOutOfRangeException ("index");

			return Activator.CreateInstance (knownTypes [index]);
		}
		
		protected override Type[] GetKnownTypes ()
		{
			return knownTypes;
		}

		public int IndexOf (DataPagerField field)
		{
			return list.IndexOf (field);
		}

		public void Insert (int index, DataPagerField field)
		{
			list.Insert (index, field);
		}

		protected override void OnClearComplete ()
		{
			base.OnClearComplete ();
			InvokeEvent (FieldsChangedEvent, EventArgs.Empty);
		}

		protected override void OnInsertComplete (int index, object value)
		{
			base.OnInsertComplete (index, value);
			DataPagerField field = value as DataPagerField;
			if (field == null)
				return;
			
			field.SetDataPager (owner);
			field.FieldChanged += new EventHandler (FieldHasChanged);
			InvokeEvent (FieldsChangedEvent, EventArgs.Empty);
		}

		protected override void OnRemoveComplete (int index, object value)
		{
			base.OnRemoveComplete (index, value);
			DataPagerField field = value as DataPagerField;
			if (field == null)
				return;

			field.SetDataPager (null);
			field.FieldChanged -= new EventHandler (FieldHasChanged);
			InvokeEvent (FieldsChangedEvent, EventArgs.Empty);
		}

		protected override void OnValidate (object o)
		{
			base.OnValidate (o);
			DataPagerField field = o as DataPagerField;
			if (field == null)
				throw new ArgumentException ("is not an instance of the DataPagerField class or of one of its derived classes.", "o");
		}

		public void Remove (DataPagerField field)
		{
			list.Remove (field);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		protected override void SetDirtyObject (object o)
		{
		}

		[BrowsableAttribute(false)]
		public DataPagerField this [int index] {
			get { return list [index] as DataPagerField; }
		}

		void FieldHasChanged (object sender, EventArgs args)
		{
			InvokeEvent (FieldsChangedEvent, EventArgs.Empty);
		}

		void AddEventHandler (object key, EventHandler handler)
		{
			if (events == null)
				events = new EventHandlerList ();
			events.AddHandler (key, handler);
		}

		void RemoveEventHandler (object key, EventHandler handler)
		{
			if (events == null)
				return;
			events.RemoveHandler (key, handler);
		}

		void InvokeEvent (object key, EventArgs args)
		{
			if (events == null)
				return;

			EventHandler eh = events [key] as EventHandler;
			if (eh == null)
				return;
			eh (this, args);
		}
	}
}
#endif
