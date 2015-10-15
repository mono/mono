//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


namespace System.Activities.Presentation.Xaml
{
    using System;
    using System.Activities;
    using System.Activities.Debugger;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Xaml;
    using System.Xaml.Schema;
    using Microsoft.Activities.Presentation.Xaml;
    using NameReferenceConverter = System.Windows.Markup.NameReferenceConverter;

    // This XamlWriter wraps an ObjectWriter to provide limited error tolerance, as follows:
    // - Buffer the node stream as a tree of fragments (one for each activity) and statically validate it.
    // - Write the node stream out to an ObjectWriter, wrapping any subtrees that failed validation 
    //   in an ErrorActivity.
    // - If validation fails at the root level, we don't load any object, we only provide validation errors.
    // We are only tolerant of errors that can be detected statically; i.e. we are not resillient
    // to XamlObjectWriter exceptions.

    internal class ErrorTolerantObjectWriter : XamlWriter, IXamlLineInfoConsumer
    {
        // We store three main types of state:
        // - The current state of the nodestream (XamlFrames), for validation purposes.
        // - Buffered XamlFragments.
        // - NameScopes, which have 

        // Current state of the nodestream, for performing validation
        WalkableStack<XamlFrame> xamlStack;

        // Root of the tree of completed fragments (except for the root, which may be in progress)
        XamlFragment rootFragment;

        // Stack of in-progress fragments
        Stack<XamlFragment> fragmentStack;

        // Stack of in-progress namescopes; fragments can overlap multiple namescopes, and vice versa
        Stack<NameScopeFrame> nameScopeStack;

        // Completed namescopes, saved so we can resolve all references at end of parse
        List<NameScopeFrame> poppedNameScopes;

        // Pending NS declarations whose corresponding StartObject hasn't been written yet
        NamespaceStackNode pendingNamespaces;

        XamlObjectWriter objectWriter;
        XamlType typeOfActivity;
        XamlMember nameOfReference;
        XamlValueConverter<XamlDeferringLoader> activityLoader;
        int lineNumber, linePosition;

        public string LocalAssemblyName { get; set; }

        public IList<XamlLoadErrorInfo> LoadErrors { get; private set; }

        public ErrorTolerantObjectWriter(XamlSchemaContext schemaContext)
        {
            this.xamlStack = new WalkableStack<XamlFrame>();
            this.rootFragment = new XamlFragment(schemaContext);
            this.fragmentStack = new Stack<XamlFragment>();
            this.fragmentStack.Push(this.rootFragment);
            this.nameScopeStack = new Stack<NameScopeFrame>();
            this.nameScopeStack.Push(new NameScopeFrame(null));
            this.poppedNameScopes = new List<NameScopeFrame>();
            this.objectWriter = new XamlObjectWriter(schemaContext);
            this.typeOfActivity = objectWriter.SchemaContext.GetXamlType(typeof(Activity));
            this.nameOfReference = XamlLanguage.Reference.GetMember("Name");
            this.activityLoader = typeOfActivity.GetMember("Implementation").DeferringLoader;
        }

        public object Result { get; private set; }

        public override XamlSchemaContext SchemaContext { get { return this.objectWriter.SchemaContext; } }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (this.rootFragment.HasError)
            {
                return;
            }
            if (this.pendingNamespaces == null)
            {
                this.pendingNamespaces = new NamespaceStackNode();
            }
            this.pendingNamespaces.Add(namespaceDeclaration.Prefix, namespaceDeclaration.Namespace);
            CurrentWriter.WriteNamespace(namespaceDeclaration);
        }

        public override void WriteStartObject(XamlType type)
        {
            // This validation must be done before pushing the object frame, so that if there is an error,
            // we treat the containing type as an error and not just the subtree.
            ValidateSetMember();
            PushXamlFrame(type);

            // Pushing the activity frame must be done before the rest of validation, because if this
            // is an unknown Activity, we want to treat just this subtree as an error, not its parent.
            PushNewActivityFrameIfNeeded();
            ValidateStartObject();
            if (this.rootFragment.HasError)
            {
                return;
            }
            CurrentWriter.WriteStartObject(type);
            CurrentFragment.ObjectDepth++;
        }

        public override void WriteGetObject()
        {
            PushXamlFrame(null);
            ValidateGetObject();
            if (this.rootFragment.HasError)
            {
                return;
            }
            CurrentWriter.WriteGetObject();
            CurrentFragment.ObjectDepth++;
        }

        public override void WriteEndObject()
        {
            this.xamlStack.Pop();
            ValidateEndObject();
            if (this.rootFragment.HasError)
            {
                return;
            }
            CurrentWriter.WriteEndObject();
            CurrentFragment.ObjectDepth--;
            if (CurrentFragment.ObjectDepth == 0)
            {
                XamlFragment completedFragment = CurrentFragment;
                this.fragmentStack.Pop();
                if (this.fragmentStack.Count == 0)
                {
                    Fx.Assert(completedFragment == this.rootFragment, "Base of stack should be root fragment");
                    CompleteLoad();
                }
                else
                {
                    CurrentFragment.AddChild(completedFragment);
                }
            }
        }

