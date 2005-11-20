//
// System.CodeDom CodeNamespaceImportCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
			if (value == null) {
				throw new NullReferenceException ();
			}

			// perform case-insensitive check to see if the namespace of the 
			// entry to add is not already in the collection
			foreach (CodeNamespaceImport import in this) {
				if (string.Compare(import.Namespace, value.Namespace, true) == 0) {
					// skip duplicate namespaces
					return;
				}
			}

			namespaceImports.Add (value);
		}

		public void AddRange (CodeNamespaceImport [] value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			foreach (CodeNamespaceImport elem in value) {
				Add (elem);
			}
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
				return null;
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
