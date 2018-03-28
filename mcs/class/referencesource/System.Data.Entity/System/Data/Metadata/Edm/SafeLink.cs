//---------------------------------------------------------------------
// <copyright file="SafeLink.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    internal class SafeLink<TParent> where TParent : class
    {
        private TParent _value;
        public TParent Value { get { return _value; } }


        internal static IEnumerable<TChild> BindChildren<TChild>(TParent parent, Func<TChild, SafeLink<TParent>> getLink, IEnumerable<TChild> children)
        {
            
            foreach (TChild child in children)
            {
                BindChild(parent, getLink, child);
            }
            return children;
        }

        internal static TChild BindChild<TChild>(TParent parent, Func<TChild, SafeLink<TParent>> getLink, TChild child)
        {
            SafeLink<TParent> link = getLink(child);

            Debug.Assert(link._value == null || link._value == parent, "don't try to hook up the same child to a different parent");
            // this is the good stuff.. 
            // only this method can actually make the link since _value is a private
            link._value = parent;

            return child;
        }
    }
}
