/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridColumnCollection
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class DataGridColumnCollection : ICollection, IEnumerable, IStateManager
	{
		private DataGrid  owner;
		private ArrayList columns;
		private bool      trackViewState = false;

		public DataGridColumnCollection(DataGrid owner, ArrayList columns)
		{
			this.owner   = owner;
			this.columns = columns;
		}

		public int Count
		{
			get
			{
				return columns.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public DataGridColumn this[int index]
		{
			get
			{
				return (DataGridColumn)(columns[index]);
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public void Add(DataGridColumn column)
		{
			AddAt(-1, column);
		}

		public void AddAt(int index, DataGridColumn column)
		{
			if(index == -1)
			{
				columns.Add(column);
			} else
			{
				columns.Insert(index, column);
			}

			column.SetOwner (owner);
			if(trackViewState)
			{
				((IStateManager)column).TrackViewState();
			}
			OnColumnsChanged();
		}

		internal void OnColumnsChanged()
		{
			if(owner != null)
			{
				owner.OnColumnsChanged();
			}
		}

		public void Clear()
		{
			columns.Clear();
			OnColumnsChanged();
		}

		public void CopyTo(Array array, int index)
		{
			foreach(DataGridColumn current in this)
			{
				array.SetValue(current, index++);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return columns.GetEnumerator();
		}

		public int IndexOf(DataGridColumn column)
		{
			if(column != null)
			{
				return columns.IndexOf(column);
			}
			return -1;
		}

		public void Remove(DataGridColumn column)
		{
			if(column != null)
			{
				RemoveAt(IndexOf(column));
			}
		}

		public void RemoveAt(int index)
		{
			if(index >= 0 && index < columns.Count)
			{
				columns.RemoveAt(index);
				OnColumnsChanged();
			}
			//This exception is not documented, but thrown
			throw new ArgumentOutOfRangeException("string");
		}

		object IStateManager.SaveViewState()
		{
			ArrayList retVal = new ArrayList(columns.Count);
			foreach(DataGridColumn current in this)
			{
				retVal.Add(((IStateManager)current).SaveViewState());
			}
			return retVal;
		}

		void IStateManager.LoadViewState(object savedState)
		{
			if(savedState != null && savedState is ArrayList)
			{
				int currentIndex = 0;
				foreach(DataGridColumn current in (ArrayList)savedState)
				{
					((IStateManager)columns[currentIndex ++]).LoadViewState(current);
				}
			}
		}

		void IStateManager.TrackViewState()
		{
			trackViewState = true;
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return trackViewState;
			}
		}
	}
}
