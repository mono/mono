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
		/// <summary>
		/// Gets or sets the name of the column within the DataSet to map to.
		/// </summary>
		string DataSetColumn
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the column within the data source to map from. The name is case-sensitive.
		/// </summary>
		string SourceColumn
		{
			get;
			set;
		}
	}
}