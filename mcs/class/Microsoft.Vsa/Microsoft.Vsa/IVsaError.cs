//
// IVsaError.cs: 
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaError
	{
		// public properties
		

		//[Guid ("")]
		string Description { get; }

	
		//[Guid ("")]
		int EndColumn { get; }


		//[Guid ("")]
		int Line { get; }


		//[Guid ("")]
		string LineText { get; }
	

		//[Guid ("")]
		int Number { get; }


		//[Guid ("")]
		int Severity { get; }
	

		//[Guid ("")]
		IVsaItem SourceItem { get; }


		//[Guid ("")]
		string SourceMoniker { get; }


		//[Guid ("")]
		int StartColumn { get; }		
	}
}
		