        public override void WriteStartMember(XamlMember member)
        {
            Fx.Assert(this.xamlStack.Count > 0 && this.xamlStack.Current.Member == null, "Unexpected StartMember");
            this.xamlStack.Current.Member = member;
            ValidateStartMember();
            if (this.rootFragment.HasError)
            {
                return;
            }
            CurrentWriter.WriteStartMember(member);
        }

        public override void WriteEndMember()
        {
            Fx.Assert(this.xamlStack.Count > 0 && this.xamlStack.Current.Member != null, "Unexpected EndMember");
            this.xamlStack.Current.Member = null;
            this.xamlStack.Current.MemberIsSet = false;
            ValidateEndMember();
            if (this.rootFragment.HasError)
            {
                return;
            }
            CurrentWriter.WriteEndMember();
        }

        public override void WriteValue(object value)
        {
            ValidateValue(value);
            if (this.rootFragment.HasError)
            {
                return;
            }
            CurrentWriter.WriteValue(value);
        }

        public void SetLineInfo(int lineNumber, int linePosition)
        {
            // We need to save the line info statically, for validation errors
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;

            // But we also need to keep it in sync with the nodestream, for XOW errors
            // XOW and XamlNodeQueue.Writer both implement IXamlLineInfoConsumer, so we can do a straight cast
            if (this.rootFragment.HasError)
            {
                return;
            }
            ((IXamlLineInfoConsumer)CurrentWriter).SetLineInfo(lineNumber, linePosition);
        }

        // ObjectWriter always wants LineInfo
        public bool ShouldProvideLineInfo { get { return true; } }

        internal static bool IsErrorActivity(Type objectType)
        {
            return objectType == typeof(ErrorActivity) ||
                (objectType != null && objectType.IsGenericType &&
                 objectType.GetGenericTypeDefinition() == typeof(ErrorActivity<>));
        }

        // Node loop that strips out ErrorActivities on Save. Assumes that ErrorActivities are never
        // nested, and that XamlObjectReader doesn't have line info.
        internal static void TransformAndStripErrors(System.Xaml.XamlReader objectReader, XamlWriter writer)
        {
            // Every ErrorActivity is prefixed with all the NamespaceDeclarations that were in scope
            // in the original document. We track the current namespaces in scope on Save, so that we
            // can strip out any redundant declarations.
            NamespaceStackNode currentNamespaces = null;
            NamespaceStackNode pendingNamespaces = null;

            while (objectReader.Read())
            {
                // Update the namespace stack
                switch (objectReader.NodeType)
                {
                    case XamlNodeType.NamespaceDeclaration:
                        if (pendingNamespaces == null)
                        {
                            pendingNamespaces = new NamespaceStackNode() { PreviousNode = currentNamespaces };
                        }
                        pendingNamespaces.Add(objectReader.Namespace.Prefix, objectReader.Namespace.Namespace);
                        break;
                    case XamlNodeType.StartObject:
                    case XamlNodeType.GetObject:
                        if (pendingNamespaces != null)
                        {
                            currentNamespaces = pendingNamespaces;
                            pendingNamespaces = null;
                        }
                        currentNamespaces.ObjectDepth++;
                        break;
                }

                if (objectReader.NodeType == XamlNodeType.StartObject && IsErrorActivity(objectReader.Type.UnderlyingType))
                {
                    ActivityFragment.TransformErrorActivityContents(objectReader, writer, currentNamespaces);
                }
                else
                {
                    writer.WriteNode(objectReader);
                }

                if (objectReader.NodeType == XamlNodeType.EndObject)
                {
                    currentNamespaces.ObjectDepth--;
                    if (currentNamespaces.ObjectDepth == 0)
                    {
                        currentNamespaces = currentNamespaces.PreviousNode;
                    }
                }
            }
        }

        XamlFragment CurrentFragment
        {
            get { return this.fragmentStack.Peek(); }
        }

        NameScopeFrame CurrentNameScope
        {
            get { return this.nameScopeStack.Peek(); }
        }

        XamlWriter CurrentWriter
        {
            get { return CurrentFragment.NodeQueue.Writer; }
        }

