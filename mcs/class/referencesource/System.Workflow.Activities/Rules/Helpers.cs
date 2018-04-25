// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    internal static class ConditionHelper
    {
        internal static Type GetContextType(ITypeProvider typeProvider, Activity currentActivity)
        {
            Type contextType = null;
            string className = String.Empty;
            Activity rootActivity = null;

            if (Helpers.IsActivityLocked(currentActivity))
            {
                rootActivity = Helpers.GetDeclaringActivity(currentActivity);
            }
            else
            {
                rootActivity = Helpers.GetRootActivity(currentActivity);
            }

            if (rootActivity != null)
            {
                className = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                if (!String.IsNullOrEmpty(className))
                    contextType = typeProvider.GetType(className, false);

                if (contextType == null)
                    contextType = typeProvider.GetType(rootActivity.GetType().FullName);

                // If all else fails (likely, we don't have a type provider), it's the root activity type.
                if (contextType == null)
                    contextType = rootActivity.GetType();
            }

            return contextType;
        }

        /// <summary>
        /// Is type a nullable value type (e.g. double?)?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsNullableValueType(Type type)
        {
            return ((type.IsValueType) && (type.IsGenericType) && (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        /// <summary>
        /// Is type a standard value type (i.e. not Nullable)?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsNonNullableValueType(Type type)
        {
            return ((type.IsValueType) && (!type.IsGenericType) && (type != typeof(string)));
        }

        internal static object CloneObject(object original)
        {
            if (original == null)
                return null;

            if (original.GetType().IsValueType)
                return original;

            ICloneable cloneable = original as ICloneable;
            if (cloneable != null)
                return cloneable.Clone();

            string message = string.Format(CultureInfo.CurrentCulture, Messages.NotCloneable, original.GetType().FullName);
            throw new NotSupportedException(message);
        }

        internal static void CloneUserData(CodeObject original, CodeObject result)
        {
            // clone UserData, if possible
            foreach (object key in original.UserData.Keys)
            {
                object newKey = CloneObject(key);
                object newValue = CloneObject(original.UserData[key]);
                result.UserData.Add(newKey, newValue);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static RuleDefinitions Load_Rules_DT(IServiceProvider serviceProvider, DependencyObject activity)
        {
            RuleDefinitions rules = (RuleDefinitions)activity.GetValue(RuleDefinitions.RuleDefinitionsProperty);
            if (rules == null)
            {
                WorkflowDesignerLoader loader = (WorkflowDesignerLoader)serviceProvider.GetService(typeof(WorkflowDesignerLoader));
                if (loader != null)
                {
                    string rulesFileName = string.Empty;
                    if (!string.IsNullOrEmpty(loader.FileName))
                        rulesFileName = Path.Combine(Path.GetDirectoryName(loader.FileName), Path.GetFileNameWithoutExtension(loader.FileName));
                    rulesFileName += ".rules";

                    try
                    {
                        using (TextReader ruleFileReader = loader.GetFileReader(rulesFileName))
                        {
                            if (ruleFileReader == null)
                            {
                                rules = new RuleDefinitions();
                            }
                            else
                            {
                                using (XmlReader xmlReader = XmlReader.Create(ruleFileReader))
                                    rules = new WorkflowMarkupSerializer().Deserialize(xmlReader) as RuleDefinitions;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        rules = new RuleDefinitions();
                        // 
                    }
                }
                activity.SetValue(RuleDefinitions.RuleDefinitionsProperty, rules);
            }
            return rules;
        }

        internal static void Flush_Rules_DT(IServiceProvider serviceProvider, Activity activity)
        {
            RuleDefinitions rules = (RuleDefinitions)activity.GetValue(RuleDefinitions.RuleDefinitionsProperty);
            if (rules != null)
            {
                WorkflowDesignerLoader loader = (WorkflowDesignerLoader)serviceProvider.GetService(typeof(WorkflowDesignerLoader));
                if (loader != null)
                {
                    string rulesFileName = string.Empty;
                    if (!string.IsNullOrEmpty(loader.FileName))
                        rulesFileName = Path.Combine(Path.GetDirectoryName(loader.FileName), Path.GetFileNameWithoutExtension(loader.FileName));
                    rulesFileName += ".rules";

                    using (TextWriter ruleFileWriter = loader.GetFileWriter(rulesFileName))
                    {
                        if (ruleFileWriter != null)
                        {
                            using (XmlWriter xmlWriter = Helpers.CreateXmlWriter(ruleFileWriter))
                            {
                                DesignerSerializationManager designerSerializationManager = new DesignerSerializationManager(serviceProvider);
                                using (designerSerializationManager.CreateSession())
                                {
                                    new WorkflowMarkupSerializer().Serialize(designerSerializationManager, xmlWriter, rules);
                                }
                            }

                        }
                    }
                }
            }
        }

        internal static RuleDefinitions Load_Rules_RT(Activity declaringActivity)
        {
            RuleDefinitions rules = declaringActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null)
            {
                rules = ConditionHelper.GetRuleDefinitionsFromManifest(declaringActivity.GetType());
                if (rules != null)
                    declaringActivity.SetValue(RuleDefinitions.RuleDefinitionsProperty, rules);
            }
            return rules;
        }

        // To improve performance, cache the RuleDefinitions deserialized from 
        // .rules resources keyed by the type of activity.
        static Hashtable cloneableOrNullRulesResources = new Hashtable();
        // It is unfortunate, however, that cloning might not always succeed, we will keep them here.
        static Hashtable uncloneableRulesResources = new Hashtable();

        internal static RuleDefinitions GetRuleDefinitionsFromManifest(Type workflowType)
        {
            if (workflowType == null)
                throw new ArgumentNullException("workflowType");

            RuleDefinitions rules = null;

            if (cloneableOrNullRulesResources.ContainsKey(workflowType))
            {
                rules = (RuleDefinitions)cloneableOrNullRulesResources[workflowType];
                if (rules != null)
                {
                    // This should always succeed, since it is coming out of the cloneable cache
                    rules = rules.Clone();
                }
            }
            else
            {
                string resourceName = workflowType.Name + ".rules";
                Stream stream = workflowType.Module.Assembly.GetManifestResourceStream(workflowType, resourceName);

                // Try just the .rules file name. This is needed for wfc.exe compilation scenarios.
                if (stream == null)
                    stream = workflowType.Module.Assembly.GetManifestResourceStream(resourceName);

                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(reader))
                            rules = new WorkflowMarkupSerializer().Deserialize(xmlReader) as RuleDefinitions;
                    }
                }
                // Don't know yet whether 'rules' is cloneable, give it a try
                if (!uncloneableRulesResources.ContainsKey(workflowType))
                {
                    try
                    {
                        RuleDefinitions originalRules = rules;
                        if (rules != null)
                        {
                            rules = rules.Clone();
                        }
                        lock (cloneableOrNullRulesResources)
                        {
                            cloneableOrNullRulesResources[workflowType] = originalRules;
                        }
                    }
                    catch (Exception)
                    {
                        lock (uncloneableRulesResources)
                        {
                            uncloneableRulesResources[workflowType] = null;
                        }
                    }
                }
            }
            return rules;
        }
    }

    internal static class EnumHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
        public static Type GetUnderlyingType(Type type)
        {
            Type underlyingType = typeof(int);
            if (type.GetType().FullName.Equals("System.Workflow.ComponentModel.Compiler.DesignTimeType", StringComparison.Ordinal))// designTimeType = type as System.Workflow.ComponentModel.Compiler.DesignTimeType;
            {
                //this is a design time type, need to get the enum type data out of it
                MethodInfo methodInfo = type.GetType().GetMethod("GetEnumType");
                Debug.Assert(methodInfo != null, "Missing GetEnumType method on the DesignTimeType!");
                if (methodInfo != null)
                {
                    Type result = methodInfo.Invoke(type, new object[0]) as Type;
                    underlyingType = (result != null) ? result : underlyingType;
                }
            }
            else
            {
                underlyingType = Enum.GetUnderlyingType(type);
            }

            return underlyingType;
        }
    }
}
