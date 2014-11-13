//---------------------------------------------------------------------
// <copyright file="CellTreeOpType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


namespace System.Data.Mapping.ViewGeneration.Structures
{

    // This enum identifies for which side we are generating the view
    internal enum ViewTarget
    {
        QueryView,
        UpdateView
    }

    // Different operations that are used in the CellTreeNode nodes
    internal enum CellTreeOpType
    {
        Leaf,  // Leaf Node
        Union, // union all
        FOJ,   // full outerjoin
        LOJ,   // left outerjoin
        IJ,    // inner join
        LASJ   // left antisemijoin
    }
}
