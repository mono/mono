// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities;
    using System.Activities.Debugger;
    using System.Activities.Debugger.Symbol;
    using System.Activities.Presentation;
    using System.Activities.Presentation.ViewState;
    using System.Activities.Presentation.Xaml;
    using System.Activities.XamlIntegration;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Xaml;

    internal class WorkflowDesignerXamlHelper
    {
        private IWorkflowDesignerXamlHelperExecutionContext executionContext;

        private XamlMember dynamicActivityPropertyNameMember;

        private XamlMember dynamicActivityPropertyValueMember;

        internal WorkflowDesignerXamlHelper(IWorkflowDesignerXamlHelperExecutionContext executionContext)
        {
            this.executionContext = executionContext;
            SharedFx.Assert(this.executionContext != null, "this.executionContext != null");
            SharedFx.Assert(this.executionContext.XamlSchemaContext != null, "this.executionContext.XamlSchemaContext != null");

            this.dynamicActivityPropertyNameMember = new XamlMember(typeof(DynamicActivityProperty).GetProperty("Name"), this.XamlSchemaContext);
            this.dynamicActivityPropertyValueMember = new XamlMember(typeof(DynamicActivityProperty).GetProperty("Value"), this.XamlSchemaContext);
        }

        private delegate void SourceLocationFoundCallback(object obj, SourceLocation sourceLocation);

        internal enum DeserializationMode
        {
            Default,
            ErrorTolerant,
        }

        public FrameworkName FrameworkName
        {
            get { return this.executionContext.FrameworkName; }
        }

        public WorkflowDesignerXamlSchemaContext XamlSchemaContext
        {
            get { return this.executionContext.XamlSchemaContext; }
        }

        public ViewStateIdManager IdManager
        {
            get { return this.executionContext.IdManager; }
        }

        public WorkflowSymbol LastWorkflowSymbol
        {
            get
            {
                return this.executionContext.LastWorkflowSymbol;
            }

            set
            {
                this.executionContext.LastWorkflowSymbol = value;
            }
        }

        public string LocalAssemblyName
        {
            get { return this.executionContext.LocalAssemblyName; }
        }

        public void OnSerializationCompleted(Dictionary<object, object> sourceLocationObjectToModelItemObjectMapping)
        {
            this.executionContext.OnSerializationCompleted(sourceLocationObjectToModelItemObjectMapping);
        }

        public void OnBeforeDeserialize()
        {
            this.executionContext.OnBeforeDeserialize();
        }

        public void OnSourceLocationFound(object target, SourceLocation sourceLocation)
        {
            this.executionContext.OnSourceLocationFound(target, sourceLocation);
        }

        public void OnAfterDeserialize(Dictionary<string, SourceLocation> viewStateDataSourceLocationMapping)
        {
            this.executionContext.OnAfterDeserialize(viewStateDataSourceLocationMapping);
        }

        // Get root Activity. Currently only handle when the object is ActivityBuilder or Activity.
        // May return null if it does not know how to get the root activity.
        internal static Activity GetRootWorkflowElement(object rootModelObject)
        {
            SharedFx.Assert(rootModelObject != null, "Cannot pass null as rootModelObject");
            Activity rootWorkflowElement;
            IDebuggableWorkflowTree debuggableWorkflowTree = rootModelObject as IDebuggableWorkflowTree;
            if (debuggableWorkflowTree != null)
            {
                rootWorkflowElement = debuggableWorkflowTree.GetWorkflowRoot();
            }
            else
            {
                // Loose xaml case.
                rootWorkflowElement = rootModelObject as Activity;
            }

            return rootWorkflowElement;
        }

        internal static Activity GetRootElementForSymbol(object rootInstance, Activity documentRootElement)
        {
            ActivityBuilder activityBuilder = rootInstance as ActivityBuilder;
            if (activityBuilder != null)
            {
                documentRootElement = activityBuilder.ConvertToDynamicActivity();
            }

            return documentRootElement;
        }

        // Copy the root namespaces from a reader to a writer.
        // DesignTimeXamlWriter follows proper XAML convention by omitting the assembly name from
        // clr-namespaces in the local assembly. However, VB Expressions aren't local-assembly-aware,
        // and require an assembly name. So for every clr-namespace with no assembly name, we add an
        // additional namespace record with an assembly name, to support VB.
        // We only do this at the root level, since the designer only writes out namespaces at the root level.
        internal void CopyNamespacesAndAddLocalAssembly(System.Xaml.XamlReader activityBuilderReader, System.Xaml.XamlWriter objectWriter)
        {
            // Designer loads alwas provide line info
            IXamlLineInfo lineInfo = (IXamlLineInfo)activityBuilderReader;
            IXamlLineInfoConsumer lineInfoConsumer = (IXamlLineInfoConsumer)objectWriter;
            HashSet<string> definedPrefixes = new HashSet<string>();
            List<NamespaceDeclaration> localAsmNamespaces = null;

            while (activityBuilderReader.Read())
            {
                lineInfoConsumer.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);

                if (activityBuilderReader.NodeType == XamlNodeType.NamespaceDeclaration)
                {
                    definedPrefixes.Add(activityBuilderReader.Namespace.Prefix);
                    if (this.XamlSchemaContext.IsClrNamespaceWithNoAssembly(activityBuilderReader.Namespace.Namespace))
                    {
                        if (localAsmNamespaces == null)
                        {
                            localAsmNamespaces = new List<NamespaceDeclaration>();
                        }

                        localAsmNamespaces.Add(activityBuilderReader.Namespace);
                    }

                    objectWriter.WriteNode(activityBuilderReader);
                }
                else
                {
                    if (localAsmNamespaces != null)
                    {
                        foreach (NamespaceDeclaration ns in localAsmNamespaces)
                        {
                            string prefix = null;
                            int i = 0;
                            do
                            {
                                i++;
                                prefix = ns.Prefix + i.ToString(CultureInfo.InvariantCulture);
                            }
                            while (definedPrefixes.Contains(prefix));
                            string fullNs = this.XamlSchemaContext.AddLocalAssembly(ns.Namespace);
                            objectWriter.WriteNamespace(new NamespaceDeclaration(fullNs, prefix));
                            definedPrefixes.Add(prefix);
                        }
                    }

                    objectWriter.WriteNode(activityBuilderReader);
                    return;
                }
            }
        }

        internal string SerializeToString(object obj, string fileName)
        {
            FrameworkName targetFramework = this.FrameworkName;

            string sourceFile = null;
            Activity rootWorkflowElement = GetRootWorkflowElement(obj);
            Dictionary<int, object> modelItemObjectSequence = null;

            // If the current target is 4.5 or Higher, let us not write the filename attribute as DebugSymbol eliminates the need for it.
            // We will serialize without the Filename by removing it from the element and adding it back after serialization
            if (targetFramework.Is45OrHigher())
            {
                if (AttachablePropertyServices.TryGetProperty<string>(rootWorkflowElement, XamlDebuggerXmlReader.FileNameName, out sourceFile))
                {
                    AttachablePropertyServices.RemoveProperty(rootWorkflowElement, XamlDebuggerXmlReader.FileNameName);
                }
            }

            TextWriter textWriter = new StringWriter(CultureInfo.InvariantCulture);

            WorkflowDesignerXamlSchemaContext schemaContext = obj is ActivityBuilder ? this.XamlSchemaContext : new WorkflowDesignerXamlSchemaContext(null);

            bool shouldWriteDebugSymbol = true;
            if (targetFramework.IsLessThan45())
            {
                shouldWriteDebugSymbol = false;
            }

            System.Xaml.XamlReader outerReader;
            XamlObjectReaderWithSequence innerReader;

            using (textWriter)
            {
                UsingXamlWriter(
                    new DesignTimeXamlWriter(textWriter, schemaContext, shouldWriteDebugSymbol),
                    delegate(DesignTimeXamlWriter designTimeXamlWriter)
                    {
                        UsingXamlWriter(
                            ActivityXamlServices.CreateBuilderWriter(designTimeXamlWriter),
                            delegate(System.Xaml.XamlWriter activityBuilderWriter)
                            {
                                UsingXamlWriter(
                                    new ActivityTemplateFactoryBuilderWriter(activityBuilderWriter, schemaContext),
                                    delegate(System.Xaml.XamlWriter writer)
                                    {
                                        // If ViewStateManager property is attached, remove it. It needs to be regenerated if we target 4.5. 
                                        // It should be removed if we're targeting 4.0.
                                        AttachablePropertyServices.RemoveProperty(obj, WorkflowViewState.ViewStateManagerProperty);

                                        this.CreateXamlObjectReaders(obj, schemaContext, out outerReader, out innerReader);

                                        using (innerReader)
                                        {
                                            using (outerReader)
                                            {
#if ERROR_TOLERANT_SUPPORT
                                                if (ErrorActivity.GetHasErrorActivities(obj))
                                                {
                                                    ErrorTolerantObjectWriter.TransformAndStripErrors(outerReader, writer);
                                                }
                                                else
                                                {
#endif
                                                    XamlServices.Transform(outerReader, writer);
#if ERROR_TOLERANT_SUPPORT
                                                }
#endif
                                            }
                                        }

                                        modelItemObjectSequence = innerReader.SequenceNumberToObjectMap;
                                    });
                            });
                    });
            }

            string retVal = textWriter.ToString();

            if (targetFramework.IsLessThan45())
            {
                if (sourceFile != null)
                {
                    XamlDebuggerXmlReader.SetFileName(rootWorkflowElement, sourceFile);
                }
            }

            IList<XamlLoadErrorInfo> loadErrors;
            Dictionary<object, SourceLocation> sourceLocations;
            object deserializedObject = this.DeserializeString(retVal, out loadErrors, out sourceLocations);

            if (!string.IsNullOrEmpty(fileName) && targetFramework.Is45OrHigher())
            {
                this.LastWorkflowSymbol = this.GetWorkflowSymbol(fileName, deserializedObject, sourceLocations);
                if (this.LastWorkflowSymbol != null)
                {
                    retVal = retVal.Replace(DesignTimeXamlWriter.EmptyWorkflowSymbol, this.LastWorkflowSymbol.Encode());
                }
            }

            // The symbol is actually removed in GetAttachedWorkflowSymbol() after deserialization completes.
            System.Xaml.AttachablePropertyServices.RemoveProperty(GetRootWorkflowElement(deserializedObject), DebugSymbol.SymbolName);
            this.CreateXamlObjectReaders(deserializedObject, schemaContext, out outerReader, out innerReader);
            Dictionary<object, object> sourceLocationObjectToModelItemObjectMapping = new Dictionary<object, object>(ObjectReferenceEqualityComparer<object>.Default);
            using (innerReader)
            {
                using (outerReader)
                {
                    while (outerReader.Read())
                    {
                    }

                    Dictionary<int, object> sourceLocationObjectSequence = innerReader.SequenceNumberToObjectMap;
                    foreach (KeyValuePair<int, object> sourceLocationObjectEntry in sourceLocationObjectSequence)
                    {
                        int key = sourceLocationObjectEntry.Key;
                        object sourceLocationObject = sourceLocationObjectEntry.Value;
                        object modelItemObject;

                        if (modelItemObjectSequence.TryGetValue(key, out modelItemObject))
                        {
                            sourceLocationObjectToModelItemObjectMapping.Add(sourceLocationObject, modelItemObject);
                        }
                    }
                }
            }

            this.OnSerializationCompleted(sourceLocationObjectToModelItemObjectMapping);
            return retVal;
        }

        internal void CreateXamlObjectReaders(object deserializedObject, XamlSchemaContext schemaContext, out XamlReader newWorkflowReader, out XamlObjectReaderWithSequence deserializedObjectSequenceBuilder)
        {
            deserializedObjectSequenceBuilder = new XamlObjectReaderWithSequence(deserializedObject, schemaContext);
            if (this.FrameworkName.Is45OrHigher())
            {
                newWorkflowReader = ViewStateXamlHelper.ConvertAttachedPropertiesToViewState(deserializedObjectSequenceBuilder, this.IdManager);
            }
            else
            {
                newWorkflowReader = ViewStateXamlHelper.RemoveIdRefs(deserializedObjectSequenceBuilder);
            }
        }

        internal object DeserializeString(string text)
        {
            IList<XamlLoadErrorInfo> loadErrors;
            Dictionary<object, SourceLocation> sourceLocations;
            return this.DeserializeString(text, out loadErrors, out sourceLocations);
        }

        internal object DeserializeString(string text, out IList<XamlLoadErrorInfo> loadErrors, out Dictionary<object, SourceLocation> sourceLocations)
        {
            try
            {
                return this.DeserializeString(text, DeserializationMode.Default, out loadErrors, out sourceLocations);
            }
            catch (XamlObjectWriterException)
            {
                // Fall back to error-tolerant path. We don't do this by default for perf reasons.
                return this.DeserializeString(text, DeserializationMode.ErrorTolerant, out loadErrors, out sourceLocations);
            }
        }

        //// XAML writer may throw exception during dispose, therefore the following code will cause exception masking
        ////
        //// using (XamlWriter xamlWriter)
        //// {
        ////      ...
        //// }
        ////
        //// If there are any exception A thrown within the block, and if xamlWriter.Dispose() throws an exception B
        //// The exception B will mask exception A and the exception is the ErrorTolerant scenario will be broken.
        ////
        //// The fix to this problem is to ---- any XamlException thrown during Dispose(), we are in general not
        //// interested in those exceptions.
        private static void UsingXamlWriter<T>(T xamlWriter, Action<T> work) where T : XamlWriter
        {
            if (xamlWriter != null)
            {
                try
                {
                    work(xamlWriter);
                }
                finally
                {
                    try
                    {
                        xamlWriter.Close();
                    }
                    catch (XamlException e)
                    {
                        // ignore any XAML exception during closing a XamlWriter
                        Trace.WriteLine(e.Message);
                    }
                }
            }
        }

        // there are two kind of attribute:
        // 1) in lined : argument="some value"
        // 2) <argument>
        //       <Expression ....../>
        //    </argument>
        // here, for (1) return the source location of "some value".
        // for (2) return null
        private static SourceLocation GetInlineAttributeValueLocation(LineColumnPair startPoint, SourceTextScanner sourceTextScanner)
        {
            const char SingleQuote = '\'';
            const char DoubleQuote = '"';
            const char StartAngleBracket = '<';
            Tuple<LineColumnPair, char> start = sourceTextScanner.SearchCharAfter(startPoint, SingleQuote, DoubleQuote, StartAngleBracket);
            if (start == null)
            {
                return null;
            }

            if (start.Item2 == StartAngleBracket)
            {
                return null;
            }

            Tuple<LineColumnPair, char> end = sourceTextScanner.SearchCharAfter(start.Item1, start.Item2);
            if (end == null)
            {
                SharedFx.Assert("end of SourceLocation is not found");
                return null;
            }

            return new SourceLocation(null, start.Item1.LineNumber, start.Item1.ColumnNumber, end.Item1.LineNumber, end.Item1.ColumnNumber);
        }

        private WorkflowSymbol GetWorkflowSymbol(string fileName, object deserializedObject, Dictionary<object, SourceLocation> sourceLocations)
        {
            if (deserializedObject != null)
            {
                Activity deserializedRootElement = GetRootWorkflowElement(deserializedObject);
                if (deserializedRootElement != null)
                {
                    try
                    {
                        deserializedRootElement = GetRootElementForSymbol(deserializedObject, deserializedRootElement);
                        return new WorkflowSymbol
                        {
                            FileName = fileName,
                            Symbols = SourceLocationProvider.GetSymbols(deserializedRootElement, sourceLocations)
                        };
                    }
                    catch (Exception ex)
                    {
                        if (SharedFx.IsFatal(ex))
                        {
                            throw;
                        }

                        // This happens when the workflow is invalid so GetSymbols fails.
                        // ---- exception here.
                    }
                }
            }

            return null;
        }

        private object DeserializeString(string text, DeserializationMode mode, out IList<XamlLoadErrorInfo> loadErrors, out Dictionary<object, SourceLocation> sourceLocations)
        {
            object result = null;
            loadErrors = null;
            Dictionary<object, SourceLocation> collectingSourceLocations = new Dictionary<object, SourceLocation>(ObjectReferenceEqualityComparer<object>.Default);
            SourceLocationFoundCallback sourceLocationFoundCallback = new SourceLocationFoundCallback((obj, sourceLocation) =>
            {
                // If an object appear more than once in the XAML stream (e.g. System.Type, which is cached by reflection)
                // we count the first occurrence.
                if (!collectingSourceLocations.ContainsKey(obj))
                {
                    collectingSourceLocations.Add(obj, sourceLocation);
                }

                this.OnSourceLocationFound(obj, sourceLocation);
            });

            this.XamlSchemaContext.ContainsConversionRequiredType = false;
            Dictionary<string, SourceLocation> viewStateDataSourceLocationMapping = null;
            using (XamlDebuggerXmlReader debuggerReader = new XamlDebuggerXmlReader(new StringReader(text), this.XamlSchemaContext))
            {
                using (System.Xaml.XamlReader activityBuilderReader = ActivityXamlServices.CreateBuilderReader(debuggerReader))
                {
                    using (System.Xaml.XamlReader activityTemplateFactoryBuilderReader = new ActivityTemplateFactoryBuilderReader(activityBuilderReader, this.XamlSchemaContext))
                    {
                        debuggerReader.SourceLocationFound += delegate(object sender, SourceLocationFoundEventArgs args)
                        {
                            sourceLocationFoundCallback(args.Target, args.SourceLocation);
                        };

                        this.OnBeforeDeserialize();
                        debuggerReader.CollectNonActivitySourceLocation = this.FrameworkName.Is45OrHigher();

                        using (System.Xaml.XamlReader reader = ViewStateXamlHelper.ConvertViewStateToAttachedProperties(activityTemplateFactoryBuilderReader, this.IdManager, out viewStateDataSourceLocationMapping))
                        {
                            switch (mode)
                            {
#if ERROR_TOLERANT_SUPPORT
                                case DeserializationMode.ErrorTolerant:
                                    {
                                        ErrorTolerantObjectWriter tolerantWriter = new ErrorTolerantObjectWriter(reader.SchemaContext);
                                        tolerantWriter.LocalAssemblyName = this.LocalAssemblyName;
                                        XamlServices.Transform(reader, tolerantWriter);
                                        loadErrors = this.CheckFileFormatError(tolerantWriter.LoadErrors);
                                        result = tolerantWriter.Result;
                                        ErrorActivity.SetHasErrorActivities(result, true);
                                    }

                                    break;
#endif
                                case DeserializationMode.Default:
                                    {
                                        result = this.TransformAndGetPropertySourceLocation(reader, new SourceTextScanner(text), sourceLocationFoundCallback);

                                        loadErrors = this.CheckFileFormatError(loadErrors);
                                    }

                                    break;
                            }
                        }
                    }
                }
            }

            sourceLocations = collectingSourceLocations;
            this.OnAfterDeserialize(viewStateDataSourceLocationMapping);
            return result;
        }

        // For dynamic activity property, we needs to collect the source location of 
        // its default value when the value is inlined.
        private KeyValuePair<string, SourceLocation> TransformDynamicActivityProperty(
            XamlReader reader,
            XamlObjectWriter objectWriter,
            SourceTextScanner sourceTextScanner)
        {
            // (Number of SM -Number of EM) since SM DAP.Name is read.
            // SO DAP                   ---nameReadingLevel=0
            //   SM NAME                ---nameReadingLevel=1
            //     SO String            ---nameReadingLevel=1
            //       SM Initialize      ---nameReadingLevel=2
            //         VA StringValue   ---nameReadingLevel=2
            //       EM                 ---nameReadingLevel=1
            //     SO                   ---nameReadingLevel=1
            //   EM                     ---nameReadingLevel=0
            // EO                       ---nameReadingLevel=0
            int nameReadingLevel = 0;

            IXamlLineInfo lineInfo = (IXamlLineInfo)reader;
            SourceLocation defaultValueLocation = null;
            string propertyName = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XamlNodeType.StartMember:
                        if (nameReadingLevel > 0
                            || reader.Member == this.dynamicActivityPropertyNameMember)
                        {
                            ++nameReadingLevel;
                        }
                        else if (reader.Member == this.dynamicActivityPropertyValueMember)
                        {
                            LineColumnPair startPoint = new LineColumnPair(lineInfo.LineNumber, lineInfo.LinePosition);
                            defaultValueLocation = GetInlineAttributeValueLocation(startPoint, sourceTextScanner);
                        }

                        break;

                    case XamlNodeType.EndMember:
                        if (nameReadingLevel > 0)
                        {
                            --nameReadingLevel;
                        }

                        break;

                    case XamlNodeType.Value:
                        if (nameReadingLevel > 0)
                        {
                            propertyName = reader.Value as string;
                        }

                        break;
                }

                objectWriter.WriteNode(reader);
            }

            if (propertyName != null && defaultValueLocation != null)
            {
                return new KeyValuePair<string, SourceLocation>(propertyName, defaultValueLocation);
            }

            return new KeyValuePair<string, SourceLocation>();
        }

        private object TransformAndGetPropertySourceLocation(XamlReader reader, SourceTextScanner sourceTextScanner, SourceLocationFoundCallback sourceLocationFoundCallback)
        {
            // <property name, value's start location>
            Dictionary<string, SourceLocation> propertyValueLocationMapping = new Dictionary<string, SourceLocation>();

            object deserializedObject = null;
            object earlyResult = null;

            UsingXamlWriter(
                new XamlObjectWriter(reader.SchemaContext),
                delegate(XamlObjectWriter objectWriter)
                {
                    if (this.XamlSchemaContext.HasLocalAssembly)
                    {
                        this.CopyNamespacesAndAddLocalAssembly(reader, objectWriter);
                    }

                    if (!(reader is IXamlLineInfo))
                    {
                        XamlServices.Transform(reader, objectWriter);
                        earlyResult = objectWriter.Result;
                        return;
                    }

                    XamlType dynamicActivityPropertyType = this.XamlSchemaContext.GetXamlType(typeof(DynamicActivityProperty));
                    while (reader.Read())
                    {
                        // read SubTree will moves the reader pointed to
                        // element after its EO. So, we need to use a while
                        while (!reader.IsEof && reader.NodeType == XamlNodeType.StartObject
                            && dynamicActivityPropertyType == reader.Type)
                        {
                            KeyValuePair<string, SourceLocation> nameSourceLocation = this.TransformDynamicActivityProperty(reader.ReadSubtree(), objectWriter, sourceTextScanner);
                            if (nameSourceLocation.Key != null && nameSourceLocation.Value != null && !propertyValueLocationMapping.ContainsKey(nameSourceLocation.Key))
                            {
                                propertyValueLocationMapping.Add(nameSourceLocation.Key, nameSourceLocation.Value);
                            }
                        }

                        if (!reader.IsEof)
                        {
                            objectWriter.WriteNode(reader);
                        }
                    }

                    deserializedObject = objectWriter.Result;
                });

            if (earlyResult != null)
            {
                return earlyResult;
            }

            ActivityBuilder activityBuilder = deserializedObject as ActivityBuilder;
            if (activityBuilder == null)
            {
                return deserializedObject;
            }

            foreach (KeyValuePair<string, SourceLocation> propertyValueLocation in propertyValueLocationMapping)
            {
                string propertyName = propertyValueLocation.Key;
                SourceLocation propertyLocation = propertyValueLocation.Value;
                if (!activityBuilder.Properties.Contains(propertyName))
                {
                    SharedFx.Assert(string.Format(CultureInfo.CurrentCulture, "no such property:{0}", propertyName));
                    continue;
                }

                DynamicActivityProperty property = activityBuilder.Properties[propertyName];

                if (property == null || property.Value == null)
                {
                    SharedFx.Assert(string.Format(CultureInfo.CurrentCulture, "no such property value:{0}", propertyName));
                    continue;
                }

                object expression = (property.Value is Argument) ? ((Argument)property.Value).Expression : null;
                if (expression != null)
                {
                    sourceLocationFoundCallback(expression, propertyLocation);
                }
                else
                {
                    sourceLocationFoundCallback(property.Value, propertyLocation);
                }
            }

            return deserializedObject;
        }

        private IList<XamlLoadErrorInfo> CheckFileFormatError(IList<XamlLoadErrorInfo> loadErrors)
        {
            IList<XamlLoadErrorInfo> result = loadErrors;

            if (this.XamlSchemaContext.ContainsConversionRequiredType)
            {
                if (result == null)
                {
                    result = new List<XamlLoadErrorInfo>();
                }

                result.Add(new XamlLoadErrorInfo(SharedSR.FileFormatError, 0, 0));
            }

            return result;
        }
    }
}
