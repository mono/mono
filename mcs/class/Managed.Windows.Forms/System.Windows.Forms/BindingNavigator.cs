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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisibleAttribute(true)]
	[ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
	public class BindingNavigator : ToolStrip, ISupportInitialize
	{
		public BindingNavigator()
			: this(false)
		{
		}

		public BindingNavigator(BindingSource bindingSource)
		{
			AttachNewSource(bindingSource);
			this.AddStandardItems();
		}

		private void AttachNewSource(BindingSource bindingSource)
		{
			if (this.bindingSource != null)
			{
				bindingSource.ListChanged -= new ListChangedEventHandler(OnListChanged);
				bindingSource.PositionChanged -= new EventHandler(OnPositionChanged);
				bindingSource.AddingNew -= new AddingNewEventHandler(OnAddingNew);
			}

			this.bindingSource = bindingSource;
			bindingSource.ListChanged += new ListChangedEventHandler(OnListChanged);
			bindingSource.PositionChanged += new EventHandler(OnPositionChanged);
			bindingSource.AddingNew += new AddingNewEventHandler(OnAddingNew);
		}

		void OnAddingNew(object sender, AddingNewEventArgs e)
		{
			OnRefreshItems();
		}

		void OnPositionChanged(object sender, EventArgs e)
		{
			OnRefreshItems();
		}

		private void OnListChanged(object sender, ListChangedEventArgs e)
		{
			OnRefreshItems();
		}

		public BindingNavigator(bool addStandardItems)
			: base()
		{
			this.bindingSource = null;
			if (addStandardItems)
				this.AddStandardItems();
		}

		public BindingNavigator(IContainer container)
		{
			container.Add(this);
			bindingSource = null;
		}

		#region private fields

		private ToolStripItem addNewItem = null;
		private BindingSource bindingSource = null;
		private bool changingText = false;
		private ToolStripItem countItem = null;
		private string countItemFormat = Locale.GetText("of {0}");
		private ToolStripItem deleteItem = null;
		private ToolStripItem moveFirstItem = null;
		private ToolStripItem moveLastItem = null;
		private ToolStripItem moveNextItem = null;
		private ToolStripItem movePreviousItem = null;
		private ToolStripItem positionItem = null;
		private bool initFlag = false;
		#endregion

		#region Public Properties

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem AddNewItem
		{
			get { return addNewItem; }
			set
			{
				if (addNewItem != null)
					addNewItem.Click -= new EventHandler(OnAddNew);
				value.Click += new EventHandler(OnAddNew);
				addNewItem = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public BindingSource BindingSource
		{
			get { return bindingSource; }
			set
			{
				bindingSource = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem CountItem
		{
			get { return countItem; }
			set
			{
				countItem = value;
				this.OnRefreshItems();
			}
		}

		public string CountItemFormat
		{
			get { return countItemFormat; }
			set
			{
				countItemFormat = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem DeleteItem
		{
			get { return deleteItem; }
			set
			{
				deleteItem.Click -= new EventHandler(OnDelete);
				value.Click += new EventHandler(OnDelete);
				deleteItem = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MoveFirstItem
		{
			get { return moveFirstItem; }
			set
			{
				moveFirstItem.Click -= new EventHandler(OnMoveFirst);
				value.Click += new EventHandler(OnMoveFirst);
				moveFirstItem = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MoveLastItem
		{
			get { return moveLastItem; }
			set
			{
				moveLastItem.Click -= new EventHandler(OnMoveLast);
				value.Click += new EventHandler(OnMoveLast);
				moveLastItem = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MoveNextItem
		{
			get { return moveNextItem; }
			set
			{
				moveNextItem.Click -= new EventHandler(OnMoveNext);
				value.Click += new EventHandler(OnMoveNext);
				moveNextItem = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem MovePreviousItem
		{
			get { return movePreviousItem; }
			set
			{
				movePreviousItem.Click -= new EventHandler(OnMovePrevious);
				value.Click += new EventHandler(OnMovePrevious);
				movePreviousItem = value;
				this.OnRefreshItems();
			}
		}

		[TypeConverter(typeof(ReferenceConverter))]
		public ToolStripItem PositionItem
		{
			get { return positionItem; }
			set
			{
				positionItem = value;
				this.OnRefreshItems();
			}
		}

		#endregion

		#region public Events

		public event EventHandler RefreshItems;

		#endregion

		#region public and protected Methodes

		public virtual void AddStandardItems()
		{
			moveFirstItem = new ToolStripButton();
			moveFirstItem.ToolTipText = Locale.GetText("Move first");
			moveFirstItem.Click += new EventHandler(OnMoveFirst);
			Items.Add(moveFirstItem);

			movePreviousItem = new ToolStripButton();
			movePreviousItem.ToolTipText = Locale.GetText("Move previous");
			movePreviousItem.Click += new EventHandler(OnMovePrevious);
			Items.Add(movePreviousItem);

			Items.Add(new ToolStripSeparator());

			positionItem = new ToolStripTextBox();
			positionItem.Text = (bindingSource == null ? 0 : 1).ToString();
			positionItem.ToolTipText = Locale.GetText("Current position");
			positionItem.TextChanged += new EventHandler(OnPositionTextChanged);
			Items.Add(positionItem);

			countItem = new ToolStripLabel();
			countItem.ToolTipText = Locale.GetText("Total number of items");
			countItem.Text = Locale.GetText(countItemFormat, bindingSource == null ? 0 : bindingSource.Count);
			Items.Add(countItem);

			Items.Add(new ToolStripSeparator());

			moveNextItem = new ToolStripButton();
			moveNextItem.ToolTipText = Locale.GetText("Move next");
			moveNextItem.Click += new EventHandler(OnMoveNext);
			Items.Add(moveNextItem);

			moveLastItem = new ToolStripButton();
			moveLastItem.ToolTipText = Locale.GetText("Move last");
			moveLastItem.Click += new EventHandler(OnMoveLast);
			Items.Add(moveLastItem);

			Items.Add(new ToolStripSeparator());

			addNewItem = new ToolStripButton();
			addNewItem.ToolTipText = Locale.GetText("Add new");
			addNewItem.Click += new EventHandler(OnAddNew);
			Items.Add(addNewItem);

			deleteItem = new ToolStripButton();
			deleteItem.ToolTipText = Locale.GetText("Delete");
			deleteItem.Click += new EventHandler(OnDelete);
			Items.Add(deleteItem);

			OnRefreshItems();
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

		protected virtual void RefreshItemsCore()
		{
			try
			{
				bool isNull = (bindingSource == null);
				initFlag = true;
				changingText = true;

				if (addNewItem != null)
					addNewItem.Enabled = isNull ? addNewItem.Enabled : false;

				if (moveFirstItem != null)
					moveFirstItem.Enabled = isNull ? moveFirstItem.Enabled : bindingSource.Position != 0;

				if (moveLastItem != null)
					moveLastItem.Enabled = isNull ? moveLastItem.Enabled : bindingSource.Position < (bindingSource.Count - 1);

				if (moveNextItem != null)
					moveNextItem.Enabled = isNull ? moveNextItem.Enabled : bindingSource.Position < (bindingSource.Count - 1);

				if (movePreviousItem != null)
					movePreviousItem.Enabled = isNull ? movePreviousItem.Enabled : bindingSource.Position > 0;

				if (deleteItem != null)
					deleteItem.Enabled = isNull ? deleteItem.Enabled : bindingSource.Count != 0;

				if (countItem != null)
					countItem.Text = string.Format(countItemFormat, isNull ? 0 : bindingSource.Count);

				if (positionItem != null)
					positionItem.Text = string.Format("{0}", isNull ? 0 : bindingSource.Position + 1);
			}
			finally
			{
				changingText = false;
				initFlag = false;
			}
		}

		[MonoTODO("implement this")]
		public bool Validate()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region private methode

		private void OnAddNew(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.AddNew();

			OnRefreshItems();
		}

		private void OnDelete(object sender, EventArgs e)
		{
			if (bindingSource != null)
				bindingSource.RemoveCurrent();

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

		private void OnPositionTextChanged(object sender, EventArgs e)
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
		}

		#endregion
	}
}

#endif