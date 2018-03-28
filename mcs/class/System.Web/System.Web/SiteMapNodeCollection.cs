//
// System.Web.SiteMapNodeCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
//  (C) 2003 Ben Maurer
//  (C) 2005-2009 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web
{
	public class SiteMapNodeCollection : IList, IHierarchicalEnumerable
	{
		ArrayList list;
		internal static SiteMapNodeCollection EmptyList;
		
		static SiteMapNodeCollection ()
		{
			EmptyList = new SiteMapNodeCollection ();
			EmptyList.list = ArrayList.ReadOnly (new ArrayList ());
		}
		
		public SiteMapNodeCollection ()
		{
		}
		
		public SiteMapNodeCollection (int capacity)
		{
			list = new ArrayList (capacity);
		}
		
		public SiteMapNodeCollection (SiteMapNode value)
		{
			Add (value);
		}
		
		public SiteMapNodeCollection (SiteMapNode[] value)
		{
			AddRangeInternal (value);
		}
		
		public SiteMapNodeCollection (SiteMapNodeCollection value)
		{
			AddRangeInternal (value);
		}
		
		internal static SiteMapNodeCollection EmptyCollection {
			get { return EmptyList; }
		}
		
		ArrayList List {
			get {
				if (list == null) list = new ArrayList ();
				return list;
			}
		}
		
		public virtual int Count {
			get { return list == null ? 0 : list.Count; }
		}
		
		public virtual bool IsSynchronized {
			get { return false; }
		}
		
		public virtual object SyncRoot {
			get { return this; }
		}
		
		public virtual IEnumerator GetEnumerator ()
		{
			return list != null ? list.GetEnumerator () : Type.EmptyTypes.GetEnumerator ();
		}
		
		public virtual void Clear ()
		{
			if (list != null) list.Clear ();
		}
		
		public virtual void RemoveAt (int index)
		{
			List.RemoveAt (index);
		}
		
		public virtual int Add (SiteMapNode value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return this.List.Add (value);
		}
		
		public virtual void AddRange (System.Web.SiteMapNode[] value)
		{
			this.AddRangeInternal (value);
		}
		
		public virtual void AddRange (SiteMapNodeCollection value)
		{
			this.AddRangeInternal (value);
		}
		
		internal virtual void AddRangeInternal (IList value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			List.AddRange (value);
		}

		public virtual bool Contains (SiteMapNode value)
		{
			return this.List.Contains (value);
		}
		
		public virtual void CopyTo (System.Web.SiteMapNode[] array, int index)
		{
			this.List.CopyTo (array, index);
		}
		
		public virtual int IndexOf (SiteMapNode value)
		{
			return this.List.IndexOf (value);
		}
		
		public virtual void Insert (int index, SiteMapNode value)
		{
			this.List.Insert (index, value);
		}
		
		protected virtual void OnValidate (object value)
		{
			if (!(value is SiteMapNode))
				throw new ArgumentException ("Invalid type");
		}

		public static SiteMapNodeCollection ReadOnly (SiteMapNodeCollection collection)
		{
			SiteMapNodeCollection col = new SiteMapNodeCollection ();
			if (collection.list != null)
				col.list = ArrayList.ReadOnly (collection.list);
			else
				col.list = ArrayList.ReadOnly (new ArrayList ());
			return col;
		}
		
		public virtual void Remove (SiteMapNode value)
		{
			this.List.Remove (value);
		}
		
		public virtual IHierarchyData GetHierarchyData (object enumeratedItem)
		{
			return enumeratedItem as IHierarchyData;
		}
		
		public SiteMapDataSourceView GetDataSourceView (SiteMapDataSource owner, string viewName)
		{
			return new SiteMapDataSourceView (owner, viewName, this);
		}
		
		public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView ()
		{
			return new SiteMapHierarchicalDataSourceView (this);
		}
		
		public virtual SiteMapNode this [int index] {
			get { return (SiteMapNode) this.List [index]; }
			set { this.List [index] = value; }
		}
		
		public virtual bool IsFixedSize {
			get { return List.IsFixedSize; }
		}

		public virtual bool IsReadOnly {
			get { return list != null && list.IsReadOnly; }
		}

		#region IList Members

		object IList.this [int index] {
			get { return List [index]; }
			set { OnValidate (value); List [index] = value; }
		}
		
		int IList.Add (object value)
		{
			OnValidate (value);
			return List.Add (value);
		}
		
		bool IList.Contains (object value)
		{
			return List.Contains (value);
		}
		
		int IList.IndexOf (object value)
		{
			return List.IndexOf (value);
		}
		
		void IList.Insert (int index, object value)
		{
			OnValidate (value);
			List.Insert (index, value);
		}
		
		void IList.Remove (object value)
		{
			OnValidate (value);
			List.Remove (value);
		}
		
		void ICollection.CopyTo (Array array, int index)
		{
			List.CopyTo (array, index);
		}

		void IList.Clear () {
			Clear ();
		}

		bool IList.IsFixedSize {
			get { return IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return IsReadOnly; }
		}

		void IList.RemoveAt (int index) {
			RemoveAt (index);
		}

		#endregion

		#region ICollection Members


		int ICollection.Count {
			get { return Count; }
		}

		bool ICollection.IsSynchronized {
			get { return IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}

		#endregion

		#region IHierarchicalEnumerable Members

		IHierarchyData IHierarchicalEnumerable.GetHierarchyData (object enumeratedItem) {
			return GetHierarchyData (enumeratedItem);
		}

		#endregion
	}
}


