//
// System.Data.ProviderBase.DbMetaDataFactory
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

using System.Data.Common;
using System.IO;

namespace System.Data.ProviderBase {
	public class DbMetaDataFactory 
	{
		#region Fields

		Stream xmlStream;
		string serverVersion;
		string serverVersionNormalized;
		
		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DbMetaDataFactory (Stream XmlStream, string serverVersion, string serverVersionNormalized)
		{
			this.xmlStream = XmlStream;
			this.serverVersion = serverVersion;
			this.serverVersionNormalized = serverVersionNormalized;
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected DataSet CollectionDataSet {
			get { throw new NotImplementedException (); }
		}

		protected string ServerVersion {
			get { return serverVersion; }
		}

		protected string ServerVersionNormalized {
			get { return serverVersionNormalized; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected DataTable CloneAndFilterCollection (string collectionName, string[] hiddenColumnNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema (DbConnection connection, DbConnectionInternal internalConnection, string collectionName, string[] restrictions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable PrepareCollection (string collectionName, string[] restrictions, DbConnection connection, DbConnectionInternal internalConnection)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		public class CollectionNames
		{
			#region Fields

			public const string DataSourceInformation = "DataSourceInformation";
			public const string DataTypes = "DataTypes";
			public const string MetaDataCollections = "MetaDataCollections";
			public const string ReservedWords = "ReservedWords";
			public const string Restrictions = "Restrictions";

			#endregion // Fields

			#region Constructors
	
			[MonoTODO]
			protected CollectionNames ()
			{
			}

			#endregion // Constructors
		}

		public class ColumnNames
		{
			#region Fields

			public const string CollectionName = "CollectionName";
			public const string ColumnSize = "ColumnSize";
			public const string CompositeIdentifierSeparatorPattern = "CompositeIdentifierSeparatorPattern";
			public const string CreateFormat = "CreateFormat";
			public const string CreateParameters = "CreateParameters";
			public const string DataSourceProductName = "DataSourceProductName";
			public const string DataSourceProductVersion = "DataSourceProductVersion";
			public const string DataSourceProductVersionNormalized = "DataSourceProductVersionNormalized";
			public const string DataType = "DataType";
			public const string GroupByBehavior = "GroupByBehavior";
			public const string IdentifierCase = "IdentifierCase";
			public const string IdentifierPattern = "IdentifierPattern";
			public const string IsAutoIncrementable = "IsAutoIncrementable";
			public const string IsBestMatch = "IsBestMatch";
			public const string IsCaseSensitive = "IsCaseSensitive";
			public const string IsFixedLength = "IsFixedLength";
			public const string IsFixedPrecisionScale = "IsFixedPrecisionScale";
			public const string IsLiteralSupported = "IsLiteralSupported";
			public const string IsLong = "IsLong";
			public const string IsNullable = "IsNullable";
			public const string IsSearchable = "IsSearchable";
			public const string IsSearchableWithLike = "IsSearchableWithLike";
			public const string IsUnsigned = "IsUnsigned";
			public const string LiteralPrefix = "LiteralPrefix";
			public const string LiteralSuffix = "LiteralSuffix";
			public const string MaximumScale = "MaximumScale";
			public const string MinimumScale = "MinimumScale";
			public const string NumberOfIdentifierParts = "NumberOfIdentifierParts";
			public const string NumberOfRestrictions = "NumberOfRestrictions";
			public const string OrderByColumnsInSelect = "OrderByColumnsInSelect";
			public const string ParameterMarkerFormat = "ParameterMarkerFormat";
			public const string ParameterMarkerPattern = "ParameterMarkerPattern";
			public const string ParameterNameMaxLength = "ParameterNameMaxLength";
			public const string ProviderDbType = "ProviderDbType";
			public const string QuotedIdentifierCase = "QuotedIdentifierCase";
			public const string QuotedIdentifierPattern = "QuotedIdentifierPattern";
			public const string ReservedWord = "ReservedWord";
			public const string SQLJoinSupport = "SQLJoinSupport";
			public const string StatementSeparatorPattern = "StatementSeparatorPattern";
			public const string StringLiteralPattern = "StringLiteralPattern";
			public const string TypeName = "TypeName";

			#endregion // Fields

			#region Constructors

			[MonoTODO]
			public ColumnNames ()
			{
			}

			#endregion // Constructors
		}
	}
}

#endif
