// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;
using System.Text;

namespace System.Collections.Specialized
{
    partial class NameValueCollection
    {
        // Allow internal extenders to avoid creating the hashtable/arraylist.
        internal NameValueCollection (DBNull dummy) : base (dummy)
        {
        }
    }
}