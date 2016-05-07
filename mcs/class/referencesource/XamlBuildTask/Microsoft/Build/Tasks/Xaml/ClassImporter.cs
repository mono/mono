//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows.Markup;
    using XamlBuildTask;

    class ClassImporter
    {
        bool DefaultClassIsPublic;
        MemberVisibility DefaultFieldVisibility;
        string xamlFileName;
        string localAssemblyName;
        string rootNamespace;
        NamespaceTable namespaceTable;

        public ClassImporter(string xamlFileName, string localAssemblyName, string rootNamespace)
        {
            this.xamlFileName = xamlFileName;
            this.localAssemblyName = localAssemblyName;
            this.rootNamespace = rootNamespace;
            this.DefaultClassIsPublic = true;
            this.DefaultFieldVisibility = MemberVisibility.Assembly;
            this.namespaceTable = new NamespaceTable(localAssemblyName);
        }

        // Throws InvalidOperationException at DesignTime: Input XAML contains invalid constructs for generating a class. For example, unexpected content or unknown class or field modifiers.
        public ClassData ReadFromXaml(XamlNodeList nodes)
        {
            if (nodes == null)
            {
                throw FxTrace.Exception.ArgumentNull("nodeList");
            }

            Stack<NamedObject> currentTypes = new Stack<NamedObject>();
            XamlReader reader = nodes.GetReader();
            XamlSchemaContext xsc = reader.SchemaContext;
            XamlNodeList strippedXamlNodes = new XamlNodeList(xsc);
            XamlWriter strippedXamlNodesWriter = strippedXamlNodes.Writer;

            ClassData result = new ClassData()
                {
                    FileName = this.xamlFileName,
                    IsPublic = this.DefaultClassIsPublic,
                    RootNamespace = this.rootNamespace
                };

            // We loop through the provided XAML; for each node, we do two things:
            //  1. If it's a directive that's relevant to x:Class, we extract the data.
            //  2. Unless it's a directive that's exclusively relevant to x:Class, we write it to strippedXamlNodes.
            // The result is two outputs: class data, and stripped XAML that can be used to initialize the
            // an instance of the class.

            bool readNextNode = false;
            while (readNextNode || reader.Read())
            {
                bool stripNodeFromXaml = false;
                readNextNode = false;

                namespaceTable.ManageNamespace(reader);

                switch (reader.NodeType)
                {
                    case XamlNodeType.StartObject:
                        if (result.BaseType == null)
                        {
                            result.BaseType = reader.Type;
                        }
                        currentTypes.Push(new NamedObject()
                            {
                                Type = reader.Type,
                                Visibility = DefaultFieldVisibility,
                            });
                        break;

                    case XamlNodeType.EndObject:
                        currentTypes.Pop();
                        break;

                    case XamlNodeType.StartMember:
                        XamlMember member = reader.Member;

                        if (member.IsDirective)
                        {
                            bool isRootElement = (currentTypes.Count == 1);
                            stripNodeFromXaml = ProcessDirective(reader, result, currentTypes.Peek(), isRootElement, strippedXamlNodes, out readNextNode);
                        }
                        else
                        {
                            NamedObject currentType = currentTypes.Peek();
                            XamlType currentXamlType = currentType.Type;
                            if (currentXamlType.IsUnknown)
                            {
                                result.RequiresCompilationPass2 = true;
                            }
                        }
                        break;

                    case XamlNodeType.EndMember:
                        break;

                    case XamlNodeType.Value:
                        break;

                    case XamlNodeType.NamespaceDeclaration:
                        break;

                    case XamlNodeType.None:
                        break;

                    case XamlNodeType.GetObject:
                        //Push a dummy NamedObject so that it gets popped when you see the corresponding EndObject
                        currentTypes.Push(new NamedObject());
                        break;

                    default:

                        Debug.Fail("Unrecognized XamlNodeType value" + reader.NodeType.ToString());
                        break;
                }

                if (!stripNodeFromXaml)
                {
                    WritestrippedXamlNode(reader, strippedXamlNodesWriter);
                }
            }

            // ClassData.Name should be initialized to a non-null non-empty value if 
            // the file contains x:Class. Throw an error if neither is found.
            if (result.Name == null)
            {
                string xClassDirectiveName = "{" + XamlLanguage.Class.PreferredXamlNamespace + "}" + XamlLanguage.Class.Name;

                throw FxTrace.Exception.AsError(LogInvalidOperationException(null, SR.TaskCannotProcessFileWithoutType(xClassDirectiveName)));
            }

            strippedXamlNodes.Writer.Close();
            strippedXamlNodes = RewriteRootNode(strippedXamlNodes, result.Name, result.Namespace);

            result.EmbeddedResourceXaml = strippedXamlNodes;
            return result;
        }

        IList<XamlType> UpdateTypeArgs(IList<XamlType> typeArgs, XamlSchemaContext xsc)
        {
            if (typeArgs != null)
            {
                IList<XamlType> updatedTypeArgs = new List<XamlType>();
                foreach (var typeArg in typeArgs)
                {
                    IList<XamlType> typeArgTypeArgs = UpdateTypeArgs(typeArg.TypeArguments, xsc);
                    string typeArgXmlns = XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(typeArg.PreferredXamlNamespace, this.localAssemblyName);
                    updatedTypeArgs.Add(new XamlType(typeArgXmlns, typeArg.Name, typeArgTypeArgs, xsc));
                }
                return updatedTypeArgs;
            }
            return typeArgs;
        }

        void WritestrippedXamlNode(XamlReader reader, XamlWriter writer)
        {
            switch (reader.NodeType)
            {
                case XamlNodeType.StartObject:
                    XamlType xamlType = reader.Type;
                    if (xamlType.IsUnknown)
                    {
                        IList<XamlType> typeArgs = UpdateTypeArgs(xamlType.TypeArguments, reader.SchemaContext);
                        string xmlns = XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(xamlType.PreferredXamlNamespace, this.localAssemblyName);
                        xamlType = new XamlType(xmlns, xamlType.Name, typeArgs, reader.SchemaContext);
                    }
                    writer.WriteStartObject(xamlType);
                    break;

                case XamlNodeType.StartMember:
                    XamlMember member = reader.Member;
                    if (member.IsUnknown && !member.IsDirective)
                    {
                        string xmlns = XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(member.DeclaringType.PreferredXamlNamespace, this.localAssemblyName);
                        XamlType memberXamlType = new XamlType(xmlns, member.DeclaringType.Name, member.DeclaringType.TypeArguments, reader.SchemaContext);
                        member = new XamlMember(member.Name, memberXamlType, member.IsAttachable);
                    }
                    writer.WriteStartMember(member);
                    break;

                case XamlNodeType.NamespaceDeclaration:
                    NamespaceDeclaration ns = new NamespaceDeclaration(
                        XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(reader.Namespace.Namespace, this.localAssemblyName),
                        reader.Namespace.Prefix);
                    writer.WriteNamespace(ns);
                    break;

                case XamlNodeType.GetObject:
                case XamlNodeType.EndObject:
                case XamlNodeType.EndMember:
                case XamlNodeType.Value:
                case XamlNodeType.None:
                    writer.WriteNode(reader);
                    break;

                default:
                    Debug.Fail("Unrecognized XamlNodeType value" + reader.NodeType.ToString());
                    break;
            }
        }

        XamlNodeList RewriteRootNode(XamlNodeList strippedXamlNodes, string name, string @namespace)
        {
            // Rewrite the root node to have the name of class declared via x:Class (rather than the base class)
            // Also, for any properties on the root object that are declared in this class, need to rewrite the
            // namespace to include the root namespace, if there is one.

            string oldNamespace = null;
            if (!string.IsNullOrEmpty(this.rootNamespace))
            {
                oldNamespace = @namespace;
                if (!string.IsNullOrEmpty(@namespace))
                {
                    @namespace = this.rootNamespace + "." + @namespace;
                }
                else
                {
                    @namespace = this.rootNamespace;
                }
            }

            string namespaceName = string.Format(CultureInfo.InvariantCulture, "{0}{1};{2}{3}", XamlBuildTaskServices.ClrNamespaceUriNamespacePart, @namespace, XamlBuildTaskServices.ClrNamespaceUriAssemblyPart, this.localAssemblyName);

            XamlReader reader = strippedXamlNodes.GetReader();
            XamlSchemaContext xsc = reader.SchemaContext;
            XamlNodeList newStrippedXamlNodes = new XamlNodeList(xsc);
            XamlWriter writer = newStrippedXamlNodes.Writer;

            int depth = 0;
            XamlType rootXamlType = null;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XamlNodeType.StartObject:
                    case XamlNodeType.GetObject:
                        depth++;
                        break;
                    case XamlNodeType.EndObject:
                        depth--;
                        break;
                }
                if (reader.NodeType == XamlNodeType.StartObject && depth == 1)
                {
                    rootXamlType = new XamlType(namespaceName, name, null, xsc);
                    writer.WriteStartObject(rootXamlType);
                }
                else if (reader.NodeType == XamlNodeType.StartMember && depth == 1 && reader.Member.IsUnknown 
                    && reader.Member.DeclaringType != null && reader.Member.DeclaringType.Name == rootXamlType.Name)
                {
                    string clrNs;
                    XamlMember member = reader.Member;
                    if (XamlBuildTaskServices.TryExtractClrNs(member.PreferredXamlNamespace, out clrNs) &&
                        clrNs == oldNamespace)
                    {
                        // This is a member defined on the document root type, but missing the project root namespace. Fix it.
                        XamlMember newMember = new XamlMember(member.Name, rootXamlType, member.IsAttachable);
                        Fx.Assert(rootXamlType != null, "First StartObject should already have been processed");
                        writer.WriteStartMember(newMember);
                    }
                    else
                    {
                        writer.WriteNode(reader);
                    }
                }
                else
                {
                    writer.WriteNode(reader);
                }
            }

            writer.Close();
            return newStrippedXamlNodes;
        }

        bool ProcessDirective(XamlReader reader, ClassData classData,
            NamedObject currentObject, bool isRootElement, XamlNodeList strippedXamlNodes, out bool readNextNode)
        {
            Fx.Assert(reader.NodeType == XamlNodeType.StartMember, "Current node should be a Start Member Node");

            XamlMember member = reader.Member;
            bool directiveRecognized = false;
            readNextNode = false;

            switch (member.Name)
            {
                case "Name":
                    // Unlike all the other directives that we process, x:Name should be written
                    // to the stripped output.
                    strippedXamlNodes.Writer.WriteStartMember(member);

                    string objectName = ReadAtom(reader, XamlLanguage.Name.Name);
                    if (!objectName.StartsWith(XamlBuildTaskServices.SerializerReferenceNamePrefix,
                        StringComparison.Ordinal))
                    {
                        currentObject.Name = objectName;
                        classData.NamedObjects.Add(currentObject);
                    }

                    strippedXamlNodes.Writer.WriteValue(objectName);
                    strippedXamlNodes.Writer.WriteEndMember();
                    directiveRecognized = true;
                    break;

                case "Class":
                    if (isRootElement)
                    {
                        string fullClassName = ReadAtom(reader, XamlLanguage.Class.Name);
                        SetClassName(fullClassName, classData);
                        directiveRecognized = true;
                    }
                    break;

                case "ClassModifier":
                    if (isRootElement)
                    {
                        string classModifier = ReadAtom(reader, XamlLanguage.ClassModifier.Name);
                        classData.IsPublic = XamlBuildTaskServices.IsPublic(classModifier);
                        directiveRecognized = true;
                    }
                    break;

                case "FieldModifier":
                    string fieldModifier = ReadAtom(reader, XamlLanguage.FieldModifier.Name);
                    currentObject.Visibility = XamlBuildTaskServices.GetMemberVisibility(fieldModifier);
                    directiveRecognized = true;
                    break;

                case "Code":
                    string codeSnippet = ReadAtom(reader, XamlLanguage.Code.Name);
                    classData.CodeSnippets.Add(codeSnippet);
                    directiveRecognized = true;
                    break;

                case "Members":
                    foreach (PropertyData property in ReadProperties(reader.ReadSubtree()))
                    {
                        classData.Properties.Add(property);
                    }
                    if (!classData.RequiresCompilationPass2)
                    {
                        foreach (PropertyData property in classData.Properties)
                        {
                            if (property.Type.IsUnknown)
                            {
                                classData.RequiresCompilationPass2 = true;
                                break;
                            }
                        }
                    }
                    directiveRecognized = true;
                    readNextNode = true;
                    break;

                case "ClassAttributes":
                    foreach (AttributeData attribute in ReadAttributesCollection(reader.ReadSubtree()))
                    {
                        classData.Attributes.Add(attribute);
                    }
                    directiveRecognized = true;
                    readNextNode = true;
                    break;

            }

            if (directiveRecognized == true && readNextNode == false)
            {
                reader.Read();
                Fx.Assert(reader.NodeType == XamlNodeType.EndMember, "Current node should be a XamlEndmember");
            }

            return directiveRecognized;
        }

        private IList<AttributeData> ReadAttributesCollection(XamlReader reader)
        {
            IList<AttributeData> attributes = new List<AttributeData>();
            bool nextNodeRead = false;
            while (nextNodeRead || reader.Read())
            {
                this.namespaceTable.ManageNamespace(reader);
                nextNodeRead = false;
                if (reader.NodeType == XamlNodeType.StartObject && reader.Type != null)
                {
                    AttributeData attribute = null;
                    try
                    {
                        attribute = AttributeData.LoadAttributeData(reader.ReadSubtree(), this.namespaceTable, this.rootNamespace);
                    }
                    catch (InvalidOperationException e)
                    {
                        throw FxTrace.Exception.AsError(LogInvalidOperationException(reader, e.Message));
                    }
                    nextNodeRead = true;
                    attributes.Add(attribute);
                }
            }

            return attributes;
        }

        IEnumerable<PropertyData> ReadProperties(XamlReader reader)
        {
            IDictionary<string, PropertyData> members = new Dictionary<string, PropertyData>();

            bool nextNodeRead = false;
            while (nextNodeRead || reader.Read())
            {
                namespaceTable.ManageNamespace(reader);
                nextNodeRead = false;
                if (reader.NodeType == XamlNodeType.StartObject)
                {
                    if (reader.Type == XamlLanguage.Property)
                    {
                        PropertyData xProperty = LoadProperty(reader.ReadSubtree());
                        nextNodeRead = true;
                        if (members.ContainsKey(xProperty.Name))
                        {
                            throw FxTrace.Exception.AsError(LogInvalidOperationException(reader, SR.DuplicatePropertyDefinition(xProperty.Name)));
                        }
                        members.Add(xProperty.Name, xProperty);
                    }
                }
            }

            return members.Values;
        }

        PropertyData LoadProperty(XamlReader xamlReader)
        {
            if (xamlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xamlReader");
            }

            PropertyData property = new PropertyData();
            while (xamlReader.Read())
            {
                if (xamlReader.NodeType == XamlNodeType.StartMember)
                {
                    XamlMember member = xamlReader.Member;
                    switch (member.Name)
                    {
                        case "Name":
                            property.Name = ReadValueAsString(xamlReader.ReadSubtree());
                            break;
                        case "Type":
                            property.Type = ReadPropertyType(xamlReader.ReadSubtree());
                            break;
                        case "Attributes":
                            foreach (AttributeData attribute in ReadAttributesCollection(xamlReader.ReadSubtree()))
                            {
                                property.Attributes.Add(attribute);
                            }
                            break;
                        case "Modifier":
                            string propertyModifier = ReadValueAsString(xamlReader.ReadSubtree());
                            property.Visibility = XamlBuildTaskServices.GetMemberVisibility(propertyModifier);
                            break;
                        default:
                            // Ignore AttachedProperties on property
                            if (!member.IsAttachable)
                            {
                                throw FxTrace.Exception.AsError(LogInvalidOperationException(xamlReader, SR.UnknownPropertyMember(member.Name)));
                            }                            
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(property.Name))
            {
                throw FxTrace.Exception.AsError(LogInvalidOperationException(xamlReader, SR.PropertyNameRequired));
            }
            if (property.Type == null)
            {
                throw FxTrace.Exception.AsError(LogInvalidOperationException(xamlReader, SR.PropertyTypeRequired(property.Name)));
            }
            return property;
        }

        XamlType ReadPropertyType(XamlReader xamlReader)
        {
            while (xamlReader.Read())
            {
                if (xamlReader.NodeType == XamlNodeType.Value && xamlReader.Value is string)
                {
                    return XamlBuildTaskServices.GetXamlTypeFromString((string)xamlReader.Value, this.namespaceTable, xamlReader.SchemaContext);
                }
            }
            return null;
        }

        string ReadValueAsString(XamlReader xamlReader)
        {
            while (xamlReader.Read())
            {
                if (xamlReader.NodeType == XamlNodeType.Value)
                {
                    return xamlReader.Value as string;
                }
            }
            return string.Empty;
        }

        string ReadAtom(XamlReader reader, string propertyName)
        {
            reader.Read();
            if (reader.NodeType != XamlNodeType.Value)
            {
                throw FxTrace.Exception.AsError(LogInvalidOperationException(reader, SR.TextRepresentationExpected(propertyName)));
            }
            return (string)reader.Value;
        }

        void SetClassName(string fullClassName, ClassData classData)
        {
            int lastIndex = fullClassName.LastIndexOf('.');
            if (lastIndex != -1)
            {
                string classNamespace = fullClassName.Substring(0, lastIndex);
                string className = fullClassName.Substring(lastIndex + 1);

                classData.Name = className;
                classData.Namespace = classNamespace;
            }
            else
            {
                classData.Name = fullClassName;
                classData.Namespace = String.Empty;
            }

            if (string.IsNullOrEmpty(classData.Name))
            {
                throw FxTrace.Exception.AsError(LogInvalidOperationException(null, SR.ClassNameMustBeNonEmpty));
            }
        }

        Exception LogInvalidOperationException(XamlReader reader, string exceptionMessage)
        {
            IXamlLineInfo lineInfo = reader == null ? null : reader as IXamlLineInfo;
            if (lineInfo != null && lineInfo.HasLineInfo)
            {
                return new LoggableException(new InvalidOperationException(exceptionMessage))
                {
                    Source = this.xamlFileName,
                    LineNumber = lineInfo.LineNumber,
                    LinePosition = lineInfo.LinePosition
                };
            }
            else
            {
                return new LoggableException(new InvalidOperationException(exceptionMessage))
                {
                    Source = this.xamlFileName
                };
            }
        }       
    }
}
