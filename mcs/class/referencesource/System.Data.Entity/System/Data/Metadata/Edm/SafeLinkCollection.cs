//---------------------------------------------------------------------
// <copyright file="SafeLinkCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// This class attempts to make a double linked connection between a parent and child without
    /// exposing the properties publicly that would allow them to be mutable and possibly dangerous
    /// in a multithreading environment
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    internal class SafeLinkCollection<TParent, TChild> : ReadOnlyMetadataCollection<TChild> where TChild : MetadataItem where TParent : class
    {
        public SafeLinkCollection(TParent parent, Func<TChild, SafeLink<TParent>> getLink, MetadataCollection<TChild> children)
            : base((IList<TChild>)SafeLink<TParent>.BindChildren(parent, getLink, children))
        {
        }
    }
}
