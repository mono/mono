// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Activities.XamlIntegration;

    public class XamlDebuggerXmlReader : XamlReader, IXamlLineInfo
    {
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier StartLineName = new AttachableMemberIdentifier(typeof(XamlDebuggerXmlReader), StartLineMemberName);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier StartColumnName = new AttachableMemberIdentifier(typeof(XamlDebuggerXmlReader), StartColumnMemberName);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier EndLineName = new AttachableMemberIdentifier(typeof(XamlDebuggerXmlReader), EndLineMemberName);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier EndColumnName = new AttachableMemberIdentifier(typeof(XamlDebuggerXmlReader), EndColumnMemberName);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier FileNameName = new AttachableMemberIdentifier(typeof(XamlDebuggerXmlReader), FileNameMemberName);

        private const string StartLineMemberName = "StartLine";
        private const string StartColumnMemberName = "StartColumn";
        private const string EndLineMemberName = "EndLine";
        private const string EndColumnMemberName = "EndColumn";
        private const string FileNameMemberName = "FileName";

        private static readonly Type attachingType = typeof(XamlDebuggerXmlReader);
        private static readonly MethodInfo startLineGetterMethodInfo = attachingType.GetMethod("GetStartLine", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo startLineSetterMethodInfo = attachingType.GetMethod("SetStartLine", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo startColumnGetterMethodInfo = attachingType.GetMethod("GetStartColumn", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo startColumnSetterMethodInfo = attachingType.GetMethod("SetStartColumn", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo endLineGetterMethodInfo = attachingType.GetMethod("GetEndLine", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo endLineSetterMethodInfo = attachingType.GetMethod("SetEndLine", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo endColumnGetterMethodInfo = attachingType.GetMethod("GetEndColumn", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo endColumnSetterMethodInfo = attachingType.GetMethod("SetEndColumn", BindingFlags.Public | BindingFlags.Static);

        private XamlMember startLineMember;
        private XamlMember startColumnMember;
        private XamlMember endLineMember;
        private XamlMember endColumnMember;
        private XamlSchemaContext schemaContext;
        private IXamlLineInfo xamlLineInfo;
        private XmlReaderWithSourceLocation xmlReaderWithSourceLocation;
        private XamlReader underlyingReader;
        private Stack<XamlNode> objectDeclarationRecords;
        private Dictionary<XamlNode, DocumentRange> initializationValueRanges;
        private Queue<XamlNode> bufferedXamlNodes;
        private XamlSourceLocationCollector sourceLocationCollector;
        private XamlNode current;
        private bool collectNonActivitySourceLocation;
        private int suppressMarkupExtensionLevel;

        public XamlDebuggerXmlReader(TextReader underlyingTextReader)
            : this(underlyingTextReader, new XamlSchemaContext())
        {
        }

        public XamlDebuggerXmlReader(TextReader underlyingTextReader, XamlSchemaContext schemaContext)
            : this(underlyingTextReader, schemaContext, localAssembly: null)
        {
        }

        internal XamlDebuggerXmlReader(TextReader underlyingTextReader, XamlSchemaContext schemaContext, Assembly localAssembly)
        {
            UnitTestUtility.Assert(underlyingTextReader != null, "underlyingTextReader should not be null and is ensured by caller.");
            this.xmlReaderWithSourceLocation = new XmlReaderWithSourceLocation(underlyingTextReader);
            this.underlyingReader = new XamlXmlReader(this.xmlReaderWithSourceLocation, schemaContext, new XamlXmlReaderSettings { ProvideLineInfo = true, LocalAssembly = localAssembly });
            this.xamlLineInfo = (IXamlLineInfo)this.underlyingReader;
            UnitTestUtility.Assert(this.xamlLineInfo.HasLineInfo, "underlyingReader is constructed with the ProvideLineInfo option above.");
            this.schemaContext = schemaContext;
            this.objectDeclarationRecords = new Stack<XamlNode>();
            this.initializationValueRanges = new Dictionary<XamlNode, DocumentRange>();
            this.bufferedXamlNodes = new Queue<XamlNode>();
            this.current = this.CreateCurrentNode();
            this.SourceLocationFound += XamlDebuggerXmlReader.SetSourceLocation;
        }

        // A XamlReader that need to collect source level information is necessary
        // the one that is closest to the source document.
        // This constructor is fundamentally flawed because it allows any XAML reader
        // Which could output some XAML node that does not correspond to source.
        [Obsolete("Don't use this constructor. Use \"public XamlDebuggerXmlReader(TextReader underlyingTextReader)\" or \"public XamlDebuggerXmlReader(TextReader underlyingTextReader, XamlSchemaContext schemaContext)\" instead.")]
        public XamlDebuggerXmlReader(XamlReader underlyingReader, TextReader textReader)
            : this(underlyingReader, underlyingReader as IXamlLineInfo, textReader)
        {
        }

        // This one is worse because in implementation we expect the same object instance through two parameters.
        [Obsolete("Don't use this constructor. Use \"public XamlDebuggerXmlReader(TextReader underlyingTextReader)\" or \"public XamlDebuggerXmlReader(TextReader underlyingTextReader, XamlSchemaContext schemaContext)\" instead.")]
        public XamlDebuggerXmlReader(XamlReader underlyingReader, IXamlLineInfo xamlLineInfo, TextReader textReader)
        {
            this.underlyingReader = underlyingReader;
            this.xamlLineInfo = xamlLineInfo;
            this.xmlReaderWithSourceLocation = new XmlReaderWithSourceLocation(textReader);
            this.initializationValueRanges = new Dictionary<XamlNode, DocumentRange>();
            // Parse the XML at once to get all the locations we wanted.
            while (this.xmlReaderWithSourceLocation.Read())
            {
            }
            this.schemaContext = underlyingReader.SchemaContext;
            this.objectDeclarationRecords = new Stack<XamlNode>();
            this.bufferedXamlNodes = new Queue<XamlNode>();
            this.current = this.CreateCurrentNode();
            this.SourceLocationFound += XamlDebuggerXmlReader.SetSourceLocation;
        }

        public event EventHandler<SourceLocationFoundEventArgs> SourceLocationFound
        {
            add { this._sourceLocationFound += value; }
            remove { this._sourceLocationFound -= value; }
        }

        private event EventHandler<SourceLocationFoundEventArgs> _sourceLocationFound;

        public bool CollectNonActivitySourceLocation
        {
            get
            {
                return this.collectNonActivitySourceLocation;
            }

            set
            {
                this.collectNonActivitySourceLocation = value;
            }
        }

        public bool HasLineInfo
        {
            get { return true; }
        }

        public int LineNumber
        {
            get { return this.Current.LineNumber; }
        }

        public int LinePosition
        {
            get { return this.Current.LinePosition; }
        }

        public override XamlNodeType NodeType
        {
            get { return this.Current.NodeType; }
        }

        public override XamlType Type
        {
            get { return this.Current.Type; }
        }

        public override XamlMember Member
        {
            get { return this.Current.Member; }
        }

        public override object Value
        {
            get { return this.Current.Value; }
        }

        public override bool IsEof
        {
            get { return this.underlyingReader.IsEof; }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return this.Current.Namespace; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return this.schemaContext; }
        }

        internal XamlMember StartLineMember
        {
            get
            {
                if (this.startLineMember == null)
                {
                    this.startLineMember = this.CreateAttachableMember(startLineGetterMethodInfo, startLineSetterMethodInfo, SourceLocationMemberType.StartLine);
                }

                return this.startLineMember;
            }
        }

        internal XamlMember StartColumnMember
        {
            get
            {
                if (this.startColumnMember == null)
                {
                    this.startColumnMember = this.CreateAttachableMember(startColumnGetterMethodInfo, startColumnSetterMethodInfo, SourceLocationMemberType.StartColumn);
                }

                return this.startColumnMember;
            }
        }

        internal XamlMember EndLineMember
        {
            get
            {
                if (this.endLineMember == null)
                {
                    this.endLineMember = this.CreateAttachableMember(endLineGetterMethodInfo, endLineSetterMethodInfo, SourceLocationMemberType.EndLine);
                }

                return this.endLineMember;
            }
        }

        internal XamlMember EndColumnMember
        {
            get
            {
                if (this.endColumnMember == null)
                {
                    this.endColumnMember = this.CreateAttachableMember(endColumnGetterMethodInfo, endColumnSetterMethodInfo, SourceLocationMemberType.EndColumn);
                }

                return this.endColumnMember;
            }
        }

        private XamlNode Current
        {
            get
            {
                return this.current;
            }

            set
            {
                this.current = value;
            }
        }

        private XamlSourceLocationCollector SourceLocationCollector
        {
            get
            {
                if (this.sourceLocationCollector == null)
                {
                    this.sourceLocationCollector = new XamlSourceLocationCollector(this);
                }

                return this.sourceLocationCollector;
            }
        }

        [Fx.Tag.InheritThrows(From = "TryGetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        static int GetIntegerAttachedProperty(object instance, AttachableMemberIdentifier memberIdentifier)
        {
            int value;
            if (AttachablePropertyServices.TryGetProperty(instance, memberIdentifier, out value))
            {
                return value;
            }
            else
            {
                return -1;
            }
        }

        [Fx.Tag.InheritThrows(From = "GetIntegerAttachedProperty")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetStartLine(object instance)
        {
            return GetIntegerAttachedProperty(instance, StartLineName);
        }

        [Fx.Tag.InheritThrows(From = "SetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static void SetStartLine(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, StartLineName, value);
        }

        [Fx.Tag.InheritThrows(From = "GetIntegerAttachedProperty")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetStartColumn(object instance)
        {
            return GetIntegerAttachedProperty(instance, StartColumnName);
        }

        [Fx.Tag.InheritThrows(From = "SetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static void SetStartColumn(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, StartColumnName, value);
        }

        [Fx.Tag.InheritThrows(From = "GetIntegerAttachedProperty")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetEndLine(object instance)
        {
            return GetIntegerAttachedProperty(instance, EndLineName);
        }

        [Fx.Tag.InheritThrows(From = "SetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static void SetEndLine(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, EndLineName, value);
        }

        [Fx.Tag.InheritThrows(From = "GetIntegerAttachedProperty")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetEndColumn(object instance)
        {
            return GetIntegerAttachedProperty(instance, EndColumnName);
        }

        [Fx.Tag.InheritThrows(From = "SetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static void SetEndColumn(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, EndColumnName, value);
        }

        [Fx.Tag.InheritThrows(From = "SetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static void SetFileName(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, FileNameName, value);
        }

        [Fx.Tag.InheritThrows(From = "TryGetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static object GetFileName(object instance)
        {
            string value;
            if (AttachablePropertyServices.TryGetProperty(instance, FileNameName, out value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        // Copy source location information from source to destination (if available)
        public static void CopyAttachedSourceLocation(object source, object destination)
        {
            int startLine, startColumn, endLine, endColumn;

            if (AttachablePropertyServices.TryGetProperty<int>(source, StartLineName, out startLine) &&
                AttachablePropertyServices.TryGetProperty<int>(source, StartColumnName, out startColumn) &&
                AttachablePropertyServices.TryGetProperty<int>(source, EndLineName, out endLine) &&
                AttachablePropertyServices.TryGetProperty<int>(source, EndColumnName, out endColumn))
            {
                SetStartLine(destination, startLine);
                SetStartColumn(destination, startColumn);
                SetEndLine(destination, endLine);
                SetEndColumn(destination, endColumn);
            }
        }

        internal static void SetSourceLocation(object sender, SourceLocationFoundEventArgs args)
        {
            object target = args.Target;
            Type targetType = target.GetType();
            XamlDebuggerXmlReader reader = (XamlDebuggerXmlReader)sender;
            bool shouldStoreAttachedProperty = false;

            if (reader.CollectNonActivitySourceLocation)
            {
                shouldStoreAttachedProperty = !targetType.Equals(typeof(string));
            }
            else 
            {
                if (typeof(Activity).IsAssignableFrom(targetType))
                {
                    if (!typeof(IExpressionContainer).IsAssignableFrom(targetType))
                    {
                        if (!typeof(IValueSerializableExpression).IsAssignableFrom(targetType))
                        {
                            shouldStoreAttachedProperty = true;
                        }
                    }
                }
            }

            shouldStoreAttachedProperty = shouldStoreAttachedProperty && !args.IsValueNode;

            if (shouldStoreAttachedProperty)
            {
                SourceLocation sourceLocation = args.SourceLocation;
                XamlDebuggerXmlReader.SetStartLine(target, sourceLocation.StartLine);
                XamlDebuggerXmlReader.SetStartColumn(target, sourceLocation.StartColumn);
                XamlDebuggerXmlReader.SetEndLine(target, sourceLocation.EndLine);
                XamlDebuggerXmlReader.SetEndColumn(target, sourceLocation.EndColumn);
            }
        }

        public override bool Read()
        {
            bool readSucceed;
            if (this.bufferedXamlNodes.Count > 0)
            {
                this.Current = this.bufferedXamlNodes.Dequeue();
                readSucceed = this.Current != null;
            }
            else
            {
                readSucceed = this.underlyingReader.Read();
                if (readSucceed)
                {
                    this.Current = CreateCurrentNode(this.underlyingReader, this.xamlLineInfo);
                    this.PushObjectDeclarationNodeIfApplicable();
                    switch (this.Current.NodeType)
                    {
                        case XamlNodeType.StartMember:

                            // When we reach a StartMember node, the next node to come might be a Value.
                            // To correctly pass SourceLocation information, we need to rewrite this node to use ValueNodeXamlMemberInvoker.
                            // But we don't know if the next node is a Value node yet, so we are buffering here and look ahead for a single node.
                            UnitTestUtility.Assert(this.bufferedXamlNodes.Count == 0, "this.bufferedXamlNodes should be empty when we reach this code path.");
                            this.bufferedXamlNodes.Enqueue(this.Current);

                            // This directive represents the XAML node or XAML information set 
                            // representation of initialization text, where a string within an 
                            // object element supplies the type construction information for 
                            // the surrounding object element.
                            bool isInitializationValue = this.Current.Member == XamlLanguage.Initialization;

                            bool moreNode = this.underlyingReader.Read();
                            UnitTestUtility.Assert(moreNode, "Start Member must followed by some other nodes.");

                            this.Current = this.CreateCurrentNode();

                            this.bufferedXamlNodes.Enqueue(this.Current);

                            // It is possible that the next node after StartMember is a StartObject/GetObject.
                            // We need to push the object declaration node to the Stack
                            this.PushObjectDeclarationNodeIfApplicable();

                            if (!this.SuppressingMarkupExtension() 
                                && this.Current.NodeType == XamlNodeType.Value)
                            {
                                DocumentRange valueRange;
                                DocumentLocation currentLocation = new DocumentLocation(this.Current.LineNumber, this.Current.LinePosition);
                                bool isInAttribute = this.xmlReaderWithSourceLocation.AttributeValueRanges.TryGetValue(currentLocation, out valueRange);
                                bool isInContent = isInAttribute ? false : this.xmlReaderWithSourceLocation.ContentValueRanges.TryGetValue(currentLocation, out valueRange);

                                if (isInAttribute || (isInContent && !isInitializationValue))
                                {
                                    // For Value Node with known line info, we want to route the value setting process through this Reader.
                                    // Therefore we need to go back to the member node and replace the XamlMemberInvoker.
                                    XamlNode startMemberNodeForValue = this.bufferedXamlNodes.Peek();
                                    XamlMember xamlMemberForValue = startMemberNodeForValue.Member;
                                    XamlMemberInvoker newXamlMemberInvoker = new ValueNodeXamlMemberInvoker(this, xamlMemberForValue.Invoker, valueRange);
                                    startMemberNodeForValue.Member = xamlMemberForValue.ReplaceXamlMemberInvoker(this.schemaContext, newXamlMemberInvoker);
                                }
                                else if (isInContent && isInitializationValue)
                                {
                                    XamlNode currentStartObject = this.objectDeclarationRecords.Peek();
                                    
                                    if (!this.initializationValueRanges.ContainsKey(currentStartObject))
                                    {
                                        this.initializationValueRanges.Add(currentStartObject, valueRange);
                                    }
                                    else
                                    {
                                        UnitTestUtility.Assert(false, 
                                            "I assume it is impossible for an object  to have more than one initialization member");
                                    }
                                }
                            }

                            this.StartAccessingBuffer();
                            break;

                        case XamlNodeType.EndObject:

                            this.InjectLineInfoXamlNodesToBuffer();
                            this.StartAccessingBuffer();
                            break;

                        case XamlNodeType.Value:
                            break;

                        default:
                            break;
                    }
                }
            }

            return readSucceed;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.underlyingReader != null)
                {
                    ((IDisposable)this.underlyingReader).Dispose();
                }

                this.underlyingReader = null;

                if (this.xmlReaderWithSourceLocation != null)
                {
                    ((IDisposable)this.xmlReaderWithSourceLocation).Dispose();
                }

                this.xmlReaderWithSourceLocation = null;
            }
        }

        private static XamlNode CreateCurrentNode(XamlReader xamlReader, IXamlLineInfo xamlLineInfo)
        {
            XamlNode currentNode = new XamlNode
            {
                Namespace = xamlReader.Namespace,
                NodeType = xamlReader.NodeType,
                Type = xamlReader.Type,
                Member = xamlReader.Member,
                Value = xamlReader.Value,
                LineNumber = xamlLineInfo.LineNumber,
                LinePosition = xamlLineInfo.LinePosition,
            };

            return currentNode;
        }

        private static bool IsMarkupExtension(XamlNode node)
        {
            Fx.Assert(node != null, "node != null");
            return node.Type != null && node.Type.IsMarkupExtension;
        }

        private bool SuppressingMarkupExtension()
        {
            return this.suppressMarkupExtensionLevel != 0;
        }

        private XamlNode CreateCurrentNode()
        {
            return CreateCurrentNode(this.underlyingReader, this.xamlLineInfo);
        }

        private void StartAccessingBuffer()
        {
            this.Current = this.bufferedXamlNodes.Dequeue();
        }

        private void PushObjectDeclarationNodeIfApplicable()
        {
            switch (this.Current.NodeType)
            {
                case XamlNodeType.StartObject:
                case XamlNodeType.GetObject:
                    this.objectDeclarationRecords.Push(this.Current);
                    if (IsMarkupExtension(this.Current))
                    {
                        ++this.suppressMarkupExtensionLevel;
                    }
                    break;
            }
        }

        private void OnValueNodeDeserialized(object value, DocumentRange attributeValueLocation)
        {
            int startLine = attributeValueLocation.Start.LineNumber.Value;
            int startColumn = attributeValueLocation.Start.LinePosition.Value;
            int endLine = attributeValueLocation.End.LineNumber.Value;
            int endColumn = attributeValueLocation.End.LinePosition.Value;
            // XamlDebuggerXmlReader has no idea what the filename is (it only knew a stream of data)
            // So we set FileName = null.

            // To enhance visual selection, endColumn + 1
            SourceLocation valueLocation = new SourceLocation(null, startLine, startColumn, endLine, endColumn + 1);
            this.NotifySourceLocationFound(value, valueLocation, isValueNode: true);
        }

        private void InjectLineInfoXamlNodesToBuffer()
        {
            XamlNode startNode = this.objectDeclarationRecords.Pop();

            if (!this.SuppressingMarkupExtension() 
                && (startNode.Type != null && !startNode.Type.IsUnknown && !startNode.Type.IsMarkupExtension))
            {
                DocumentLocation myStartBracket = null;
                DocumentLocation myEndBracket = null;
                DocumentRange myRange;
                DocumentLocation myStartLocation = new DocumentLocation(startNode.LineNumber, startNode.LinePosition);
                if (this.xmlReaderWithSourceLocation.EmptyElementRanges.TryGetValue(myStartLocation, out myRange))
                {
                    myStartBracket = myRange.Start;
                    myEndBracket = myRange.End;
                }
                else
                {
                    DocumentLocation myEndLocation = new DocumentLocation(this.Current.LineNumber, this.Current.LinePosition);
                    this.xmlReaderWithSourceLocation.StartElementLocations.TryGetValue(myStartLocation, out myStartBracket);
                    this.xmlReaderWithSourceLocation.EndElementLocations.TryGetValue(myEndLocation, out myEndBracket);
                }

                // To enhance visual selection
                DocumentLocation myRealEndBracket = new DocumentLocation(myEndBracket.LineNumber.Value, myEndBracket.LinePosition.Value + 1);

                this.bufferedXamlNodes.Clear();
                this.InjectLineInfoMembersToBuffer(myStartBracket, myRealEndBracket);

                DocumentRange valueRange;
                if (this.initializationValueRanges.TryGetValue(startNode, out valueRange))
                {
                    DocumentRange realValueRange = new DocumentRange(valueRange.Start,
                        new DocumentLocation(valueRange.End.LineNumber.Value, valueRange.End.LinePosition.Value + 1));
                    this.SourceLocationCollector.AddValueRange(new DocumentRange(myStartBracket, myRealEndBracket), realValueRange);
                }
            }

            if (IsMarkupExtension(startNode))
            {
                // Pop a level
                Fx.Assert(this.suppressMarkupExtensionLevel > 0, "this.suppressMarkupExtensionLevel > 0");
                --this.suppressMarkupExtensionLevel;
            }

            // We need to make sure we also buffer the current node so that this is not missed when the buffer exhausts.
            this.bufferedXamlNodes.Enqueue(this.Current);
        }

        private void InjectLineInfoMembersToBuffer(DocumentLocation startPosition, DocumentLocation endPosition)
        {
            this.InjectLineInfoMemberToBuffer(this.StartLineMember, startPosition.LineNumber.Value);
            this.InjectLineInfoMemberToBuffer(this.StartColumnMember, startPosition.LinePosition.Value);
            this.InjectLineInfoMemberToBuffer(this.EndLineMember, endPosition.LineNumber.Value);
            this.InjectLineInfoMemberToBuffer(this.EndColumnMember, endPosition.LinePosition.Value);
        }

        private void InjectLineInfoMemberToBuffer(XamlMember member, int value)
        {
            this.bufferedXamlNodes.Enqueue(new XamlNode { NodeType = XamlNodeType.StartMember, Member = member });
            this.bufferedXamlNodes.Enqueue(new XamlNode { NodeType = XamlNodeType.Value, Value = value });
            this.bufferedXamlNodes.Enqueue(new XamlNode { NodeType = XamlNodeType.EndMember, Member = member });
        }

        private XamlMember CreateAttachableMember(MethodInfo getter, MethodInfo setter, SourceLocationMemberType memberType)
        {
            string memberName = memberType.ToString();
            SourceLocationMemberInvoker invoker = new SourceLocationMemberInvoker(this.SourceLocationCollector, memberType);
            return new XamlMember(memberName, getter, setter, this.schemaContext, invoker);
        }

        private void NotifySourceLocationFound(object instance, SourceLocation currentLocation, bool isValueNode)
        {
            Argument argumentInstance = instance as Argument;

            // For Argument containing an IValueSerializable expression serializing as a ValueNode.
            // We associate the SourceLocation to the expression instead of the Argument.
            // For example, when we have <WriteLine Text="[abc]" />, Then the SourceLocation found for the InArgument object 
            // is associated with the VisualBasicValue object instead.
            if (argumentInstance != null && argumentInstance.Expression is IValueSerializableExpression && isValueNode)
            {
                instance = argumentInstance.Expression;
            }
            if (this._sourceLocationFound != null)
            {
                this._sourceLocationFound(this, new SourceLocationFoundEventArgs(instance, currentLocation, isValueNode));
            }
        }

        private class XamlSourceLocationCollector
        {
            private XamlDebuggerXmlReader parent;
            private object currentObject;
            private int startLine;
            private int startColumn;
            private int endLine;
            private int endColumn;
            private Dictionary<DocumentRange, DocumentRange> objRgnToInitValueRgnMapping;

            internal XamlSourceLocationCollector(XamlDebuggerXmlReader parent)
            {
                this.parent = parent;
                objRgnToInitValueRgnMapping = new Dictionary<DocumentRange, DocumentRange>();
            }

            internal void OnStartLineFound(object instance, int value)
            {
                UnitTestUtility.Assert(this.currentObject == null, "This should be ensured by the XamlSourceLocationObjectReader to emit attachable property in proper order");
                this.currentObject = instance;
                this.startLine = value;
            }

            internal void OnStartColumnFound(object instance, int value)
            {
                UnitTestUtility.Assert(instance == this.currentObject, "This should be ensured by the XamlSourceLocationObjectReader to emit attachable property in proper order");
                this.startColumn = value;
            }

            internal void OnEndLineFound(object instance, int value)
            {
                UnitTestUtility.Assert(instance == this.currentObject, "This should be ensured by the XamlSourceLocationObjectReader to emit attachable property in proper order");
                this.endLine = value;
            }

            internal void OnEndColumnFound(object instance, int value)
            {
                UnitTestUtility.Assert(instance == this.currentObject, "This should be ensured by the XamlSourceLocationObjectReader to emit attachable property in proper order");
                this.endColumn = value;

                // Notify value first to keep the order from "inner to outer".
                this.NotifyValueIfNeeded(instance);

                // XamlDebuggerXmlReader has no idea what the filename is (it only knew a stream of data)
                // So we set FileName = null.
                this.parent.NotifySourceLocationFound(instance, new SourceLocation(/* FileName = */ null, startLine, startColumn, endLine, endColumn), isValueNode: false);
                this.currentObject = null;
            }

            internal void AddValueRange(DocumentRange startNodeRange, DocumentRange valueRange)
            {
                this.objRgnToInitValueRgnMapping.Add(startNodeRange, valueRange);
            }

            private static bool ShouldReportValue(object instance)
            {
                return instance is Argument;
            }

            // in the case:
            // <InArgument x:TypeArguments="x:String">["abc" + ""]</InArgument>
            // instance is a Argument, with a VB Expression.
            // We hope, the VB expression got notified, too.
            private void NotifyValueIfNeeded(object instance)
            {
                if (!ShouldReportValue(instance))
                {
                    return;
                }

                DocumentRange valueRange;
                if (this.objRgnToInitValueRgnMapping.TryGetValue(
                    new DocumentRange(this.startLine, this.startColumn, this.endLine, this.endColumn), out valueRange))
                {
                    this.parent.NotifySourceLocationFound(instance,
                        new SourceLocation(/* FileName = */ null,
                        valueRange.Start.LineNumber.Value,
                        valueRange.Start.LinePosition.Value,
                        valueRange.End.LineNumber.Value,
                        valueRange.End.LinePosition.Value),
                        isValueNode: true);
                }
            }
        }

        private class SourceLocationMemberInvoker : XamlMemberInvoker
        {
            private XamlSourceLocationCollector sourceLocationCollector;
            private SourceLocationMemberType sourceLocationMember;

            public SourceLocationMemberInvoker(XamlSourceLocationCollector sourceLocationCollector, SourceLocationMemberType sourceLocationMember)
            {
                this.sourceLocationCollector = sourceLocationCollector;
                this.sourceLocationMember = sourceLocationMember;
            }

            public override object GetValue(object instance)
            {
                UnitTestUtility.Assert(false, "This method should not be called within framework code.");
                return null;
            }

            public override void SetValue(object instance, object propertyValue)
            {
                UnitTestUtility.Assert(propertyValue is int, "The value for this attachable property should be an integer and is ensured by the emitter.");
                int value = (int)propertyValue;
                switch (this.sourceLocationMember)
                {
                    case SourceLocationMemberType.StartLine:
                        this.sourceLocationCollector.OnStartLineFound(instance, value);
                        break;
                    case SourceLocationMemberType.StartColumn:
                        this.sourceLocationCollector.OnStartColumnFound(instance, value);
                        break;
                    case SourceLocationMemberType.EndLine:
                        this.sourceLocationCollector.OnEndLineFound(instance, value);
                        break;
                    case SourceLocationMemberType.EndColumn:
                        this.sourceLocationCollector.OnEndColumnFound(instance, value);
                        break;
                    default:
                        UnitTestUtility.Assert(false, "All possible SourceLocationMember are exhausted.");
                        break;
                }
            }
        }

        private class ValueNodeXamlMemberInvoker : XamlMemberInvoker
        {
            private XamlDebuggerXmlReader parent;
            private XamlMemberInvoker wrapped;
            private DocumentRange attributeValueRange;

            internal ValueNodeXamlMemberInvoker(XamlDebuggerXmlReader parent, XamlMemberInvoker wrapped, DocumentRange attributeValueRange)
            {
                this.parent = parent;
                this.wrapped = wrapped;
                this.attributeValueRange = attributeValueRange;
            }

            public override ShouldSerializeResult ShouldSerializeValue(object instance)
            {
                return this.wrapped.ShouldSerializeValue(instance);
            }

            public override object GetValue(object instance)
            {
                return this.wrapped.GetValue(instance);
            }

            public override void SetValue(object instance, object value)
            {
                this.parent.OnValueNodeDeserialized(value, this.attributeValueRange);
                this.wrapped.SetValue(instance, value);
            }
        }
    }
}
