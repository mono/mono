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
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		int Count { get; }


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		IVsaItem this [int index] { get; }
	
		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		IVsaItem this [string name] { get; }


		// public methods

		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		IVsaItem CreateItem (string name, VsaItemType itemType, VsaItemFlag itemFlag);
	
		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void Remove (int index);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void Remove (string name);
	}
}			
