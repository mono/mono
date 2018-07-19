//---------------------------------------------------------------------
// <copyright file="BasicCommandTreeVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;

using System.Data.Metadata.Edm;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// An abstract base type for types that implement the IExpressionVisitor interface to derive from.
    /// </summary>
    /*CQT_PUBLIC_API(*/internal/*)*/ abstract class BasicCommandTreeVisitor : BasicExpressionVisitor
    {
        #region protected API, may be overridden to add functionality at specific points in the traversal

        protected virtual void VisitSetClause(DbSetClause setClause)
        {
            EntityUtil.CheckArgumentNull(setClause, "setClause");
            this.VisitExpression(setClause.Property);
            this.VisitExpression(setClause.Value);
        }

        protected virtual void VisitModificationClause(DbModificationClause modificationClause)
        {
            EntityUtil.CheckArgumentNull(modificationClause, "modificationClause");
            // Set clause is the only current possibility
            this.VisitSetClause((DbSetClause)modificationClause);
        }

        protected virtual void VisitModificationClauses(IList<DbModificationClause> modificationClauses)
        {
            EntityUtil.CheckArgumentNull(modificationClauses, "modificationClauses");
            for (int idx = 0; idx < modificationClauses.Count; idx++)
            {
                this.VisitModificationClause(modificationClauses[idx]);
            }
        }
        
        #endregion

        #region public convenience API

        public virtual void VisitCommandTree(DbCommandTree commandTree)
        {
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");
            switch (commandTree.CommandTreeKind)
            {
                case DbCommandTreeKind.Delete:
                    this.VisitDeleteCommandTree((DbDeleteCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Function:
                    this.VisitFunctionCommandTree((DbFunctionCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Insert:
                    this.VisitInsertCommandTree((DbInsertCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Query:
                    this.VisitQueryCommandTree((DbQueryCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Update:
                    this.VisitUpdateCommandTree((DbUpdateCommandTree)commandTree);
                    break;

                default:
                    throw EntityUtil.NotSupported();
            }
        }

        #endregion

        #region CommandTree-specific Visitor Methods

        protected virtual void VisitDeleteCommandTree(DbDeleteCommandTree deleteTree)
        {
            EntityUtil.CheckArgumentNull(deleteTree, "deleteTree");
            this.VisitExpressionBindingPre(deleteTree.Target);
            this.VisitExpression(deleteTree.Predicate);
            this.VisitExpressionBindingPost(deleteTree.Target);
        }

        protected virtual void VisitFunctionCommandTree(DbFunctionCommandTree functionTree)
        {
            EntityUtil.CheckArgumentNull(functionTree, "functionTree");

        }

        protected virtual void VisitInsertCommandTree(DbInsertCommandTree insertTree)
        {
            EntityUtil.CheckArgumentNull(insertTree, "insertTree");
            this.VisitExpressionBindingPre(insertTree.Target);
            this.VisitModificationClauses(insertTree.SetClauses);
            if (insertTree.Returning != null)
            {
                this.VisitExpression(insertTree.Returning);
            }
            this.VisitExpressionBindingPost(insertTree.Target);
        }

        protected virtual void VisitQueryCommandTree(DbQueryCommandTree queryTree)
        {
            EntityUtil.CheckArgumentNull(queryTree, "queryTree");
            this.VisitExpression(queryTree.Query);
        }

        protected virtual void VisitUpdateCommandTree(DbUpdateCommandTree updateTree)
        {
            EntityUtil.CheckArgumentNull(updateTree, "updateTree");
            this.VisitExpressionBindingPre(updateTree.Target);
            this.VisitModificationClauses(updateTree.SetClauses);
            this.VisitExpression(updateTree.Predicate);
            if (updateTree.Returning != null)
            {
                this.VisitExpression(updateTree.Returning);
            }
            this.VisitExpressionBindingPost(updateTree.Target);
        }

        #endregion

    }
}
