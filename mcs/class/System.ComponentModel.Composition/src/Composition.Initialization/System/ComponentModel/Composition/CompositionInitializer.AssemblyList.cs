// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;

namespace System.ComponentModel.Composition
{
    public static partial class CompositionInitializer
    {
        // This method is the only Silverlight specific code dependency in CompositionHost
        private static List<Assembly> GetAssemblyList()
        {
            var assemblies = new List<Assembly>();

            // While this may seem like somewhat of a hack, walking the AssemblyParts in the active 
            // deployment object is the only way to get the list of assemblies loaded by the XAP. 
            foreach (AssemblyPart ap in Deployment.Current.Parts)
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(ap.Source, UriKind.Relative));
                if (sri != null)
                {
                    // Keep in mind that calling Load on an assembly that is already loaded will
                    // be a no-op and simply return the already loaded assembly object.
                    Assembly assembly = ap.Load(sri.Stream);
                    assemblies.Add(assembly);
                }
            }

            return assemblies;
        }
    }
}