
//---------------------------------------------------------------------
// <copyright file="DbQueryCommandTree.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;
using System.Linq;
using System.Diagnostics;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Represents a query operation expressed as a canonical command tree.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbQueryCommandTree : DbCommandTree
    {
        // Query expression
        private readonly DbExpression _query;

        // Parameter information (will be retrieved from the query expression of the command tree during construction)
        private System.Collections.ObjectModel.ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

        private DbQueryCommandTree(MetadataWorkspace metadata,
                                   DataSpace dataSpace, 
                                   DbExpression query,
                                   bool validate)
            : base(metadata, dataSpace)
        {
            // Ensure the query expression is non-null
            EntityUtil.CheckArgumentNull(query, "query");

            if (validate)
            {
                // Use the valid workspace and data space to validate the query expression
                DbExpressionValidator validator = new DbExpressionValidator(metadata, dataSpace);
                validator.ValidateExpression(query, "query");

                this._parameters = validator.Parameters.Select(paramInfo => paramInfo.Value).ToList().AsReadOnly();
            }
            this._query = query;
        }

        /// <summary>
        /// Constructs a new DbQueryCommandTree that uses the specified metadata workspace.
        /// </summary>
        /// <param name="metadata">The metadata workspace that the command tree should use.</param>
        /// <param name="dataSpace">The logical 'space' that metadata in the expressions used in this command tree must belong to.</param>
        /// <param name="query">A <see cref="DbExpression"/> that defines the logic of the query.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/> or <paramref name="query"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="dataSpace"/> does not represent a valid data space</exception>
        /*CQT_PUBLIC_API(*/internal/*)*/ DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query)
            : this(metadata, dataSpace, query, true)
        {
            
        }

        /// <summary>
        /// Gets a <see cref="DbExpression"/> that defines the logic of the query.
        /// </summary>
        public DbExpression Query
        {
            get { return this._query; }
        }
                        
        internal override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Query; }
        }

        internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
        {
            if (this._parameters == null)
            {
                this._parameters = ParameterRetriever.GetParameters(this);
            }
            return this._parameters.Select(p => new KeyValuePair<string, TypeUsage>(p.ParameterName, p.ResultType));
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            if (this.Query != null)
            {
                dumper.Dump(this.Query, "Query");
            }
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this); 
        }

        internal static DbQueryCommandTree FromValidExpression(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query)
        {
#if DEBUG
            return new DbQueryCommandTree(metadata, dataSpace, query);
#else
            return new DbQueryCommandTree(metadata, dataSpace, query, false);
#endif
        }
    }
}