        static void AppendShortName(StringBuilder result, XamlType type)
        {
            result.Append(type.Name);
            if (type.IsGeneric)
            {
                result.Append("(");
                bool isFirst = true;
                foreach (XamlType typeArg in type.TypeArguments)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        result.Append(",");
                    }
                    AppendShortName(result, typeArg);
                }
                result.Append(")");
            }
        }

        // If a generic type is unknown, we don't know whether the open generic couldn't be resolved,
        // or just its children. So we only want to surface errors for types that don't have unknown
        // children.
        static void GetLeafUnresolvedTypeArgs(XamlType type, HashSet<XamlType> unresolvedTypeArgs)
        {
            Fx.Assert(type.IsUnknown, "Method should only be called for unknown types");
            bool hasUnknownChildren = false;
            if (type.IsGeneric)
            {
                foreach (XamlType typeArg in type.TypeArguments)
                {
                    if (typeArg.IsUnknown)
                    {
                        GetLeafUnresolvedTypeArgs(typeArg, unresolvedTypeArgs);
                        hasUnknownChildren = true;
                    }
                }
            }
            if (!hasUnknownChildren)
            {
                unresolvedTypeArgs.Add(type);
            }
        }

        internal static string GetXamlMemberName(XamlMember member)
        {
            if (member.IsDirective)
            {
                return "{" + member.PreferredXamlNamespace + "}" + member.Name;
            }
            else
            {
                return GetXamlTypeName(member.DeclaringType) + "." + member.Name;
            }
        }

        internal static string GetXamlTypeName(XamlType type)
        {
            string typeNs = type.PreferredXamlNamespace;
            string typeName = GetFullTypeNameWithoutNamespace(type);
            string clrns, assembly;
            if (XamlNamespaceHelper.TryParseClrNsUri(typeNs, out clrns, out assembly))
            {
                return clrns + "." + typeName;
            }
            else
            {
                return typeNs + ":" + typeName;
            }
        }

        static bool IsWhitespace(string value)
        {
            foreach (char c in value)
            {
                if (c != '\r' && c != '\n' && c != ' ' && c != '\t')
                {
                    return false;
                }
            }
            return true;
        }

        // Validate named references and write out the complete nodestream to the ObjectWriter
        void CompleteLoad()
        {
            CompleteNameReferences();
            XamlFragment.FindBrokenReferences(this.rootFragment);

            if (this.rootFragment.HasError)
            {
                this.Result = null;
            }
            else
            {
                this.rootFragment.WriteTo(this.objectWriter, false);
                this.Result = this.objectWriter.Result;
            }
        }

        // Gets the property type of the containing member, or its item type if it's a collection.
        XamlType GetParentPropertyType(out bool parentIsDictionary)
        {
            XamlMember parentMember;
            XamlType collectionType;
            XamlType result = GetParentPropertyType(out parentMember, out collectionType);
            parentIsDictionary = collectionType != null && collectionType.IsDictionary;
            return result;
        }

        XamlType GetParentPropertyType(out XamlMember parentMember, out XamlType collectionType)
        {
            parentMember = this.xamlStack.Previous(1).Member;
            Fx.Assert(parentMember != null, "StartObject or Value without preceding StartMember");
            if (parentMember.IsDirective &&
                (parentMember.Type.IsCollection || parentMember.Type.IsDictionary))
            {
                if (parentMember == XamlLanguage.Items)
                {
                    collectionType = this.xamlStack.Previous(1).Type;
                    if (collectionType == null)
                    {
                        // This is a GetObject, need to look at the containing member
                        collectionType = this.xamlStack.Previous(2).Member.Type;
                    }
                }
                else
                {
                    collectionType = parentMember.Type;
                }
                return collectionType.ItemType;
            }
            collectionType = null;
            return parentMember.Type;
        }

        // Checks whether to push a new ActivityFrame for a new StartObject (i.e. whether the object
        // is an activity and is replaceable in case of error).
        void PushNewActivityFrameIfNeeded()
        {
            Fx.Assert(this.xamlStack.Count > 0, "PushNewActivityFrameIfNeeded called without a StartObject");
            if (this.xamlStack.Count == 1)
            {
                // This is the root of the document
                return;
            }
            if (CurrentFragment.HasError)
            {
                // We're already inside an error frame, no point pushing any more frames
                return;
            }
            // Check the parent property type (not the object type) because that's what determines
            // whether we can inject an ErrorActivity.
            bool parentIsDictionary;
            XamlType parentType = GetParentPropertyType(out parentIsDictionary);
            if (parentType != null && parentType.UnderlyingType != null && !parentIsDictionary &&
                ActivityFragment.IsActivityType(parentType.UnderlyingType))
            {
                this.fragmentStack.Push(new ActivityFragment(SchemaContext) { Type = parentType.UnderlyingType });
                CurrentFragment.Namespaces = this.xamlStack.Current.Namespaces;
            }
        }

        void PushNameScope()
        {
            this.nameScopeStack.Push(new NameScopeFrame(this.nameScopeStack.Peek()));
        }

        void PushXamlFrame(XamlType type)
        {
            NamespaceStackNode currentNamespaces = this.xamlStack.Count > 0 ? this.xamlStack.Current.Namespaces : null;
            this.xamlStack.Push(new XamlFrame { Type = type });
            if (this.pendingNamespaces != null)
            {
                this.pendingNamespaces.PreviousNode = currentNamespaces;
                this.xamlStack.Current.Namespaces = this.pendingNamespaces;
                this.pendingNamespaces = null;
            }
            else
            {
                this.xamlStack.Current.Namespaces = currentNamespaces;
            }
        }

        void ValidateStartObject()
        {
            // Check if type is known
            XamlType type = this.xamlStack.Current.Type;
            if (type.IsUnknown)
            {
                HashSet<XamlType> unresolvedTypes = null;
                if (type.IsGeneric)
                {
                    unresolvedTypes = new HashSet<XamlType>();
                    GetLeafUnresolvedTypeArgs(type, unresolvedTypes);
                }
                if (unresolvedTypes != null &&
                    (unresolvedTypes.Count > 1 || !unresolvedTypes.Contains(type)))
                {
                    ValidationError(SR.UnresolvedGenericType, GetXamlTypeName(type));
                    foreach (XamlType unresolvedTypeArg in unresolvedTypes)
                    {
                        ValidationErrorUnknownType(unresolvedTypeArg);
                    }
                }
                else
                {
                    ValidationErrorUnknownType(type);
                }
            }
            else if (this.xamlStack.Count > 1)
            {
                // Check assignability to parent member
                if (!type.IsMarkupExtension)
                {
                    XamlMember parentMember;
                    XamlType collectionType;
                    XamlType expectedType = GetParentPropertyType(out parentMember, out collectionType);
                    if (collectionType != null)
                    {
                        if (!CollectionAcceptsType(collectionType, type))
                        {
                            ValidationError(SR.UnassignableCollection, type, collectionType.ItemType, collectionType);
                        }
                    }
                    else if (parentMember != null && !parentMember.IsUnknown &&
                        !type.CanAssignTo(parentMember.Type) && parentMember.DeferringLoader == null)
                    {
                        ValidationError(SR.UnassignableObject, type, parentMember.Type, parentMember.Name);
                    }
                }
            }

            // Update the NameScope stack
            if (type.IsNameScope && this.xamlStack.Count > 1)
            {
                PushNameScope();
            }
            CurrentNameScope.Depth++;
        }

        void ValidateGetObject()
        {
            XamlType type = this.xamlStack.Previous(1).Member.Type;
            if (type.IsNameScope)
            {
                PushNameScope();
            }
            CurrentNameScope.Depth++;
        }

        // Check whether a member is set more than once
        bool ValidateSetMember()
        {
            XamlFrame frame = this.xamlStack.Current;
            if (frame != null)
            {
                if (frame.MemberIsSet && !frame.Member.IsUnknown && !frame.Member.IsDirective)
                {
                    ValidationError(SR.MemberCanOnlyBeSetOnce, frame.Member);
                    return false;
                }
                frame.MemberIsSet = true;
            }
            return true;
        }

        bool CollectionAcceptsType(XamlType collectionType, XamlType type)
        {
            return collectionType.IsUnknown ||
                collectionType.AllowedContentTypes == null ||
                collectionType.AllowedContentTypes.Any(contentType => type.CanAssignTo(contentType));
        }

        void ValidateStartMember()
        {
            XamlFrame currentFrame = this.xamlStack.Current;
            XamlMember member = currentFrame.Member;

            // Make sure that the member is known.
            // Don't bother surfacing an error for unknown instance properties or unknown content on 
            // unknown types. It's redundant, since we'll already surface an error for the unknown type.
            if (member == XamlLanguage.UnknownContent)
            {
                if (!currentFrame.Type.IsUnknown)
                {
                    ValidationError(SR.UnknownContent, this.xamlStack.Current.Type);
                }
            }
            else if (member.IsUnknown && (member.IsAttachable || member.IsDirective || !member.DeclaringType.IsUnknown))
            {
                ValidationError(SR.UnresolvedMember, member.Name, member.DeclaringType);
            }

            // Check for duplicate members
            if (currentFrame.PastMembers == null)
            {
                currentFrame.PastMembers = new HashSet<XamlMember>();
            }
            if (currentFrame.PastMembers.Contains(member))
            {
                ValidationError(SR.DuplicateMember, member);
            }
            else
            {
                currentFrame.PastMembers.Add(member);
            }

            // Check for misplaced attachable members
            if (member.IsAttachable && !currentFrame.Type.IsUnknown && !currentFrame.Type.CanAssignTo(member.TargetType))
            {
                ValidationError(SR.MemberOnBadTargetType, member.Name, member.TargetType);
            }

            // Update the NameScope stack
            if (member.DeferringLoader != null)
            {
                PushNameScope();
            }
            CurrentNameScope.Depth++;
        }

        void ValidateEndMember()
        {
            DecrementNameScopeDepth();
        }

        void ValidateEndObject()
        {
            DecrementNameScopeDepth();
        }

        void ValidateValue(object value)
        {
            XamlType type = this.xamlStack.Current.Type;
            XamlMember member = this.xamlStack.Current.Member;
            string valueString = value as string;
            if (valueString == null || member.IsUnknown || !ValidateSetMember() || IsWhitespace(valueString))
            {
                return;
            }

            // Check if this is x:Name or RuntimeNameProperty
            if (member == XamlLanguage.Name || (type != null && member == type.GetAliasedProperty(XamlLanguage.Name)))
            {
                if (!CurrentNameScope.RegisterName(valueString, CurrentFragment))
                {
                    ValidationError(SR.DuplicateName, valueString);
                }
                return;
            }

            // Check if this is an x:Reference
            if (type == XamlLanguage.Reference && (member == this.nameOfReference || member == XamlLanguage.PositionalParameters))
            {
                CurrentNameScope.AddNeededName(CurrentFragment, valueString, this.lineNumber, this.linePosition);
                return;
            }
            XamlValueConverter<TypeConverter> converter =
                (member == XamlLanguage.Initialization) ? type.TypeConverter : member.TypeConverter;
            if (converter != null && converter.ConverterType == typeof(NameReferenceConverter))
            {
                CurrentNameScope.AddNeededName(CurrentFragment, valueString, this.lineNumber, this.linePosition);
            }

            // Check if text is supported on this member
            if (member == XamlLanguage.Initialization)
            {
                if (!type.IsUnknown && type.TypeConverter == null && !XamlLanguage.String.CanAssignTo(type))
                {
                    ValidationError(SR.NoTypeConverter, type);
                }
            }
            else if (member.IsDirective)
            {
                if (member == XamlLanguage.Items)
                {
                    if (type == null)
                    {
                        // Inside a GetObject - get the type from the parent member
                        type = this.xamlStack.Previous(1).Member.Type;
                    }
                    if (!CollectionAcceptsType(type, XamlLanguage.String))
                    {
                        ValidationError(SR.NoTextInCollection, type);
                    }
                }
            }
            else if (member.TypeConverter == null && !XamlLanguage.String.CanAssignTo(member.Type) &&
                (member.DeferringLoader == null || member.DeferringLoader == this.activityLoader))
            {
                ValidationError(SR.NoTextInProperty, XamlLanguage.String, member.Type, member.Name);
            }
        }

        void ValidationError(string message, params object[] arguments)
        {
            ValidationError(message, this.lineNumber, this.linePosition, arguments);
            CurrentFragment.HasError = true;
        }

        void ValidationError(string message, int lineNumber, int linePosition, params object[] arguments)
        {
            // The default ToString implementations can be very clunky, especially for generics.
            // Use our own friendlier versions instead.
            for (int i = 0; i < arguments.Length; i++)
            {
                XamlType type = arguments[i] as XamlType;
                if (type != null)
                {
                    arguments[i] = GetXamlTypeName(type);
                }
                else
                {
                    XamlMember member = arguments[i] as XamlMember;
                    if (member != null)
                    {
                        arguments[i] = GetXamlMemberName(member);
                    }
                }
            }
            string error = string.Format(CultureInfo.CurrentCulture, message, arguments);
            if (LoadErrors == null)
            {
                LoadErrors = new List<XamlLoadErrorInfo>();
            }
            LoadErrors.Add(new XamlLoadErrorInfo(error, lineNumber, linePosition));
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotIgnoreMethodResults, Justification =
            "StringBuilder.Append just returns the same instance that was called")]
        void ValidationErrorUnknownType(XamlType type)
        {
            StringBuilder result = new StringBuilder();
            string clrns, assembly;
            if (XamlNamespaceHelper.TryParseClrNsUri(type.PreferredXamlNamespace, out clrns, out assembly))
            {
                if (assembly == null)
                {
                    assembly = this.LocalAssemblyName;
                }
                StringBuilder typeName = new StringBuilder();
                typeName.Append(clrns);
                typeName.Append(".");
                AppendShortName(typeName, type);
                ValidationError(SR.UnresolvedTypeInAssembly, typeName, assembly);
            }
            else
            {
                StringBuilder typeName = new StringBuilder();
                AppendShortName(typeName, type);
                ValidationError(SR.UnresolvedTypeInNamespace, typeName, type.PreferredXamlNamespace);
            }
        }

        void DecrementNameScopeDepth()
        {
            CurrentNameScope.Depth--;
            if (CurrentNameScope.Depth == 0)
            {
                this.poppedNameScopes.Add(this.nameScopeStack.Pop());
            }
        }

        // Resolves all simple name references in the tree, raising validation errors for any that
        // can't be resolved.
        void CompleteNameReferences()
        {
            foreach (NameScopeFrame nameScope in this.poppedNameScopes)
            {
                if (nameScope.NeededNames == null)
                {
                    continue;
                }
                foreach (NameReference reference in nameScope.NeededNames)
                {
                    XamlFragment target = nameScope.FindName(reference.Name);
                    if (target == null)
                    {
                        ValidationError(SR.UnresolvedName, reference.LineNumber, reference.LinePosition, reference.Name);
                        reference.Fragment.HasError = true;
                    }
                    else
                    {
                        if (target.ReferencedBy == null)
                        {
                            target.ReferencedBy = new HashSet<XamlFragment>();
                        }
                        target.ReferencedBy.Add(reference.Fragment);
                    }
                }
            }
        }

        private static string GetFullTypeNameWithoutNamespace(XamlType xamlType)
        {
            string typeName = string.Empty;
            if (xamlType != null)
            {
                typeName = xamlType.Name;
                bool firstTypeArg = true;
                if (xamlType.TypeArguments != null && xamlType.TypeArguments.Count > 0)
                {
                    typeName += "(";
                    foreach (XamlType typeArg in xamlType.TypeArguments)
                    {
                        if (!firstTypeArg)
                        {
                            typeName += ",";
                        }
                        else
                        {
                            firstTypeArg = false;
                        }
                        typeName += typeArg.Name;
                    }
                    typeName += ")";
                }
            }
            return typeName;
        }

        class XamlFrame
        {
            public XamlType Type { get; set; }
            public XamlMember Member { get; set; }
            public bool MemberIsSet { get; set; }
            public NamespaceStackNode Namespaces { get; set; }
            public HashSet<XamlMember> PastMembers { get; set; }
        }

        // A stack that is implemented as a list to allow walking up the stack.
        class WalkableStack<T> : List<T> where T : class
        {
            public T Pop()
            {
                T result = this[Count - 1];
                this.RemoveAt(Count - 1);
                return result;
            }

            public T Previous(int index)
            {
                return this[Count - 1 - index];
            }

            public void Push(T frame)
            {
                Add(frame);
            }

            public T Current
            {
                get { return Count > 0 ? this[Count - 1] : null; }
            }
        }

        // Class to buffer a tree of XAML fragments and write them back out in the correct order.
        class XamlFragment
        {
            private XamlFragment firstChild;
            private XamlFragment nextSibling;

            public XamlFragment(XamlSchemaContext schemaContext)
            {
                NodeQueue = new XamlNodeQueue(schemaContext);
            }

            public XamlNodeQueue NodeQueue { get; private set; }
            public int ObjectDepth { get; set; }
            public bool HasError { get; set; }
            public NamespaceStackNode Namespaces { get; set; }
            public HashSet<XamlFragment> ReferencedBy { get; set; }

            // Adds a child fragment at the current position of the NodeQueue.
            // We store the fragment as a Value Node, and expand out its contents at Write time.
            // We also store the fragments in a simple tree structure to so we can iterate them quickly.
            public void AddChild(XamlFragment newChild)
            {
                NodeQueue.Writer.WriteValue(newChild);
                XamlFragment curChild = this.firstChild;
                if (curChild == null)
                {
                    this.firstChild = newChild;
                }
                else
                {
                    while (curChild.nextSibling != null)
                    {
                        curChild = curChild.nextSibling;
                    }
                    curChild.nextSibling = newChild;
                }
            }

            // Find all references to error fragments and mark the referencing fragments as also errored.
            public static void FindBrokenReferences(XamlFragment rootFragment)
            {
                // By starting from the root of the tree and walking its children, we ensure we traverse
                // each node at least once, and so find every error fragment.
                // Given an error fragment, we want to mark all its children and all its referencing fragments
                // as errors, and process them recursively.
                Queue<XamlFragment> queue = new Queue<XamlFragment>();
                queue.Enqueue(rootFragment);
                while (queue.Count > 0)
                {
                    if (rootFragment.HasError)
                    {
                        // We found an error at the root. We won't be able to load any part of the document,
                        // so skip this redundant processing.
                        return;
                    }

                    XamlFragment current = queue.Dequeue();
                    if (current.HasError)
                    {
                        // Mark all this fragment's children as errored, and enqueue them for recursive processing.
                        XamlFragment child = current.firstChild;
                        while (child != null)
                        {
                            child.HasError = true;
                            queue.Enqueue(child);
                            child = child.nextSibling;
                        }

                        // Mark all fragments that reference this fragment as errored, and enqueue them for recursive processing.
                        if (current.ReferencedBy != null)
                        {
                            foreach (XamlFragment referencingFragment in current.ReferencedBy)
                            {
                                referencingFragment.HasError = true;
                                queue.Enqueue(referencingFragment);
                            }
                        }

                        // Clear the links so that we don't traverse them again if there is a cycle.
                        current.firstChild = null;
                        current.ReferencedBy = null;
                    }
                    else
                    {
                        // This fragment is healthy, but we need to check for any errors in its children.
                        // Don't remove the children, we'll need to traverse them if this fragment gets
                        // marked as errored later.
                        XamlFragment child = current.firstChild;
                        while (child != null)
                        {
                            queue.Enqueue(child);
                            child = child.nextSibling;
                        }
                    }
                }
            }

            // Write this fragment and all its children out to the specified writer.
            public virtual void WriteTo(XamlWriter writer, bool parentHasError)
            {
                // In the constrained designer scenario, we can always assume that there is line info.
                XamlReader nodeReader = NodeQueue.Reader;
                IXamlLineInfo lineInfo = (IXamlLineInfo)nodeReader;
                IXamlLineInfoConsumer lineInfoConsumer = (IXamlLineInfoConsumer)writer;
                int lineNumber = 0;
                int linePosition = 0;

                while (nodeReader.Read())
                {
                    if (lineInfo.LineNumber > 0 &&
                            (lineInfo.LineNumber != lineNumber || lineInfo.LinePosition != linePosition))
                    {
                        lineNumber = lineInfo.LineNumber;
                        linePosition = lineInfo.LinePosition;
                        lineInfoConsumer.SetLineInfo(lineNumber, linePosition);
                    }
                    XamlFragment child = (nodeReader.NodeType == XamlNodeType.Value) ? nodeReader.Value as XamlFragment : null;
                    if (child != null)
                    {
                        child.WriteTo(writer, parentHasError || HasError);
                    }
                    else
                    {
                        writer.WriteNode(nodeReader);
                    }
                }
            }
        }

        class ActivityFragment : XamlFragment
        {
            public ActivityFragment(XamlSchemaContext schemaContext)
                : base(schemaContext)
            {
            }

            public Type Type { get; set; }

            // We can only construct an ErrorActivity that is assignable to properties of type
            // Activity or Activity<T>, not any of their descendants.
            public static bool IsActivityType(Type type)
            {
                return type == typeof(Activity) ||
                    (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Activity<>));
            }

            public override void WriteTo(XamlWriter writer, bool parentHasError)
            {
                if (HasError && !parentHasError)
                {
                    Fx.Assert(this.Type != null && IsActivityType(this.Type), "Cannot create ErrorActivity for non-Activity property");
                    Type errorType;
                    if (this.Type == typeof(Activity))
                    {
                        errorType = typeof(ErrorActivity);
                    }
                    else
                    {
                        errorType = typeof(ErrorActivity<>).MakeGenericType(this.Type.GetGenericArguments()[0]);
                    }
                    XamlType errorXamlType = writer.SchemaContext.GetXamlType(errorType);
                    writer.WriteStartObject(errorXamlType);
                    writer.WriteStartMember(errorXamlType.GetMember(ErrorActivity.ErrorNodesProperty));

                    XamlNodeList errorNodes = GetErrorNodes();
                    ErrorActivity.WriteNodeList(writer, errorNodes);

                    writer.WriteEndMember(); // </ErrorActivity.ErrorNodeList>
                    writer.WriteEndObject(); // </ErrorActivity>
                }
                else
                {
                    base.WriteTo(writer, parentHasError);
                }
            }

            // Extracts the Error Nodes contents out of the ErrorActivity and writes them to the 
            // specified writer.
            // Expects reader to be positioned on SO ErrorActivity and leaves it on corresponding EO.
            public static void TransformErrorActivityContents(System.Xaml.XamlReader objectReader, XamlWriter writer,
                NamespaceStackNode currentNamespaces)
            {
                XamlMember errorNodesMember = objectReader.Type.GetMember(ErrorActivity.ErrorNodesProperty);
                // Skip past off <ErrorActivity>
                objectReader.Read();

                do
                {
                    Fx.Assert(objectReader.NodeType == XamlNodeType.StartMember, "Expected StartMember");
                    if (objectReader.Member == errorNodesMember)
                    {
                        // Skip past <ErrorActivity.ErrorNodes>
                        objectReader.Read();

                        // Skip past the dummy StartObject & StartMember
                        Fx.Assert(objectReader.NodeType == XamlNodeType.StartObject, "Expected StartObject");
                        objectReader.Read();
                        Fx.Assert(objectReader.NodeType == XamlNodeType.StartMember, "Expected StartMember");
                        objectReader.Read();

                        // Strip redundant namespaces
                        while (objectReader.NodeType == XamlNodeType.NamespaceDeclaration)
                        {
                            string ns = currentNamespaces.LookupNamespace(objectReader.Namespace.Prefix);
                            if (ns != objectReader.Namespace.Namespace &&
                                !IsIgnorableCompatNamespace(objectReader.Namespace, currentNamespaces))
                            {
                                writer.WriteNamespace(objectReader.Namespace);
                            }
                            objectReader.Read();
                        }

                        // Pass through the original contents, stripping out any hidden APs added by
                        // the XamlDebuggerXmlReader, since XOR wouldn't write them out.
                        XamlType debuggerReaderType = objectReader.SchemaContext.GetXamlType(typeof(XamlDebuggerXmlReader));
                        Fx.Assert(objectReader.NodeType == XamlNodeType.StartObject, "Expected StartObject");
                        XamlReader subReader = objectReader.ReadSubtree();
                        subReader.Read();
                        while (!subReader.IsEof)
                        {
                            if (subReader.NodeType == XamlNodeType.StartMember &&
                                subReader.Member.DeclaringType == debuggerReaderType &&
                                subReader.Member.SerializationVisibility == DesignerSerializationVisibility.Hidden)
                            {
                                subReader.Skip();
                            }
                            else
                            {
                                writer.WriteNode(subReader);
                                subReader.Read();
                            }
                        }

                        // Close out the dummy StartObject & StartMember
                        Fx.Assert(objectReader.NodeType == XamlNodeType.EndMember, "Expected EndMember");
                        objectReader.Read();
                        Fx.Assert(objectReader.NodeType == XamlNodeType.EndObject, "Expected EndObject");
                        objectReader.Read();

                        // Skip past </ErrorActivity.ErrorNodes>
                        Fx.Assert(objectReader.NodeType == XamlNodeType.EndMember, "Expected EndMember");
                        objectReader.Read();
                    }
                    else
                    {
                        // Skip any APs added by the designer
                        Fx.Assert(objectReader.Member.IsAttachable, "Unexpected member on ErrorActivity");
                        objectReader.Skip();
                    }
                }
                while (objectReader.NodeType != XamlNodeType.EndObject); // </ErrorActivity>
            }

            // If the namespace is the markup-compat namespace, we skip writing it out as long as there
            // is an ignorable namespace at the root of the doc; DesignTimeXamlWriter will add it to the
            // root later. We assume that the exact prefix for markup-compat doesn't matter, just whether the
            // namespace is defined.
            static bool IsIgnorableCompatNamespace(NamespaceDeclaration ns, NamespaceStackNode currentNamespaces)
            {
                if (ns.Namespace == NameSpaces.Mc)
                {
                    NamespaceStackNode rootNamespaces = currentNamespaces;
                    while (rootNamespaces.PreviousNode != null)
                    {
                        rootNamespaces = rootNamespaces.PreviousNode;
                    }
                    foreach (string rootNs in rootNamespaces.Values)
                    {
                        if (NameSpaces.ShouldIgnore(rootNs))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            XamlNodeList GetErrorNodes()
            {
                XamlNodeList result = new XamlNodeList(NodeQueue.Writer.SchemaContext);

                // Dummy StartObject & StartMember. This is here so that ObjectReader doesn't try
                // to hoist all the namespaces on save, which would cause them to be added as
                // Imports by the VBExpression converter.
                result.Writer.WriteStartObject(XamlLanguage.Object);
                result.Writer.WriteStartMember(XamlLanguage.UnknownContent);

                // Write out all namespaces in scope to the NodeList, to ensure that the any type
                // converters that use namespaces/prefixes still work on round trip.
                // (We can strip out the redundant ones on Save.)
                foreach (KeyValuePair<string, string> ns in Namespaces.FlattenNamespaces())
                {
                    result.Writer.WriteNamespace(new NamespaceDeclaration(ns.Value, ns.Key));
                }

                // Write out the original contents of this fragment, expanding our children if any.
                base.WriteTo(result.Writer, true);

                // Close the dummy object
                result.Writer.WriteEndMember();
                result.Writer.WriteEndObject();

                result.Writer.Close();
                return result;
            }
        }

        class NameScopeFrame
        {
            private Dictionary<string, XamlFragment> declaredNames;
            private List<NameReference> neededNames;

            public NameScopeFrame Parent { get; private set; }
            public int Depth { get; set; }
            public List<NameReference> NeededNames { get { return this.neededNames; } }

            public NameScopeFrame(NameScopeFrame parent)
            {
                Parent = parent;
            }

            public void AddNeededName(XamlFragment fragment, string name, int lineNumber, int linePosition)
            {
                if (this.neededNames == null)
                {
                    this.neededNames = new List<NameReference>();
                }
                this.neededNames.Add(new NameReference
                {
                    Fragment = fragment,
                    Name = name,
                    LineNumber = lineNumber,
                    LinePosition = linePosition
                });
            }

            public XamlFragment FindName(string name)
            {
                NameScopeFrame current = this;
                do
                {
                    XamlFragment result = null;
                    if (current.declaredNames != null && current.declaredNames.TryGetValue(name, out result))
                    {
                        return result;
                    }
                    current = current.Parent;
                }
                while (current != null);
                return null;
            }

            public bool RegisterName(string name, XamlFragment containingFragment)
            {
                if (this.declaredNames == null)
                {
                    this.declaredNames = new Dictionary<string, XamlFragment>();
                }
                if (this.declaredNames.ContainsKey(name))
                {
                    return false;
                }
                this.declaredNames.Add(name, containingFragment);
                return true;
            }
        }

        class NameReference
        {
            public XamlFragment Fragment { get; set; }
            public string Name { get; set; }
            public int LineNumber { get; set; }
            public int LinePosition { get; set; }
        }

        class NamespaceStackNode : Dictionary<string, string>
        {
            public NamespaceStackNode PreviousNode { get; set; }

            public int ObjectDepth { get; set; }

            public IEnumerable<KeyValuePair<string, string>> FlattenNamespaces()
            {
                // We need to hide not only any shadowed prefixes, but any shadowed namespaces, since
                // XamlXmlWriter doesn't allow declaration of multiple prefixes for the same namespaace
                // at the same scope.
                HashSet<string> prefixes = new HashSet<string>();
                HashSet<string> namespaces = new HashSet<string>();
                NamespaceStackNode current = this;
                do
                {
                    foreach (KeyValuePair<string, string> pair in current)
                    {
                        if (!prefixes.Contains(pair.Key) && !namespaces.Contains(pair.Value))
                        {
                            yield return pair;
                            prefixes.Add(pair.Key);
                            namespaces.Add(pair.Value);
                        }
                    }
                    current = current.PreviousNode;
                }
                while (current != null);
            }

            public string LookupNamespace(string prefix)
            {
                NamespaceStackNode current = this;
                do
                {
                    string ns;
                    if (current.TryGetValue(prefix, out ns))
                    {
                        return ns;
                    }
                    current = current.PreviousNode;
                }
                while (current != null);
                return null;
            }
        }
    }
}
