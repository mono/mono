// System.Data.Common.DbMetaDataCollectionNames
//
// Author: Senganal T	(tsenganal@novell.com)
//
// (C)  Senganal T 2005

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
namespace System.Data.Common {
	public static class DbMetaDataColumnNames {
		public static readonly string CollectionName = "CollectionName";
		public static readonly string ColumnSize = "ColumnSize";
		public static readonly string CompositeIdentifierSeparatorPattern = "CompositeIdentifierSeparatorPattern";
		public static readonly string CreateFormat = "CreateFormat";
		public static readonly string CreateParameters = "CreateParameters";
		public static readonly string DataSourceProductName = "DataSourceProductName";
		public static readonly string DataSourceProductVersion = "DataSourceProductVersion";
		public static readonly string DataType = "DataType";
		public static readonly string DataSourceProductVersionNormalized = "DataSourceProductVersionNormalized";
		public static readonly string GroupByBehavior = "GroupByBehavior";
		public static readonly string IdentifierCase = "IdentifierCase";
		public static readonly string IdentifierPattern = "IdentifierPattern";
		public static readonly string IsAutoIncrementable = "IsAutoIncrementable";
		public static readonly string IsBestMatch = "IsBestMatch";
		public static readonly string IsCaseSensitive = "IsCaseSensitive";
		public static readonly string IsConcurrencyType = "IsConcurrencyType";
		public static readonly string IsFixedLength = "IsFixedLength";
		public static readonly string IsFixedPrecisionScale = "IsFixedPrecisionScale";
		public static readonly string IsLiteralSupported = "IsLiteralSupported";
		public static readonly string IsLong = "IsLong";
		public static readonly string IsNullable = "IsNullable";
		public static readonly string IsSearchable = "IsSearchable";
		public static readonly string IsSearchableWithLike = "IsSearchableWithLike";
		public static readonly string IsUnsigned = "IsUnsigned";
		public static readonly string LiteralPrefix = "LiteralPrefix";
		public static readonly string LiteralSuffix = "LiteralSuffix";
		public static readonly string MaximumScale = "MaximumScale";
		public static readonly string MinimumScale = "MinimumScale";
		public static readonly string NumberOfIdentifierParts = "NumberOfIdentifierParts";
		public static readonly string NumberOfRestrictions = "NumberOfRestrictions";
		public static readonly string OrderByColumnsInSelect = "OrderByColumnsInSelect";
		public static readonly string ParameterMarkerFormat = "ParameterMarkerFormat";
		public static readonly string ParameterMarkerPattern = "ParameterMarkerPattern";
		public static readonly string ParameterNameMaxLength = "ParameterNameMaxLength";
		public static readonly string ParameterNamePattern = "ParameterNamePattern";
		public static readonly string ProviderDbType = "ProviderDbType";
		public static readonly string QuotedIdentifierCase = "QuotedIdentifierCase";
		public static readonly string QuotedIdentifierPattern = "QuotedIdentifierPattern";
		public static readonly string ReservedWord = "ReservedWord";
		public static readonly string StatementSeparatorPattern = "StatementSeparatorPattern";
		public static readonly string StringLiteralPattern = "StringLiteralPattern";
		public static readonly string SupportedJoinOperators = "SupportedJoinOperators";
		public static readonly string TypeName = "TypeName";
	}
}
#endif
