//---------------------------------------------------------------------
// <copyright file="ExpressionBindings.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;
using System.Data.Common.CommandTrees.ExpressionBuilder;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Describes a binding for an expression. Conceptually similar to a foreach loop
    /// in C#. The DbExpression property defines the collection being iterated over,
    /// while the Var property provides a means to reference the current element
    /// of the collection during the iteration. DbExpressionBinding is used to describe the set arguments
    /// to relational expressions such as <see cref="DbFilterExpression"/>, <see cref="DbProjectExpression"/>
    /// and <see cref="DbJoinExpression"/>.
    /// </summary>
    /// <seealso cref="DbExpression"/>
    /// <seealso cref="Variable"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbExpressionBinding
    {
        private readonly DbExpression _expr;
        private readonly DbVariableReferenceExpression _varRef;
        
        internal DbExpressionBinding(DbExpression input, DbVariableReferenceExpression varRef)
        {
            Debug.Assert(input != null, "DbExpressionBinding input cannot be null");
            Debug.Assert(varRef != null, "DbExpressionBinding variable cannot be null");

            _expr = input;
            _varRef = varRef;
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the input set.
        /// </summary>
        public DbExpression Expression { get { return _expr; } }
        
        /// <summary>
        /// Gets the name assigned to the element variable.
        /// </summary>
        public string VariableName { get { return _varRef.VariableName; } }

        /// <summary>
        /// Gets the type metadata of the element variable.
        /// </summary>
        public TypeUsage VariableType { get { return _varRef.ResultType; } }
        
        /// <summary>
        /// Gets the <see cref="DbVariableReferenceExpression"/> that references the element variable.
        /// </summary>
        public DbVariableReferenceExpression Variable { get { return _varRef;} }
    }

    /// <summary>
    /// Defines the binding for the input set to a <see cref="DbGroupByExpression"/>.
    /// In addition to the properties of <see cref="DbExpressionBinding"/>, DbGroupExpressionBinding
    /// also provides access to the group element via the <seealso cref="GroupVariable"/> variable reference
    /// and to the group aggregate via the <seealso cref="GroupAggregate"/> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbGroupExpressionBinding
    {
        private /*readonly*/ DbExpression _expr;
        private readonly DbVariableReferenceExpression _varRef;
        private readonly DbVariableReferenceExpression _groupVarRef;
        private  DbGroupAggregate _groupAggregate;
        
        internal DbGroupExpressionBinding(DbExpression input, DbVariableReferenceExpression inputRef, DbVariableReferenceExpression groupRef)
        {    
            _expr = input;
            _varRef = inputRef;
            _groupVarRef = groupRef;
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the input set.
        /// </summary>
        public DbExpression Expression { get { return _expr; } }
                
        /// <summary>
        /// Gets the name assigned to the element variable.
        /// </summary>
        public string VariableName { get { return _varRef.VariableName; } }

        /// <summary>
        /// Gets the type metadata of the element variable.
        /// </summary>
        public TypeUsage VariableType { get { return _varRef.ResultType; } }

        /// <summary>
        /// Gets the DbVariableReferenceExpression that references the element variable.
        /// </summary>
        public DbVariableReferenceExpression Variable { get { return _varRef; } }

        /// <summary>
        /// Gets the name assigned to the group element variable.
        /// </summary>
        public string GroupVariableName { get { return _groupVarRef.VariableName; } }

        /// <summary>
        /// Gets the type metadata of the group element variable.
        /// </summary>
        public TypeUsage GroupVariableType { get { return _groupVarRef.ResultType; } }

        /// <summary>
        /// Gets the DbVariableReferenceExpression that references the group element variable.
        /// </summary>
        public DbVariableReferenceExpression GroupVariable { get { return _groupVarRef; } }

        /// <summary>
        /// Gets the DbGroupAggregate that represents the collection of elements of the group. 
        /// </summary>
        public DbGroupAggregate GroupAggregate
        {
            get
            {
                if (_groupAggregate == null)
                {
                    _groupAggregate = DbExpressionBuilder.GroupAggregate(this.GroupVariable);                    
                }
                return _groupAggregate;
            }
        }
    }
}
