// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if(SILVERLIGHT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.ComponentModel;

namespace System.ComponentModel.Composition.Hosting
{
    /// <summary>
    ///     Helper functions for accessing the Silverlight manifest
    /// </summary>
    internal static class Package
    {
        /// <summary>
        ///     Retrieves The current list of assemblies for the application XAP load. Depends on the Deployment.Current property being setup and
        ///     so can only be accessed after the Application object has be completely constructed.
        ///     No caching occurs at this level.
        /// </summary>
        public static IEnumerable<Assembly> CurrentAssemblies
        {
            get
            {
                var assemblies = new List<Assembly>();

                // While this may seem like somewhat of a hack, walking the AssemblyParts in the active 
                // deployment object is the only way to get the list of assemblies loaded by the initial XAP. 
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


        public static IEnumerable<Assembly> LoadPackagedAssemblies(Stream packageStream)
        {
            List<Assembly> assemblies = new List<Assembly>();
            StreamResourceInfo packageStreamInfo = new StreamResourceInfo(packageStream, null);

            IEnumerable<AssemblyPart> parts = GetDeploymentParts(packageStreamInfo);

            foreach (AssemblyPart ap in parts)
            {
                StreamResourceInfo sri = Application.GetResourceStream(
                    packageStreamInfo, new Uri(ap.Source, UriKind.Relative));

                assemblies.Add(ap.Load(sri.Stream));
            }
            packageStream.Close();
            return assemblies;
        }

        /// <summary>
        ///     Only reads AssemblyParts and does not support external parts (aka Platform Extensions or TPEs).
        /// </summary>
        private static IEnumerable<AssemblyPart> GetDeploymentParts(StreamResourceInfo xapStreamInfo)
        {
            Uri manifestUri = new Uri("AppManifest.xaml", UriKind.Relative);
            StreamResourceInfo manifestStreamInfo = Application.GetResourceStream(xapStreamInfo, manifestUri);
            List<AssemblyPart> assemblyParts = new List<AssemblyPart>();

            // The code assumes the following format in AppManifest.xaml
            //<Deployment ... >
            //  <Deployment.Parts>
            //    <AssemblyPart x:Name="A" Source="A.dll" />
            //    <AssemblyPart x:Name="B" Source="B.dll" />
            //      ...
            //    <AssemblyPart x:Name="Z" Source="Z.dll" />
            //  </Deployment.Parts>
            //</Deployment>
            if (manifestStreamInfo != null)
            {
                Stream manifestStream = manifestStreamInfo.Stream;
                using (XmlReader reader = XmlReader.Create(manifestStream))
                {
                    if (reader.ReadToFollowing("AssemblyPart"))
                    {
                        do
                        {
                            string source = reader.GetAttribute("Source");

                            if (source != null)
                            {
                                assemblyParts.Add(new AssemblyPart() { Source = source });
                            }
                        }
                        while (reader.ReadToNextSibling("AssemblyPart"));
                    }
                }
            }

            return assemblyParts;
        }
    }
}
#endif
