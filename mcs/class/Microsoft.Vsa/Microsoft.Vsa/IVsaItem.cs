//
// IVsaItem.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaItem
	{
		// public properties

		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		bool IsDirty { get; }


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		VsaItemType ItemType { get; }

		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		string Name { 
			get;
			set;
		}


		// public methods

		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		object GetOption (string name);
				       
		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void SetOption (string name, object value);
	}
}
