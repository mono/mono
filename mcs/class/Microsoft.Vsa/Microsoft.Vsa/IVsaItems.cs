//
// IVsaItems.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;
	using System.Collections;

	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaItems : IEnumerable
	{
		// public properties
	
		//[Guid ("")]
		int Count { get; }


		//[Guid ("")]
		IVsaItem this [int index] { get; }
	
		
		//[Guid ("")]
		IVsaItem this [string name] { get; }


		// public methods

		//[Guid ("")]
		IVsaItem CreateItem (string name, VsaItemType itemType, VsaItemFlag itemFlag);
	
		
		//[Guid ("")]
		void Remove (int index);


		//[Guid ("")]
		void Remove (string name);
	}
}			
