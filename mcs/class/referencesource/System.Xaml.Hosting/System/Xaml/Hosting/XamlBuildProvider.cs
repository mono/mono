//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xaml.Hosting
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Xaml;
    using System.Xaml.Hosting.Configuration;
    using System.Xml;   

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
    public class XamlBuildProvider : BuildProvider
    {
        IXamlBuildProviderExtension xamlBuildProviderExtension;
        static volatile IXamlBuildProviderExtensionFactory xamlBuildProviderExtensionFactory;

        internal new string VirtualPath
        {
            get { return base.VirtualPath; }
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            XamlType rootXamlType = GetRootXamlType();

            this.GetXamlBuildProviderExtension(rootXamlType);

            if (this.xamlBuildProviderExtension != null)
            {
                using (Stream xamlStream = base.OpenStream())
                {
                    this.xamlBuildProviderExtension.GenerateCode(assemblyBuilder, xamlStream, this);
                }
            }
        }        

        [Fx.Tag.Throws(typeof(TypeLoadException), "The type resolution of the root element failed.")]
        public override Type GetGeneratedType(CompilerResults results)
        {
            if (this.xamlBuildProviderExtension != null)
            {
                Type result = this.xamlBuildProviderExtension.GetGeneratedType(results);
                if (result != null)
                {
                    return result;
                }
            }
            
            try
            {
                XamlType rootXamlType = GetRootXamlType();
                if (rootXamlType.IsUnknown)
                {
                    StringBuilder typeName = new StringBuilder();
                    AppendTypeName(rootXamlType, typeName);
                    throw FxTrace.Exception.AsError(new TypeLoadException(SR.CouldNotResolveType(typeName)));
                }
                return rootXamlType.UnderlyingType;
            }
            catch (XamlParseException ex)
            {
                throw FxTrace.Exception.AsError(new HttpCompileException(ex.Message, ex));
            }
        }        

        public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.ShutdownAppDomainOnChange;
        }

        private void AppendTypeName(XamlType xamlType, StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(xamlType.PreferredXamlNamespace))
            {
                sb.Append("{");
                sb.Append(xamlType.PreferredXamlNamespace);
                sb.Append("}");
            }
            sb.Append(xamlType.Name);
            if (xamlType.IsGeneric)
            {
                sb.Append("(");
                for (int i = 0; i < xamlType.TypeArguments.Count; i++)
                {
                    AppendTypeName(xamlType.TypeArguments[i], sb);
                    if (i < xamlType.TypeArguments.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
            }
        }     

        XamlType GetRootXamlType()
        {
            try
            {
                using (Stream xamlStream = base.OpenStream())
                {
                    XmlReader xmlReader = XmlReader.Create(xamlStream);
                    XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);

                    // Read to the root object
                    while (xamlReader.Read())
                    {
                        if (xamlReader.NodeType == XamlNodeType.StartObject)
                        {
                            return xamlReader.Type;
                        }
                    }
                    throw FxTrace.Exception.AsError(new HttpCompileException(SR.UnexpectedEof));
                }
            }
            catch (XamlParseException ex)
            {
                throw FxTrace.Exception.AsError(new HttpCompileException(ex.Message, ex));
            }
        }

        void GetXamlBuildProviderExtension(XamlType rootXamlType)
        {
            if (rootXamlType.UnderlyingType == null)
            {
                return;
            }

            this.GetXamlBuildProviderExtensionFactory(rootXamlType);

            if (xamlBuildProviderExtensionFactory != null)
            {
                this.xamlBuildProviderExtension = xamlBuildProviderExtensionFactory.GetXamlBuildProviderExtension();
            }
        }

        void GetXamlBuildProviderExtensionFactory(XamlType rootXamlType)
        {
            if (xamlBuildProviderExtensionFactory != null)
            {
                return;
            }

            // Get the HttpHandler type
            Type httpHandlerType;
            XamlHostingConfiguration.TryGetHttpHandlerType(this.VirtualPath, rootXamlType.UnderlyingType, out httpHandlerType);
            
            if (httpHandlerType != null && typeof(IXamlBuildProviderExtensionFactory).IsAssignableFrom(httpHandlerType))
            {
                xamlBuildProviderExtensionFactory = (IXamlBuildProviderExtensionFactory)Activator.CreateInstance(httpHandlerType,
                    BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null, null, null);
            }
        }
    }
}

