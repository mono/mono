//
// System.CodeDom CodeCatchClauseCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeCatchClauseCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeCatchClauseCollection()
		{
		}

		public CodeCatchClauseCollection( CodeCatchClause[] value )
		{
			AddRange( value );
		}

		public CodeCatchClauseCollection( CodeCatchClauseCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
                public CodeCatchClause this[int index] {
                        get {
                                return (CodeCatchClause)List[index];
                        }
			set {
				List[index] = value;
			}
                }

		//
		// Methods
		//
		public int Add (CodeCatchClause value)
		{
			return List.Add (value);
		}

		public void AddRange (CodeCatchClause [] value)
		{
			foreach (CodeCatchClause ca in value) 
				Add (ca);
		}

		public void AddRange (CodeCatchClauseCollection value )
		{
			foreach (CodeCatchClause ca in value)
				Add (ca);
		}

		public bool Contains( CodeCatchClause value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeCatchClause[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeCatchClause value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeCatchClause value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeCatchClause value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
