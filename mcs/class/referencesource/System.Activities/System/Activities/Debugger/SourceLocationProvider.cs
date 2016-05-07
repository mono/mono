//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Activities.Hosting;
    using System.Activities.XamlIntegration;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Xaml;
    using System.Xml;
    using System.IO;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Activities.Debugger.Symbol;
    using System.Globalization;

    // Provide SourceLocation information for activities in given root activity.
    // This is integration point with Workflow project system (TBD).
    // The current plan is to get SourceLocation from (in this order):
    //  1. pdb (when available)
    //  2a. parse xaml files available in the same project (or metadata store) or
    //  2b. ask user to point to the correct xaml source.
    //  3.  Publish (serialize to tmp file) and deserialize it to collect SourceLocation (for loose xaml).
    // Current code cover only step 3.

    [DebuggerNonUserCode]
    public static class SourceLocationProvider
    {
        [Fx.Tag.Throws(typeof(Exception), "Calls Serialize/Deserialize to temporary file")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "We catch all exceptions to avoid leaking security sensitive information.")]
        [SuppressMessage(FxCop.Category.Security, "CA2103:ReviewImperativeSecurity",
            Justification = "This is security reviewed.")]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts,
            Justification = "The Assert is only enforce while reading the file and the contents is not leaked.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "We catch all exceptions to avoid leaking security sensitive information.")]
        [Fx.Tag.SecurityNote(Critical = "Asserting FileIOPermission(Read) for the specified file name that is contained the attached property on the XAML.",
            Safe = "We are not exposing the contents of the file.")]
        [SecuritySafeCritical]
        static internal Dictionary<object, SourceLocation> GetSourceLocations(Activity rootActivity, out string sourcePath, out bool isTemporaryFile, out byte[] checksum)
        {
            isTemporaryFile = false;
            checksum = null;
            string symbolString = DebugSymbol.GetSymbol(rootActivity) as String;
            if (string.IsNullOrEmpty(symbolString) && rootActivity.Children != null && rootActivity.Children.Count > 0)
            { // In case of actual root is wrapped either in x:Class activity or CorrelationScope
                Activity body = rootActivity.Children[0];
                string bodySymbolString = DebugSymbol.GetSymbol(body) as String;
                if (!string.IsNullOrEmpty(bodySymbolString))
                {
                    rootActivity = body;
                    symbolString = bodySymbolString;
                }
            }

            if (!string.IsNullOrEmpty(symbolString))
            {
                try
                {
                    WorkflowSymbol wfSymbol = WorkflowSymbol.Decode(symbolString);
                    if (wfSymbol != null)
                    {
                        sourcePath = wfSymbol.FileName;
                        checksum = wfSymbol.GetChecksum();
                        // rootActivity is the activity with the attached symbol string.
                        // rootActivity.RootActivity is the workflow root activity.
                        // if they are not the same, then it must be compiled XAML, because loose XAML (i.e. XAMLX) always have the symbol attached at the root.
                        if (rootActivity.RootActivity != rootActivity)
                        {
                            Fx.Assert(rootActivity.Parent != null, "Compiled XAML implementation always have a parent.");
                            rootActivity = rootActivity.Parent;
                        }
                        return GetSourceLocations(rootActivity, wfSymbol, translateInternalActivityToOrigin: false);
                    }
                }
                catch (SerializationException)
                {
                    // Ignore invalid symbol.
                }
            }

            sourcePath = XamlDebuggerXmlReader.GetFileName(rootActivity) as string;
            Dictionary<object, SourceLocation> mapping;
            Assembly localAssembly;
            bool permissionRevertNeeded = false;

            // This may not be the local assembly since it may not be the real root for x:Class 
            localAssembly = rootActivity.GetType().Assembly;

            if (rootActivity.Parent != null)
            {
                localAssembly = rootActivity.Parent.GetType().Assembly;
            }

            if (rootActivity.Children != null && rootActivity.Children.Count > 0)
            { // In case of actual root is wrapped either in x:Class activity or CorrelationScope
                Activity body = rootActivity.Children[0];
                string bodySourcePath = XamlDebuggerXmlReader.GetFileName(body) as string;
                if (!string.IsNullOrEmpty(bodySourcePath))
                {
                    rootActivity = body;
                    sourcePath = bodySourcePath;
                }
            }

            try
            {
                Fx.Assert(!string.IsNullOrEmpty(sourcePath), "If sourcePath is null, it should have been short-circuited before reaching here.");

                SourceLocation tempSourceLocation;
                Activity tempRootActivity;

                checksum = SymbolHelper.CalculateChecksum(sourcePath);

                if (TryGetSourceLocation(rootActivity, sourcePath, checksum, out tempSourceLocation)) // already has source location.
                {
                    tempRootActivity = rootActivity;
                }
                else
                {
                    byte[] buffer;
                    // Need to store the file in memory temporary so don't have to re-read the file twice
                    // for XamlDebugXmlReader's BracketLocator.
                    // If there is a debugger attached, Assert FileIOPermission for Read access to the specific file.
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        permissionRevertNeeded = true;
                        FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.Read, sourcePath);
                        permission.Assert();
                    }

                    try
                    {
                        FileInfo fi = new FileInfo(sourcePath);
                        buffer = new byte[fi.Length];

                        using (FileStream fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                        {
                            fs.Read(buffer, 0, buffer.Length);
                        }
                    }
                    finally
                    {
                        // If we Asserted FileIOPermission, revert it.
                        if (permissionRevertNeeded)
                        {
                            CodeAccessPermission.RevertAssert();
                            permissionRevertNeeded = false;
                        }
                    }

                    object deserializedObject = Deserialize(buffer, localAssembly);
                    IDebuggableWorkflowTree debuggableWorkflowTree = deserializedObject as IDebuggableWorkflowTree;
                    if (debuggableWorkflowTree != null)
                    { // Declarative Service and x:Class case
                        tempRootActivity = debuggableWorkflowTree.GetWorkflowRoot();
                    }
                    else
                    { // Loose XAML case.
                        tempRootActivity = deserializedObject as Activity;
                    }

                    Fx.Assert(tempRootActivity != null, "Unexpected workflow xaml file");
                }

                mapping = new Dictionary<object, SourceLocation>();
                if (tempRootActivity != null)
                {
                    CollectMapping(rootActivity, tempRootActivity, mapping, sourcePath, checksum);
                }
            }
            catch (Exception)
            {
                // Only eat the exception if we were running in partial trust.
                if (!PartialTrustHelpers.AppDomainFullyTrusted)
                {
                    // Eat the exception and return an empty dictionary.
                    return new Dictionary<object, SourceLocation>();
                }
                else
                {
                    throw;
                }
            }

            return mapping;
        }

        public static Dictionary<object, SourceLocation> GetSourceLocations(Activity rootActivity, WorkflowSymbol symbol)
        {
            return GetSourceLocations(rootActivity, symbol, translateInternalActivityToOrigin: true);
        }

        // For most of the time, we need source location for object that appear on XAML.
        // During debugging, however, we must not transform the internal activity to their origin to make sure it stop when the internal activity is about the execute
        // Therefore, in debugger scenario, translateInternalActivityToOrigin will be set to false.
        internal static Dictionary<object, SourceLocation> GetSourceLocations(Activity rootActivity, WorkflowSymbol symbol, bool translateInternalActivityToOrigin)
        {
            Activity workflowRoot = rootActivity.RootActivity ?? rootActivity;
            if (!workflowRoot.IsMetadataFullyCached)
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(workflowRoot, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.ValidationOptions, null, ref validationErrors);
            }

            Dictionary<object, SourceLocation> newMapping = new Dictionary<object, SourceLocation>();

            // Make sure the qid we are using to TryGetElementFromRoot
            // are shifted appropriately such that the first digit that QID is
            // the same as the last digit of the rootActivity.QualifiedId.

            int[] rootIdArray = rootActivity.QualifiedId.AsIDArray();
            int idOffset = rootIdArray[rootIdArray.Length - 1] - 1;

            foreach (ActivitySymbol actSym in symbol.Symbols)
            {
                QualifiedId qid = new QualifiedId(actSym.QualifiedId);
                if (idOffset != 0)
                {
                    int[] idArray = qid.AsIDArray();
                    idArray[0] += idOffset;
                    qid = new QualifiedId(idArray);
                }
                Activity activity;
                if (QualifiedId.TryGetElementFromRoot(rootActivity, qid, out activity))
                {
                    object origin = activity;
                    if (translateInternalActivityToOrigin && activity.Origin != null)
                    {
                        origin = activity.Origin;
                    }

                    newMapping.Add(origin,
                        new SourceLocation(symbol.FileName, symbol.GetChecksum(), actSym.StartLine, actSym.StartColumn, actSym.EndLine, actSym.EndColumn));
                }
            }
            return newMapping;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - We are deserializing XAML from a file. The file may have been read under and Assert for FileIOPermission. The data hould be validated and not cached.")]
        internal static object Deserialize(byte[] buffer, Assembly localAssembly)
        {
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (TextReader streamReader = new StreamReader(memoryStream))
                {
                    using (XamlDebuggerXmlReader xamlDebuggerReader = new XamlDebuggerXmlReader(streamReader, new XamlSchemaContext(), localAssembly))
                    {
                        xamlDebuggerReader.SourceLocationFound += XamlDebuggerXmlReader.SetSourceLocation;

                        using (XamlReader activityBuilderReader = ActivityXamlServices.CreateBuilderReader(xamlDebuggerReader))
                        {
                            return XamlServices.Load(activityBuilderReader);
                        }
                    }
                }
            }
        }

        public static void CollectMapping(Activity rootActivity1, Activity rootActivity2, Dictionary<object, SourceLocation> mapping, string path)
        {
            CollectMapping(rootActivity1, rootActivity2, mapping, path, null, requirePrepareForRuntime: true);
        }        

        // Collect mapping for activity1 and its descendants to their corresponding source location.
        // activity2 is the shadow of activity1 but with SourceLocation information.
        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - We are dealing with activity and SourceLocation information that came from the user, possibly under an Assert for FileIOPermission. The data hould be validated and not cached.")]
        static void CollectMapping(Activity rootActivity1, Activity rootActivity2, Dictionary<object, SourceLocation> mapping, string path, byte[] checksum, bool requirePrepareForRuntime)
        {
            // For x:Class, the rootActivity here may not be the real root, but it's the first child of the x:Class activity.
            Activity realRoot1 = (rootActivity1.RootActivity != null) ? rootActivity1.RootActivity : rootActivity1;
            if ((requirePrepareForRuntime && !realRoot1.IsRuntimeReady) || (!requirePrepareForRuntime && !realRoot1.IsMetadataFullyCached))
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(realRoot1, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.ValidationOptions, null, ref validationErrors);
            }

            // Similarly for rootActivity2.
            Activity realRoot2 = (rootActivity2.RootActivity != null) ? rootActivity2.RootActivity : rootActivity2;
            if (rootActivity1 != rootActivity2 && (requirePrepareForRuntime && !realRoot2.IsRuntimeReady) || (!requirePrepareForRuntime && !realRoot2.IsMetadataFullyCached))
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(realRoot2, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.ValidationOptions, null, ref validationErrors);
            }

            Queue<KeyValuePair<Activity, Activity>> pairsRemaining = new Queue<KeyValuePair<Activity, Activity>>();

            pairsRemaining.Enqueue(new KeyValuePair<Activity, Activity>(rootActivity1, rootActivity2));
            KeyValuePair<Activity, Activity> currentPair;
            HashSet<Activity> visited = new HashSet<Activity>();

            while (pairsRemaining.Count > 0)
            {
                currentPair = pairsRemaining.Dequeue();
                Activity activity1 = currentPair.Key;
                Activity activity2 = currentPair.Value;

                visited.Add(activity1);

                SourceLocation sourceLocation;
                if (TryGetSourceLocation(activity2, path, checksum, out sourceLocation))
                {
                    mapping.Add(activity1, sourceLocation);
                }
                else if (!((activity2 is IExpressionContainer) || (activity2 is IValueSerializableExpression))) // Expression is known not to have source location.
                {
                    //Some activities may not have corresponding Xaml node, e.g. ActivityFaultedOutput.                    
                    Trace.WriteLine("WorkflowDebugger: Does not have corresponding Xaml node for: " + activity2.DisplayName + "\n");
                }

                // This to avoid comparing any value expression with DesignTimeValueExpression (in designer case).
                if (!((activity1 is IExpressionContainer) || (activity2 is IExpressionContainer) ||
                      (activity1 is IValueSerializableExpression) || (activity2 is IValueSerializableExpression)))
                {
                    IEnumerator<Activity> enumerator1 = WorkflowInspectionServices.GetActivities(activity1).GetEnumerator();
                    IEnumerator<Activity> enumerator2 = WorkflowInspectionServices.GetActivities(activity2).GetEnumerator();
                    bool hasNextItem1 = enumerator1.MoveNext();
                    bool hasNextItem2 = enumerator2.MoveNext();
                    while (hasNextItem1 && hasNextItem2)
                    {
                        if (!visited.Contains(enumerator1.Current))  // avoid adding the same activity (e.g. some default implementation).
                        {
                            if (enumerator1.Current.GetType() != enumerator2.Current.GetType())
                            {
                                // Give debugger log instead of just asserting; to help user find out mismatch problem.
                                Trace.WriteLine(
                                    "Unmatched type: " + enumerator1.Current.GetType().FullName +
                                    " vs " + enumerator2.Current.GetType().FullName + "\n");
                            }
                            pairsRemaining.Enqueue(new KeyValuePair<Activity, Activity>(enumerator1.Current, enumerator2.Current));
                        }
                        hasNextItem1 = enumerator1.MoveNext();
                        hasNextItem2 = enumerator2.MoveNext();
                    }

                    // If enumerators do not finish at the same time, then they have unmatched number of activities.
                    // Give debugger log instead of just asserting; to help user find out mismatch problem.
                    if (hasNextItem1 || hasNextItem2)
                    {
                        Trace.WriteLine("Unmatched number of children\n");
                    }
                }
            }
        }

        static void CollectMapping(Activity rootActivity1, Activity rootActivity2, Dictionary<object, SourceLocation> mapping, string path, byte[] checksum)
        {
            CollectMapping(rootActivity1, rootActivity2, mapping, path, checksum, requirePrepareForRuntime: true);
        }
        // Get SourceLocation for object deserialized with XamlDebuggerXmlReader in deserializer stack.
        static bool TryGetSourceLocation(object obj, string path, byte[] checksum, out SourceLocation sourceLocation)
        {
            sourceLocation = null;
            int startLine, startColumn, endLine, endColumn;

            if (AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.StartLineName, out startLine) &&
                AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.StartColumnName, out startColumn) &&
                AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.EndLineName, out endLine) &&
                AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.EndColumnName, out endColumn) &&
                SourceLocation.IsValidRange(startLine, startColumn, endLine, endColumn))
            {
                sourceLocation = new SourceLocation(path, checksum, startLine, startColumn, endLine, endColumn);
                return true;
            }
            return false;
        }

        public static ICollection<ActivitySymbol> GetSymbols(Activity rootActivity, Dictionary<object, SourceLocation> sourceLocations)
        {
            List<ActivitySymbol> symbols = new List<ActivitySymbol>();
            Activity realRoot = (rootActivity.RootActivity != null) ? rootActivity.RootActivity : rootActivity;
            if (!realRoot.IsMetadataFullyCached)
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(realRoot, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.ValidationOptions, null, ref validationErrors);
            }
            Queue<Activity> activitiesRemaining = new Queue<Activity>();
            activitiesRemaining.Enqueue(realRoot);
            HashSet<Activity> visited = new HashSet<Activity>();
            while (activitiesRemaining.Count > 0)
            {
                Activity currentActivity = activitiesRemaining.Dequeue();
                SourceLocation sourceLocation;
                object origin = currentActivity.Origin == null ? currentActivity : currentActivity.Origin;
                if (!visited.Contains(currentActivity) && sourceLocations.TryGetValue(origin, out sourceLocation))
                {
                    symbols.Add(new ActivitySymbol
                    {
                        QualifiedId = currentActivity.QualifiedId.AsByteArray(),
                        StartLine = sourceLocation.StartLine,
                        StartColumn = sourceLocation.StartColumn,
                        EndLine = sourceLocation.EndLine,
                        EndColumn = sourceLocation.EndColumn
                    });
                }
                visited.Add(currentActivity);
                foreach (Activity childActivity in WorkflowInspectionServices.GetActivities(currentActivity))
                {
                    activitiesRemaining.Enqueue(childActivity);
                }
            }
            return symbols;
        }
    }
}
