//------------------------------------------------------------------------------
// <copyright file="RuleInfoComparer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Compilation;

    //
    // This class is used to compare two RuleInfo object.  Basically, the array list
    // will be sorted based on class hierachical order.  The smaller the value, the
    // more nearer the class is to the root (WebBaseEvent), in a class inheritance sense.
    // 
    // On the other hand, if x > y, it means x is NOT a parent class of y.
    //
    // The array is sorted in this way so that if we want to find out the config setting of
    // an event class x, we start searching in a decreasing order, and the first entry (E)
    // that satifies the test: if (x is E), then we find the right settings for x.
    //
    // BTW, this is just a trick to save me from writing too much code.  A n-node tree
    // method is faster, but I was too lazy.
    //
    internal class RuleInfoComparer : IComparer{
        public int Compare(object x, object y) {
            int res;
            
            Type xType = 
                ((HealthMonitoringSectionHelper.RuleInfo)x)._eventMappingSettings.RealType;
            Type yType = 
                ((HealthMonitoringSectionHelper.RuleInfo)y)._eventMappingSettings.RealType;
    
            if (xType.Equals(yType)) {
                res = 0;
            } 
            else if (xType.IsSubclassOf(yType)) {
                res = 1;
            } 
            else if (yType.IsSubclassOf(xType)) {
                res = -1;
            }
            else {
                // If they're unrelated, we can't return 0 because it
                // will confuse the sorting method.
                // We can return 1 or -1, but it must be consistent.
                return String.Compare(xType.ToString(), yType.ToString(), StringComparison.Ordinal);
            }
    
            Debug.Trace("RuleInfoComparer", "xType=" + xType.ToString() +
                "; yType=" + yType.ToString() + "; res=" + res);
            
            return res;
        }
    }
}
