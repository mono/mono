namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Text;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Reflection;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System.Collections;
    using System.Globalization;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.Specialized;

    internal sealed class IdentifierCreationService : IIdentifierCreationService
    {
        private IServiceProvider serviceProvider = null;
        private WorkflowDesignerLoader loader = null;
        private CodeDomProvider provider = null;

        internal IdentifierCreationService(IServiceProvider serviceProvider, WorkflowDesignerLoader loader)
        {
            this.serviceProvider = serviceProvider;
            this.loader = loader;
        }

        internal CodeDomProvider Provider
        {
            get
            {
                if (this.provider == null)
                {
                    SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(this.serviceProvider);
                    if (language == SupportedLanguages.CSharp)
                        this.provider = CompilerHelpers.CreateCodeProviderInstance(typeof(CSharpCodeProvider));
                    else
                        this.provider = CompilerHelpers.CreateCodeProviderInstance(typeof(VBCodeProvider));
                }
                return this.provider;
            }
        }

        #region IIdentifierCreationService
        void IIdentifierCreationService.ValidateIdentifier(Activity activity, string identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (activity == null)
                throw new ArgumentNullException("activity");

            if (activity.Name.ToLowerInvariant().Equals(identifier.ToLowerInvariant()))
                return;

            if (this.Provider != null)
            {
                SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(this.serviceProvider);
                if (language == SupportedLanguages.CSharp && identifier.StartsWith("@", StringComparison.Ordinal) ||
                    language == SupportedLanguages.VB && identifier.StartsWith("[", StringComparison.Ordinal) && identifier.EndsWith("]", StringComparison.Ordinal) ||
                    !this.Provider.IsValidIdentifier(identifier))
                {
                    throw new Exception(SR.GetString(SR.Error_InvalidLanguageIdentifier, identifier));
                }
            }

            StringDictionary identifiers = new StringDictionary();
            CompositeActivity rootActivity = Helpers.GetRootActivity(activity) as CompositeActivity;
            if (rootActivity != null)
            {
                foreach (string existingIdentifier in Helpers.GetIdentifiersInCompositeActivity(rootActivity))
                    identifiers[existingIdentifier] = existingIdentifier;
            }

            Type customActivityType = GetRootActivityType(this.serviceProvider);
            if (customActivityType != null)
            {
                foreach (MemberInfo member in customActivityType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    Type memberType = null;
                    if (member is FieldInfo)
                        memberType = ((FieldInfo)member).FieldType;

                    if (memberType == null || !typeof(Activity).IsAssignableFrom(memberType))
                        identifiers[member.Name] = member.Name;
                }
            }

            if (identifiers.ContainsKey(identifier))
                throw new ArgumentException(SR.GetString(SR.DuplicateActivityIdentifier, identifier));
        }

        /// <summary>
        /// This method will ensure that the identifiers of the activities to be added to the parent activity
        /// are unique within the scope of the parent activity.  
        /// </summary>
        /// <param name="parentActivity">THis activity is the parent activity which the child activities are being added</param>
        /// <param name="childActivities"></param>
        void IIdentifierCreationService.EnsureUniqueIdentifiers(CompositeActivity parentActivity, ICollection childActivities)
        {
            if (parentActivity == null)
                throw new ArgumentNullException("parentActivity");
            if (childActivities == null)
                throw new ArgumentNullException("childActivities");

            ArrayList allActivities = new ArrayList();

            Queue activities = new Queue(childActivities);
            while (activities.Count > 0)
            {
                Activity activity = (Activity)activities.Dequeue();
                if (activity is CompositeActivity)
                {
                    foreach (Activity child in ((CompositeActivity)activity).Activities)
                        activities.Enqueue(child);
                }

                //If we are moving activities, we need not regenerate their identifiers
                if (((IComponent)activity).Site != null)
                    continue;

                //If the activity is built-in, we won't generate a new ID.
                if (IsPreBuiltActivity(activity))
                    continue;

                allActivities.Add(activity);
            }

            // get the root activity
            CompositeActivity rootActivity = Helpers.GetRootActivity(parentActivity) as CompositeActivity;
            StringDictionary identifiers = new StringDictionary(); // all the identifiers in the workflow

            Type customActivityType = GetRootActivityType(this.serviceProvider);

            if (rootActivity != null)
            {
                foreach (string identifier in Helpers.GetIdentifiersInCompositeActivity(rootActivity))
                    identifiers[identifier] = identifier;
            }

            if (customActivityType != null)
            {
                foreach (MemberInfo member in customActivityType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    Type memberType = null;
                    if (member is FieldInfo)
                        memberType = ((FieldInfo)member).FieldType;

                    if (memberType == null || !typeof(Activity).IsAssignableFrom(memberType))
                        identifiers[member.Name] = member.Name;
                }
            }

            // now loop until we find a identifier that hasn't been used
            foreach (Activity activity in allActivities)
            {
                int index = 0;
                string baseIdentifier = Helpers.GetBaseIdentifier(activity);
                string finalIdentifier = null;

                if (string.IsNullOrEmpty(activity.Name) || string.Equals(activity.Name, activity.GetType().Name, StringComparison.Ordinal))
                    finalIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { baseIdentifier, ++index });
                else
                    finalIdentifier = activity.Name;

                while (identifiers.ContainsKey(finalIdentifier))
                {
                    finalIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { baseIdentifier, ++index });
                    if (this.Provider != null)
                        finalIdentifier = this.Provider.CreateValidIdentifier(finalIdentifier);
                }

                // add new identifier to collection 
                identifiers[finalIdentifier] = finalIdentifier;
                activity.Name = finalIdentifier;
            }
        }
        #endregion

        private Type GetRootActivityType(IServiceProvider serviceProvider)
        {
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            string className = host.RootComponentClassName;
            if (string.IsNullOrEmpty(className))
                return null;

            ITypeProvider typeProvider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            return typeProvider.GetType(className, false);
        }

        private static bool IsPreBuiltActivity(Activity activity)
        {
            CompositeActivity parent = activity.Parent;
            while (parent != null)
            {
                // Any custom activity found, then locked
                if (Helpers.IsCustomActivity(parent))
                    return true;

                parent = parent.Parent;
            }

            return false;
        }
    }
}
