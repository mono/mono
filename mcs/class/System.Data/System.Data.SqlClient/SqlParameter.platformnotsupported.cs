// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Xml;
using MSS = Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Server;
using System.ComponentModel.Design.Serialization;

namespace System.Data.SqlClient
{
	public sealed partial class SqlParameter : DbParameter, IDbDataParameter, ICloneable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlParameter is not supported on the current platform.";

		public SqlParameter() : base() {}
		public SqlParameter(string parameterName, SqlDbType dbType) : this() {}
		public SqlParameter(string parameterName, object value) : this() {}
		public SqlParameter(string parameterName, SqlDbType dbType, int size) : this() {}
		public SqlParameter(string parameterName, SqlDbType dbType, int size, string sourceColumn) : this() {}

		public SqlParameter(
			string parameterName,
			SqlDbType dbType,
			int size,
			ParameterDirection direction,
			bool isNullable,
			byte precision,
			byte scale,
			string sourceColumn,
			DataRowVersion sourceVersion,
			object value
		) : this(parameterName, dbType, size, sourceColumn)
		{
		}

		public SqlParameter(
			string parameterName,
			SqlDbType dbType, 
			int size,
			ParameterDirection direction,
			byte precision,
			byte scale,
			string sourceColumn,
			DataRowVersion sourceVersion,
			bool sourceColumnNullMapping,
			object value,
			string xmlSchemaCollectionDatabase,
			string xmlSchemaCollectionOwningSchema,
			string xmlSchemaCollectionName
		) : this()
		{
		}

		internal SqlCollation Collation
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string XmlSchemaCollectionDatabase
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string XmlSchemaCollectionOwningSchema
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string XmlSchemaCollectionName
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override DbType DbType
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void ResetDbType()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal MetaType InternalMetaType
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int LocaleId
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal MSS.SmiParameterMetaData MetaDataForSmi(out ParameterPeekAheadValue peekAhead)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool ParameterIsSqlType
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string ParameterName
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal string ParameterNameFixed
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public new byte Precision
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal byte PrecisionInternal
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new byte Scale
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal byte ScaleInternal
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlDbType SqlDbType
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void ResetSqlDbType()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public object SqlValue
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string UdtTypeName
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public String TypeName
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override object Value
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal INullable ValueAsINullable
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool IsNull
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal int GetActualSize()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		object ICloneable.Clone()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal static object CoerceValue(object value, MetaType destinationType, out bool coercedToDataFeed, out bool typeChanged, bool allowStreaming = true)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void FixStreamDataForNonPLP()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override DataRowVersion SourceVersion
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal byte GetActualPrecision()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal byte GetActualScale()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal int GetParameterSize()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal object GetCoercedValue()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool CoercedValueIsSqlType
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool CoercedValueIsDataFeed
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		[Conditional("DEBUG")]
		internal void AssertCachedPropertiesAreValid()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		[Conditional("DEBUG")]
		internal void AssertPropertiesAreValid(object value, bool? isSqlType = null, bool? isDataFeed = null, bool? isNull = null)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void Prepare(SqlCommand cmd)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void SetSqlBuffer(SqlBuffer buff)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void SetUdtLoadError(Exception e)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void Validate(int index, bool isCommandProc)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal MetaType ValidateTypeLengths()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal static string[] ParseTypeName(string typeName, bool isUdtTypeName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal sealed class SqlParameterConverter : ExpandableObjectConverter
		{
			public SqlParameterConverter()
				=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
				=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override ParameterDirection Direction
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool IsNullable
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int Offset
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int Size
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string SourceColumn
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool SourceColumnNullMapping
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal void CopyTo(SqlParameter destination)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCompareOptions CompareInfo
		{
			get	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set	=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}

	internal abstract class DataFeed {}

	internal class StreamDataFeed : DataFeed
	{
		internal Stream _source;
		internal StreamDataFeed(Stream source) {}
	}

	internal class TextDataFeed : DataFeed
	{
		internal TextReader _source;
		internal TextDataFeed(TextReader source) {}
	}

	internal class XmlDataFeed : DataFeed
	{
		internal XmlReader _source;
		internal XmlDataFeed(XmlReader source) {}
	}
}