//
// System.ComponentModel.ISupportInitialize.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.ComponentModel
{
	/// <summary>
	/// Specifies that this object supports a simple, transacted notification for batch initialization.
	/// </summary>
	public interface ISupportInitialize
	{
		void BeginInit ();

		void EndInit ();
	}
		
}
	
