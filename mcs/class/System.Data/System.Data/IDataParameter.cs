//
// System.Data.IDataParameter.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents a parameter to a Command object, and optionally, its mapping to DataSet columns; and is implemented by .NET data providers that access data sources.
	/// </summary>
	public interface IDataParameter
	{
		
		DbType DbType
		{
			get
			{
			}
			set
			{
			}
		}

		ParameterDirection Direction
		{
			get
			{
			}
			set
			{
			}
		}

		bool IsNullable
		{
			get
			{
			}
		}

		string ParameterName
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

		DataRowVersion SourceVersion
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