namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Diagnostics.CodeAnalysis;

    #region Class WorkflowMarkupSerializationManager
    public class WorkflowMarkupSerializationManager : IDesignerSerializationManager
    {
        private Assembly localAssembly = null;
        private int writerDepth = 0;
        private ContextStack workflowMarkupStack = new ContextStack();
        // Stack to keep a list of objects being serialized, to avoid stack overflow
        private Stack serializationStack = new Stack();
        private IDesignerSerializationManager serializationManager;
        private bool designMode = false;
        internal event EventHandler<WorkflowMarkupElementEventArgs> FoundDefTag;

        //These are temporary variables for speedy lookup
        private Dictionary<int, WorkflowMarkupSerializerMapping> clrNamespaceBasedMappings = new Dictionary<int, WorkflowMarkupSerializerMapping>();
        private Dictionary<string, List<WorkflowMarkupSerializerMapping>> xmlNamespaceBasedMappings = new Dictionary<string, List<WorkflowMarkupSerializerMapping>>();
        private Dictionary<string, List<WorkflowMarkupSerializerMapping>> prefixBasedMappings = new Dictionary<string, List<WorkflowMarkupSerializerMapping>>();
        private List<WorkflowMarkupSerializer> extendedPropertiesProviders;
        private Dictionary<XmlQualifiedName, Type> cachedXmlQualifiedNameTypes = new Dictionary<XmlQualifiedName, Type>();

        public WorkflowMarkupSerializationManager(IDesignerSerializationManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.serializationManager = manager;
            AddSerializationProvider(new WellKnownTypeSerializationProvider());

            // push standard mappings
            AddMappings(WorkflowMarkupSerializerMapping.WellKnownMappings);

            //Set the local assembly correctly
            ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider != null)
                LocalAssembly = typeProvider.LocalAssembly;

            this.designMode = (manager.GetService(typeof(ITypeResolutionService)) != null);
        }

        public ContextStack Context
        {
            get
            {
                return this.serializationManager.Context;
            }
        }

        internal Stack SerializationStack
        {
            get
            {
                return this.serializationStack;
            }
        }

        public void ReportError(object errorInformation)
        {
            if (errorInformation == null)
                throw new ArgumentNullException("errorInformation");

            this.serializationManager.ReportError(errorInformation);
        }

        protected internal IDesignerSerializationManager SerializationManager
        {
            get
            {
                return this.serializationManager;
            }

            set
            {
                this.serializationManager = value;
                this.serializationManager.AddSerializationProvider(new WellKnownTypeSerializationProvider());
            }
        }

        public void AddSerializationProvider(IDesignerSerializationProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            this.serializationManager.AddSerializationProvider(provider);
        }

        public void RemoveSerializationProvider(IDesignerSerializationProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            this.serializationManager.RemoveSerializationProvider(provider);
        }

        public Assembly LocalAssembly
        {
            get
            {
                return this.localAssembly;
            }
            set
            {
                this.localAssembly = value;
            }
        }

        #region Public Methods
        public virtual XmlQualifiedName GetXmlQualifiedName(Type type, out string prefix)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            string typeNamespace = (type.Namespace != null) ? type.Namespace : String.Empty;
            string assemblyName = (type.Assembly != null && type.Assembly != this.localAssembly) ? type.Assembly.FullName : String.Empty;

            WorkflowMarkupSerializerMapping mappingForType = null;
            int key = typeNamespace.GetHashCode() ^ assemblyName.GetHashCode();
            if (!this.clrNamespaceBasedMappings.TryGetValue(key, out mappingForType))
            {
                IList<WorkflowMarkupSerializerMapping> collectedMappings = null;
                WorkflowMarkupSerializerMapping.GetMappingFromType(this, type, out mappingForType, out collectedMappings);
                AddMappings(new List<WorkflowMarkupSerializerMapping>(new WorkflowMarkupSerializerMapping[] { mappingForType }));
                AddMappings(collectedMappings);
            }

            string typeName = WorkflowMarkupSerializer.EnsureMarkupExtensionTypeName(type);

            //Make sure that while writting the workflow namespaces will always be the default
            prefix = (mappingForType.Prefix.Equals(StandardXomlKeys.WorkflowPrefix, StringComparison.Ordinal)) ? String.Empty : mappingForType.Prefix;
            return new XmlQualifiedName(typeName, mappingForType.XmlNamespace);
        }

        public virtual Type GetType(XmlQualifiedName xmlQualifiedName)
        {
            if (xmlQualifiedName == null)
                throw new ArgumentNullException("xmlQualifiedName");

            string xmlns = xmlQualifiedName.Namespace;
            string typeName = WorkflowMarkupSerializer.EnsureMarkupExtensionTypeName(xmlQualifiedName);

            Type resolvedType = null;

            // first check our cache 
            cachedXmlQualifiedNameTypes.TryGetValue(xmlQualifiedName, out resolvedType);

            if (resolvedType == null)
            {
                // lookup in well known types
                resolvedType = WorkflowMarkupSerializerMapping.ResolveWellKnownTypes(this, xmlns, typeName);
            }

            if (resolvedType == null)
            {
                //Lookup existing mapping
                List<WorkflowMarkupSerializerMapping> xmlnsMappings = null;
                if (!this.xmlNamespaceBasedMappings.TryGetValue(xmlns, out xmlnsMappings))
                {
                    IList<WorkflowMarkupSerializerMapping> matchingMappings = null;
                    IList<WorkflowMarkupSerializerMapping> collectedMappings = null;
                    WorkflowMarkupSerializerMapping.GetMappingsFromXmlNamespace(this, xmlns, out matchingMappings, out collectedMappings);
                    AddMappings(matchingMappings);
                    AddMappings(collectedMappings);

                    xmlnsMappings = new List<WorkflowMarkupSerializerMapping>(matchingMappings);
                }

                foreach (WorkflowMarkupSerializerMapping xmlnsMapping in xmlnsMappings)
                {
                    string assemblyName = xmlnsMapping.AssemblyName;
                    string clrNamespace = xmlnsMapping.ClrNamespace;

                    // append dot net namespace name
                    string fullTypeName = xmlQualifiedName.Name;
                    if (clrNamespace.Length > 0)
                        fullTypeName = clrNamespace + "." + xmlQualifiedName.Name;

                    // Work around  for component model assembly
                    if (assemblyName.Equals(Assembly.GetExecutingAssembly().FullName, StringComparison.Ordinal))
                    {
                        resolvedType = Assembly.GetExecutingAssembly().GetType(fullTypeName);
                    }
                    else if (assemblyName.Length == 0)
                    {
                        if (this.localAssembly != null)
                            resolvedType = this.localAssembly.GetType(fullTypeName);
                    }
                    else
                    {
                        string assemblyQualifiedName = fullTypeName;
                        if (assemblyName.Length > 0)
                            assemblyQualifiedName += (", " + assemblyName);

                        // now grab the actual type
                        try
                        {
                            resolvedType = GetType(assemblyQualifiedName);
                        }
                        catch
                        {
                            // 



                        }

                        if (resolvedType == null)
                        {
                            resolvedType = GetType(fullTypeName);
                            if (resolvedType != null && !resolvedType.AssemblyQualifiedName.Equals(assemblyQualifiedName, StringComparison.Ordinal))
                                resolvedType = null;
                        }
                    }

                    //We found the type
                    if (resolvedType != null)
                    {
                        cachedXmlQualifiedNameTypes[xmlQualifiedName] = resolvedType;
                        break;
                    }
                }
            }

            return resolvedType;
        }
        #endregion

        #region WorkflowMarkupSerializationManager Overrides
        public object GetSerializer(Type objectType, Type serializerType)
        {
            return serializationManager.GetSerializer(objectType, serializerType);
        }


        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)", Justification = "Not a security threat since it is called in design time scenarios")]
        public virtual Type GetType(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            // try serialization manager
            Type type = null;


            if (this.designMode)
            {
                try
                {
                    type = this.serializationManager.GetType(typeName);
                }
                catch
                {
                    //Debug.Assert(false, "VSIP framwork threw exception on resolving type." + e.ToString());
                }
            }

            if (type == null)
            {
                // If this is a design time type, we need to get it from our type provider.
                ITypeProvider typeProvider = this.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                    type = typeProvider.GetType(typeName, false);
            }


            if (type != null)
                return type;

            // try loading the assembly directly
            string assemblyName = string.Empty;
            int commaIndex = typeName.IndexOf(",");
            string fullyQualifiedTypeName = typeName;
            if (commaIndex > 0)
            {
                assemblyName = typeName.Substring(commaIndex + 1);
                typeName = typeName.Substring(0, commaIndex);
            }

            Assembly assembly = null;
            assemblyName = assemblyName.Trim();
            if (assemblyName.Length > 0)
            {
                if (assemblyName.IndexOf(',') >= 0)
                {
                    try
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    catch
                    {
                        // 


                    }
                }

                typeName = typeName.Trim();
                if (assembly != null)
                    type = assembly.GetType(typeName, false);
                else
                    type = Type.GetType(fullyQualifiedTypeName, false);
            }
            return type;
        }
        #endregion

        #region Helpers
        internal int WriterDepth
        {
            get
            {
                return this.writerDepth;
            }
            set
            {
                this.writerDepth = value;
            }
        }

        internal ContextStack WorkflowMarkupStack
        {
            get
            {
                return this.workflowMarkupStack;
            }
        }

        internal void FireFoundDefTag(WorkflowMarkupElementEventArgs args)
        {
            if (this.FoundDefTag != null)
                this.FoundDefTag(this, args);
        }

        internal IDictionary<int, WorkflowMarkupSerializerMapping> ClrNamespaceBasedMappings
        {
            get
            {
                return this.clrNamespaceBasedMappings;
            }
        }

        internal IDictionary<string, List<WorkflowMarkupSerializerMapping>> XmlNamespaceBasedMappings
        {
            get
            {
                return this.xmlNamespaceBasedMappings;
            }
        }

        internal Dictionary<string, List<WorkflowMarkupSerializerMapping>> PrefixBasedMappings
        {
            get
            {
                return this.prefixBasedMappings;
            }
        }

        internal void AddMappings(IList<WorkflowMarkupSerializerMapping> mappingsToAdd)
        {
            foreach (WorkflowMarkupSerializerMapping mapping in mappingsToAdd)
            {
                if (!this.clrNamespaceBasedMappings.ContainsKey(mapping.GetHashCode()))
                    this.clrNamespaceBasedMappings.Add(mapping.GetHashCode(), mapping);

                List<WorkflowMarkupSerializerMapping> xmlnsMappings = null;
                if (!this.xmlNamespaceBasedMappings.TryGetValue(mapping.XmlNamespace, out xmlnsMappings))
                {
                    xmlnsMappings = new List<WorkflowMarkupSerializerMapping>();
                    this.xmlNamespaceBasedMappings.Add(mapping.XmlNamespace, xmlnsMappings);
                }
                xmlnsMappings.Add(mapping);

                List<WorkflowMarkupSerializerMapping> prefixMappings = null;
                if (!this.prefixBasedMappings.TryGetValue(mapping.Prefix, out prefixMappings))
                {
                    prefixMappings = new List<WorkflowMarkupSerializerMapping>();
                    this.prefixBasedMappings.Add(mapping.Prefix, prefixMappings);
                }
                prefixMappings.Add(mapping);
            }
        }

        internal IList<WorkflowMarkupSerializer> ExtendedPropertiesProviders
        {
            get
            {
                if (this.extendedPropertiesProviders == null)
                    this.extendedPropertiesProviders = new List<WorkflowMarkupSerializer>();
                return this.extendedPropertiesProviders;
            }
        }

        internal ExtendedPropertyInfo[] GetExtendedProperties(object extendee)
        {
            List<ExtendedPropertyInfo> extendedProperties = new List<ExtendedPropertyInfo>();
            foreach (WorkflowMarkupSerializer markupSerializer in ExtendedPropertiesProviders)
                extendedProperties.AddRange(markupSerializer.GetExtendedProperties(this, extendee));
            return extendedProperties.ToArray();
        }
        #endregion

        #region IDesignerSerializationManager Implementation
        object IDesignerSerializationManager.CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
        {
            return this.serializationManager.CreateInstance(type, arguments, name, addToContainer);
        }

        object IDesignerSerializationManager.GetInstance(string name)
        {
            return this.serializationManager.GetInstance(name);
        }

        string IDesignerSerializationManager.GetName(object value)
        {
            return this.serializationManager.GetName(value);
        }

        PropertyDescriptorCollection IDesignerSerializationManager.Properties
        {
            get { return this.serializationManager.Properties; }
        }

        event ResolveNameEventHandler IDesignerSerializationManager.ResolveName { add { } remove { } }

        event EventHandler IDesignerSerializationManager.SerializationComplete { add { } remove { } }

        void IDesignerSerializationManager.SetName(object instance, string name)
        {
            this.serializationManager.SetName(instance, name);
        }
        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            return this.serializationManager.GetService(serviceType);
        }

        #endregion

        #region Class WellKnownTypeSerializationProvider
        private sealed class WellKnownTypeSerializationProvider : IDesignerSerializationProvider
        {
            #region IDesignerSerializationProvider Members
            object IDesignerSerializationProvider.GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
            {
                if (serializerType == typeof(WorkflowMarkupSerializer) && objectType != null)
                {
                    if (TypeProvider.IsAssignable(typeof(ICollection<string>), objectType) && TypeProvider.IsAssignable(objectType, typeof(List<string>)) && !TypeProvider.IsAssignable(typeof(Array), objectType))
                        return new StringCollectionMarkupSerializer();
                    else if (typeof(Color).IsAssignableFrom(objectType))
                        return new ColorMarkupSerializer();
                    else if (typeof(Size).IsAssignableFrom(objectType))
                        return new SizeMarkupSerializer();
                    else if (typeof(Point).IsAssignableFrom(objectType))
                        return new PointMarkupSerializer();
                    else if (objectType == typeof(CodeTypeReference))
                        return new CodeTypeReferenceSerializer();
                }

                return null;
            }
            #endregion
        }
        #endregion
    }
    #endregion

}

