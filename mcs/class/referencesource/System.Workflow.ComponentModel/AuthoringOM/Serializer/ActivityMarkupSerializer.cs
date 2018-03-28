namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Reflection;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Xml;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ComponentModel.Design.Serialization;
    using System.Text;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections;
    using System.IO;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    #region Class ActivityMarkupSerializer
    [DefaultSerializationProvider(typeof(ActivityMarkupSerializationProvider))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityMarkupSerializer : WorkflowMarkupSerializer
    {
        private const int minusOne = -1;

        // some user data keys need to be visible to the user, see #15400 "We should convert the UserDataKeys.DynamicEvents to DependencyProperty."
        public static readonly DependencyProperty StartLineProperty = DependencyProperty.RegisterAttached("StartLine", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(minusOne, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty StartColumnProperty = DependencyProperty.RegisterAttached("StartColumn", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(minusOne, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty EndLineProperty = DependencyProperty.RegisterAttached("EndLine", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(minusOne, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty EndColumnProperty = DependencyProperty.RegisterAttached("EndColumn", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(minusOne, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String,System.Int32)", Justification = "This is not a security threat since it is called in design time (compile) scenarios")]
        protected override void OnBeforeSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            Activity activity = obj as Activity;
            if (activity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (writer == null)
            {
                //We should not throw an exception here as both of the above properties are internal and
                //our serializer makes sure that they are always set. Note that OnBeforeSerialize can be 
                //only called by WorkflowMarkupSerializer.
                Debug.Assert(false);
                return;
            }

            StringWriter stringWriter = serializationManager.WorkflowMarkupStack[typeof(StringWriter)] as StringWriter;
            if (stringWriter != null)
            {
                // we capture the start and end line of the activity getting serialized to xoml
                writer.Flush();
                string currentXoml = stringWriter.ToString();
                int startLine = 0;
                int currentIndex = 0;
                string newLine = stringWriter.NewLine;
                int newLineLength = newLine.Length;

                // Get to the starting line of this activity.
                while (true)
                {
                    int nextNewLineIndex = currentXoml.IndexOf(newLine, currentIndex, StringComparison.Ordinal);
                    if (nextNewLineIndex == -1)
                        break;

                    currentIndex = nextNewLineIndex + newLineLength;
                    startLine++;
                }

                // We always serialize an element start tag onto exactly 1 line.
                activity.SetValue(ActivityMarkupSerializer.StartLineProperty, startLine);
                activity.SetValue(ActivityMarkupSerializer.EndLineProperty, startLine);

                // Cache the index of the beginning of the line.
                activity.SetValue(ActivityMarkupSerializer.EndColumnProperty, currentIndex);
                activity.SetValue(ActivityMarkupSerializer.StartColumnProperty, (currentXoml.IndexOf('<', currentIndex) - currentIndex + 1));
            }

            // write x:Class attribute
            string className = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
            if (className != null)
                writer.WriteAttributeString(StandardXomlKeys.Definitions_XmlNs_Prefix, StandardXomlKeys.Definitions_Class_LocalName, StandardXomlKeys.Definitions_XmlNs, className);

        }

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader == null)
            {
                Debug.Assert(false);
                return null;
            }
            object instance = base.CreateInstance(serializationManager, type);
            if (instance is Activity && (serializationManager.Context[typeof(Activity)] == null && serializationManager.Context[typeof(WorkflowCompilerParameters)] != null))
                (instance as Activity).UserData[UserDataKeys.CustomActivity] = false;
            WorkflowMarkupSourceAttribute[] sourceAttrs = (WorkflowMarkupSourceAttribute[])type.GetCustomAttributes(typeof(WorkflowMarkupSourceAttribute), false);
            if (instance is CompositeActivity && sourceAttrs.Length > 0 && type.Assembly == serializationManager.LocalAssembly)
            {
                object instance2 = null;
                using (XmlReader reader2 = XmlReader.Create(sourceAttrs[0].FileName))
                    instance2 = Deserialize(serializationManager, reader2);
                ReplaceChildActivities(instance as CompositeActivity, instance2 as CompositeActivity);
            }

            if (instance is Activity)
            {
                int lineNumber = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1;
                int linePosition = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1;
                int startLine = lineNumber - 1;
                int startColumn = linePosition - 1;

                bool fAttributeExists = false;
                while (reader.MoveToNextAttribute())
                    fAttributeExists = true;

                int endLine = lineNumber - 1;
                int endColumn;

                if (fAttributeExists)
                {
                    reader.ReadAttributeValue();
                    endColumn = linePosition + reader.Value.Length;
                }
                else
                    endColumn = linePosition + reader.Name.Length - 1;

                reader.MoveToElement();
                System.Diagnostics.Debug.Assert(startLine + 1 == lineNumber && startColumn + 1 == linePosition, "Error getting (line, column)!");

                Activity activity = (Activity)instance;
                activity.SetValue(ActivityMarkupSerializer.StartLineProperty, startLine);
                activity.SetValue(ActivityMarkupSerializer.StartColumnProperty, startColumn);
                activity.SetValue(ActivityMarkupSerializer.EndLineProperty, endLine);
                activity.SetValue(ActivityMarkupSerializer.EndColumnProperty, endColumn);
            }

            return instance;
        }
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String,System.Int32)", Justification = "This is not a security threat since it is called in design time (compile) scenarios")]
        protected override void OnAfterSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            Activity activity = obj as Activity;
            if (activity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (writer == null)
            {
                Debug.Assert(false);
                return;
            }

            StringWriter stringWriter = serializationManager.WorkflowMarkupStack[typeof(StringWriter)] as StringWriter;
            if (stringWriter != null)
            {
                string currentXoml = stringWriter.ToString();
                int lineStartIndex = (int)activity.GetValue(ActivityMarkupSerializer.EndColumnProperty);

                int lastNewLine = currentXoml.IndexOf(stringWriter.NewLine, (int)lineStartIndex, StringComparison.Ordinal);
                if (lastNewLine == -1)
                    activity.SetValue(ActivityMarkupSerializer.EndColumnProperty, (currentXoml.Length - lineStartIndex - 1));
                else
                    activity.SetValue(ActivityMarkupSerializer.EndColumnProperty, lastNewLine - lineStartIndex);
            }

            CodeTypeMemberCollection codeSegments = activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
            if (codeSegments != null)
            {
                foreach (CodeSnippetTypeMember cs in codeSegments)
                {
                    if (cs.Text == null)
                        continue;

                    writer.WriteStartElement(StandardXomlKeys.Definitions_XmlNs_Prefix, StandardXomlKeys.Definitions_Code_LocalName, StandardXomlKeys.Definitions_XmlNs);
                    int depth = serializationManager.WriterDepth;

                    StringBuilder prettySegment = new StringBuilder();
                    if (cs.UserData.Contains(UserDataKeys.CodeSegment_New))
                    {
                        prettySegment.AppendLine();
                        string[] lines = cs.Text.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        foreach (string line in lines)
                        {
                            prettySegment.Append(writer.Settings.IndentChars);
                            prettySegment.Append(line);
                            prettySegment.AppendLine();
                        }
                        prettySegment.Append(writer.Settings.IndentChars);
                    }
                    else
                    {
                        prettySegment.Append(cs.Text);
                    }

                    writer.WriteCData(prettySegment.ToString());
                    writer.WriteEndElement();
                }
            }
        }

        internal static void ReplaceChildActivities(CompositeActivity instanceActivity, CompositeActivity xomlActivity)
        {
            ArrayList activities = new ArrayList();
            foreach (Activity activity1 in (xomlActivity as CompositeActivity).Activities)
            {
                activities.Add(activity1);
            }
            try
            {
                // Clear the activities 
                instanceActivity.CanModifyActivities = true;
                xomlActivity.CanModifyActivities = true;
                instanceActivity.Activities.Clear();
                xomlActivity.Activities.Clear();

                foreach (Activity activity in activities)
                {
                    instanceActivity.Activities.Add(activity);
                }
            }
            finally
            {
                instanceActivity.CanModifyActivities = false;
                xomlActivity.CanModifyActivities = false;
            }
            if (!instanceActivity.UserData.Contains(UserDataKeys.CustomActivity))
                instanceActivity.UserData[UserDataKeys.CustomActivity] = (instanceActivity.Activities.Count > 0);
        }
    }

    #endregion
}
