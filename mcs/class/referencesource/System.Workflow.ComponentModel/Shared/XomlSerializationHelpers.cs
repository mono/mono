namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.CodeDom.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Text;
    using System.Diagnostics.CodeAnalysis;

    internal static class StandardXomlKeys
    {
        internal const string WorkflowXmlNs = "http://schemas.microsoft.com/winfx/2006/xaml/workflow";
        internal const string WorkflowPrefix = "wf";
        internal const string CLRNamespaceQualifier = "clr-namespace:";
        internal const string AssemblyNameQualifier = "Assembly=";
        internal const string GlobalNamespace = "{Global}";
        internal const string MarkupExtensionSuffix = "Extension";

        internal const string Definitions_XmlNs = "http://schemas.microsoft.com/winfx/2006/xaml";
        internal const string Definitions_XmlNs_Prefix = "x";
        internal const string Definitions_Class_LocalName = "Class";
        internal const string Definitions_Code_LocalName = "Code";
        internal const string Definitions_ActivityVisible_LocalName = "Visible";
        internal const string Definitions_ActivityEditable_LocalName = "Editable";
        internal const string Definitions_Type_LocalName = "Type";
    }

    internal static class WorkflowMarkupSerializationHelpers
    {
        internal static string[] standardNamespaces = {
                "System", 
                "System.Collections", 
                "System.ComponentModel",
                "System.ComponentModel.Design",
                "System.Collections.Generic", 
                "System.Workflow.ComponentModel",
                "System.Workflow.Runtime", 
                "System.Workflow.Activities"
        };

        public static Activity LoadXomlDocument(WorkflowMarkupSerializationManager xomlSerializationManager, XmlReader textReader, string fileName)
        {
            if (xomlSerializationManager == null)
                throw new ArgumentNullException("xomlSerializationManager");

            Activity rootActivity = null;
            try
            {
                xomlSerializationManager.Context.Push(fileName);
                rootActivity = new WorkflowMarkupSerializer().Deserialize(xomlSerializationManager, textReader) as Activity;
            }
            finally
            {
                xomlSerializationManager.Context.Pop();
            }

            return rootActivity;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void ProcessDefTag(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, Activity activity, bool newSegment, string fileName)
        {
            System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("System.Workflow.ComponentModel.StringResources", typeof(System.Workflow.ComponentModel.ActivityBind).Assembly);
            if (reader.NodeType == XmlNodeType.Attribute)
            {
                switch (reader.LocalName)
                {
                    case StandardXomlKeys.Definitions_Class_LocalName:
                        activity.SetValue(WorkflowMarkupSerializer.XClassProperty, reader.Value);
                        break;
                    default:
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(string.Format(CultureInfo.CurrentCulture, resourceManager.GetString("UnknownDefinitionTag"), new object[] { StandardXomlKeys.Definitions_XmlNs_Prefix, reader.LocalName, StandardXomlKeys.Definitions_XmlNs }), (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1, (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1));
                        break;
                }
                return;
            }

            bool exitLoop = false;
            bool isEmptyElement = reader.IsEmptyElement;
            int initialDepth = reader.Depth;
            do
            {
                XmlNodeType currNodeType = reader.NodeType;
                switch (currNodeType)
                {
                    case XmlNodeType.Element:
                        {
                            /*
                            if (!reader.LocalName.Equals(localName))
                            {
                                serializationManager.ReportError(new WorkflowMarkupSerializationException(string.Format(resourceManager.GetString("DefnTagsCannotBeNested"), "def", localName, reader.LocalName), reader.LineNumber, reader.LinePosition));
                                return;
                            }
                            */

                            switch (reader.LocalName)
                            {
                                case StandardXomlKeys.Definitions_Code_LocalName:
                                    break;

                                case "Constructor":
                                default:
                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(string.Format(CultureInfo.CurrentCulture, resourceManager.GetString("UnknownDefinitionTag"), StandardXomlKeys.Definitions_XmlNs_Prefix, reader.LocalName, StandardXomlKeys.Definitions_XmlNs), (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1, (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1));
                                    return;
                            }

                            // if an empty element do a Reader then exit
                            if (isEmptyElement)
                                exitLoop = true;
                            break;
                        }

                    case XmlNodeType.EndElement:
                        {
                            //reader.Read();
                            if (reader.Depth == initialDepth)
                                exitLoop = true;
                            break;
                        }

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        {
                            // 


                            int lineNumber = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1;
                            int linePosition = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1;
                            CodeSnippetTypeMember codeSegment = new CodeSnippetTypeMember(reader.Value);
                            codeSegment.LinePragma = new CodeLinePragma(fileName, Math.Max(lineNumber - 1, 1));
                            codeSegment.UserData[UserDataKeys.CodeSegment_New] = newSegment;
                            codeSegment.UserData[UserDataKeys.CodeSegment_ColumnNumber] = linePosition + reader.Name.Length - 1;

                            CodeTypeMemberCollection codeSegments = activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
                            if (codeSegments == null)
                            {
                                codeSegments = new CodeTypeMemberCollection();
                                activity.SetValue(WorkflowMarkupSerializer.XCodeProperty, codeSegments);
                            }
                            codeSegments.Add(codeSegment);
                            //}
                            /*else
                            {
                                serializationManager.ReportError( new WorkflowMarkupSerializationException(
                                                                            string.Format(resourceManager.GetString("IllegalCDataTextScoping"), 
                                                                                        "def", 
                                                                                        reader.LocalName,
                                                                                        (currNodeType == XmlNodeType.CDATA ? resourceManager.GetString("CDATASection") : resourceManager.GetString("TextSection"))), 
                                                                            reader.LineNumber, 
                                                                            reader.LinePosition)
                                                                );
                            }
                            */
                            break;
                        }
                }
            }
            while (!exitLoop && reader.Read());
        }

        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5350:MD5CannotBeUsed", 
            Justification = "Design has been approved.  We are not using MD5 for any security or cryptography purposes but rather as a hash.")]
        internal static CodeNamespaceCollection GenerateCodeFromXomlDocument(Activity rootActivity, string filePath, string rootNamespace, SupportedLanguages language, IServiceProvider serviceProvider)
        {
            CodeNamespaceCollection codeNamespaces = new CodeNamespaceCollection();
            CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(language);

            // generate activity class
            string activityFullClassName = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
            CodeTypeDeclaration activityTypeDeclaration = null;
            if (codeDomProvider != null && !string.IsNullOrEmpty(activityFullClassName))
            {
                // get class and namespace names
                string activityNamespaceName, activityClassName;
                Helpers.GetNamespaceAndClassName(activityFullClassName, out activityNamespaceName, out activityClassName);
                if (codeDomProvider.IsValidIdentifier(activityClassName))
                {
                    DesignerSerializationManager designerSerializationManager = new DesignerSerializationManager(serviceProvider);
                    using (designerSerializationManager.CreateSession())
                    {
                        ActivityCodeDomSerializationManager codeDomSerializationManager = new ActivityCodeDomSerializationManager(designerSerializationManager);
                        TypeCodeDomSerializer typeCodeDomSerializer = codeDomSerializationManager.GetSerializer(rootActivity.GetType(), typeof(TypeCodeDomSerializer)) as TypeCodeDomSerializer;

                        // get all activities
                        bool generateCode = true;

                        ArrayList allActivities = new ArrayList();
                        allActivities.Add(rootActivity);
                        if (rootActivity is CompositeActivity)
                        {
                            foreach (Activity activity in Helpers.GetNestedActivities((CompositeActivity)rootActivity))
                            {
                                if (Helpers.IsActivityLocked(activity))
                                    continue;
                                if (codeDomProvider.IsValidIdentifier(codeDomSerializationManager.GetName(activity)))
                                {
                                    // WinOE Bug 14561.  This is to fix a performance problem.  When an activity is added to the activity
                                    // tree at the runtime, it's much faster if the ID of the activity is already set.  The code that
                                    // the CodeDomSerializer generates will add the activity first before it sets the ID for the child
                                    // activity.  We can change that order by always serializing the children first.  Therefore, we 
                                    // construct a list where we guarantee that the child will be serialized before its parent.
                                    allActivities.Insert(0, activity);
                                }
                                else
                                {
                                    generateCode = false;
                                    break;
                                }
                            }
                        }

                        if (generateCode)
                        {
                            // Work around!! TypeCodeDomSerializer checks that root component has a site or not, otherwise it
                            // does not serialize it look at ComponentTypeCodeDomSerializer.cs
                            DummySite dummySite = new DummySite();
                            foreach (Activity nestedActivity in allActivities)
                                ((IComponent)nestedActivity).Site = dummySite;
                            ((IComponent)rootActivity).Site = dummySite;

                            // create activity partial class
                            activityTypeDeclaration = typeCodeDomSerializer.Serialize(codeDomSerializationManager, rootActivity, allActivities);
                            activityTypeDeclaration.IsPartial = true;

                            // add checksum attribute
                            if (filePath != null && filePath.Length > 0)
                            {
                                MD5 md5 = new MD5CryptoServiceProvider();
                                byte[] checksumBytes = null;
                                using (StreamReader streamReader = new StreamReader(filePath))
                                    checksumBytes = md5.ComputeHash(streamReader.BaseStream);
                                string checksum = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}", new object[] { checksumBytes[0].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[1].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[2].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[3].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[4].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[5].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[6].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[7].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[8].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[9].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[10].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[11].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[12].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[13].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[14].ToString("X2", CultureInfo.InvariantCulture), checksumBytes[15].ToString("X2", CultureInfo.InvariantCulture) });
                                CodeAttributeDeclaration xomlSourceAttribute = new CodeAttributeDeclaration(typeof(WorkflowMarkupSourceAttribute).FullName);
                                xomlSourceAttribute.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(filePath)));
                                xomlSourceAttribute.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(checksum)));
                                activityTypeDeclaration.CustomAttributes.Add(xomlSourceAttribute);
                            }

                            // create a new namespace and add activity class into that
                            CodeNamespace activityCodeNamespace = new CodeNamespace(activityNamespaceName);
                            activityCodeNamespace.Types.Add(activityTypeDeclaration);
                            codeNamespaces.Add(activityCodeNamespace);
                        }
                    }
                }
            }

            // generate code for x:Code
            if (activityTypeDeclaration != null)
            {
                Queue activitiesQueue = new Queue(new object[] { rootActivity });
                while (activitiesQueue.Count > 0)
                {
                    Activity activity = (Activity)activitiesQueue.Dequeue();
                    if (Helpers.IsActivityLocked(activity))
                        continue;

                    Queue childActivities = new Queue(new object[] { activity });
                    while (childActivities.Count > 0)
                    {
                        Activity childActivity = (Activity)childActivities.Dequeue();
                        if (childActivity is CompositeActivity)
                        {
                            foreach (Activity nestedChildActivity in ((CompositeActivity)childActivity).Activities)
                            {
                                childActivities.Enqueue(nestedChildActivity);
                            }
                        }

                        // generate x:Code
                        CodeTypeMemberCollection codeSegments = childActivity.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
                        if (codeSegments != null)
                        {
                            foreach (CodeSnippetTypeMember codeSegmentMember in codeSegments)
                                activityTypeDeclaration.Members.Add(codeSegmentMember);
                        }
                    }
                }

                if (language == SupportedLanguages.CSharp)
                    activityTypeDeclaration.LinePragma = new CodeLinePragma((string)rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int)rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));

                //Now make sure we that we also emit line pragma around the constructor
                CodeConstructor constructor = null;
                CodeMemberMethod method = null;
                foreach (CodeTypeMember typeMember in activityTypeDeclaration.Members)
                {
                    if (constructor == null && typeMember is CodeConstructor)
                        constructor = typeMember as CodeConstructor;

                    if (method == null && typeMember is CodeMemberMethod && typeMember.Name.Equals("InitializeComponent", StringComparison.Ordinal))
                        method = typeMember as CodeMemberMethod;

                    if (constructor != null && method != null)
                        break;
                }

                if (constructor != null)
                    constructor.LinePragma = new CodeLinePragma((string)rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int)rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));

                if (method != null && language == SupportedLanguages.CSharp)
                    method.LinePragma = new CodeLinePragma((string)rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int)rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
            }

            // generate mappings
            List<String> clrNamespaces = rootActivity.GetValue(WorkflowMarkupSerializer.ClrNamespacesProperty) as List<String>;
            if (clrNamespaces != null)
            {
                // foreach namespace add these mappings
                foreach (CodeNamespace codeNamespace in codeNamespaces)
                {
                    foreach (string clrNamespace in clrNamespaces)
                    {
                        if (!String.IsNullOrEmpty(clrNamespace))
                        {
                            CodeNamespaceImport codeNamespaceImport = new CodeNamespaceImport(clrNamespace);
                            codeNamespaceImport.LinePragma = new CodeLinePragma((string)rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int)rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                            codeNamespace.Imports.Add(codeNamespaceImport);
                        }
                    }
                }
            }
            // return namespaces
            return codeNamespaces;
        }

        internal static void FixStandardNamespacesAndRootNamespace(CodeNamespaceCollection codeNamespaces, string rootNS, SupportedLanguages language)
        {
            // add the standard imports to all the namespaces.
            if (language == SupportedLanguages.VB)
            {
                foreach (CodeNamespace codeNamespace in codeNamespaces)
                {
                    if (codeNamespace.Name == rootNS)
                    {
                        codeNamespace.Name = string.Empty;
                        codeNamespace.UserData.Add("TruncatedNamespace", null);
                    }
                    else if (codeNamespace.Name.StartsWith(rootNS + ".", StringComparison.Ordinal))
                    {
                        codeNamespace.Name = codeNamespace.Name.Substring(rootNS.Length + 1);
                        codeNamespace.UserData.Add("TruncatedNamespace", null);
                    }
                }
            }

            foreach (CodeNamespace codeNamespace in codeNamespaces)
            {
                Hashtable definedNamespaces = new Hashtable();
                foreach (CodeNamespaceImport codeNamespaceImport in codeNamespace.Imports)
                    definedNamespaces.Add(codeNamespaceImport.Namespace, codeNamespaceImport);

                foreach (string standardNS in standardNamespaces)
                    if (!definedNamespaces.Contains(standardNS))//add only new imports
                        codeNamespace.Imports.Add(new CodeNamespaceImport(standardNS));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void ReapplyRootNamespace(CodeNamespaceCollection codeNamespaces, string rootNS, SupportedLanguages language)
        {
            if (language == SupportedLanguages.VB)
            {
                foreach (CodeNamespace codeNamespace in codeNamespaces)
                {
                    if (codeNamespace.UserData.Contains("TruncatedNamespace"))
                    {
                        if (codeNamespace.Name == null || codeNamespace.Name.Length == 0)
                            codeNamespace.Name = rootNS;
                        else if (codeNamespace.Name.StartsWith(rootNS + ".", StringComparison.Ordinal))
                            codeNamespace.Name = rootNS + "." + codeNamespace.Name;

                        codeNamespace.UserData.Remove("TruncatedNamespace");
                    }
                }
            }
        }

        #region Event Support
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string GetEventHandlerName(object owner, string eventName)
        {
            string handler = null;
            DependencyObject dependencyObject = owner as DependencyObject;
            if (!string.IsNullOrEmpty(eventName) && owner != null && dependencyObject != null)
            {
                if (dependencyObject.GetValue(WorkflowMarkupSerializer.EventsProperty) != null)
                {
                    Hashtable dynamicEvents = dependencyObject.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                    if (dynamicEvents != null && dynamicEvents.ContainsKey(eventName))
                        handler = dynamicEvents[eventName] as string;
                }
            }
            return handler;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void SetEventHandlerName(object owner, string eventName, string value)
        {
            DependencyObject dependencyObject = owner as DependencyObject;
            if (!string.IsNullOrEmpty(eventName) && owner != null && dependencyObject != null)
            {
                if (dependencyObject.GetValue(WorkflowMarkupSerializer.EventsProperty) == null)
                    dependencyObject.SetValue(WorkflowMarkupSerializer.EventsProperty, new Hashtable());

                Hashtable dynamicEvents = dependencyObject.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                dynamicEvents[eventName] = value;
            }
        }
        #endregion

        private class DummySite : ISite
        {
            public IComponent Component { get { return null; } }
            public IContainer Container { get { return null; } }
            public bool DesignMode { get { return true; } }
            public string Name { get { return string.Empty; } set { } }
            public object GetService(Type type) { return null; }
        }
    }

}
