//---------------------------------------------------------------------
// <copyright file="FunctionDetailsReader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data.EntityClient;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Entity.Design.SsdlGenerator
{
    /// <summary>
    /// The purpose of this class is to give us strongly typed access to the results of the reader.
    /// NOTE: this class will dispose of the command when the reader is disposed.
    /// </summary>
    internal abstract class FunctionDetailsReader : IDisposable
    {
        private DbDataReader _reader;
        private EntityCommand _command;
        private EntityConnection _connection;
        private object[] _currentRow;

        public static FunctionDetailsReader Create(EntityConnection connection, IEnumerable<EntityStoreSchemaFilterEntry> filters, Version storeSchemaModelVersion)
        {
            Debug.Assert(connection != null, "the parameter connection is null");
            Debug.Assert(connection.State == ConnectionState.Open, "the connection is not Open");

            if (storeSchemaModelVersion >= EntityFrameworkVersions.Version3)
            {
                return new FunctionDetailsReaderV3(connection, filters);
            }
            else
            {
                return new FunctionDetailsReaderV1(connection, filters);
            }
        }

        protected void InitializeReader(EntityConnection connection, IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            _connection = connection;

            _command = EntityStoreSchemaGeneratorDatabaseSchemaLoader.CreateFilteredCommand(
                        _connection,
                        FunctionDetailSql,
                        FunctionOrderByClause,
                        EntityStoreSchemaFilterObjectTypes.Function,
                        new List<EntityStoreSchemaFilterEntry>(filters),
                        new string[] { FunctionDetailAlias });
            _reader = _command.ExecuteReader(CommandBehavior.SequentialAccess);
        }

        internal bool Read()
        {
            Debug.Assert(_reader != null, "don't Read() when it is created from a memento");
            bool haveRow = _reader.Read();
            if (haveRow)
            {
                if (_currentRow == null)
                {
                    _currentRow = new object[ColumnCount];
                }
                _reader.GetValues(_currentRow);
            }
            else
            {
                _currentRow = null;
            }
            return haveRow;
        }

        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            Debug.Assert(_reader != null, "don't Dispose() when it is created from a memento");
            _reader.Dispose();
            _command.Dispose();
        }

        internal abstract int ColumnCount { get; }

        public abstract string Catalog { get; }

        public abstract string Schema { get; }

        public abstract string ProcedureName { get; }

        public abstract string ReturnType { get; }

        public abstract bool IsIsAggregate { get; }

        public abstract bool IsBuiltIn { get; }

        public abstract bool IsComposable { get; }

        public abstract bool IsNiladic { get; }

        public abstract bool IsTvf { get; }

        public abstract string ParameterName { get; }

        public abstract bool IsParameterNameNull { get; }

        public abstract string ParameterType { get; }

        public abstract bool IsParameterTypeNull { get; }

        public abstract string ProcParameterMode { get; }

        public abstract bool IsParameterModeNull { get; }

        public bool TryGetParameterMode(out ParameterMode mode)
        {
            if (IsParameterModeNull)
            {
                mode = (ParameterMode)(-1);
                return false;
            }

            switch (ProcParameterMode)
            {
                case "IN":
                    mode = ParameterMode.In;
                    return true;
                case "OUT":
                    mode = ParameterMode.Out;
                    return true;
                case "INOUT":
                    mode = ParameterMode.InOut;
                    return true;
                default:
                    mode = (ParameterMode)(-1);
                    return false;
            }
        }

        internal EntityStoreSchemaGenerator.DbObjectKey CreateDbObjectKey()
        {
            Debug.Assert(_currentRow != null, "don't call this method when you not reading");
            return new EntityStoreSchemaGenerator.DbObjectKey(this.Catalog, this.Schema, this.ProcedureName, EntityStoreSchemaGenerator.DbObjectType.Function);
        }

        protected static T ConvertDBNull<T>(object value)
        {
            return Convert.IsDBNull(value) ? default(T) : (T)value;
        }

        public abstract void Attach(Memento memento);

        public abstract Memento CreateMemento();

        private static readonly string FunctionDetailAlias = "sp";
        protected abstract string FunctionDetailSql { get; }
        private static readonly string FunctionOrderByClause = @" 
            ORDER BY
                sp.SchemaName
            ,   sp.Name
            ,   sp.Ordinal
            ";

        internal sealed class FunctionDetailsReaderV1 : FunctionDetailsReader
        {
            internal FunctionDetailsReaderV1(MementoV1 memento)
            {
                _currentRow = memento.Values;
            }

            public FunctionDetailsReaderV1(EntityConnection connection, IEnumerable<EntityStoreSchemaFilterEntry> filters)
            {
                InitializeReader(connection, filters);
            }

            public override void Attach(Memento memento)
            {
                Debug.Assert(memento != null, "the parameter memento is null");
                Debug.Assert(memento.Values != null, "the values in the memento are null");
                Debug.Assert(memento is MementoV1, "the memento is for a different version");
                Debug.Assert(_reader == null, "don't attach to a real reader");
                _currentRow = memento.Values;
            }

            public override Memento CreateMemento()
            {
                Debug.Assert(_currentRow != null, "don't call this method when you not reading");
                return new MementoV1((object[])_currentRow.Clone());
            }

            const int PROC_SCHEMA_INDEX = 0;
            const int PROC_NAME_INDEX = 1;
            const int PROC_RET_TYPE_INDEX = 2;
            const int PROC_ISAGGREGATE_INDEX = 3;
            const int PROC_ISCOMPOSABLE_INDEX = 4;
            const int PROC_ISBUILTIN_INDEX = 5;
            const int PROC_ISNILADIC_INDEX = 6;
            const int PARAM_NAME_INDEX = 7;
            const int PARAM_TYPE_INDEX = 8;
            const int PARAM_DIRECTION_INDEX = 9;

            internal override int ColumnCount { get { return 10; } }

            public override string Catalog { get { return null; } }

            public override string Schema
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_SCHEMA_INDEX]); }
            }

            public override string ProcedureName
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_NAME_INDEX]); }
            }

            public override string ReturnType
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_RET_TYPE_INDEX]); }
            }

            public override bool IsIsAggregate
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISAGGREGATE_INDEX]); }
            }

            public override bool IsBuiltIn
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISBUILTIN_INDEX]); }
            }

            public override bool IsComposable
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISCOMPOSABLE_INDEX]); }
            }

            public override bool IsNiladic
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISNILADIC_INDEX]); }
            }

            public override bool IsTvf { get { return false; } }

            public override string ParameterName
            {
                get { return (string)_currentRow[PARAM_NAME_INDEX]; }
            }

            public override bool IsParameterNameNull
            {
                get { return Convert.IsDBNull(_currentRow[PARAM_NAME_INDEX]); }
            }

            public override string ParameterType
            {
                get { return (string)_currentRow[PARAM_TYPE_INDEX]; }
            }

            public override bool IsParameterTypeNull
            {
                get { return Convert.IsDBNull(_currentRow[PARAM_TYPE_INDEX]); }
            }

            public override string ProcParameterMode
            {
                get { return (string)_currentRow[PARAM_DIRECTION_INDEX]; }
            }

            public override bool IsParameterModeNull
            {
                get { return Convert.IsDBNull(_currentRow[PARAM_DIRECTION_INDEX]); }
            }

            protected override string FunctionDetailSql
            {
                get
                {
                    return @"
            SELECT
                  sp.SchemaName
                , sp.Name 
                , sp.ReturnTypeName
                , sp.IsAggregate
                , sp.IsComposable
                , sp.IsBuiltIn
                , sp.IsNiladic
                , sp.ParameterName
                , sp.ParameterType
                , sp.Mode
            FROM (  
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   r.ReturnType.TypeName as ReturnTypeName
              ,   r.IsAggregate as IsAggregate
              ,   true as IsComposable
              ,   r.IsBuiltIn as IsBuiltIn
              ,   r.IsNiladic as IsNiladic
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                OfType(SchemaInformation.Functions, Store.ScalarFunction) as r 
                 OUTER APPLY
                r.Parameters as p)
            UNION ALL
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   CAST(NULL as string) as ReturnTypeName
              ,   false as IsAggregate
              ,   false as IsComposable
              ,   false as IsBuiltIn
              ,   false as IsNiladic
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                SchemaInformation.Procedures as r 
                 OUTER APPLY
                r.Parameters as p)) as sp
            ";
                }
            }
        }

        internal sealed class FunctionDetailsReaderV3 : FunctionDetailsReader
        {
            internal FunctionDetailsReaderV3(MementoV3 memento)
            {
                _currentRow = memento.Values;
            }

            public FunctionDetailsReaderV3(EntityConnection connection, IEnumerable<EntityStoreSchemaFilterEntry> filters)
            {
                InitializeReader(connection, filters);
            }

            public override void Attach(Memento memento)
            {
                Debug.Assert(memento != null, "the parameter memento is null");
                Debug.Assert(memento.Values != null, "the values in the memento are null");
                Debug.Assert(memento is MementoV3, "the memento is for a different version");
                Debug.Assert(_reader == null, "don't attach to a real reader");
                _currentRow = memento.Values;
            }

            public override Memento CreateMemento()
            {
                Debug.Assert(_currentRow != null, "don't call this method when you not reading");
                return new MementoV3((object[])_currentRow.Clone());
            }

            const int PROC_CATALOG_INDEX = 0;
            const int PROC_SCHEMA_INDEX = 1;
            const int PROC_NAME_INDEX = 2;
            const int PROC_RET_TYPE_INDEX = 3;
            const int PROC_ISAGGREGATE_INDEX = 4;
            const int PROC_ISCOMPOSABLE_INDEX = 5;
            const int PROC_ISBUILTIN_INDEX = 6;
            const int PROC_ISNILADIC_INDEX = 7;
            const int PROC_ISTVF_INDEX = 8;
            const int PARAM_NAME_INDEX = 9;
            const int PARAM_TYPE_INDEX = 10;
            const int PARAM_DIRECTION_INDEX = 11;

            internal override int ColumnCount { get { return 12; } }

            public override string Catalog
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_CATALOG_INDEX]); }
            }

            public override string Schema
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_SCHEMA_INDEX]); }
            }

            public override string ProcedureName
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_NAME_INDEX]); }
            }

            public override string ReturnType
            {
                get { return ConvertDBNull<string>(_currentRow[PROC_RET_TYPE_INDEX]); }
            }

            public override bool IsIsAggregate
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISAGGREGATE_INDEX]); }
            }

            public override bool IsBuiltIn
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISBUILTIN_INDEX]); }
            }

            public override bool IsComposable
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISCOMPOSABLE_INDEX]); }
            }

            public override bool IsNiladic
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISNILADIC_INDEX]); }
            }

            public override bool IsTvf
            {
                get { return ConvertDBNull<bool>(_currentRow[PROC_ISTVF_INDEX]); }
            }

            public override string ParameterName
            {
                get { return (string)_currentRow[PARAM_NAME_INDEX]; }
            }

            public override bool IsParameterNameNull
            {
                get { return Convert.IsDBNull(_currentRow[PARAM_NAME_INDEX]); }
            }

            public override string ParameterType
            {
                get { return (string)_currentRow[PARAM_TYPE_INDEX]; }
            }

            public override bool IsParameterTypeNull
            {
                get { return Convert.IsDBNull(_currentRow[PARAM_TYPE_INDEX]); }
            }

            public override string ProcParameterMode
            {
                get { return (string)_currentRow[PARAM_DIRECTION_INDEX]; }
            }

            public override bool IsParameterModeNull
            {
                get { return Convert.IsDBNull(_currentRow[PARAM_DIRECTION_INDEX]); }
            }

            protected override string FunctionDetailSql
            {
                get
                {
                    return @"
            Function IsTvf(f Store.Function) as (f is of (Store.TableValuedFunction))
            SELECT
                  sp.CatalogName
                , sp.SchemaName
                , sp.Name 
                , sp.ReturnTypeName
                , sp.IsAggregate
                , sp.IsComposable
                , sp.IsBuiltIn
                , sp.IsNiladic
                , sp.IsTvf
                , sp.ParameterName
                , sp.ParameterType
                , sp.Mode
            FROM (  
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   TREAT(r as Store.ScalarFunction).ReturnType.TypeName as ReturnTypeName
              ,   TREAT(r as Store.ScalarFunction).IsAggregate as IsAggregate
              ,   true as IsComposable
              ,   r.IsBuiltIn as IsBuiltIn
              ,   r.IsNiladic as IsNiladic
              ,   IsTvf(r) as IsTvf
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                SchemaInformation.Functions as r 
                 OUTER APPLY
                r.Parameters as p)
            UNION ALL
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   CAST(NULL as string) as ReturnTypeName
              ,   false as IsAggregate
              ,   false as IsComposable
              ,   false as IsBuiltIn
              ,   false as IsNiladic
              ,   false as IsTvf
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                SchemaInformation.Procedures as r 
                 OUTER APPLY
                r.Parameters as p)) as sp
            ";
                }
            }
        }

        internal abstract class Memento
        {
            protected object[] _values;

            internal object[] Values
            {
                get { return _values; }
            }

            public abstract FunctionDetailsReader CreateReader();
        }

        internal sealed class MementoV1 : Memento
        {
            internal MementoV1(object[] values)
            {
                _values = values;
            }

            public override FunctionDetailsReader CreateReader()
            {
                return new FunctionDetailsReaderV1(this);
            }
        }

        internal sealed class MementoV3 : Memento
        {
            internal MementoV3(object[] values)
            {
                _values = values;
            }

            public override FunctionDetailsReader CreateReader()
            {
                return new FunctionDetailsReaderV3(this);
            }
        }
    }
}
