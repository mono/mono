//
// System.CodeDom CodeNamespaceImportCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections;

namespace System.CodeDom
{
	/*
	 * Should probably be derived from CollectionBase like any
	 * other System.CodeDom.*Collection. MS docs say it currently
	 * is not, for whichever reason.
	 */
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeNamespaceImportCollection
		: IList, ICollection, IEnumerable
	{
		private ArrayList namespaceImports;

		//
		// Constructors
		//
		public CodeNamespaceImportCollection ()
		{
			namespaceImports = new ArrayList();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return namespaceImports.Count;
			}
		}

                public CodeNamespaceImport this [int index] {
                        get {
                                return (CodeNamespaceImport)namespaceImports[index];
                        }
			set {
				namespaceImports[index] = value;
			}
                }

		//
		// Methods
		//
		public void Add (CodeNamespaceImport value)
		{
			namespaceImports.Add (value);
		}

		public void AddRange (CodeNamespaceImport [] value)
		{
			foreach (CodeNamespaceImport elem in value) 
				namespaceImports.Add (elem);
		}

		public void Clear ()
		{
			namespaceImports.Clear ();
		}

		// IList implementation
		bool IList.IsFixedSize {
			get {
				return false;
			}
		}

		bool IList.IsReadOnly {
			get {
				return false;
			}
		}

		object IList.this[int index] {
			get {
				return namespaceImports[index];
			}
			set {
				namespaceImports[index] = value;
			}
		}

		int IList.Add( object value )
		{
			return namespaceImports.Add( value );
		}
		
		bool IList.Contains( object value )
		{
			return namespaceImports.Contains( value );
		}
		
		int IList.IndexOf( object value )
		{
			return namespaceImports.IndexOf( value );
		}

		void IList.Insert( int index, object value )
		{
			namespaceImports.Insert( index, value );
		}

		void IList.Remove( object value )
		{
			namespaceImports.Remove( value );
		}
		
		void IList.RemoveAt( int index )
		{
			namespaceImports.RemoveAt( index );
		}

		// ICollection implementation
		object ICollection.SyncRoot {
			get {
				return namespaceImports.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return namespaceImports.IsSynchronized;
			}
		}

		void ICollection.CopyTo( Array array, int index )
		{
			namespaceImports.CopyTo( array, index );
		}

		// IEnumerable implementation
		public IEnumerator GetEnumerator ()
		{
			return namespaceImports.GetEnumerator();
		}
	}
}
