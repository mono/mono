//---------------------------------------------------------------------
// <copyright file="DbSetClause.cs" company="Microsoft">
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
using System.Data.Common.Utils;
using System.Diagnostics;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Specifies a clause in a modification operation setting the value of a property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbSetClause : DbModificationClause
    {
        private DbExpression _prop;
        private DbExpression _val;

        internal DbSetClause(DbExpression targetProperty, DbExpression sourceValue)
            : base()
        {
            EntityUtil.CheckArgumentNull(targetProperty, "targetProperty");
            EntityUtil.CheckArgumentNull(sourceValue, "sourceValue");
            _prop = targetProperty;
            _val = sourceValue;
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies the property that should be updated.
        /// </summary>
        /// <remarks>
        /// Constrained to be a <see cref="DbPropertyExpression"/>.
        /// </remarks>
        public DbExpression Property
        {
            get
            {
                return _prop;
            }
        }
        
        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies the new value with which to update the property.
        /// </summary>
        /// <remarks>
        /// Constrained to be a <see cref="DbConstantExpression"/> or <see cref="DbNullExpression"/>
        /// </remarks>
        public DbExpression Value
        { 
            get
            {
                return _val;
            }
        }
                
        internal override void DumpStructure(ExpressionDumper dumper)
        {
            dumper.Begin("DbSetClause");
            if (null != this.Property)
            {
                dumper.Dump(this.Property, "Property");
            }
            if (null != this.Value)
            {
                dumper.Dump(this.Value, "Value");
            }
            dumper.End("DbSetClause");
        }

        internal override TreeNode Print(DbExpressionVisitor<TreeNode> visitor)
        {
            TreeNode node = new TreeNode("DbSetClause");
            if (null != this.Property)
            {
                node.Children.Add(new TreeNode("Property", this.Property.Accept(visitor)));
            }
            if (null != this.Value)
            {
                node.Children.Add(new TreeNode("Value", this.Value.Accept(visitor)));
            }
            return node;
        }
    }
}
