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
		bool IsDirty { get; }


		//[Guid ("")]
		VsaItemType ItemType { get; }

		
		//[Guid ("")]
		string Name { 
			get;
			set;
		}


		// public methods

		//[Guid ("")]
		object GetOption (string name);
				       
		
		//[Guid ("")]
		void SetOption (string name, object value);
	}
}
