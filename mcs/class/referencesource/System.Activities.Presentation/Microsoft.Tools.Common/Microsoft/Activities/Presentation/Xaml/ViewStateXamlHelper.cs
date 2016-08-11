// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities;
    using System.Activities.Debugger;
    using System.Activities.DynamicUpdate;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.ViewState;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Xaml;

    internal static class ViewStateXamlHelper
    {
        static readonly string ViewStateManager = WorkflowViewState.ViewStateManagerProperty.MemberName;
        static readonly string IdRef = WorkflowViewState.IdRefProperty.MemberName;
        static readonly MethodInfo GetViewStateManager = typeof(WorkflowViewState).GetMethod("GetViewStateManager");
        static readonly MethodInfo SetViewStateManager = typeof(WorkflowViewState).GetMethod("SetViewStateManager");
        static readonly MethodInfo GetIdRef = typeof(WorkflowViewState).GetMethod("GetIdRef");
        static readonly MethodInfo SetIdRef = typeof(WorkflowViewState).GetMethod("SetIdRef");
        static readonly List<string> SourceLocationNames = new List<string> 
            {
                XamlDebuggerXmlReader.StartLineName.MemberName,
                XamlDebuggerXmlReader.StartColumnName.MemberName,
                XamlDebuggerXmlReader.EndLineName.MemberName,
                XamlDebuggerXmlReader.EndColumnName.MemberName
            };

        // These are used to discover that we have found a DynamicUpdateInfo.OriginalDefintion or OriginalActivityBuilder
        // attached property member. We have "hardcoded" the *MemberName" here because DynamicUpdateInfo has the
        // AttachableMemberIdentifier properties marked as private. But the DynamicUpdateInfo class itself is public,
        // as are the Get and Set methods.
        static readonly string DynamicUpdateOriginalDefinitionMemberName = "OriginalDefinition";
        static readonly MethodInfo GetOriginalDefinition = typeof(DynamicUpdateInfo).GetMethod("GetOriginalDefinition");
        static readonly MethodInfo SetOriginalDefinition = typeof(DynamicUpdateInfo).GetMethod("SetOriginalDefinition");

        static readonly string DynamicUpdateOriginalActivityBuilderMemberName = "OriginalActivityBuilder";
        static readonly MethodInfo GetOriginalActivityBuilder = typeof(DynamicUpdateInfo).GetMethod("GetOriginalActivityBuilder");
        static readonly MethodInfo SetOriginalActivityBuilder = typeof(DynamicUpdateInfo).GetMethod("SetOriginalActivityBuilder");

        // This method collects view state attached properties and generates a Xaml node stream 
        // with all view state information appearing within the ViewStateManager node. 
        // It is called when workflow definition is being serialized to string.
        // inputReader  - Nodestream with view state information as attached properties on the activity nodes.
        //                The reader is positioned at the begining of the workflow definition.
        // idManager    - This component issues running sequence numbers for IdRef.
        // Result       - Node stream positioned at the begining of the workflow definition with a  
        //                ViewStateManager node containing all view state information.
        // Implementation logic:
        // 1. Scan the input nodestream Objects for attached properties that need to be converted (VirtualizedContainerService.HintSize and WorkflowViewStateService.ViewState).
        // 2. If the Object had a IdRef value then use it otherwise generate a new value.
        // 3. Store idRef value and corresponding viewstate related attached property nodes (from step 1) 
        //    in the viewStateInfo dictionary.
        // 4. Use the viewStateInfo dictionary to generate ViewStateManager node which is then inserted
        //    into the end of output nodestream.
        public static XamlReader ConvertAttachedPropertiesToViewState(XamlObjectReader inputReader, ViewStateIdManager idManager)
        {
            // Stack to track StartObject/GetObject and EndObject nodes.
            Stack<Frame> stack = new Stack<Frame>();

            XamlMember viewStateManager = new XamlMember(ViewStateManager, GetViewStateManager, SetViewStateManager, inputReader.SchemaContext);
            XamlMember idRefMember = new XamlMember(IdRef, GetIdRef, SetIdRef, inputReader.SchemaContext);
            
            // Xaml member corresponding to x:Class property of the workflow definition. Used to find x:Class value in the node stream.
            XamlMember activityBuilderName = new XamlMember(typeof(ActivityBuilder).GetProperty("Name"), inputReader.SchemaContext);
            string activityBuilderTypeName = typeof(ActivityBuilder).Name;

            // Dictionary to keep track of IdRefs and corresponding viewstate related
            // attached property nodes.
            Dictionary<string, XamlNodeList> viewStateInfo = new Dictionary<string, XamlNodeList>();

            // Output node list
            XamlNodeList workflowDefinition = new XamlNodeList(inputReader.SchemaContext);

            using (XamlWriter workflowDefinitionWriter = workflowDefinition.Writer)
            {
                bool design2010NamespaceFound = false;
                bool inIdRefMember = false;
                bool inxClassMember = false;
                bool skipWritingWorkflowDefinition = false;
                bool skipReadingWorkflowDefinition = false;
                string xClassName = null;

                while (skipReadingWorkflowDefinition || inputReader.Read())
                {
                    skipWritingWorkflowDefinition = false;
                    skipReadingWorkflowDefinition = false;
                    switch (inputReader.NodeType)
                    {
                        case XamlNodeType.NamespaceDeclaration:
                            if (inputReader.Namespace.Namespace.Equals(NameSpaces.Design2010, StringComparison.Ordinal))
                            {
                                design2010NamespaceFound = true;
                            }
                            break;

                        case XamlNodeType.StartObject:
                            // Save the Xaml type and clr object on the stack frame. These are used later to generate 
                            // IdRef values and attaching the same to the clr object.
                            stack.Push(new Frame() { Type = inputReader.Type, InstanceObject = inputReader.Instance });

                            // If the design2010 namespace was not found add the namespace node
                            // before the start object is written out.
                            if (!design2010NamespaceFound)
                            {
                                workflowDefinitionWriter.WriteNamespace(new NamespaceDeclaration(NameSpaces.Design2010, NameSpaces.Design2010Prefix));
                                design2010NamespaceFound = true;
                            }
                            break;

                        case XamlNodeType.GetObject:
                            // Push an empty frame to balance the Pop operation when the EndObject node 
                            // is encountered.
                            stack.Push(new Frame() { Type = null });
                            break;

                        case XamlNodeType.StartMember:
                            // Track when we enter IdRef member so that we can save its value.
                            if (inputReader.Member.Equals(idRefMember))
                            {
                                inIdRefMember = true;
                            }
                            // Track when we enter x:Class member so that we can save its value.
                            else if (inputReader.Member.Equals(activityBuilderName))
                            {
                                inxClassMember = true;
                            }
                            // Start of VirtualizedContainerService.HintSize or WorkflowViewStateService.ViewState property.
                            else if (IsAttachablePropertyForConvert(inputReader)) 
                            {
                                // The top of stack here corresponds to the activity on which
                                // the above properties are attached.
                                if (stack.Peek().AttachedPropertyNodes == null)
                                {
                                    stack.Peek().AttachedPropertyNodes = new XamlNodeList(inputReader.SchemaContext);
                                }

                                // Write the attached property's xaml nodes into the stack.
                                XamlReader subTreeReader = inputReader.ReadSubtree();
                                XamlWriter attachedPropertyWriter = stack.Peek().AttachedPropertyNodes.Writer;
                                while (subTreeReader.Read())
                                {
                                    attachedPropertyWriter.WriteNode(subTreeReader);
                                }

                                // The subtree reader loop put us at the begining of the next node in the input stream. 
                                // So skip reading/writing it out just yet.
                                skipReadingWorkflowDefinition = true;
                                skipWritingWorkflowDefinition = true;
                            }
                            break;

                        case XamlNodeType.Value:
                            // Read and save IdRef/x:Class member values.
                            // Also update idManager to keep track of prefixes and ids seen.
                            if (inIdRefMember)
                            {
                                string idRef = inputReader.Value as string;
                                stack.Peek().IdRef = idRef;
                                idManager.UpdateMap(idRef); 
                            }
                            else if (inxClassMember)
                            {
                                xClassName = inputReader.Value as string;
                                idManager.UpdateMap(xClassName);
                            }
                            break;

                        case XamlNodeType.EndMember:
                            // Exit IdRef/x:Class member state.
                            if (inIdRefMember)
                            {
                                inIdRefMember = false;
                            }
                            else if (inxClassMember)
                            {
                                inxClassMember = false;
                            }
                            break;

                        case XamlNodeType.EndObject:
                            // Remove an item from the stack because we encountered the end of an object definition.
                            Frame frameObject = stack.Pop();

                            // If the object had (viewstate related) attached properties we need to save them
                            // into the viewStateInfo dictionary.
                            if (frameObject.AttachedPropertyNodes != null)
                            {
                                frameObject.AttachedPropertyNodes.Writer.Close();

                                // If the object didn't have IdRef, generate a new one.
                                if (string.IsNullOrWhiteSpace(frameObject.IdRef))
                                {
                                    // Use the object type name (or x:Class value) to generate a new id.
                                    if (frameObject.Type != null)
                                    {
                                        string prefix = frameObject.Type.Name;
                                        if (frameObject.Type.UnderlyingType != null)
                                        {
                                            prefix = frameObject.Type.UnderlyingType.Name;
                                        }

                                        if (string.CompareOrdinal(prefix, activityBuilderTypeName) == 0 && !string.IsNullOrWhiteSpace(xClassName))
                                        {
                                            frameObject.IdRef = idManager.GetNewId(xClassName);
                                        }
                                        else
                                        {
                                            frameObject.IdRef = idManager.GetNewId(prefix);
                                        }
                                    }
                                    else //Fallback to generating a guid value.
                                    {
                                        frameObject.IdRef = Guid.NewGuid().ToString();
                                    }

                                    // Since we didn't see a IdRef on this object, insert the generated 
                                    // viewstate id into the output Xaml node-stream.
                                    workflowDefinitionWriter.WriteStartMember(idRefMember);
                                    workflowDefinitionWriter.WriteValue(frameObject.IdRef);
                                    workflowDefinitionWriter.WriteEndMember();

                                    // Save the generated idRef on the corresponding clr object as well.
                                    if (frameObject.InstanceObject != null)
                                    {
                                        WorkflowViewState.SetIdRef(frameObject.InstanceObject, frameObject.IdRef);
                                    }
                                }

                                viewStateInfo[frameObject.IdRef] = frameObject.AttachedPropertyNodes;
                            }

                            // We're at the end of input nodestream and have collected data in viewStateInfo
                            // so we need to create and insert the ViewStateManager nodes into the output nodestream.
                            if (stack.Count == 0 && viewStateInfo.Count > 0)
                            {
                                XamlNodeList viewStateManagerNodeList = CreateViewStateManagerNodeList(viewStateInfo, inputReader.SchemaContext);
                                XamlReader viewStateManagerNodeReader = viewStateManagerNodeList.GetReader();

                                // Insert the ViewStateManager nodes into the output node stream.
                                workflowDefinitionWriter.WriteStartMember(viewStateManager);
                                while (viewStateManagerNodeReader.Read())
                                {
                                    workflowDefinitionWriter.WriteNode(viewStateManagerNodeReader);
                                }
                                workflowDefinitionWriter.WriteEndMember(); // viewStateManager
                            }
                            break;
                    }

                    if (!skipWritingWorkflowDefinition)
                    {
                        workflowDefinitionWriter.WriteNode(inputReader);
                    }
                }
            }

            return workflowDefinition.GetReader();
        }

        // This method converts view state information stored within the ViewStateManager node back as
        // attached properties on corresponding activity nodes.
        // It is called when workflow definition is being deserialized from a string.
        // inputReader - Nodestream that may have all view state information in the ViewStateManager node at the end of workflow definition. 
        //               The reader is positioned at the begining of the workflow definition.
        // idManager   - This component issues running sequence numbers for IdRef.
        // viewStateManager - (output) ViewStateManager object instance deserialized from the workflow definition.
        // Result      - Node stream positioned at the begining of the workflow definition with view state related information
        //               appearing as attached properties on activities. The ViewStateManager nodes are removed from the stream.
        // Implementation logic:
        // 1. Scan the input nodestream for ViewStateManager node.
        // 2. If ViewStateManager node is found, store Id and corresponding attached property nodes 
        //    in viewStateInfo dictionary. Otherwise return early.
        // 3. Walk activity nodes in the workflow definition and apply viewstate related attached properties (from 
        //    viewStateInfo dictionary) to each node.
        // 4. If multiple activities have same IdRef values then corresponding viewstate related attached properties 
        //    (from viewStateInfo dictionary) are applied to the first of those activities. The other activities with duplicate
        //    IdRef values do not get view state information.
        public static XamlReader ConvertViewStateToAttachedProperties(XamlReader inputReader, ViewStateIdManager idManager, out Dictionary<string, SourceLocation> viewStateSourceLocationMap)
        {
            int idRefLineNumber = 0;
            int idRefLinePosition = 0;
            bool shouldWriteIdRefEndMember = false;

            XamlReader retVal = null;

            // Xaml member definition for IdRef. Used to identify existing IdRef properties in the input nodestream.
            XamlMember idRefMember = new XamlMember(IdRef, GetIdRef, SetIdRef, inputReader.SchemaContext);

            // These are used to ignore the IdRef members that are inside a DynamicUpdateInfo.OriginalDefinition/OriginalActivityBuilder attached property.
            // We need to ignore these because if we don't, the IdRef values for the objects in the actual workflow defintion will be ignored because of the
            // duplicate IdRef value. This causes problems with activity designers that depend on the ViewStateManager data to correctly display the workflow
            // on the WorkflowDesigner canvas.
            XamlMember originalDefinitionMember = new XamlMember(DynamicUpdateOriginalDefinitionMemberName, GetOriginalDefinition, SetOriginalDefinition, inputReader.SchemaContext);
            XamlMember originalActivityBuilderMember = new XamlMember(DynamicUpdateOriginalActivityBuilderMemberName, GetOriginalActivityBuilder, SetOriginalActivityBuilder, inputReader.SchemaContext);

            // insideOriginalDefintion gets set to true when we find a "StartMember" node for either of the above two attached properties.
            // originalDefintionMemberCount gets incremented if we find any "StartMember" and insideOriginalDefinition is true.
            // originalDefintionMemberCount gets decremented if we find any "EndMember" and insideOriginalDefintion is true.
            // insideOriginalDefintion gets set to false when we find an "EndMember" and originalDefinitionMemberCount gets decremented to 0.
            // If insideOriginalDefintion is true when we find an "IdRef" member, we do NOT add that IdRef to the idRefsSeen HashSet to avoid
            // duplicates being defined by the IdRefs inside of the OriginalDefinition attached properties.
            bool insideOriginalDefinition = false;
            int originalDefinitionMemberCount = 0;

            // Dictionary containing Ids and corresponding viewstate related
            // attached property nodes. Populated by StripViewStateElement method.
            Dictionary<string, XamlNodeList> viewStateInfo = null;
            XamlReader workflowDefinition = StripViewStateElement(inputReader, out viewStateInfo, out viewStateSourceLocationMap);

            // This is used to keep track of duplicate IdRefs in the workflow definition.
            HashSet<string> idRefsSeen = new HashSet<string>();
            
            // If the inputReader did not have a ViewStateManager node (4.0 format)
            // return early.
            if (viewStateInfo == null)
            {
                retVal = workflowDefinition;
            }
            else
            {
                // Stack to track StartObject/GetObject and EndObject nodes.
                Stack<Frame> stack = new Stack<Frame>();

                // Output node list.
                XamlNodeList mergedNodeList = new XamlNodeList(workflowDefinition.SchemaContext);
                bool inIdRefMember = false;

                using (XamlWriter mergedNodeWriter = mergedNodeList.Writer)
                {
                    IXamlLineInfo lineInfo = workflowDefinition as IXamlLineInfo;
                    IXamlLineInfoConsumer lineInfoComsumer = mergedNodeWriter as IXamlLineInfoConsumer;
                    bool shouldPassLineInfo = lineInfo != null && lineInfo.HasLineInfo && lineInfoComsumer != null && lineInfoComsumer.ShouldProvideLineInfo;

                    while (workflowDefinition.Read())
                    {
                        bool skipWritingWorkflowDefinition = false;

                        switch (workflowDefinition.NodeType)
                        {
                            case XamlNodeType.StartObject:
                                stack.Push(new Frame { Type = workflowDefinition.Type });
                                break;

                            case XamlNodeType.GetObject:
                                stack.Push(new Frame { Type = null });
                                break;

                            case XamlNodeType.StartMember:
                                // If we find a StartMember for DynamicUpdateInfo.OriginalDefinition or OriginalActivityBuilder, remember that we are
                                // inside one of those. We don't want to "remember" IdRef values in the idRefsSeen HashSet while inside these attached properties.
                                if (workflowDefinition.Member.Equals(originalDefinitionMember) || workflowDefinition.Member.Equals(originalActivityBuilderMember))
                                {
                                    insideOriginalDefinition = true;
                                }
                                
                                if (insideOriginalDefinition)
                                {
                                    originalDefinitionMemberCount++;
                                }

                                // Track when the reader enters IdRef. Skip writing the start 
                                // node to the output nodelist until we check for duplicates.
                                else if (workflowDefinition.Member.Equals(idRefMember))
                                {
                                    inIdRefMember = true;
                                    skipWritingWorkflowDefinition = true;

                                    if (shouldPassLineInfo)
                                    {
                                        idRefLineNumber = lineInfo.LineNumber;
                                        idRefLinePosition = lineInfo.LinePosition;
                                    }
                                }
                                break;

                            case XamlNodeType.Value:
                                if (inIdRefMember)
                                {
                                    // We don't want to deal with the IdRef if we are inside a DynamicUpdateInfo.OriginalDefinition/OriginalActivityBuilder
                                    // attached property.
                                    if (!insideOriginalDefinition)
                                    {
                                        string idRef = workflowDefinition.Value as string;
                                        if (!string.IsNullOrWhiteSpace(idRef))
                                        {
                                            // If IdRef value is a duplicate then do not associate it with 
                                            // the stack frame (top of stack == activity node with IdRef member on it).
                                            if (idRefsSeen.Contains(idRef))
                                            {
                                                stack.Peek().IdRef = null;
                                            }
                                            // If the IdRef value is unique then associate it with the
                                            // stack frame and also write its value into the output nodestream.
                                            else
                                            {
                                                stack.Peek().IdRef = idRef;
                                                idManager.UpdateMap(idRef);
                                                idRefsSeen.Add(idRef);

                                                if (shouldPassLineInfo)
                                                {
                                                    lineInfoComsumer.SetLineInfo(idRefLineNumber, idRefLinePosition);
                                                }

                                                mergedNodeWriter.WriteStartMember(idRefMember);

                                                if (shouldPassLineInfo)
                                                {
                                                    lineInfoComsumer.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
                                                }

                                                mergedNodeWriter.WriteValue(idRef);

                                                shouldWriteIdRefEndMember = true;
                                            }
                                        }
                                    }
                                    // Don't need to write IdRef value into the output
                                    // nodestream. If the value was valid, it would have been written above.
                                    skipWritingWorkflowDefinition = true;
                                }
                                break;

                            case XamlNodeType.EndMember:
                                // If we are inside an OriginalDefinition/OriginalActivityBuilder attached property,
                                // decrement the count and if it goes to zero, set insideOriginalDefintion to false
                                // because we just encountered the EndMember for it.
                                if (insideOriginalDefinition)
                                {
                                    originalDefinitionMemberCount--;
                                    if (originalDefinitionMemberCount == 0)
                                    {
                                        insideOriginalDefinition = false;
                                    }
                                }

                                // Exit IdRef node. Skip writing the EndMember node, we would have done 
                                // it as part of reading the IdRef value.
                                if (inIdRefMember && !insideOriginalDefinition)
                                {
                                    inIdRefMember = false;
                                    skipWritingWorkflowDefinition = true;

                                    if (shouldWriteIdRefEndMember)
                                    {
                                        shouldWriteIdRefEndMember = false;
                                        
                                        if (shouldPassLineInfo)
                                        {
                                            lineInfoComsumer.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
                                        }

                                        mergedNodeWriter.WriteEndMember();
                                    }
                                }

                                break;

                            case XamlNodeType.EndObject:
                                Frame frameObject = stack.Pop();
                                // Before we exit the end of an object, check if it had IdRef
                                // associated with it. If it did, look-up viewStateInfo for viewstate
                                // related attached property nodes and add them to the output nodelist.
                                if (!string.IsNullOrWhiteSpace(frameObject.IdRef))
                                {
                                    XamlNodeList viewStateNodeList;
                                    if (viewStateInfo.TryGetValue(frameObject.IdRef, out viewStateNodeList))
                                    {
                                        XamlReader viewStateReader = viewStateNodeList.GetReader();

                                        IXamlLineInfo viewStateLineInfo = viewStateReader as IXamlLineInfo;
                                        bool viewStateShouldPassLineInfo = viewStateLineInfo != null && viewStateLineInfo.HasLineInfo && lineInfoComsumer != null && lineInfoComsumer.ShouldProvideLineInfo;

                                        while (viewStateReader.Read())
                                        {
                                            if (viewStateShouldPassLineInfo)
                                            {
                                                lineInfoComsumer.SetLineInfo(viewStateLineInfo.LineNumber, viewStateLineInfo.LinePosition);
                                            }

                                            mergedNodeWriter.WriteNode(viewStateReader);
                                        }
                                    }
                                }
                                break;
                        }
                        if (!skipWritingWorkflowDefinition)
                        {
                            if (shouldPassLineInfo)
                            {
                                lineInfoComsumer.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
                            }

                            mergedNodeWriter.WriteNode(workflowDefinition);
                        }
                    }
                }

                retVal = mergedNodeList.GetReader();
            }

            return retVal;
        }

        // This method removes IdRef nodes from the nodestream. This method would be called
        // when a 4.5 workflow definition is retargeted to 4.0.
        public static XamlReader RemoveIdRefs(XamlObjectReader inputReader)
        {
            XamlMember idRefMember = new XamlMember(IdRef, GetIdRef, SetIdRef, inputReader.SchemaContext);

            XamlNodeList outputNodeList = new XamlNodeList(inputReader.SchemaContext);
            using (XamlWriter outputWriter = outputNodeList.Writer)
            {
                while (inputReader.Read())
                {
                    if (inputReader.NodeType == XamlNodeType.StartMember && inputReader.Member.Equals(idRefMember))
                    {
                        // Exhaust the idRefMember sub-tree.
                        XamlReader idRefReader = inputReader.ReadSubtree();
                        while (idRefReader.Read());
                    }
                    outputWriter.WriteNode(inputReader);
                }
            }
            return outputNodeList.GetReader();
        }

        // This is a helper method to output the nodestream sequence for debugging/diagnostic purposes.
        public static void NodeLoopTest(XamlReader xamlReader)
        {
#if DEBUG
            string tabs = "";
            int depth = 1;
            while (xamlReader.Read())
            {
                switch (xamlReader.NodeType)
                {
                    case XamlNodeType.NamespaceDeclaration:
                        System.Diagnostics.Debug.WriteLine(tabs + "Namespace declaration: {0}:{1}", xamlReader.Namespace.Prefix, xamlReader.Namespace.Namespace);
                        break;
                    case XamlNodeType.StartObject:
                        tabs = new String(' ', depth++);
                        System.Diagnostics.Debug.WriteLine(tabs + "Start object: {0}", xamlReader.Type.Name);
                        break;
                    case XamlNodeType.GetObject:
                        tabs = new String(' ', depth++);
                        System.Diagnostics.Debug.WriteLine(tabs + "Get object");
                        break;
                    case XamlNodeType.StartMember:
                        tabs = new String(' ', depth++);
                        System.Diagnostics.Debug.WriteLine(tabs + "Start member: {0}, Attachable: {1}", xamlReader.Member.Name, xamlReader.Member.IsAttachable);
                        break;
                    case XamlNodeType.Value:
                        tabs = new String(' ', depth++);
                        System.Diagnostics.Debug.WriteLine(tabs + "Value: {0}", xamlReader.Value);
                        --depth;
                        break;
                    case XamlNodeType.EndMember:
                        tabs = new String(' ', --depth);
                        System.Diagnostics.Debug.WriteLine(tabs + "End member");
                        break;
                    case XamlNodeType.EndObject:
                        tabs = new String(' ', --depth);
                        System.Diagnostics.Debug.WriteLine(tabs + "End object");
                        break;
                }
            }
#endif
        }

        // Given the viewStateInfo dictionary, this method returns a xaml node list matching a ViewStateManager
        // object.
        static XamlNodeList CreateViewStateManagerNodeList(Dictionary<string, XamlNodeList> viewStateInfo, XamlSchemaContext schemaContext)
        {
            XamlNodeList viewStateManagerNodeList = new XamlNodeList(schemaContext);

            XamlMember viewStateDataMember = new XamlMember(typeof(ViewStateManager).GetProperty("ViewStateData"), schemaContext);
            XamlType viewStateManagerType = new XamlType(typeof(ViewStateManager), schemaContext);
            XamlType viewStateDataType = new XamlType(typeof(ViewStateData), schemaContext);
            XamlMember idMember = new XamlMember(typeof(ViewStateData).GetProperty("Id"), schemaContext);

            using (XamlWriter viewStateManagerNodeWriter = viewStateManagerNodeList.Writer)
            {
                viewStateManagerNodeWriter.WriteStartObject(viewStateManagerType);
                viewStateManagerNodeWriter.WriteStartMember(viewStateDataMember);
                viewStateManagerNodeWriter.WriteGetObject();
                viewStateManagerNodeWriter.WriteStartMember(XamlLanguage.Items);

                foreach (KeyValuePair<string, XamlNodeList> entry in viewStateInfo)
                {
                    viewStateManagerNodeWriter.WriteStartObject(viewStateDataType);

                    viewStateManagerNodeWriter.WriteStartMember(idMember);
                    viewStateManagerNodeWriter.WriteValue(entry.Key);
                    viewStateManagerNodeWriter.WriteEndMember(); // idMember

                    XamlReader viewStateValueReader = entry.Value.GetReader();
                    while (viewStateValueReader.Read())
                    {
                        viewStateManagerNodeWriter.WriteNode(viewStateValueReader);
                    }

                    viewStateManagerNodeWriter.WriteEndObject(); // viewStateDataType
                }

                viewStateManagerNodeWriter.WriteEndMember(); // XamlLanguage.Items
                viewStateManagerNodeWriter.WriteEndObject(); // GetObject
                viewStateManagerNodeWriter.WriteEndMember(); // viewStateDataMember
                viewStateManagerNodeWriter.WriteEndObject(); // viewStateManagerType
                viewStateManagerNodeWriter.Close();
            }

            return viewStateManagerNodeList;
        }

        // Checks if the Xaml reader is on one of WorkflowViewStateService.ViewState or
        // VirtualizedContainerService.HintSize members in the nodestream. These
        // members need to be moved into the ViewStateManager node during the conversion.
        static bool IsAttachablePropertyForConvert(XamlReader reader)
        {
            if (reader.NodeType == XamlNodeType.StartMember)
            {
                XamlMember member = reader.Member;
                if (member.IsAttachable)
                {
                    if (member.DeclaringType.UnderlyingType == typeof(WorkflowViewStateService) && member.Name.Equals("ViewState", StringComparison.Ordinal))
                    {
                        return true;
                    }
                    else if (member.DeclaringType.UnderlyingType == typeof(VirtualizedContainerService) && member.Name.Equals("HintSize", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // This method reads ViewStateManager nodes from the xaml nodestream and outputs that in the
        // viewStateInfo dictionary. The input reader is positioned at the begining of the workflow definition.
        // The method returns a reader positioned at the begining of the workflow definition with the ViewStateManager 
        // nodes removed.
        static XamlReader StripViewStateElement(XamlReader inputReader, out Dictionary<string, XamlNodeList> viewStateInfo, out Dictionary<string, SourceLocation> viewStateSourceLocationMap)
        {
            viewStateSourceLocationMap = null;
            XamlNodeList strippedNodeList = new XamlNodeList(inputReader.SchemaContext);
            XamlMember viewStateManager = new XamlMember(ViewStateManager, GetViewStateManager, SetViewStateManager, inputReader.SchemaContext);

            using (XamlWriter strippedWriter = strippedNodeList.Writer)
            {
                IXamlLineInfo lineInfo = inputReader as IXamlLineInfo;
                IXamlLineInfoConsumer lineInfoComsumer = strippedWriter as IXamlLineInfoConsumer;
                bool shouldPassLineInfo = lineInfo != null && lineInfo.HasLineInfo && lineInfoComsumer != null && lineInfoComsumer.ShouldProvideLineInfo;

                viewStateInfo = null;
                while (inputReader.Read())
                {
                    if (inputReader.NodeType == XamlNodeType.StartMember && inputReader.Member.Equals(viewStateManager))
                    {
                        ReadViewStateInfo(inputReader.ReadSubtree(), out viewStateInfo, out viewStateSourceLocationMap);
                        
                    }

                    if (shouldPassLineInfo)
                    {
                        lineInfoComsumer.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
                    }

                    strippedWriter.WriteNode(inputReader);
                }
            }

            return strippedNodeList.GetReader();
        }

        // This method reads ViewStateManager nodes from the xaml nodestream and outputs that in the 
        // viewStateInfo dictionary. The input reader is positioned on the ViewStateManagerNode in the nodestream.
        static void ReadViewStateInfo(XamlReader inputReader, out Dictionary<string, XamlNodeList> viewStateInfo, out Dictionary<string, SourceLocation> viewStateSourceLocationMap)
        {
            XamlType viewStateType = new XamlType(typeof(ViewStateData), inputReader.SchemaContext);

            viewStateInfo = new Dictionary<string, XamlNodeList>();
            viewStateSourceLocationMap = new Dictionary<string, SourceLocation>();
            bool skipReading = false;
            while (skipReading || inputReader.Read())
            {
                skipReading = false;
                if (inputReader.NodeType == XamlNodeType.StartObject && inputReader.Type.Equals(viewStateType))
                {
                    string id;
                    XamlNodeList viewStateNodeList;
                    SourceLocation viewStateSourceLocation = null;
                    ReadViewState(viewStateType, inputReader.ReadSubtree(), out id, out viewStateNodeList, out viewStateSourceLocation);
                    if (id != null)
                    {
                        viewStateInfo[id] = viewStateNodeList;
                        viewStateSourceLocationMap[id] = viewStateSourceLocation;
                    }

                    //inputReader will be positioned on the next node so no need to advance it.
                    skipReading = true;
                }
            }
        }

        // This method reads a ViewStateData node from the xaml nodestream. It outputs the Id property into viewStateId
        // and the attached viewstate related properties in viewStateNodes. The input reader is positioned on a 
        // ViewStateData node within ViewStateManager.
        static void ReadViewState(XamlType viewStateType, XamlReader xamlReader, out string viewStateId, out XamlNodeList viewStateNodes, out SourceLocation sourceLocation)
        {
            int globalMemberLevel = 0;
            bool skippingUnexpectedAttachedProperty = false;
            int skippingUnexpectedAttachedPropertyLevel = 0;
            viewStateId = null;
            viewStateNodes = new XamlNodeList(viewStateType.SchemaContext);
            sourceLocation = null;

            Stack<Frame> objectNodes = new Stack<Frame>();
            XamlMember idMember = new XamlMember(typeof(ViewStateData).GetProperty("Id"), xamlReader.SchemaContext);
            int[] viewStateDataSourceLocation = new int[4];
            int sourceLocationIndex = -1;

            IXamlLineInfo lineInfo = xamlReader as IXamlLineInfo;
            IXamlLineInfoConsumer lineInfoComsumer = viewStateNodes.Writer as IXamlLineInfoConsumer;
            bool shouldPassLineInfo = lineInfo != null && lineInfo.HasLineInfo && lineInfoComsumer != null && lineInfoComsumer.ShouldProvideLineInfo;

            while (xamlReader.Read())
            {
                bool skipWritingToNodeList = false;
                switch (xamlReader.NodeType)
                {
                    case XamlNodeType.StartObject:
                        if (xamlReader.Type.Equals(viewStateType))
                        {
                            skipWritingToNodeList = true;
                        }
                        objectNodes.Push(new Frame { Type = xamlReader.Type });
                        break;

                    case XamlNodeType.GetObject:
                        objectNodes.Push(new Frame { Type = null });
                        break;

                    case XamlNodeType.StartMember:
                        globalMemberLevel++;
                        if (xamlReader.Member.Equals(idMember))
                        {
                            XamlReader idNode = xamlReader.ReadSubtree();
                            while (idNode.Read())
                            {
                                if (idNode.NodeType == XamlNodeType.Value)
                                {
                                    viewStateId = idNode.Value as string;
                                }
                            }
                        } 
                        // The xamlReader.ReadSubtree and subsequent while loop to get the Id member
                        // has moved the xamlReader forward to the next member. We need to check to see
                        // if it is an Attached Property that we care about. If it isn't then we need to
                        // skip it and not put it in the resulting XamlNodeList.
                        if (globalMemberLevel == 1 && !IsAttachablePropertyForConvert(xamlReader)) 
                        {
                            skippingUnexpectedAttachedProperty = true;
                        }
                        if (skippingUnexpectedAttachedProperty)
                        {
                            skippingUnexpectedAttachedPropertyLevel++;
                        }

                        sourceLocationIndex = GetViewStateDataSourceLocationIndexFromCurrentReader(xamlReader);
                        break;

                    case XamlNodeType.EndMember:
                        globalMemberLevel--;
                        if (skippingUnexpectedAttachedProperty)
                        {
                            skippingUnexpectedAttachedPropertyLevel--;
                        }
                        break;
                    case XamlNodeType.Value:
                        if (xamlReader.Value is int
                            && sourceLocationIndex >= 0 
                            && sourceLocationIndex < viewStateDataSourceLocation.Length)
                        {
                            viewStateDataSourceLocation[sourceLocationIndex] = (int)xamlReader.Value;
                        }

                        break;
                    case XamlNodeType.EndObject:
                        Frame objectNode = objectNodes.Pop();
                        if (objectNode.Type != null && objectNode.Type.Equals(viewStateType))
                        {
                            skipWritingToNodeList = true;
                            // The ViewStateData's source location should be valid, because
                            // before each EndObject, its SourceLocation is injected.
                            // If not, an exception will be thrown from constructor 
                            // of SourceLocation.
                            sourceLocation = new SourceLocation(null,
                                viewStateDataSourceLocation[0],
                                viewStateDataSourceLocation[1],
                                viewStateDataSourceLocation[2],
                                viewStateDataSourceLocation[3]
                                );
                        }

                        Array.Clear(viewStateDataSourceLocation, 0, viewStateDataSourceLocation.Length);
                        break;
                };

                if (!skipWritingToNodeList && !skippingUnexpectedAttachedProperty)
                {
                    if (shouldPassLineInfo)
                    {
                        lineInfoComsumer.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
                    }

                    viewStateNodes.Writer.WriteNode(xamlReader);
                }

                if (skippingUnexpectedAttachedPropertyLevel == 0)
                {
                    skippingUnexpectedAttachedProperty = false;
                }
            }
            viewStateNodes.Writer.Close();
        }

        private static int GetViewStateDataSourceLocationIndexFromCurrentReader(XamlReader xamlReader)
        {
            if (xamlReader.NodeType != XamlNodeType.StartMember
                || xamlReader.Member == null
                || xamlReader.Member.UnderlyingMember == null
                || xamlReader.Member.UnderlyingMember.DeclaringType != typeof(XamlDebuggerXmlReader))
            {
                return -1;
            }

            // if UnderlineType is XamlDebuggerXmlReader, see if it 
            // is one of {StartLine, StartColumn, EndLine, EndColumn}
            return SourceLocationNames.IndexOf(xamlReader.Member.Name);
        }

        // This class is used for tracking Xaml nodestream data.
        class Frame
        {
            // IdRef value if any associated with the node.
            public string IdRef { get; set; }

            // XamlType of the node. Helps generating IdRef values.
            public XamlType Type { get; set; }

            // XamlNodes corresponding to viewstate related attached property nodes.
            public XamlNodeList AttachedPropertyNodes { get; set; }

            // Underlying CLR object. Used to attach generated IdRef values 
            // when serializing workflow definition.
            public object InstanceObject { get; set; }
        }
    }
}
