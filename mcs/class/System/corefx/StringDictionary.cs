// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Collections.Generic;

namespace System.Collections.Specialized
{
    partial class StringDictionary
    {
        internal void ReplaceHashtable (Hashtable useThisHashtableInstead)
        {
            contents = useThisHashtableInstead;
        } 

        internal IDictionary<string, string> AsGenericDictionary ()
        {
            return new GenericAdapter(this);
        }
    }
}