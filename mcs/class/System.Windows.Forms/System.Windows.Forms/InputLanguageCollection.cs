//
// System.Windows.Forms.InputLanguageCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class InputLanguageCollection : ReadOnlyCollectionBase {

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public InputLanguage this[int index] {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public bool Contains(InputLanguage lang)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo(InputLanguage[] array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		[MonoTODO]
		public int IndexOf(InputLanguage lang)
		{
			throw new NotImplementedException ();
		}
 		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
}
}
