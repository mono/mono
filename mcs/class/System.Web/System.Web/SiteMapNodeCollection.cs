//
// System.Web.SiteMapNodeCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
//  (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;

namespace System.Web {
	public class SiteMapNodeCollection : CollectionBase, IHierarchicalEnumerable {
		public SiteMapNodeCollection () {}
		public SiteMapNodeCollection (SiteMapNode value) { Add (value); }
		public SiteMapNodeCollection (SiteMapNode[] values) { AddRangeInternal (values); }
		public SiteMapNodeCollection (SiteMapNodeCollection values) { AddRangeInternal (values); }
		
		public virtual int Add (SiteMapNode value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return this.List.Add (value);
		}
		
		public virtual void AddRange (System.Web.SiteMapNode[] value)
		{
			this.OnAddRange (value);
			this.AddRangeInternal (value);
		}
		
		public virtual void AddRange (SiteMapNodeCollection value)
		{
			this.OnAddRange (value);
			this.AddRangeInternal (value);
		}
		
		private void AddRangeInternal (IList value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			this.InnerList.AddRange (value);
		}
		public bool Contains (SiteMapNode value)
		{
			return this.List.Contains (value);
		}
		
		public void CopyTo (System.Web.SiteMapNode[] array, int index)
		{
			this.List.CopyTo (array, index);
		}
		
//		public SiteMapDataSourceView GetDataSourceView ()
//		{
//			return new SiteMapDataSourceView (this);
//		}
		
		public int IndexOf (SiteMapNode value)
		{
			return this.List.IndexOf (value);
		}
		
		public virtual void Insert (int index, SiteMapNode value)
		{
			this.List.Insert (index, value);
		}
		
		protected virtual void OnAddRange (IList value)
		{
		}
		
		protected override void OnValidate (object value)
		{
			this.OnValidate (value);
			if (value as SiteMapNode == null)
				throw new ArgumentException ("Invalid type");
		}
		
		public static SiteMapNodeCollection ReadOnly (SiteMapNodeCollection collection)
		{
			return new ReadOnlySiteMapNodeCollection (collection);
		}
		
		public virtual void Remove (SiteMapNode value)
		{
			this.List.Remove (value);
		}
		
		IHierarchyData System.Web.UI.IHierarchicalEnumerable.GetHierarchyData (object enumeratedItem)
		{
			return enumeratedItem as IHierarchyData;
		}
		
		public virtual SiteMapNode this [int index] {
			get { return (SiteMapNode) this.List [index]; }
			set { this.List [index] = value; }
		}

		private class ReadOnlySiteMapNodeCollection : SiteMapNodeCollection {
			
			internal ReadOnlySiteMapNodeCollection (SiteMapNodeCollection collection) : base (collection) {}

			protected override void OnAddRange (IList value) { throw new NotSupportedException ("Readonly collection"); }
			protected override void OnClear () { throw new NotSupportedException ("Readonly collection"); }
			protected override void OnInsert (int index, object value) { throw new NotSupportedException ("Readonly collection"); }
			protected override void OnRemove (int index, object value) { throw new NotSupportedException ("Readonly collection"); }
			protected override void OnSet (int index, object oldValue, object newValue) { throw new NotSupportedException ("Readonly collection"); }
		}
		 
	}
}
#endif

