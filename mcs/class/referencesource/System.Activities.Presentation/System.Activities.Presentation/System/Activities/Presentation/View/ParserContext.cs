//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using Microsoft.VisualBasic.Activities;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Xaml;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Xaml;



    class ParserContext : LocationReferenceEnvironment, IValueSerializerContext, IXamlNameResolver, INamespacePrefixLookup, IXamlNamespaceResolver
    {
        ModelItem baseModelItem;
        EditingContext context;
        IDictionary<string, string> namespaceLookup;

        public ParserContext()
        {
        }

        public ParserContext(ModelItem modelItem)
        {
            this.Initialize(modelItem);
        }

        public IContainer Container
        {
            get { return null; }
        }

        public object Instance
        {
            get;
            internal set;
        }

        public PropertyDescriptor PropertyDescriptor
        {
            get;
            internal set;
        }

        public override Activity Root
        {
            get { return null; }
        }

        IDictionary<string, string> NamespaceLookup
        {
            get
            {
                if (this.namespaceLookup == null)
                {
                    this.namespaceLookup = new Dictionary<string, string>();
                }
                return this.namespaceLookup;
            }
        }

        public bool Initialize(ModelItem modelItem)
        {
            this.baseModelItem = modelItem;
            if (null != modelItem)
            {
                this.context = modelItem.GetEditingContext();
            }
            return (null != this.baseModelItem);
        }

        public override bool IsVisible(LocationReference reference)
        {
            object other = this.Resolve(reference.Name);
            
            return object.ReferenceEquals(other, reference);
        }

        public override bool TryGetLocationReference(string name, out LocationReference result)
        {
            result = (LocationReference)this.Resolve(name);
            return result != null;
        }


        public string GetNamespace(string prefix)
        {
            var nameSpace = this.NamespaceLookup
                .Where(p => string.Equals(p.Value, prefix))
                .Select(p => p.Key)
                .FirstOrDefault();

            return nameSpace;
        }
        
        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            List<NamespaceDeclaration> namespacePrefixes = new List<NamespaceDeclaration>();
            LoadNameSpace(namespacePrefixes);
            return namespacePrefixes;
        }

        public override IEnumerable<LocationReference> GetLocationReferences()
        {
            List<LocationReference> toReturn = new List<LocationReference>();
            if (this.baseModelItem != null)
            {
                List<ModelItem> declaredVariables = VisualBasicEditor.GetVariablesInScope(this.baseModelItem);

                foreach (ModelItem modelItem in declaredVariables)
                {
                    toReturn.Add(modelItem.GetCurrentValue() as LocationReference);
                }
            }
            return toReturn;
        }

        public object Resolve(string name)
        {
            IEnumerable<LocationReference> variables = this.GetLocationReferences();
            return variables.FirstOrDefault<LocationReference>(p =>
            {
                return p != null && p.Name != null && p.Name.Equals(name);
            });
        }
        
        public object Resolve(string name, out bool isFullyInitialized)
        {
            object result = Resolve(name);
            isFullyInitialized = (result != null);
            return result;
        }

        public bool IsFixupTokenAvailable
        {
            get
            {
                
                return false;
            }
        }

        internal IEnumerable<string> Namespaces
        {
            get
            {
                var namespacesToReturn = new HashSet<string>();
                //combine default import namespaces
                foreach (var import in VisualBasicSettings.Default.ImportReferences)
                {
                    namespacesToReturn.Add(import.Import);
                }
                //with custom ones, defined in user provided assemblies
                if (null != this.namespaceLookup)
                {
                    foreach (var nameSpace in this.namespaceLookup.Keys)
                    {
                        //out of full namespace declaration (i.e. "clr-namespace:<namespace>;assembly=<assembly>"
                        //get clear namespace name
                        int startIndex = nameSpace.IndexOf(":", StringComparison.Ordinal);
                        int endIndex = nameSpace.IndexOf(";", StringComparison.Ordinal);
                        if (startIndex >= 0 && endIndex >= 0)
                        {
                            string clrNamespace = nameSpace.Substring(startIndex + 1, endIndex - startIndex - 1);
                            namespacesToReturn.Add(clrNamespace);
                        }
                    }
                }

                ImportedNamespaceContextItem importedNamespaces = this.context.Items.GetValue<ImportedNamespaceContextItem>();
                namespacesToReturn.UnionWith(importedNamespaces.ImportedNamespaces);
                //return all namespaces
                return namespacesToReturn;
            }
        }

        public object GetFixupToken(IEnumerable<string> names)
        {
            return null;
        }

        public object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly)
        {
            return null;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IXamlNameResolver)
                || serviceType == typeof(INamespacePrefixLookup)
                || serviceType == typeof(IXamlNamespaceResolver))
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public ValueSerializer GetValueSerializerFor(Type type)
        {
            return null;
        }

        public ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor)
        {
            return null;
        }

        public string LookupPrefix(string ns)
        {
            //get reference to namespace lookup dictionary (create one if necessary)
            var lookupTable = this.NamespaceLookup;
            string prefix;
            //check if given namespace is already registered
            if (!lookupTable.TryGetValue(ns, out prefix))
            {
                //no, create a unique prefix
                prefix = string.Format(CultureInfo.InvariantCulture, "__{0}", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5));
                //and store value in the dictionary
                lookupTable[ns] = prefix;
            }
            //return prefix
            return prefix;
        }

        public void OnComponentChanged()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public bool OnComponentChanging()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        void LoadNameSpace(List<NamespaceDeclaration> result)
        {
            if (null == this.context)
            {
                Fx.Assert("EditingContext is null");
                return;
            }
            AssemblyContextControlItem assemblyContext = this.context.Items.GetValue<AssemblyContextControlItem>();
            if (null == assemblyContext)
            {
                Fx.Assert("AssemblyContextControlItem not defined in EditingContext.Items");
                return;
            }
            if (null != assemblyContext.LocalAssemblyName)
            {
                result.Add(GetEntry(assemblyContext.LocalAssemblyName));
            }
            if (null != assemblyContext.ReferencedAssemblyNames)
            {
                foreach (AssemblyName name in assemblyContext.ReferencedAssemblyNames)
                {
                    result.Add(GetEntry(name));
                }
            }
        }

        NamespaceDeclaration GetEntry(AssemblyName name)
        {
            string ns =
                string.Format(CultureInfo.InvariantCulture, "clr-namespace:{0};assembly={1}",
                Guid.NewGuid().ToString().Replace('-', '_'), name.Name);
            return new NamespaceDeclaration(ns, Guid.NewGuid().ToString());
        }     
        
        IEnumerable<KeyValuePair<string, object>> IXamlNameResolver.GetAllNamesAndValuesInScope()
        {
            return null;
        }
        
        event EventHandler IXamlNameResolver.OnNameScopeInitializationComplete
        {
            add { }
            remove { }
        }

    }


}
