//
// System.CodeDom CodeTypeDeclarationCollection Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeDeclarationCollection 
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeTypeDeclarationCollection()
		{
		}

		public CodeTypeDeclarationCollection( CodeTypeDeclaration[] value )
		{
			AddRange( value );
		}

		public CodeTypeDeclarationCollection( CodeTypeDeclarationCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeTypeDeclaration this[int index]
		{
			get {
				return (CodeTypeDeclaration)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public void Add (CodeTypeDeclaration value)
		{
			List.Add (value);
		}

		public void AddRange (CodeTypeDeclaration [] value)
		{
			foreach (CodeTypeDeclaration ca in value) 
				Add( ca );
		}

		public void AddRange (CodeTypeDeclarationCollection value)
		{
			foreach (CodeTypeDeclaration ca in value) 
				Add( ca );
		}

		public bool Contains( CodeTypeDeclaration value )
		{
			return List.Contains( value );
		}

		public void CopyTo( CodeTypeDeclaration[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeTypeDeclaration value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeTypeDeclaration value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeTypeDeclaration value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
