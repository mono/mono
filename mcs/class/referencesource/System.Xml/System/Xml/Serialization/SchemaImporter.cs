//------------------------------------------------------------------------------
// <copyright file="SchemaImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Serialization  {

    using System;
    using System.Xml.Schema;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Configuration;
    using System.Xml.Serialization.Configuration;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Security.Permissions;
    using System.Xml.Serialization.Advanced;

#if DEBUG
    using System.Diagnostics;
#endif

    /// <include file='doc\SchemaImporter.uex' path='docs/doc[@for="SchemaImporter"]/*' />
    ///<internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class SchemaImporter {
        XmlSchemas schemas;
        StructMapping root;
        CodeGenerationOptions options;
        CodeDomProvider codeProvider;
        TypeScope scope;
        ImportContext context;
        bool rootImported;
        NameTable typesInUse;
        NameTable groupsInUse;
        SchemaImporterExtensionCollection extensions;

        internal SchemaImporter(XmlSchemas schemas, CodeGenerationOptions options, CodeDomProvider codeProvider, ImportContext context) {
            if (!schemas.Contains(XmlSchema.Namespace)) {
                schemas.AddReference(XmlSchemas.XsdSchema);
                schemas.SchemaSet.Add(XmlSchemas.XsdSchema);
            }
            if (!schemas.Contains(XmlReservedNs.NsXml)) {
                schemas.AddReference(XmlSchemas.XmlSchema);
                schemas.SchemaSet.Add(XmlSchemas.XmlSchema);
            }
            this.schemas = schemas;
            this.options = options;
            this.codeProvider = codeProvider;
            this.context = context;
            Schemas.SetCache(Context.Cache, Context.ShareTypes);

            SchemaImporterExtensionsSection section = PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SchemaImporterExtensionsSectionPath) as SchemaImporterExtensionsSection;
            if (section != null)
                extensions = section.SchemaImporterExtensionsInternal;
            else
                extensions = new SchemaImporterExtensionCollection();
        }

        internal ImportContext Context {
            get { 
                if (context == null)
                    context = new ImportContext();
                return context;
            }
        }

        internal CodeDomProvider CodeProvider {
            get {
                if (codeProvider == null)
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                return codeProvider;
            }
        }

        public SchemaImporterExtensionCollection Extensions {
            get {
                if (extensions == null)
                    extensions = new SchemaImporterExtensionCollection();
                return extensions;
            }
        }

        internal Hashtable ImportedElements {
            get { return Context.Elements; }
        }

        internal Hashtable ImportedMappings {
            get { return Context.Mappings; }
        }

        internal CodeIdentifiers TypeIdentifiers {
            get { return Context.TypeIdentifiers; }
        }

        internal XmlSchemas Schemas {
            get { 
                if (schemas == null)
                    schemas = new XmlSchemas();
                return schemas;
            }
        }

        internal TypeScope Scope {
            get { 
                if (scope == null)
                    scope = new TypeScope();
                return scope;
            }
        }

        internal NameTable GroupsInUse {
            get { 
                if (groupsInUse == null)
                    groupsInUse = new NameTable();
                return groupsInUse;
            }
        }

        internal NameTable TypesInUse {
            get { 
                if (typesInUse == null)
                    typesInUse = new NameTable();
                return typesInUse;
            }
        }

        internal CodeGenerationOptions Options {
            get { return options; }
        }

        internal void MakeDerived(StructMapping structMapping, Type baseType, bool baseTypeCanBeIndirect) {
            structMapping.ReferencedByTopLevelElement = true;
            TypeDesc baseTypeDesc;
            if (baseType != null) {
                baseTypeDesc = Scope.GetTypeDesc(baseType);
                if (baseTypeDesc != null) {
                    TypeDesc typeDescToChange = structMapping.TypeDesc;
                    if (baseTypeCanBeIndirect) {
                        // if baseTypeCanBeIndirect is true, we apply the supplied baseType to the top of the
                        // inheritance chain, not necessarily directly to the imported type.
                        while (typeDescToChange.BaseTypeDesc != null && typeDescToChange.BaseTypeDesc != baseTypeDesc)
                            typeDescToChange = typeDescToChange.BaseTypeDesc;
                    }
                    if (typeDescToChange.BaseTypeDesc != null && typeDescToChange.BaseTypeDesc != baseTypeDesc)
                        throw new InvalidOperationException(Res.GetString(Res.XmlInvalidBaseType, structMapping.TypeDesc.FullName, baseType.FullName, typeDescToChange.BaseTypeDesc.FullName));
                    typeDescToChange.BaseTypeDesc = baseTypeDesc;
                }
            }
        }

        internal string GenerateUniqueTypeName(string typeName) {
            typeName = CodeIdentifier.MakeValid(typeName);
            return TypeIdentifiers.AddUnique(typeName, typeName);
        }

        StructMapping CreateRootMapping() {
            TypeDesc typeDesc = Scope.GetTypeDesc(typeof(object));
            StructMapping mapping = new StructMapping();
            mapping.TypeDesc = typeDesc;
            mapping.Members = new MemberMapping[0];
            mapping.IncludeInSchema = false;
            mapping.TypeName = Soap.UrType;
            mapping.Namespace = XmlSchema.Namespace;

            return mapping;
        }

        internal StructMapping GetRootMapping() {
            if (root == null)
                root = CreateRootMapping();
            return root;
        }

        internal StructMapping ImportRootMapping() {
            if (!rootImported) {
                rootImported = true;
                ImportDerivedTypes(XmlQualifiedName.Empty);
            }
            return GetRootMapping();
        }

        internal abstract void ImportDerivedTypes(XmlQualifiedName baseName);

        internal void AddReference(XmlQualifiedName name, NameTable references, string error) {
            if (name.Namespace == XmlSchema.Namespace)
                return;
            if (references[name] != null) {
                throw new InvalidOperationException(Res.GetString(error, name.Name, name.Namespace));
            }
            references[name] = name;
        }

        internal void RemoveReference(XmlQualifiedName name, NameTable references) {
            references[name] = null;
        }

        internal void AddReservedIdentifiersForDataBinding(CodeIdentifiers scope)
        {
            if ((options & CodeGenerationOptions.EnableDataBinding) != 0)
            {
                scope.AddReserved(CodeExporter.PropertyChangedEvent.Name);
                scope.AddReserved(CodeExporter.RaisePropertyChangedEventMethod.Name);
            }
        }

    }
}
