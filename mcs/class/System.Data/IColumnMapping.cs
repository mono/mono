//
// System.Data.IColumnMapping.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Associates a data source column with a DataSet column, and is implemented by the DataColumnMapping class, which is used in common by .NET data providers.
	/// </summary>
	public interface IColumnMapping
	{
		string DataSetColumn
		{
			get
			{
			}
			set
			{
			}
		}

		string SourceColumn
		{
			get
			{
			}
			set
			{
			}
		}
	}
}