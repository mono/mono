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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Olivier Dufour	olivier.duff@free.fr
//  Alan McGovern alan.mcgovern@gmail.com
//

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisibleAttribute(true)]
	[DefaultEvent ("RefreshItems")]
	[DefaultProperty ("BindingSource")]
	[ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
	[Designer ("System.Windows.Forms.Design.BindingNavigatorDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class BindingNavigator : ToolStrip, ISupportInitialize
	{
		#region Private Fields

		private ToolStripItem addNewItem = null;
		private BindingSource bindingSource = null;
		//private bool changingText = false;
		private ToolStripItem countItem = null;
		private string countItemFormat = Locale.GetText("of {0}");
		private ToolStripItem deleteItem = null;
		private bool initFlag = false;
		private ToolStripItem moveFirstItem = null;
		private ToolStripItem moveLastItem = null;
		private ToolStripItem moveNextItem = null;
		private ToolStripItem movePreviousItem = null;
		private ToolStripItem positionItem = null;

		#endregion Private Fields


		#region Public Properties

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem AddNewItem
		{
			get { return addNewItem; }
			set
			{
				ReplaceItem(ref addNewItem, value, new EventHandler(OnAddNew));
				OnRefreshItems();
			}
		}

		[DefaultValue (null)]
		[TypeConverter(typeof(ReferenceConverter))]
		public BindingSource BindingSource
		{
			get { return bindingSource; }
			set
			{
				AttachNewSource(value);
				OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem CountItem
		{
			get { return countItem; }
			set
			{
				countItem = value;
				OnRefreshItems();
			}
		}

		public string CountItemFormat
		{
			get { return countItemFormat; }
			set
			{
				countItemFormat = value;
				OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem DeleteItem
		{
			get { return deleteItem; }
			set
			{
				ReplaceItem(ref deleteItem, value, new EventHandler(OnDelete));
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MoveFirstItem
		{
			get { return moveFirstItem; }
			set
			{
				ReplaceItem(ref moveFirstItem, value, new EventHandler(OnMoveFirst));
				OnRefreshItems();
			}
		}

		private void ReplaceItem(ref ToolStripItem existingItem, ToolStripItem newItem, EventHandler clickHandler)
		{
			if (existingItem != null)
				existingItem.Click -= clickHandler;
			if (newItem != null)
				newItem.Click += clickHandler;

			existingItem = newItem;
		}


		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MoveLastItem
		{
			get { return moveLastItem; }
			set
			{
				ReplaceItem(ref moveLastItem, value, new EventHandler(OnMoveLast));
				OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MoveNextItem
		{
			get { return moveNextItem; }
			set
			{
				ReplaceItem(ref moveNextItem, value, new EventHandler(OnMoveNext));
				OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MovePreviousItem
		{
			get { return movePreviousItem; }
			set
			{
				ReplaceItem(ref movePreviousItem, value, new EventHandler(OnMovePrevious));
				OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem PositionItem
		{
			get { return positionItem; }
			set
			{
				positionItem = value;
				OnRefreshItems();
			}
		}

		#endregion


		#region Constructors

		[EditorBrowsable (EditorBrowsableState.Never)]
		public BindingNavigator ()
			: this(false)
		{
		}

		public BindingNavigator(BindingSource bindingSource)
			:base()
		{
			AttachNewSource(bindingSource);
			this.AddStandardItems();
		}


		public BindingNavigator(bool addStandardItems)
			: base()
		{
			bindingSource = null;
			if (addStandardItems)
				this.AddStandardItems();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public BindingNavigator(IContainer container)
			:base()
		{
			bindingSource = null;
			container.Add(this);
		}

		#endregion Constructors


		#region Public Events

		public event EventHandler RefreshItems;

		#endregion


		#region Public And Protected Methods

		public virtual void AddStandardItems()
		{
			BeginInit();

			MoveFirstItem = new ToolStripButton();
			moveFirstItem.Image = ResourceImageLoader.Get("nav_first.png");
			moveFirstItem.ToolTipText = Locale.GetText("Move first");
			Items.Add(moveFirstItem);

			MovePreviousItem = new ToolStripButton();
			movePreviousItem.Image = ResourceImageLoader.Get("nav_previous.png");
			movePreviousItem.ToolTipText = Locale.GetText("Move previous");
			Items.Add(movePreviousItem);

			Items.Add(new ToolStripSeparator());

			PositionItem = new ToolStripTextBox();
			positionItem.Width = 50;
			positionItem.Text = (bindingSource == null ? 0 : 1).ToString();
			positionItem.Width = 50;
			positionItem.ToolTipText = Locale.GetText("Current position");
			Items.Add(positionItem);

			CountItem = new ToolStripLabel();
			countItem.ToolTipText = Locale.GetText("Total number of items");
			countItem.Text = Locale.GetText(countItemFormat, bindingSource == null ? 0 : bindingSource.Count);
			Items.Add(countItem);

			Items.Add(new ToolStripSeparator());

			MoveNextItem = new ToolStripButton();
			moveNextItem.Image = ResourceImageLoader.Get("nav_next.png");
			moveNextItem.ToolTipText = Locale.GetText("Move next");
			Items.Add(moveNextItem);

			MoveLastItem = new ToolStripButton();
			moveLastItem.Image = ResourceImageLoader.Get("nav_end.png");
			moveLastItem.ToolTipText = Locale.GetText("Move last");
			Items.Add(moveLastItem);

			Items.Add(new ToolStripSeparator());

			AddNewItem = new ToolStripButton();
			addNewItem.Image = ResourceImageLoader.Get("nav_plus.png");
			addNewItem.ToolTipText = Locale.GetText("Add new");
			Items.Add(addNewItem);

			DeleteItem = new ToolStripButton();
			deleteItem.Image = ResourceImageLoader.Get("nav_delete.png");
			deleteItem.ToolTipText = Locale.GetText("Delete");
			Items.Add(deleteItem);

			EndInit();
		}

		public void BeginInit()
		{
			initFlag = true;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void EndInit()
		{
			initFlag = false;
			OnRefreshItems();
		}

		protected virtual void OnRefreshItems()
		{
			if (initFlag)
				return;

			if (this.RefreshItems != null)
				this.RefreshItems(this, EventArgs.Empty);

			this.RefreshItemsCore();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void RefreshItemsCore ()
		{
			try
			{
				bool is_source_available = bindingSource != null;
				initFlag = true;
				//changingText = true;

				if (addNewItem != null)
					addNewItem.Enabled = is_source_available && bindingSource.AllowNew;

				if (moveFirstItem != null)
					moveFirstItem.Enabled = is_source_available && bindingSource.Position > 0;

				if (moveLastItem != null)
					moveLastItem.Enabled = is_source_available && bindingSource.Position < (bindingSource.Count - 1);

				if (moveNextItem != null)
					moveNextItem.Enabled = is_source_available && bindingSource.Position < (bindingSource.Count - 1);

				if (movePreviousItem != null)
					movePreviousItem.Enabled = is_source_available && bindingSource.Position > 0;

				if (deleteItem != null)
					deleteItem.Enabled = is_source_available && (bindingSource.Count != 0 && bindingSource.AllowRemove);

				if (countItem != null) {
					countItem.Text = string.Format(countItemFormat, is_source_available ? bindingSource.Count : 0);
					countItem.Enabled = is_source_available && bindingSource.Count > 0;
				}

				if (positionItem != null) {
					positionItem.Text = string.Format("{0}", is_source_available? bindingSource.Position + 1 : 0);
					positionItem.Enabled = is_source_available && bindingSource.Count > 0;
				}
			}
			finally
			{
				//changingText = false;
				initFlag = false;
			}
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		public bool Validate ()
		{
			throw new NotImplementedException();
		}

		#endregion


		#region Private Methode

		private void AttachNewSource(BindingSource source)
		{
			if (bindingSource != null)
			{
				bindingSource.ListChanged -= new ListChangedEventHandler(OnListChanged);
				bindingSource.PositionChanged -= new EventHandler(OnPositionChanged);
				bindingSource.AddingNew -= new AddingNewEventHandler(OnAddingNew);
			}

			bindingSource = source;

			if (bindingSource != null)
			{
				bindingSource.ListChanged += new ListChangedEventHandler(OnListChanged);
				bindingSource.PositionChanged += new EventHandler(OnPositionChanged);
				bindingSource.AddingNew += new AddingNewEventHandler(OnAddingNew);
			}
		}

		private void OnAddNew(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.AddNew();

			OnRefreshItems();
		}

		private void OnAddingNew(object sender, AddingNewEventArgs e)
		{
			OnRefreshItems();
		}

		private void OnDelete(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.RemoveCurrent();

			OnRefreshItems();
		}

		private void OnListChanged(object sender, ListChangedEventArgs e)
		{
			OnRefreshItems();
		}

		private void OnMoveFirst(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.MoveFirst();

			OnRefreshItems();
		}

		private void OnMoveLast(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.MoveLast();

			OnRefreshItems();
		}

		private void OnMoveNext(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.MoveNext();

			OnRefreshItems();
		}

		private void OnMovePrevious(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.MovePrevious();

			OnRefreshItems();
		}

		private void OnPositionChanged(object sender, EventArgs e)
		{
			OnRefreshItems();
		}

		/*private void OnPositionTextChanged(object sender, EventArgs e)
		{
			if (changingText)
				return;

			try
			{
				changingText = true;

				int position;
				ToolStripTextBox txt = sender as ToolStripTextBox;

				if (txt == null)
					return;

				if (!int.TryParse(txt.Text, out position))
				{
					txt.Text = (bindingSource == null ? 0 : bindingSource.Position).ToString();
				}
				else
				{
					if (position < 0)
						position = 1;

					if (position > bindingSource.Count)
						position = bindingSource.Count;

					bindingSource.Position = position;
				}
			}
			finally
			{
				changingText = false;
				OnRefreshItems();
			}
		}*/

		#endregion
	}
}
