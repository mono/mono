//
// System.Windows.Forms.StatusBar.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Collections;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//	Represents a Windows status bar control.
	// </summary>
	public class StatusBar : Control {

		//
		//  --- Private Fields
		//
		private bool showPanels;
		private bool sizingGrip;

		//
		//  --- Constructors/Destructors
		//
		[MonoTODO]
		public StatusBar() : base()
		{
			Dock = DockStyle.Bottom;
			showPanels = false;
			sizingGrip = true;
			throw new NotImplementedException ();
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override void CreateHandle()
		{
			throw new NotImplementedException ();
		}

		//inherited
		//protected override void Dispose(bool disposing)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected virtual void OnDrawItem(StatusBarDrawItemEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnLayout(LayoutEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnPanelClick(StatusBarPanelClickEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		public event StatusBarDrawItemEventHandler DrawItem;
		public event StatusBarPanelClickEventHandler PanelClick;

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override Color BackColor {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override Image BackgroundImage {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override DockStyle Dock {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override Font Font {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override Color ForeColor {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public new ImeMode ImeMode {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public StatusBar.StatusBarPanelCollection Panels {

			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool ShowPanels {// default false {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool SizingGrip // default true {
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public new bool TabStop {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override string Text {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {

			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		protected override ImeMode DefaultImeMode {

			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		protected override Size DefaultSize {

			get { throw new NotImplementedException (); }
		}

		//
		// System.Windows.Forms.StatusBar.StatusBarPanelCollection
		//
		// Author:
		//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
		//   stub ammended by Jaak Simm (jaaksimm@firm.ee)
		//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
		// (C) Ximian, Inc., 2002
		//
		// <summary>
		//	Represents the collection of panels in a StatusBar control.
		// </summary>
		public class StatusBarPanelCollection : IList, ICollection, IEnumerable {

			//
			//  --- Private Fields
			//
			private ArrayList list;
			private StatusBar owner;
			private static string class_string = "System.Windows.Forms.StatusBar.StatusBarPanelCollection::";

			//
			//  --- Constructors/Destructors
			//
			StatusBarPanelCollection(StatusBar owner) : base()
			{
				list = new ArrayList();
				this.owner = owner;
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public virtual int Add(StatusBarPanel panel)
			{
				string method_string = "Add(StatusBarPanel) ";
				if (panel == null) {

					throw new ArgumentNullException(class_string + method_string + "panel == null");
				}
				if (panel.Parent == null) {

					throw new ArgumentException(class_string + method_string + "panel.Parent != null");
				}
				// FIXME: StatusBarPanel.Parent is readonly!
				//panel.Parent = owner;
				return list.Add(panel);
			}
			[MonoTODO]
			public virtual StatusBarPanel Add(string s)
			{
				throw new NotImplementedException ();
			//	StatusBarPanel tmp = new StatusBarPanel();
			//	tmp.Text = s;
			//	// FIXME: StatusBarPanel.Parent is readonly!
			//	//tmp.Parent = owner;
			//	list.Add(tmp);
			//	return tmp;
			}
			[MonoTODO]
			public virtual void AddRange(StatusBarPanel[] panels)
			{
				string method_string = "AddRange(StatusBarPanel[]) ";
				if (panels == null) {

					throw new ArgumentNullException(class_string + method_string + "panels == null");
				}
				for (int i = 0; i < panels.Length; i++) {
					// FIXME: StatusBarPanel.Parent is readonly!
					//panels[i].Parent = owner;
				}
				list.AddRange(panels);
			}
			public virtual void Clear()
			{
				list.Clear();
			}
			public bool Contains(StatusBarPanel panel)
			{
				return list.Contains(panel);
			}
			public IEnumerator GetEnumerator()
			{
				return list.GetEnumerator();
			}
			public int IndexOf(StatusBarPanel panel)
			{
				return list.IndexOf(panel);
			}
			[MonoTODO]
			public virtual void Insert(int index, StatusBarPanel panel)
			{
				string method_string = "Insert(int,StatusBarPanel) ";
				if (panel == null) {

					throw new ArgumentNullException(class_string + method_string + "panel == null");
				}
				if (panel.Parent == null) {

					throw new ArgumentException(class_string + method_string + "panel.Parent != null");
				}
				if  (panel.AutoSize != StatusBarPanelAutoSize.None &&
				     panel.AutoSize != StatusBarPanelAutoSize.Contents &&
				     panel.AutoSize != StatusBarPanelAutoSize.Spring)
				{
					throw new InvalidEnumArgumentException(class_string + method_string + "panel.AutoSize is not a valid StatusBarPanelAutoSize value");
				}
				list.Insert(index,panel);
				
				                      // do this after insert because insert does the range checking and might throw an exception
				// FIXME: StatusBarPanel.Parent is readonly!
				// panel.Parent = owner; // a rethrow for a better exception message, or an extra range check, would incur an unnecessary performance cost
			}
			public virtual void Remove(StatusBarPanel panel)
			{
				string method_string = "Remove(StatusBarPanel) ";
				if (panel == null) {

					throw new ArgumentNullException(class_string + method_string + "panel == null");
				}
				list.Remove(panel);
			}
			public virtual void RemoveAt(int index)
			{
				list.RemoveAt(index);
			}
			void ICollection.CopyTo(Array dest, int index)
			{
				string method_string = "ICollection.CopyTo(Array,int) ";
				if (dest == null) {

					throw new ArgumentNullException(class_string + method_string + "array == null");
				}
				if (index < 0) {

					throw new ArgumentOutOfRangeException(class_string + method_string + "index < 0");
				}
				if (dest.Rank != 1) {

					throw new ArgumentException(class_string + method_string + "array is multidimensional");
				}
				if (index >= dest.Length) {

					throw new ArgumentException(class_string + method_string + "index >= array.Length");
				}
				if (Count+index >= dest.Length) {

					throw new ArgumentException(class_string + method_string + "insufficient array capacity");
				}
				// easier/quicker to let the runtime throw the invalid cast exception if necessary
				for (int i = 0; index < dest.Length; i++, index++) {

					dest.SetValue(list[i], index);
				}
			}
			int IList.Add(object panel)
			{
				string method_string = "IList.Add(object) ";
				if (!(panel is StatusBarPanel)) {

					throw new ArgumentException(class_string + method_string + "panel is not a StatusBarPanel");
				}
				return Add((StatusBarPanel) panel);
			}
			bool IList.Contains(object panel)
			{
				if (!(panel is StatusBarPanel)) {

					return false;
				}
				return Contains((StatusBarPanel) panel);
			}
			int IList.IndexOf(object panel)
			{
				if (!(panel is StatusBarPanel)) {

					return -1;
				}
				return IndexOf((StatusBarPanel) panel);
			}
			void IList.Insert(int index, object panel)
			{
				string method_string = "IList.Insert(int,object) ";
				if (!(panel is StatusBarPanel)) {

					throw new ArgumentException(class_string + method_string + "panel is not a StatusBarPanel");
				}
				Insert(index, (StatusBarPanel) panel);
			}
			void IList.Remove(object panel)
			{
				string method_string = "IList.Remove(object) ";
				if (!(panel is StatusBarPanel)) {

					throw new ArgumentException(class_string + method_string + "panel is not a StatusBarPanel");
				}
				Remove((StatusBarPanel) panel);
			}

			
			//  --- Public Properties
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
				//get { return list.Count; }
			}
			public bool IsReadOnly {

				get { return false; }
			}
			object IList.this[int index] {

				get { return this[index]; }
				set { this[index]=(StatusBarPanel)value; }
			}
			public virtual StatusBarPanel this[int index] {

				get
				{
					// The same checks are done by the list, so this is redundant
					// This is left here in case you prefer better exception messages over performance
					//string method_string = "get_Item(int) ";
					//if (index < 0)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index < 0");
					//}
					//if (index >= Count)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index >= Count");
					//}
					return (StatusBarPanel)list[index];
				}
				set
				{
					// The same checks are done by the list, so this is redundant
					// This is left here in case you prefer better exception messages over performance
					//string method_string = "set_Item(int,StatusBarPanel) ";
					//if (index < 0)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index < 0");
					//}
					//if (index >= Count)
					//{
					//	throw new ArgumentOutOfRangeException(class_string + method_string + "index >= Count");
					//}
					//if (value == null)
					//{
					//	throw new ArgumentNullException(class_string + method_string + "panel == null");
					//}
					list[index] = value;
				}
			}
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}
			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			
			//  --- Private Properties
			
			private bool IsFixedSize { get { return false; } }

//			private object ILList.this[int index]
//			{
//				get { return (StatusBarPanel) this[index]; }
//				set
//				{
//					string method_string = "IList.set_Item(int,object) ";
//					if (!(value is StatusBarPanel))
//					{
//						throw new ArgumentException(class_string + method_string + "panel is not a StatusBarPanel");
//					}
//					this[index] = (StatusBarPanel) value;
//				}
//			}
		}
	}
}

