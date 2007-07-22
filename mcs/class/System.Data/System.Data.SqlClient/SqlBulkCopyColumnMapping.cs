//
// System.Data.SqlClient.SqlBulkCopyColumnMapping.cs
//
// Author:
//   Umadevi S <sumadevi@novell.com>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
namespace System.Data.SqlClient
{
	/// <summary>
	/// Class that defines the mapping between a column in the destination table and an
	/// column in the datasource of SqlBulkCopy's instance
	/// </summary>
	
	public sealed class SqlBulkCopyColumnMapping {

	#region Fields
	
	int sourceOrdinal = -1;
	int destinationOrdinal = -1;
	string sourceColumn = null;
	string destinationColumn = null;

	#endregion //Fields

	#region Constructors
	
	public SqlBulkCopyColumnMapping() {
	}
	
	public SqlBulkCopyColumnMapping(int sourceColumnOrdinal, int destinationOrdinal){
		SourceOrdinal = sourceColumnOrdinal;
		DestinationOrdinal = destinationOrdinal;
	}

	public SqlBulkCopyColumnMapping(int sourceColumnOrdinal, string destinationColumn){
		SourceOrdinal = sourceColumnOrdinal;
		DestinationColumn = destinationColumn;	
	}

	public SqlBulkCopyColumnMapping(string sourceColumn, int destinationOrdinal){
		SourceColumn = sourceColumn;		
		DestinationOrdinal = destinationOrdinal;
	}

	public SqlBulkCopyColumnMapping(string sourceColumn, string destinationColumn){
		SourceColumn = sourceColumn;
		DestinationColumn = destinationColumn;
	}

	# endregion //Constructors	

	# region Properties
	
	public String DestinationColumn {
		get {
			if (this.destinationColumn != null)
				return destinationColumn;
			else
				return string.Empty; //ms:doesnot return null.
		}
		set {
			// ms: whenever the name is set the ordinal is reset to -1
			this.destinationOrdinal = -1;
			this.destinationColumn = value;
		}
	}
	
	public String SourceColumn {
                get {
                        if (this.sourceColumn != null)
                                return sourceColumn;
                        else
                                return string.Empty;//ms doesnot return null
                }
                set {
                        // ms: whenever the name is set the ordinal is reset to -1
                        this.sourceOrdinal = -1;
                        this.sourceColumn = value;
                }
        }

	public int DestinationOrdinal {
                get {
                         return this.destinationOrdinal;
                }
                set {
                        // ms: whenever the ordinal is set, the name is null
                        if (value < 0)
				throw new IndexOutOfRangeException ();
                        this.destinationColumn = null;
			this.destinationOrdinal =  value;
                }
        }
	
	public int SourceOrdinal {
                get {
                         return this.sourceOrdinal;
                }
                set {
                        // ms: whenever the ordinal is set, the name is null
                        if (value < 0)
                                throw new IndexOutOfRangeException ();
                        this.sourceColumn = null;
                        this.sourceOrdinal =  value;
                }
        }

	#endregion //Properties	

	}
}

#endif
