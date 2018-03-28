//---------------------------------------------------------------------
// <copyright file="NodeCounter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Data.Common;
using md=System.Data.Metadata.Edm;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// Counts the number of nodes in a tree
    /// </summary>
    internal class NodeCounter : BasicOpVisitorOfT<int>
    {
        /// <summary>
        /// Public entry point - Calculates the nubmer of nodes in the given subTree
        /// </summary>
        /// <param name="subTree"></param>
        /// <returns></returns>
        internal static int Count(Node subTree)
        {
            NodeCounter counter = new NodeCounter();
            return counter.VisitNode(subTree);
        }

        /// <summary>
        /// Common processing for all node types
        /// Count = 1 (self) + count of children
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override int VisitDefault(Node n)
        {
            int count = 1;
            foreach (Node child in n.Children)
            {
                count += VisitNode(child);
            }
            return count;
        }
    }
}


