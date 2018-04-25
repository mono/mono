using System;
using System.Collections;
using System.Workflow.ComponentModel.Compiler;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Workflow.ComponentModel.Serialization;
using System.ComponentModel.Design;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

namespace System.Workflow.ComponentModel.Serialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class XmlnsDefinitionAttribute : Attribute
    {
        public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");

            if (clrNamespace == null)
                throw new ArgumentNullException("clrNamespace");

            this.xmlNamespace = xmlNamespace;
            this.clrNamespace = clrNamespace;
        }
        public string XmlNamespace
        {
            get { return this.xmlNamespace; }
        }
        public string ClrNamespace
        {
            get { return this.clrNamespace; }
        }
        public string AssemblyName
        {
            get { return this.assemblyName; }
            set { this.assemblyName = value; }
        }

        private string xmlNamespace;
        private string clrNamespace;
        private string assemblyName;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class XmlnsPrefixAttribute : Attribute
    {
        private string xmlNamespace;
        private string prefix;

        public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (prefix == null)
                throw new ArgumentNullException("prefix");

            this.xmlNamespace = xmlNamespace;
            this.prefix = prefix;
        }
        public string XmlNamespace
        {
            get { return this.xmlNamespace; }
        }
        public string Prefix
        {
            get { return this.prefix; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RuntimeNamePropertyAttribute : Attribute
    {
        private string name = null;
        public RuntimeNamePropertyAttribute(string name)
        {
            this.name = name;
        }
        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        private string name;
        public ContentPropertyAttribute() { }
        public ContentPropertyAttribute(string name)
        {
            this.name = name;
        }
        public string Name
        {
            get { return this.name; }
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ConstructorArgumentAttribute : Attribute
    {
        private string argumentName;

        public ConstructorArgumentAttribute(string argumentName)
        {
            this.argumentName = argumentName;
        }
        public string ArgumentName
        {
            get { return this.argumentName; }
        }
    }

    public abstract class MarkupExtension
    {
        public abstract object ProvideValue(IServiceProvider provider);
    }

    [DesignerSerializer(typeof(MarkupExtensionSerializer), typeof(WorkflowMarkupSerializer))]
    internal sealed class NullExtension : MarkupExtension
    {
        public NullExtension() { }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }
    }

    [DesignerSerializer(typeof(TypeExtensionSerializer), typeof(WorkflowMarkupSerializer))]
    internal sealed class TypeExtension : MarkupExtension
    {
        private string typeName;
        private Type type;

        public TypeExtension() { }

        public TypeExtension(string type)
        {
            if (type == null)
                throw new ArgumentNullException("typeName");

            this.typeName = type;
        }
        public TypeExtension(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.type = type;
        }
        public override object ProvideValue(IServiceProvider provider)
        {
            if (this.type != null)
                return this.type;

            if (provider == null)
                throw new ArgumentNullException("provider");

            if (this.typeName == null)
                throw new InvalidOperationException("typename");

            WorkflowMarkupSerializationManager manager = provider as WorkflowMarkupSerializationManager;
            if (manager == null)
                throw new ArgumentNullException("provider");

            XmlReader reader = manager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader == null)
            {
                Debug.Assert(false);
                return this.typeName;
            }

            string typename = this.typeName.Trim();
            string prefix = String.Empty;
            int typeIndex = typename.IndexOf(':');
            if (typeIndex >= 0)
            {
                prefix = typename.Substring(0, typeIndex);
                typename = typename.Substring(typeIndex + 1);
                type = manager.GetType(new XmlQualifiedName(typename, reader.LookupNamespace(prefix)));
                if (type != null)
                    return type;

                // To Support types whose assembly is not available, we need to still resolve the clr namespace
                List<WorkflowMarkupSerializerMapping> xmlnsMappings = null;
                if (manager.XmlNamespaceBasedMappings.TryGetValue(reader.LookupNamespace(prefix), out xmlnsMappings) && xmlnsMappings != null && xmlnsMappings.Count > 0)
                    return xmlnsMappings[0].ClrNamespace + "." + typename;
                else
                    return typename;
            }
            type = manager.GetType(new XmlQualifiedName(typename, reader.LookupNamespace(string.Empty)));

            // To Support Beta2 format
            if (type == null)
            {
                ITypeProvider typeProvider = provider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                    type = typeProvider.GetType(typename);

                // If not design mode, get the value from serialization manager
                // At design time, we need to get the type from ITypeProvider else
                // we need to store the string in the hashtable we maintain internally
                if (type == null && manager.GetService(typeof(ITypeResolutionService)) == null)
                    type = manager.SerializationManager.GetType(typename);
            }
            if (type != null)
                return type;

            return this.typeName;
        }

        [DefaultValue(null)]
        [ConstructorArgument("type")]
        public string TypeName
        {
            get
            {
                if (this.type != null)
                    return this.type.FullName;
                return this.typeName;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                this.typeName = value;
            }
        }
        internal Type Type
        {
            get { return this.type; }
        }
    }

    [ContentProperty("Items")]
    internal sealed class ArrayExtension : MarkupExtension
    {
        private ArrayList arrayElementList = new ArrayList();
        private Type arrayType;

        public ArrayExtension()
        {
        }

        public ArrayExtension(Type arrayType)
        {
            if (arrayType == null)
                throw new ArgumentNullException("arrayType");

            this.arrayType = arrayType;
        }

        public ArrayExtension(Array elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");

            arrayElementList.AddRange(elements);
            this.arrayType = elements.GetType().GetElementType();
        }

        //

        public Type Type
        {
            get
            {
                return this.arrayType;
            }

            set
            {
                this.arrayType = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList Items
        {
            get
            {
                return arrayElementList;
            }
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (this.arrayType == null)
                throw new InvalidOperationException("ArrayType needs to be set.");

            object retArray = null;
            try
            {
                retArray = arrayElementList.ToArray(this.arrayType);
            }
            catch (System.InvalidCastException)
            {
                //



                throw new InvalidOperationException();
            }

            return retArray;
        }
    }
}
