//
// Collection.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
using System.Collections;

namespace Microsoft.VisualBasic {
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Collection : System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList {
		// Declarations
		// Constructors
		// Properties
		System.Boolean IList.IsReadOnly { get {return false;} }
		private System.Object[] Items { get {return null;} }
		System.Int32 ICollection.Count { get {return 0;} }
		System.Boolean ICollection.IsSynchronized { get {return false;} }
		System.Object ICollection.SyncRoot { get {return null;} }
		System.Boolean IList.IsFixedSize { get {return false;} }
		public System.Int32 Count { get {return 0;} }
		[System.Runtime.CompilerServices.IndexerName("Item")] 
		public System.Object this [System.Int32 Index] { get {return null;} set {} }
		[System.Runtime.CompilerServices.IndexerName("Item")] 
		public System.Object this [System.Object Index] { get {return null;} }
		// Methods
		System.Int32 IList.IndexOf (System.Object value) { return 0;}
		System.Boolean IList.Contains (System.Object value) { return false;}
		void IList.Clear () { }
		void IList.Remove (System.Object value) { }
		void IList.RemoveAt (System.Int32 index) { }
		void IList.Insert (System.Int32 index, System.Object value) { }
		System.Int32 IList.Add (System.Object Item) { return 0;}
		void ICollection.CopyTo (System.Array array, System.Int32 index) { }
		private void IndexCheck (System.Int32 Index) { }
		private System.Boolean DataIsEqual (System.Object obj1, System.Object obj2) { return false;}
		public void Add (System.Object Item, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Key, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Before, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object After) { }
		public void Remove (System.String Key) { }
		public void Remove (System.Int32 Index) { }
		public System.Collections.IEnumerator GetEnumerator () { return null;}
		// Events
	};
}
