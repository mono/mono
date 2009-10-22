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
		private Hashtable keys;
		private ArrayList data;

		//
		// Constructors
		//
		public CodeNamespaceImportCollection ()
		{
			data = new ArrayList ();
			keys = new Hashtable (CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
		}

		//
		// Properties
		//
		int ICollection.Count {
			get {
				return data.Count;
			}
		}
		
		public int Count {
			get {
				return data.Count;
			}
		}

		public CodeNamespaceImport this [int index] {
			get {
				return (CodeNamespaceImport)data[index];
			}
			set {
				CodeNamespaceImport oldImport = (CodeNamespaceImport) data [index];
				CodeNamespaceImport newImport = (CodeNamespaceImport) value;
				keys.Remove (oldImport.Namespace);
				data[index] = value;
				keys [newImport.Namespace] = newImport;
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

			if (!keys.ContainsKey (value.Namespace)) {
				keys [value.Namespace] = value;
				data.Add (value);
			}
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

		void IList.Clear ()
		{
			Clear ();
		}
		
		public void Clear ()
		{
			data.Clear ();
			keys.Clear ();
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
				return data[index];
			}
			set {
				this [index] = (CodeNamespaceImport) value;
			}
		}

		int IList.Add( object value )
		{
			Add ((CodeNamespaceImport) value);
			return data.Count - 1;
		}
		
		bool IList.Contains( object value )
		{
			return data.Contains( value );
		}
		
		int IList.IndexOf( object value )
		{
			return data.IndexOf( value );
		}

		void IList.Insert( int index, object value )
		{
			data.Insert( index, value );
			CodeNamespaceImport import = (CodeNamespaceImport) value;
			keys [import.Namespace] = import;
		}

		void IList.Remove( object value )
		{
			string ns = ((CodeNamespaceImport)value).Namespace;
			data.Remove( value );
			foreach (CodeNamespaceImport import in data) {
				if (import.Namespace == ns) {
					keys [ns] = import;
					return;
				}
			}
			keys.Remove (ns);
		}
		
		void IList.RemoveAt( int index )
		{
			string ns = this [index].Namespace;
			data.RemoveAt( index );
			foreach (CodeNamespaceImport import in data) {
				if (import.Namespace == ns) {
					keys [ns] = import;
					return;
				}
			}
			keys.Remove (ns);
		}

		// ICollection implementation
		object ICollection.SyncRoot {
			get {
				return null;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return data.IsSynchronized;
			}
		}

		void ICollection.CopyTo( Array array, int index )
		{
			data.CopyTo( array, index );
		}

		// IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return data.GetEnumerator();
		}
		
		// IEnumerable implementation
		public IEnumerator GetEnumerator ()
		{
			return data.GetEnumerator();
		}
	}
}
