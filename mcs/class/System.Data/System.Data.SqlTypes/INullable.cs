//
// System.Data.SqlTypes.INullable
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Data.SqlTypes
{
	/// <summary>
	/// All of the System.Data.SqlTypes objects and structures implement the INullable interface, 
	/// reflecting the fact that, unlike the corresponding system types, SqlTypes can legally contain the value null.
	/// </summary>
	public interface INullable
	{
		bool IsNull {
			get;
		}
	}
}
