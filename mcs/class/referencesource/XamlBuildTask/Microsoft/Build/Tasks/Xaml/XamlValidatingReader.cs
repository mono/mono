//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Reflection;
    using System.Runtime;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    internal class XamlValidatingReader : XamlWrappingReader
    {
        XamlStackWriter _stack = new XamlStackWriter();
        Assembly assembly;
        Type definedType;
        string rootNamespace;
        string localAssemblyName;
        string realAssemblyName;

        // We use this instead of XamlLanguage.Null, because XamlLanguage uses live types
        // where we use ROL
        XamlType xNull;

        public event EventHandler<ValidationEventArgs> OnValidationError;

        public XamlValidatingReader(XamlReader underlyingReader, Assembly assembly, string rootNamespace, string realAssemblyName)
            : base(underlyingReader)
        {
            this.assembly = assembly;
            this.definedType = null;
            this.rootNamespace = rootNamespace;
            this.localAssemblyName = assembly != null ? assembly.GetName().Name : null;
            this.realAssemblyName = realAssemblyName;
            this.xNull = underlyingReader.SchemaContext.GetXamlType(new XamlTypeName(XamlLanguage.Null));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Need to catch and log the exception here so that all the errors, including the exception thrown, are surfaced.")]
        public override bool Read()
        {
            if (!base.Read())
            {
                return false;
            }

            try
            {
                if (_stack.Depth == 0)
                {
                    State_AtRoot();
                }
                else if (_stack.TopFrame.FrameType == XamlStackFrameType.Member)
                {
                    if (_stack.TopFrame.IsSet() && !AllowsMultiple(_stack.TopFrame.Member))
                    {
                        State_ExpectEndMember();
                    }
                    else
                    {
                        State_InsideMember();
                    }
                }
                else
                {
                    if (_stack.TopFrame.FrameType != XamlStackFrameType.Object && _stack.TopFrame.FrameType != XamlStackFrameType.GetObject)
                    {
                        ValidationError(SR.UnexpectedXaml);
                    }
                    State_InsideObject();
                }
            }
            catch (FileLoadException e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                ValidationError(SR.AssemblyCannotBeResolved(XamlBuildTaskServices.FileNotLoaded));
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                ValidationError(e.Message);
            }
            return true;
        }

        protected virtual void ValidationError(string message, params object[] args)
        {
            EventHandler<ValidationEventArgs> handler = OnValidationError;
            if (handler != null)
            {
                string formattedMessage =
                    (args == null || args.Length == 0) ?
                    message : string.Format(CultureInfo.InvariantCulture, message, args);
                handler(this, new ValidationEventArgs(formattedMessage, LineNumber, LinePosition));
            }
        }

        private void State_AtRoot()
        {
            switch (NodeType)
            {
                case XamlNodeType.NamespaceDeclaration:
                    return;
                case XamlNodeType.StartObject:
                    ValidateUnknown(Type);
                    break;
                default:
                    ValidationError(SR.UnexpectedXaml);
                    break;
            }
            _stack.WriteNode(this);
        }

        private void State_InsideObject()
        {
            switch (NodeType)
            {
                case XamlNodeType.NamespaceDeclaration:
                    return;
                case XamlNodeType.StartMember:
                    if (_stack.TopFrame.IsSet(Member))
                    {
                        ValidationError(SR.UnexpectedXaml);
                    }
                    if (_stack.TopFrame.FrameType == XamlStackFrameType.GetObject)
                    {
                        ValidateMemberOnGetObject(Member);
                    }
                    else
                    {
                        ValidateUnknown(Member);
                        ValidateMemberOnType(Member, _stack.TopFrame.Type);
                    }
                    break;
                case XamlNodeType.EndObject:
                    break;
                default:
                    ValidationError(SR.UnexpectedXamlDupMember);
                    break;
            }
            _stack.WriteNode(this);
        }

        private void State_InsideMember()
        {
            switch (NodeType)
            {
                case XamlNodeType.NamespaceDeclaration:
                    
                    return;
                case XamlNodeType.StartObject:
                    ValidateUnknown(Type);
                    ValidateTypeToMemberOnStack(Type);
                    break;
                case XamlNodeType.GetObject:
                    ValidateGetObjectOnMember(_stack.TopFrame.Member);
                    break;
                case XamlNodeType.Value:
                    ValidateValueToMemberOnStack(Value);
                    break;
                case XamlNodeType.EndMember:
                    break;
                default:
                    ValidationError(SR.UnexpectedXaml);
                    break;
            }
            _stack.WriteNode(this);
        }

        private void State_ExpectEndMember()
        {
            if (NodeType != XamlNodeType.EndMember)
            {
                ValidationError(SR.UnexpectedXaml);
            }
            _stack.WriteNode(this);
        }

        private void ValidateGetObjectOnMember(XamlMember member)
        {
            if (member == XamlLanguage.Items || member == XamlLanguage.PositionalParameters)
            {
                ValidationError(SR.UnexpectedXaml);
            }
            else if (!member.IsUnknown && member != XamlLanguage.UnknownContent &&
                !member.Type.IsCollection && !member.Type.IsDictionary)
            {
                ValidationError(SR.UnexpectedXaml);
            }
        }

        private void ValidateMemberOnGetObject(XamlMember member)
        {
            if (member != XamlLanguage.Items)
            {
                ValidationError(SR.UnexpectedXaml);
            }
        }

        private void ValidateMemberOnType(XamlMember member, XamlType type)
        {
            if (member.IsUnknown || type.IsUnknown)
            {
                return;
            }
            if (member.IsDirective)
            {
                if (member == XamlLanguage.Items)
                {
                    if (!type.IsCollection && !type.IsDictionary)
                    {
                        ValidationError(SR.UnexpectedXamlDictionary(member.Name, GetXamlTypeName(_stack.TopFrame.Type)));
                    }
                }
                if (member == XamlLanguage.Class && _stack.Depth > 1)
                {
                    ValidationError(SR.UnexpectedXamlClass);
                }
            }
            else if (member.IsAttachable)
            {
                if (!type.CanAssignTo(member.TargetType))
                {
                    ValidationError(SR.UnexpectedXamlAttachableMember(member.Name, GetXamlTypeName(member.TargetType)));
                }
            }
            else if (!member.IsDirective && !type.CanAssignTo(member.DeclaringType))
            {
                ValidationError(SR.UnexpectedXamlMemberNotAssignable(member.Name, GetXamlTypeName(type)));
            }
        }

        private void ValidateTypeToMemberOnStack(XamlType type)
        {
            if (type.IsUnknown)
            {
                return;
            }
            if (type == this.xNull)
            {
                ValidateValueToMemberOnStack(null);
            }
            XamlMember member = _stack.TopFrame.Member;
            if (member == XamlLanguage.PositionalParameters || type.IsMarkupExtension || member.IsUnknown)
            {
                return;
            }
            if (member == XamlLanguage.Items)
            {
                XamlType collectionType = GetCollectionTypeOnStack();
                if (collectionType == null || collectionType.IsUnknown || collectionType.AllowedContentTypes == null)
                {
                    return;
                }
                if (!collectionType.AllowedContentTypes.Any(contentType => type.CanAssignTo(contentType)))
                {
                    ValidationError(SR.UnassignableCollection(GetXamlTypeName(type), GetXamlTypeName(collectionType.ItemType), GetXamlTypeName(collectionType)));
                }
            }
            else if (member.IsDirective && (member.Type.IsCollection || member.Type.IsDictionary))
            {
                XamlType collectionType = member.Type;
                if (collectionType == null || collectionType.IsUnknown || collectionType.AllowedContentTypes == null)
                {
                    return;
                }
                if (!collectionType.AllowedContentTypes.Any(contentType => type.CanAssignTo(contentType)))
                {
                    ValidationError(SR.UnassignableCollection(GetXamlTypeName(type), GetXamlTypeName(collectionType.ItemType), GetXamlTypeName(collectionType)));
                }
            }
            else if (!type.CanAssignTo(member.Type))
            {
                if (member.DeferringLoader != null)
                {
                    return;
                }
                if (NodeType == XamlNodeType.Value)
                {
                    ValidationError(SR.UnassignableTypes(GetXamlTypeName(type), GetXamlTypeName(member.Type), member.Name));
                }
                else
                {
                    ValidationError(SR.UnassignableTypesObject(GetXamlTypeName(type), GetXamlTypeName(member.Type), member.Name));
                }
            }
        }

        private void ValidateValueToMemberOnStack(object value)
        {
            XamlMember member = _stack.TopFrame.Member;
            if (member.IsUnknown)
            {
                return;
            }
            if (value != null)
            {
                if (member.IsEvent)
                {
                    if (this.definedType != null && this.definedType.GetMethod(value as string, 
                        BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) == null)
                    {
                        ValidationError(SR.UnexpectedXamlEventHandlerNotFound(value, definedType.FullName));
                    }
                    return;
                }
                else if (member == XamlLanguage.Class)
                {
                    string className = value as string;
                    Fx.Assert(!string.IsNullOrEmpty(className), "ClassName cannot be null");
                    if (!string.IsNullOrEmpty(this.rootNamespace))
                    {
                        className = this.rootNamespace + "." + className;
                    }
                    if (this.assembly != null)
                    {
                        this.definedType = this.assembly.GetType(className);
                    }
                    return; 
                }
                else if (member.TypeConverter != null)
                {
                    return;
                }
                XamlType typeOfValue = SchemaContext.GetXamlType(value.GetType());
                ValidateTypeToMemberOnStack(typeOfValue);
            }
            else
            {
                if (member == XamlLanguage.PositionalParameters)
                {
                    return;
                }
                if (member == XamlLanguage.Items)
                {
                    XamlType collectionType = GetCollectionTypeOnStack();
                    if (collectionType == null || collectionType.IsUnknown || collectionType.AllowedContentTypes == null)
                    {
                        return;
                    }
                    if (!collectionType.AllowedContentTypes.Any(contentType => contentType.IsNullable))
                    {
                        ValidationError(SR.UnassignableCollection("(null)", GetXamlTypeName(collectionType.ItemType), GetXamlTypeName(collectionType)));
                    }
                }
                else
                {
                    if (!member.Type.IsNullable)
                    {
                        ValidationError(SR.UnassignableTypes("(null)", GetXamlTypeName(member.Type), member.Name));
                    }
                }
            }
        }

        private bool AllowsMultiple(XamlMember member)
        {
            return
                member == XamlLanguage.Items ||
                member == XamlLanguage.PositionalParameters ||
                member == XamlLanguage.UnknownContent;
        }

        private XamlType GetCollectionTypeOnStack()
        {
            Fx.Assert(_stack.TopFrame.Member == XamlLanguage.Items, "CollectionType should have _Items member");
            XamlType result;
            if (_stack.FrameAtDepth(_stack.Depth - 1).FrameType == XamlStackFrameType.GetObject)
            {
                XamlMember member = _stack.FrameAtDepth(_stack.Depth - 2).Member;
                if (member.IsUnknown)
                {
                    return null;
                }
                result = member.Type;
            }
            else
            {
                result = _stack.FrameAtDepth(_stack.Depth - 1).Type;
            }
            Fx.Assert(result.IsUnknown || result.IsCollection || result.IsDictionary, 
                "Incorrect Collection Type Encountered");
            return result;
        }

        private void ValidateUnknown(XamlMember member)
        {
            if (member == XamlLanguage.UnknownContent)
            {
                ValidationError(SR.MemberUnknownContect(GetXamlTypeName(_stack.TopFrame.Type)));
            }            
            else if (member.IsUnknown)
            {
                bool retryAttachable = false;
                XamlType declaringType = member.DeclaringType;
                if (_stack.Depth == 1 && declaringType.IsUnknown &&
                    !string.IsNullOrEmpty(this.rootNamespace) &&
                    this.definedType != null && declaringType.Name == this.definedType.Name)
                {
                    // Need to handle the case where the namespace of a member on the document root
                    // is missing the project root namespace
                    string clrNs;
                    if (XamlBuildTaskServices.TryExtractClrNs(declaringType.PreferredXamlNamespace, out clrNs))
                    {
                        clrNs = string.IsNullOrEmpty(clrNs) ? this.rootNamespace : this.rootNamespace + "." + clrNs;
                        if (clrNs == this.definedType.Namespace)
                        {
                            declaringType = SchemaContext.GetXamlType(this.definedType);
                            retryAttachable = true;
                        }
                    }
                }
                XamlMember typeMember = declaringType.GetMember(member.Name);
                if (typeMember == null && retryAttachable)
                {
                    typeMember = declaringType.GetAttachableMember(member.Name);
                }
                if (typeMember == null || typeMember.IsUnknown)
                {
                    if (member.IsAttachable)
                    {
                        ValidationError(SR.UnresolvedAttachableMember(GetXamlTypeName(member.DeclaringType) + "." + member.Name));
                    }
                    else if (member.IsDirective)
                    {
                        ValidationError(SR.UnresolvedDirective(member.PreferredXamlNamespace + ":" + member.Name));
                    }
                    else
                    {
                        // Skip if declaring type is unknown as the member unknown error messages become redundant.
                        if (declaringType != null && !declaringType.IsUnknown)
                        {
                            ValidationError(SR.UnresolvedMember(member.Name, GetXamlTypeName(declaringType)));
                        }
                    }
                }
            }
        }

        private void ValidateUnknown(XamlType type)
        {
            if (type.IsUnknown)
            {
                if (type.IsGeneric)
                {
                    ThrowGenericTypeValidationError(type);
                }
                else
                {
                    ThrowTypeValidationError(type);
                }
            }
        }

        private void ThrowGenericTypeValidationError(XamlType type)
        {
            IList<XamlType> unresolvedLeafTypeList = new List<XamlType>();
            XamlBuildTaskServices.GetUnresolvedLeafTypeArg(type, ref unresolvedLeafTypeList);
            if (unresolvedLeafTypeList.Count > 1 || !unresolvedLeafTypeList.Contains(type))
            {
                string fullTypeName = GetXamlTypeName(type);
                ValidationError(SR.UnresolvedGenericType(fullTypeName));
                foreach (XamlType xamlType in unresolvedLeafTypeList)
                {
                    ThrowTypeValidationError(xamlType);
                }
            }
            else
            {
                ThrowTypeValidationError(type);
            }
        }

        private void ThrowTypeValidationError(XamlType type)
        {
            string typeName, assemblyName, ns;
            if (XamlBuildTaskServices.GetTypeNameInAssemblyOrNamespace(type, this.localAssemblyName, this.realAssemblyName, out typeName, out assemblyName, out ns))
            {
                ValidationError(SR.UnresolvedTypeWithAssemblyName(ns + "." + typeName, assemblyName));
            }
            else
            {
                ValidationError(SR.UnresolvedTypeWithNamespace(typeName, ns));
            }
        }

        private string GetXamlTypeName(XamlType type)
        {
            return XamlBuildTaskServices.GetTypeName(type, this.localAssemblyName, this.realAssemblyName);
        }
    }
}
