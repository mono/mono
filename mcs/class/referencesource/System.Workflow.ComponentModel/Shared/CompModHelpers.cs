// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in sync.  Any change made here must also
 * be made to WF\Activities\Common\CompModHelpers.cs
*********************************************************************/
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Serialization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Text;
    using System.Reflection;
    using System.Xml;
    using System.Globalization;
    using Microsoft.Win32;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;

    #region Class Helpers
    internal static class Helpers
    {
        private static readonly string VSExtensionProductRegistrySubKey = "Visual Studio Ext for Windows Workflow";
        // 

        internal static readonly string ProductRootRegKey = @"SOFTWARE\Microsoft\Net Framework Setup\NDP\v4.0\Setup\Windows Workflow Foundation";
        internal static readonly string ProductInstallDirectory = GetInstallDirectory(false);
        internal static readonly string ProductSDKInstallDirectory = GetInstallDirectory(true);
        internal static readonly string TypeProviderAssemblyRegValueName = "References";

        private static readonly string ProductRootRegKey30 = @"SOFTWARE\Microsoft\Net Framework Setup\NDP\v3.0\Setup\Windows Workflow Foundation";
        internal static readonly string ProductInstallDirectory30 = GetInstallDirectory30();

        private const string ProductCode = "{B644FB52-BB3D-4C43-80EC-57644210536A}";
        private const string ProductSDKCode = "{C8A7718A-FF6D-4DDC-AE36-BBF968D6799B}";
        private const string INSTALLPROPERTY_INSTALLLOCATION = "InstallLocation";

        internal const int FILENAME_MAX = 260; //"stdio.h"

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "LastIndexOf(\"\\\") not a security issue.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string PerUserRegistryKey
        {
            get
            {
                string keyPath = String.Empty;
                using (RegistryKey userRegistryKey = Application.UserAppDataRegistry)
                {
                    keyPath = userRegistryKey.ToString().Substring(Registry.CurrentUser.ToString().Length + 1);
                    keyPath = keyPath.Substring(0, keyPath.LastIndexOf("\\"));
                    keyPath += "\\" + VSExtensionProductRegistrySubKey;
                }
                return keyPath;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static string TypeProviderRegistryKeyPath
        {
            get
            {
                return PerUserRegistryKey + "\\TypeProvider";
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool IsFileNameValid(string fileName)
        {
            int length = Path.GetInvalidPathChars().GetLength(0) + 5;
            char[] invalidChars = new char[length];
            Path.GetInvalidPathChars().CopyTo(invalidChars, 0);
            invalidChars[length - 5] = ':';
            invalidChars[length - 4] = '?';
            invalidChars[length - 3] = '*';
            invalidChars[length - 2] = '/';
            invalidChars[length - 1] = '\\';

            return (fileName != null &&
                    fileName.Length != 0 &&
                    fileName.Length <= FILENAME_MAX &&
                    fileName.IndexOfAny(invalidChars) == -1);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool AreAllActivities(ICollection c)
        {
            if (c == null)
                throw new ArgumentNullException("c");

            foreach (object obj in c)
            {
                if (!(obj is Activity))
                    return false;
            }
            return true;
        }

        // this will return IDictionary, whose keys are parent and value is arraylist of child activities
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static IDictionary PairUpCommonParentActivities(ICollection activities)
        {
            if (activities == null)
                throw new ArgumentNullException("activities");

            Hashtable commonParentActivities = new Hashtable();
            foreach (Activity activity in activities)
            {
                if (activity.Parent != null)
                {
                    ArrayList childActivities = (ArrayList)commonParentActivities[activity.Parent];
                    if (childActivities == null)
                    {
                        childActivities = new ArrayList();
                        commonParentActivities.Add(activity.Parent, childActivities);
                    }
                    childActivities.Add(activity);
                }
            }
            return commonParentActivities;
        }

        // this will remove any activity from the collection whose parent is already there in the collection
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity[] GetTopLevelActivities(ICollection activities)
        {
            if (activities == null)
                throw new ArgumentNullException("activities");

            List<Activity> filteredActivities = new List<Activity>();
            foreach (object obj in activities)
            {
                Activity activity = obj as Activity;
                if (activity != null)
                {
                    bool foundParent = false;
                    Activity parentActivity = activity.Parent;
                    while (parentActivity != null && !foundParent)
                    {
                        foreach (object obj2 in activities)
                        {
                            if (obj2 == parentActivity)
                            {
                                foundParent = true;
                                break;
                            }
                        }
                        parentActivity = parentActivity.Parent;
                    }

                    if (!foundParent)
                        filteredActivities.Add(activity);
                }
            }
            return filteredActivities.ToArray();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity[] GetNestedActivities(CompositeActivity compositeActivity)
        {

            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            IList<Activity> childActivities = null;
            ArrayList nestedActivities = new ArrayList();
            Queue compositeActivities = new Queue();
            compositeActivities.Enqueue(compositeActivity);
            while (compositeActivities.Count > 0)
            {
                CompositeActivity compositeActivity2 = (CompositeActivity)compositeActivities.Dequeue();
                childActivities = compositeActivity2.Activities;

                foreach (Activity activity in childActivities)
                {
                    nestedActivities.Add(activity);
                    if (activity is CompositeActivity)
                        compositeActivities.Enqueue(activity);
                }
            }
            return (Activity[])nestedActivities.ToArray(typeof(Activity));
        }


        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static IList GetIdentifiersInCompositeActivity(CompositeActivity compositeActivity)
        {
            ArrayList identifiers = new ArrayList();
            if (compositeActivity != null)
            {
                identifiers.Add(compositeActivity.Name);
                IList<Activity> allChildren = GetAllNestedActivities(compositeActivity);
                foreach (Activity activity in allChildren)
                    identifiers.Add(activity.Name);
            }
            return ArrayList.ReadOnly(identifiers);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity[] GetAllNestedActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            ArrayList nestedActivities = new ArrayList();

            // Note: GetAllNestedActivities will not check for black box activities.
            // This is to allow it to be invoked from within the activity's 
            // constructor.
            //if(Helpers.IsCustomActivity(compositeActivity))
            //return (Activity[])nestedActivities.ToArray(typeof(Activity));

            Queue compositeActivities = new Queue();
            compositeActivities.Enqueue(compositeActivity);
            while (compositeActivities.Count > 0)
            {
                CompositeActivity compositeActivity2 = (CompositeActivity)compositeActivities.Dequeue();
                if (compositeActivity2 == compositeActivity || !Helpers.IsCustomActivity(compositeActivity2))
                {
                    foreach (Activity activity in compositeActivity2.Activities)
                    {
                        nestedActivities.Add(activity);
                        if (activity is CompositeActivity)
                            compositeActivities.Enqueue(activity);
                    }

                    foreach (Activity activity in compositeActivity2.EnabledActivities)
                    {
                        if (!nestedActivities.Contains(activity))
                        {
                            nestedActivities.Add(activity);
                            if (activity is CompositeActivity)
                                compositeActivities.Enqueue(activity);
                        }
                    }
                }
            }
            return (Activity[])nestedActivities.ToArray(typeof(Activity));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string MergeNamespaces(string primaryNs, string secondaryNs)
        {
            string newNs = primaryNs;
            if (secondaryNs != null && secondaryNs.Length > 0)
            {
                if (newNs != null && newNs.Length > 0)
                    newNs += ("." + secondaryNs);
                else
                    newNs = secondaryNs;
            }

            if (newNs == null)
                newNs = string.Empty;
            return newNs;
        }

        internal static Activity GetRootActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            while (activity.Parent != null)
                activity = activity.Parent;

            return activity;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Stream SerializeDesignersToStream(ICollection activities)
        {
            Stream stateStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stateStream);

            Queue<IComponent> serializedComponents = new Queue<IComponent>();
            foreach (IComponent activity in activities)
                serializedComponents.Enqueue(activity);

            while (serializedComponents.Count > 0)
            {
                IComponent component = serializedComponents.Dequeue();
                if (component != null && component.Site != null)
                {
                    IDesignerHost designerHost = component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (designerHost == null)
                        throw new InvalidOperationException(
                            SR.GetString(SR.General_MissingService, typeof(IDesignerHost).Name));

                    ActivityDesigner activityDesigner = designerHost.GetDesigner(component) as ActivityDesigner;
                    if (activityDesigner != null)
                    {
                        try
                        {
                            ((IPersistUIState)activityDesigner).SaveViewState(writer);

                            CompositeActivity compositeActivity = component as CompositeActivity;
                            if (compositeActivity != null)
                            {
                                foreach (IComponent childActivity in compositeActivity.Activities)
                                {
                                    serializedComponents.Enqueue(childActivity);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return stateStream;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void DeserializeDesignersFromStream(ICollection activities, Stream stateStream)
        {
            if (stateStream.Length == 0)
                return;

            BinaryReader reader = new BinaryReader(stateStream);
            stateStream.Seek(0, SeekOrigin.Begin);

            Queue<IComponent> serializedComponents = new Queue<IComponent>();
            foreach (IComponent component in activities)
                serializedComponents.Enqueue(component);

            while (serializedComponents.Count > 0)
            {
                IComponent component = serializedComponents.Dequeue();
                if (component != null && component.Site != null)
                {
                    IDesignerHost designerHost = component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (designerHost == null)
                        throw new InvalidOperationException(
                            SR.GetString(SR.General_MissingService, typeof(IDesignerHost).Name));

                    ActivityDesigner activityDesigner = designerHost.GetDesigner(component) as ActivityDesigner;
                    if (activityDesigner != null)
                    {
                        try
                        {
                            ((IPersistUIState)activityDesigner).LoadViewState(reader);

                            CompositeActivity compositeActivity = component as CompositeActivity;
                            if (compositeActivity != null)
                            {
                                foreach (IComponent childActivity in compositeActivity.Activities)
                                {
                                    serializedComponents.Enqueue(childActivity);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string GetBaseIdentifier(Activity activity)
        {
            string baseIdentifier = activity.GetType().Name;
            StringBuilder b = new StringBuilder(baseIdentifier.Length);
            for (int i = 0; i < baseIdentifier.Length; i++)
            {
                if (Char.IsUpper(baseIdentifier[i]) && (i == 0 || i == baseIdentifier.Length - 1 || Char.IsUpper(baseIdentifier[i + 1])))
                {
                    b.Append(Char.ToLowerInvariant(baseIdentifier[i]));
                }
                else
                {
                    b.Append(baseIdentifier.Substring(i));
                    break;
                }
            }
            return b.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string GetRootNamespace(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            string rootNs = string.Empty;
            IWorkflowCompilerOptionsService compilerOptionsService = (IWorkflowCompilerOptionsService)serviceProvider.GetService(typeof(IWorkflowCompilerOptionsService));
            if (compilerOptionsService != null && compilerOptionsService.RootNamespace != null)
                rootNs = compilerOptionsService.RootNamespace; //e.g. WorkflowApp1
            return rootNs;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Type GetDataSourceClass(Activity activity, IServiceProvider serviceProvider)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            Type activityType = null;
            string className = null;
            if (activity == GetRootActivity(activity))
                className = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as String;

            if (!String.IsNullOrEmpty(className))
            {
                ITypeProvider typeProvider = (ITypeProvider)serviceProvider.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                    throw new InvalidOperationException(
                        SR.GetString(SR.General_MissingService, typeof(ITypeProvider).Name));

                activityType = typeProvider.GetType(className);
            }
            else
            {
                return activity.GetType();
            }

            return activityType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity GetDataSourceActivity(Activity activity, string inputName, out string name)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (string.IsNullOrEmpty(inputName))
                throw new ArgumentException("inputName");

            name = inputName;
            if (inputName.IndexOf('.') == -1)
                return activity;

            int indexOfDot = inputName.LastIndexOf('.');
            string scopeID = inputName.Substring(0, indexOfDot);
            name = inputName.Substring(indexOfDot + 1);

            Activity contextActivity = Helpers.ParseActivityForBind(activity, scopeID);
            if (contextActivity == null)
                contextActivity = Helpers.ParseActivity(Helpers.GetRootActivity(activity), scopeID);

            // activity can be either the qualified id of the scope activity or the qualified id of the custom activity.
            return contextActivity;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void GetNamespaceAndClassName(string fullQualifiedName, out string namespaceName, out string className)
        {
            namespaceName = String.Empty;
            className = String.Empty;

            if (fullQualifiedName == null)
                return;

            int indexOfDot = fullQualifiedName.LastIndexOf('.');
            if (indexOfDot != -1)
            {
                namespaceName = fullQualifiedName.Substring(0, indexOfDot);
                className = fullQualifiedName.Substring(indexOfDot + 1);
            }
            else
            {
                className = fullQualifiedName;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static CodeTypeDeclaration GetCodeNamespaceAndClass(CodeNamespaceCollection namespaces, string namespaceName, string className, out CodeNamespace codeNamespace)
        {
            codeNamespace = null;
            foreach (CodeNamespace ns in namespaces)
            {
                if (ns.Name == namespaceName)
                {
                    codeNamespace = ns;
                    break;
                }
            }

            CodeTypeDeclaration codeTypeDeclaration = null;
            if (codeNamespace != null)
            {
                foreach (CodeTypeDeclaration typeDecl in codeNamespace.Types)
                {
                    if (typeDecl.Name == className)
                    {
                        codeTypeDeclaration = typeDecl;
                        break;
                    }
                }
            }
            return codeTypeDeclaration;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string GetClassName(string fullQualifiedName)
        {
            if (fullQualifiedName == null)
                return null;

            string className = fullQualifiedName;
            int indexOfDot = fullQualifiedName.LastIndexOf('.');
            if (indexOfDot != -1)
                className = fullQualifiedName.Substring(indexOfDot + 1);
            return className;
        }


        [DllImport("msi.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern int MsiGetProductInfoW(string szProduct, string szProperty, StringBuilder lpValueBuf, ref int pcchValueBuf);
        private static string GetInstallDirectory(bool getSDKDir)
        {
            string path = string.Empty;
            try
            {
                //ERROR_UNKNOWN_PROPERTY           1608L
                //ERROR_INVALID_PARAMETER          87L
                int length = FILENAME_MAX + 1;
                StringBuilder location = new StringBuilder(length);
                int hr = MsiGetProductInfoW(getSDKDir ? ProductSDKCode : ProductCode, INSTALLPROPERTY_INSTALLLOCATION, location, ref length);
                int error = Marshal.GetLastWin32Error();
                if (hr == 0)
                {
                    path = location.ToString();
                }
                else
                {
                    Debug.WriteLine("Error loading install directory: " + error.ToString(CultureInfo.CurrentCulture));
                }

            }
            catch
            {
            }

            if (string.IsNullOrEmpty(path))
            {
                try
                {
                    if (!getSDKDir)
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ProductRootRegKey))
                        {
                            if (key != null)
                                path = (string)key.GetValue("InstallDir");
                        }
                    }
                }
                catch
                {
                }
            }

            // 
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return path;
        }

        private static string GetInstallDirectory30()
        {
            string path = string.Empty;
            try
            {
                //ERROR_UNKNOWN_PROPERTY           1608L
                //ERROR_INVALID_PARAMETER          87L
                int length = FILENAME_MAX + 1;
                StringBuilder location = new StringBuilder(length);
                int hr = MsiGetProductInfoW(ProductCode, INSTALLPROPERTY_INSTALLLOCATION, location, ref length);
                int error = Marshal.GetLastWin32Error();
                if (hr == 0)
                {
                    path = location.ToString();
                }
                else
                {
                    Debug.WriteLine("Error loading 3.0 install directory: " + error.ToString(CultureInfo.CurrentCulture));
                }

            }
            catch
            {
            }

            if (string.IsNullOrEmpty(path))
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ProductRootRegKey30))
                    {
                        if (key != null)
                        {
                            path = (string)key.GetValue("InstallDir");
                        }
                    }
                }
                catch
                {
                }
            }

            return path;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Type GetBaseType(PropertyInfo property, object owner, IServiceProvider serviceProvider)
        {
            //When we are emitting code for the dynamic properties we might get the propertyinfo as null
            if (owner == null)
                throw new ArgumentNullException("owner");

            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            if (property != null)
            {
                IDynamicPropertyTypeProvider basetypeProvider = owner as IDynamicPropertyTypeProvider;
                if (basetypeProvider != null)
                {
                    Type type = basetypeProvider.GetPropertyType(serviceProvider, property.Name);
                    if (type != null)
                        return type;
                }

                return property.PropertyType;
            }

            return null;
        }

        //
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static AccessTypes GetAccessType(PropertyInfo property, object owner, IServiceProvider serviceProvider)
        {
            //When we are emitting code for the dynamic properties we might get the propertyinfo as null
            if (owner == null)
                throw new ArgumentNullException("owner");

            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            if (property != null)
            {
                IDynamicPropertyTypeProvider basetypeProvider = owner as IDynamicPropertyTypeProvider;
                if (basetypeProvider != null)
                    return basetypeProvider.GetAccessType(serviceProvider, property.Name);
            }

            return AccessTypes.Read;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool IsChildActivity(CompositeActivity parent, Activity activity)
        {
            foreach (Activity containedActivity in parent.Activities)
            {
                if (activity == containedActivity)
                    return true;

                if (containedActivity is CompositeActivity &&
                    Helpers.IsChildActivity(containedActivity as CompositeActivity, activity))
                    return true;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool TypesEqual(CodeTypeReference typeLeft, Type typeRight)
        {
            if (typeRight.IsArray && typeLeft.ArrayRank != typeRight.GetArrayRank()) return false;
            // 
            if (!typeLeft.BaseType.Equals(typeRight.FullName)) return false;

            if (typeLeft.ArrayRank > 0)
                return TypesEqual(typeLeft.ArrayElementType, typeRight.GetElementType());
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool TypesEqual(CodeTypeReference typeLeft, CodeTypeReference typeRight)
        {
            if (typeLeft.ArrayRank != typeRight.ArrayRank) return false;
            if (!typeLeft.BaseType.Equals(typeRight.BaseType)) return false;

            if (typeLeft.ArrayRank > 0)
                return TypesEqual(typeLeft.ArrayElementType, typeRight.ArrayElementType);
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static DesignerSerializationVisibility GetSerializationVisibility(MemberInfo memberInfo)
        {

            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            DesignerSerializationVisibility designerSerializationVisibility = DesignerSerializationVisibility.Visible;

            // Calling GetCustomAttributes on PropertyInfo or EventInfo when the inherit parameter of GetCustomAttributes 
            // is true does not walk the type hierarchy. But System.Attribute.GetCustomAttributes causes perf issues.
            object[] attributes = memberInfo.GetCustomAttributes(typeof(DesignerSerializationVisibilityAttribute), true);
            if (attributes.Length > 0)
                designerSerializationVisibility = (attributes[0] as DesignerSerializationVisibilityAttribute).Visibility;
            else if (Attribute.IsDefined(memberInfo, typeof(DesignerSerializationVisibilityAttribute)))
                designerSerializationVisibility = (Attribute.GetCustomAttribute(memberInfo, typeof(DesignerSerializationVisibilityAttribute)) as DesignerSerializationVisibilityAttribute).Visibility;

            return designerSerializationVisibility;
        }

        // Method parameters must match exactly
        // 
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static MethodInfo GetMethodExactMatch(Type type, string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            MethodInfo foundMethod = null;
            MethodInfo[] methods = type.GetMethods(bindingAttr);
            foreach (MethodInfo method in methods)
            {
                bool matchName = ((bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase) ? string.Compare(method.Name, name, StringComparison.OrdinalIgnoreCase) == 0 : string.Compare(method.Name, name, StringComparison.Ordinal) == 0;
                if (matchName)
                {
                    bool mismatch = false;
                    if (types != null)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.GetLength(0) == types.Length)
                        {
                            for (int index = 0; !mismatch && index < parameters.Length; index++)
                                mismatch = (parameters[index].ParameterType == null) || (!parameters[index].ParameterType.IsAssignableFrom(types[index]));
                        }
                        else
                            mismatch = true;
                    }
                    if (!mismatch)
                    {
                        foundMethod = method;
                        break;
                    }
                }
            }

            return foundMethod;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static T GetAttributeFromObject<T>(object attributeObject) where T : Attribute
        {
            if (attributeObject is AttributeInfoAttribute)
                return (T)((AttributeInfoAttribute)attributeObject).AttributeInfo.CreateAttribute();

            if (attributeObject is T)
                return (T)attributeObject;

            return null;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Type GetDelegateFromEvent(EventInfo eventInfo)
        {
            if (eventInfo.EventHandlerType != null)
                return eventInfo.EventHandlerType;

            return TypeProvider.GetEventHandlerType(eventInfo);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void AddTypeProviderAssembliesFromRegistry(TypeProvider typeProvider, IServiceProvider serviceProvider)
        {
            if (typeProvider == null)
                throw new ArgumentNullException("typeProvider");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            RegistryKey referenceKey = Registry.CurrentUser.OpenSubKey(TypeProviderRegistryKeyPath);
            if (referenceKey != null)
            {
                ITypeProviderCreator typeProviderCreator = serviceProvider.GetService(typeof(ITypeProviderCreator)) as ITypeProviderCreator;
                foreach (string assemblyName in ((string[])referenceKey.GetValue(TypeProviderAssemblyRegValueName)))
                {
                    try
                    {
                        if (typeProviderCreator != null)
                        {
                            bool addAssembly = true;
                            Assembly assembly = typeProviderCreator.GetTransientAssembly(AssemblyName.GetAssemblyName(assemblyName));
                            // Check to see if a copy of the assembly is already added.
                            if (assembly != null)
                            {
                                foreach (Type type in assembly.GetTypes())
                                {
                                    if (typeProvider.GetType(type.AssemblyQualifiedName) != null)
                                        addAssembly = false;
                                    break;
                                }
                                if (addAssembly)
                                    typeProvider.AddAssembly(assembly);
                            }
                        }
                        else
                            // AddAssemblyReference should take care of duplicates.
                            typeProvider.AddAssemblyReference(assemblyName);

                    }
                    catch
                    {
                        // Continue loading.
                    }
                }

                referenceKey.Close();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void UpdateTypeProviderAssembliesRegistry(string assemblyName)
        {
            RegistryKey referenceKey = Registry.CurrentUser.CreateSubKey(TypeProviderRegistryKeyPath);
            if (referenceKey != null)
            {
                try
                {
                    ArrayList references = null;
                    if (referenceKey.ValueCount > 0)
                        references = new ArrayList((string[])referenceKey.GetValue(TypeProviderAssemblyRegValueName));
                    else
                        references = new ArrayList();

                    if (!references.Contains(assemblyName))
                    {
                        references.Add(assemblyName);
                        referenceKey.SetValue(TypeProviderAssemblyRegValueName, ((string[])references.ToArray(typeof(string))));
                    }
                }
                catch
                {
                    //We eat the exception
                }
                finally
                {
                    referenceKey.Close();
                }
            }
        }

        internal static CompositeActivity GetDeclaringActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            CompositeActivity parent = activity.Parent;
            while (parent != null)
            {
                // This will be the root
                if (parent.Parent == null)
                    return parent;

                // Any custom activity found is the declaring activity
                if (IsCustomActivity(parent))
                    return parent;

                parent = parent.Parent;
            }
            return null;
        }

        internal static bool IsActivityLocked(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            CompositeActivity parent = activity.Parent;
            while (parent != null)
            {
                // If root, not locked
                if (parent.Parent == null)
                    return false;

                // Any custom activity found, then locked
                if (IsCustomActivity(parent))
                    return true;

                parent = parent.Parent;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity GetEnclosingActivity(Activity activity)
        {
            Activity enclosingActivity;

            if (IsActivityLocked(activity))
                enclosingActivity = Helpers.GetDeclaringActivity(activity);
            else
                enclosingActivity = GetRootActivity(activity);

            return enclosingActivity;
        }

        // This function returns all the executable activities including secondary flow activities.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IList<Activity> GetAllEnabledActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            List<Activity> allActivities = new List<Activity>(compositeActivity.EnabledActivities);
            foreach (Activity childActivity in compositeActivity.Activities)
            {
                if (childActivity.Enabled &&
                        IsFrameworkActivity(childActivity))
                    allActivities.Add(childActivity);
            }
            return allActivities;
        }
        public static bool IsFrameworkActivity(Activity activity)
        {
            return (activity is CancellationHandlerActivity ||
                        activity is CompensationHandlerActivity ||
                        activity is FaultHandlersActivity);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static MethodInfo GetInterfaceMethod(Type interfaceType, string methodName)
        {
            MethodInfo methodInfo = null;
            string interfaceName = String.Empty;
            string method = String.Empty;

            if (methodName.LastIndexOf('.') > 0)
            {
                interfaceName = methodName.Substring(0, methodName.LastIndexOf('.'));
                method = methodName.Substring(methodName.LastIndexOf('.') + 1);
            }

            if (!String.IsNullOrEmpty(interfaceName))
            {
                foreach (Type inheritedInterface in interfaceType.GetInterfaces())
                {
                    if (String.Compare(inheritedInterface.FullName, interfaceName, StringComparison.Ordinal) == 0)
                    {
                        methodInfo = inheritedInterface.GetMethod(method);
                        break;
                    }
                }
            }
            else
            {
                methodInfo = interfaceType.GetMethod(methodName);
            }

            return methodInfo;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static XmlWriter CreateXmlWriter(object output)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;
            settings.CloseOutput = true;

            if (output is string)
                return XmlWriter.Create(output as string, settings);
            else if (output is TextWriter)
                return XmlWriter.Create(output as TextWriter, settings);
            else
            {
                Debug.Assert(false, "Invalid argument type.  'output' must either be string or TextWriter.");
                return null;
            }
        }

        #region DesignTimeType Support
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string GetDesignTimeTypeName(object owner, object key)
        {
            string typeName = null;
            DependencyObject dependencyObject = owner as DependencyObject;
            if (dependencyObject != null && key != null)
            {
                if (dependencyObject.UserData.Contains(UserDataKeys.DesignTimeTypeNames))
                {
                    Hashtable typeNames = dependencyObject.UserData[UserDataKeys.DesignTimeTypeNames] as Hashtable;
                    if (typeNames != null && typeNames.ContainsKey(key))
                        typeName = typeNames[key] as string;
                }
            }
            return typeName;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void SetDesignTimeTypeName(object owner, object key, string value)
        {
            DependencyObject dependencyObject = owner as DependencyObject;
            if (dependencyObject != null && key != null)
            {
                if (!dependencyObject.UserData.Contains(UserDataKeys.DesignTimeTypeNames))
                    dependencyObject.UserData[UserDataKeys.DesignTimeTypeNames] = new Hashtable();

                Hashtable typeNames = dependencyObject.UserData[UserDataKeys.DesignTimeTypeNames] as Hashtable;
                typeNames[key] = value;
            }
        }
        #endregion

        #region Helpers from ExecutableCompModHelpers
        // This only works for composite activity.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool IsCustomActivity(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            if (compositeActivity.UserData.Contains(UserDataKeys.CustomActivity))
            {
                return (bool)(compositeActivity.UserData[UserDataKeys.CustomActivity]);
            }
            else
            {
                try
                {
                    CompositeActivity activity = Activator.CreateInstance(compositeActivity.GetType()) as CompositeActivity;
                    if (activity != null && activity.Activities.Count > 0)
                    {
                        compositeActivity.UserData[UserDataKeys.CustomActivityDefaultName] = activity.Name;
                        compositeActivity.UserData[UserDataKeys.CustomActivity] = true; //
                        return true;
                    }
                }
                catch
                {
                    // 
                }
            }

            compositeActivity.UserData[UserDataKeys.CustomActivity] = false; //
            return false;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "IndexOf(\".\") not a security issue.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity ParseActivity(Activity parsingContext, string activityName)
        {
            if (parsingContext == null)
                throw new ArgumentNullException("parsingContext");
            if (activityName == null)
                throw new ArgumentNullException("activityName");

            string currentAtivityName = activityName;
            string nextActivityName = string.Empty;
            int indexOfDot = activityName.IndexOf(".");
            if (indexOfDot != -1)
            {
                currentAtivityName = activityName.Substring(0, indexOfDot);
                nextActivityName = activityName.Substring(indexOfDot + 1);
                if (nextActivityName.Length == 0)
                    return null;
            }

            Activity currentActivity = GetActivity(parsingContext, currentAtivityName);
            if (currentActivity != null)
            {
                if (nextActivityName.Length > 0)
                {
                    if (!(currentActivity is CompositeActivity) || !IsCustomActivity(currentActivity as CompositeActivity))
                        // current activity must be a custom activity, otherwise there should be no dots in the name.
                        return null;

                    string[] names = nextActivityName.Split('.');
                    for (int i = 0; i < names.Length; i++)
                    {
                        Activity nextActivity = GetActivity(currentActivity, names[i]);
                        if (nextActivity == null || !Helpers.IsActivityLocked(nextActivity))
                            return null;

                        CompositeActivity declaringActivity = GetDeclaringActivity(nextActivity);
                        if (currentActivity != declaringActivity)
                            return null;

                        currentActivity = nextActivity;
                    }

                    return currentActivity;
                }
                else
                {
                    // This activity should always be unlocked, unless if GetChildActivity() is called from
                    // within the custom activity's companion class, then we don't have the full qualified ID available
                    // at that time.  We allow this to succeed only if the declaring activity is the same as the declaring
                    // activity of the context activity passed in.
                    if (Helpers.IsActivityLocked(currentActivity))//.IsLocked)
                    {
                        if (!IsDeclaringActivityMatchesContext(currentActivity, parsingContext))
                            return null;
                    }

                    return currentActivity;
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Activity ParseActivityForBind(Activity context, string activityName)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (activityName == null)
                throw new ArgumentNullException("activityName");

            if (string.Equals(activityName, "/Self", StringComparison.Ordinal))
            {
                return context;
            }
            else if (activityName.StartsWith("/Parent", StringComparison.OrdinalIgnoreCase))
            {
                Activity activity = context;
                string[] paths = activityName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length && activity != null; i++)
                {
                    string parent = paths[i].Trim();
                    activity = (string.Equals(parent, "Parent", StringComparison.OrdinalIgnoreCase)) ? activity.Parent : null;
                }
                return activity;
            }
            else if (Helpers.IsActivityLocked(context))
            {
                // Look for the matching activity inside the custom activity context first.  This is because if a bind
                // is coming from a custom activity, it's activity ref ID may not match the qualified ID of the
                // the activity.
                Activity activity = null;
                Activity declaringActivity = Helpers.GetDeclaringActivity(context);
                Guid currentContextGuid = GetRuntimeContextGuid(context);
                Guid declaringContextGuid = GetRuntimeContextGuid(declaringActivity);

                Activity currentContextActivity = context;
                Activity parentContextActivity = context.Parent;
                Guid parentContextGuid = GetRuntimeContextGuid(parentContextActivity);
                while (activity == null && declaringContextGuid != currentContextGuid)
                {
                    // WinOE Bug 17931: if the context id is different, it means that this activity is running in a child context (such as 
                    // the children of a replicator or a while).  we need to resolve the activity within the child context
                    // first.  If we go up to the declaring activity, we'd be finding children of the template instead of
                    // the actual running instance.
                    while (parentContextActivity != null && parentContextGuid == currentContextGuid)
                    {
                        currentContextActivity = parentContextActivity;
                        parentContextActivity = parentContextActivity.Parent;
                        parentContextGuid = GetRuntimeContextGuid(parentContextActivity);
                    }

                    activity = Helpers.ParseActivity(currentContextActivity, activityName);
                    currentContextGuid = parentContextGuid;
                }

                if (activity == null)
                {
                    // Check the declaring activity
                    activity = Helpers.ParseActivity(declaringActivity, activityName);
                }

                // Nothing found, let's see if this is bind to the custom activity itself.
                if (activity == null)
                {
                    if (!declaringActivity.UserData.Contains(UserDataKeys.CustomActivityDefaultName))
                    {
                        //we need to activate a new instance of the declaringActivity's type and check if its name equals to the one we are looking for
                        Activity newCustomActivity = Activator.CreateInstance(declaringActivity.GetType()) as Activity;
                        declaringActivity.UserData[UserDataKeys.CustomActivityDefaultName] = newCustomActivity.Name;
                    }

                    if (((string)declaringActivity.UserData[UserDataKeys.CustomActivityDefaultName]) == activityName)
                        activity = declaringActivity;
                }

                // if this is a locked activity, its bind reference must be within its declaring activity. We should not try
                // to resolve it beyond that scope.
                return activity;
            }

            Activity targetActivity = null;
            Activity parentActivity = context;

            //if it's a custom activity and it has parent, start looking for the target activity at the parent level
            //otherwise we'll get children of the custom activity
            bool mayLookInside = false; //we may look inside the custom activity if the target is not found outside
            CompositeActivity compositeParentActivity = parentActivity as CompositeActivity;
            if (compositeParentActivity != null && parentActivity.Parent != null && IsCustomActivity(compositeParentActivity))
            {
                mayLookInside = true;
                parentActivity = parentActivity.Parent;
            }

            while (targetActivity == null && parentActivity != null)
            {
                targetActivity = parentActivity.GetActivityByName(activityName, true);
                parentActivity = parentActivity.Parent;
            }

            if (mayLookInside && targetActivity == null)
            {
                //if we have not found an appropriate activity at the parent level, try looking inside
                //we dont need to look outside (loop while parent is not null) - it has already been done above
                parentActivity = context;
                targetActivity = parentActivity.GetActivityByName(activityName, true);
            }


            return (targetActivity == null) ? Helpers.ParseActivity(Helpers.GetRootActivity(context), activityName) : targetActivity;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static Guid GetRuntimeContextGuid(Activity currentActivity)
        {
            Activity contextActivity = currentActivity;
            Guid contextGuid = (Guid)contextActivity.GetValue(Activity.ActivityContextGuidProperty);
            while (contextGuid == Guid.Empty && contextActivity.Parent != null)
            {
                contextActivity = contextActivity.Parent;
                contextGuid = (Guid)contextActivity.GetValue(Activity.ActivityContextGuidProperty);
            }
            return contextGuid;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool IsDeclaringActivityMatchesContext(Activity currentActivity, Activity context)
        {
            CompositeActivity declaringContext = context as CompositeActivity;

            CompositeActivity declaringActivityOfCurrent = Helpers.GetDeclaringActivity(currentActivity);

            // If the context activity is locked and it is a primitive activity
            // or NOT a custom activity then we need to find its enclosing 
            // custom activity.
            if (Helpers.IsActivityLocked(context) &&
                    (declaringContext == null || !Helpers.IsCustomActivity(declaringContext))
                )
                declaringContext = Helpers.GetDeclaringActivity(context);

            if (declaringContext == declaringActivityOfCurrent)
                return true;
            else
                return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool IsAlternateFlowActivity(Activity activity)
        {
            if (activity == null)
                return false;

            bool isAlternateFlowActivityAttribute = false;
            //dont want to use reflection to check for the alternate flow attribute
            //this is a fix for a perf issue - use of reflection in a ui loop
            if (!activity.UserData.Contains(typeof(AlternateFlowActivityAttribute)))
            {
                isAlternateFlowActivityAttribute = activity.GetType().GetCustomAttributes(typeof(AlternateFlowActivityAttribute), true).Length != 0;
                activity.UserData[typeof(AlternateFlowActivityAttribute)] = isAlternateFlowActivityAttribute;
            }
            else
            {
                isAlternateFlowActivityAttribute = (bool)activity.UserData[typeof(AlternateFlowActivityAttribute)];
            }

            return isAlternateFlowActivityAttribute;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static Activity GetActivity(Activity containerActivity, string id)
        {
            if (containerActivity != null)
            {
                Queue activities = new Queue();
                activities.Enqueue(containerActivity);
                while (activities.Count > 0)
                {
                    Activity activity = (Activity)activities.Dequeue();
                    if (activity.Enabled)
                    {
                        if (activity.Name == id)
                            return activity;

                        if (activity is CompositeActivity)
                        {
                            foreach (Activity child in ((CompositeActivity)activity).Activities)
                                activities.Enqueue(child);
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region Class TypeDescriptorContext (SHARED WITH ACTIVITIES, ORCHESTRATIONDESIGNER)
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Not all assemblies that include this file instantiate the class.")]
    internal sealed class TypeDescriptorContext : ITypeDescriptorContext
    {
        private IServiceProvider serviceProvider;
        private PropertyDescriptor propDesc;
        private object instance;

        public TypeDescriptorContext(IServiceProvider serviceProvider, PropertyDescriptor propDesc, object instance)
        {
            this.serviceProvider = serviceProvider;
            this.propDesc = propDesc;
            this.instance = instance;
        }

        public IContainer Container
        {
            get
            {
                return (IContainer)this.serviceProvider.GetService(typeof(IContainer));
            }
        }
        public object Instance
        {
            get
            {
                return this.instance;
            }
        }
        public PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return this.propDesc;
            }
        }
        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public bool OnComponentChanging()
        {
            IComponentChangeService componentChangeService = (IComponentChangeService)this.serviceProvider.GetService(typeof(IComponentChangeService));
            if (componentChangeService != null)
            {
                try
                {
                    componentChangeService.OnComponentChanging(this.instance, this.propDesc);
                }
                catch (CheckoutException ce)
                {
                    if (ce == CheckoutException.Canceled)
                        return false;
                    throw ce;
                }
            }

            return true;
        }
        public void OnComponentChanged()
        {
            IComponentChangeService componentChangeService = (IComponentChangeService)this.serviceProvider.GetService(typeof(IComponentChangeService));
            if (componentChangeService != null)
                componentChangeService.OnComponentChanged(this.instance, this.propDesc, null, null);
        }
    }
    #endregion

    // This class has been added as a fix for bug 18214 in order to 
    // create an independent code-path for debugger's use of ParseActivity functionality.
    // The GetActivity method of this class uses QualifiedName instead of Name property
    // for finding activities.
    internal static class DebuggerHelpers
    {
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "IndexOf(\".\") not a security issue.")]
        internal static Activity ParseActivity(Activity parsingContext, string activityName)
        {
            if (parsingContext == null)
                throw new ArgumentNullException("parsingContext");
            if (activityName == null)
                throw new ArgumentNullException("activityName");

            string currentAtivityName = activityName;
            string nextActivityName = string.Empty;
            int indexOfDot = activityName.IndexOf(".");
            if (indexOfDot != -1)
            {
                currentAtivityName = activityName.Substring(0, indexOfDot);
                nextActivityName = activityName.Substring(indexOfDot + 1);
                if (nextActivityName.Length == 0)
                    return null;
            }

            Activity currentActivity = null;
            currentActivity = GetActivity(parsingContext, currentAtivityName);

            // The check for parsingContext.Parent != null is added here because IsCustomActivity returns true for root activities.
            if (currentActivity == null && (parsingContext is CompositeActivity) && parsingContext.Parent != null && Helpers.IsCustomActivity(parsingContext as CompositeActivity))
                currentActivity = GetActivity(parsingContext, parsingContext.QualifiedName + "." + currentAtivityName);

            if (currentActivity != null)
            {
                if (nextActivityName.Length > 0)
                {
                    if (!(currentActivity is CompositeActivity) || !Helpers.IsCustomActivity(currentActivity as CompositeActivity))
                        // current activity must be a custom activity, otherwise there should be no dots in the name.
                        return null;

                    string[] names = nextActivityName.Split('.');
                    for (int i = 0; i < names.Length; i++)
                    {
                        Activity nextActivity = GetActivity(currentActivity, currentActivity.QualifiedName + "." + names[i]);
                        if (nextActivity == null || !Helpers.IsActivityLocked(nextActivity))
                            return null;

                        CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(nextActivity);
                        if (currentActivity != declaringActivity)
                            return null;

                        currentActivity = nextActivity;
                    }

                    return currentActivity;
                }
                else
                {
                    // This activity should always be unlocked, unless if GetChildActivity() is called from
                    // within the custom activity's companion class, then we don't have the full qualified ID available
                    // at that time.  We allow this to succeed only if the declaring activity is the same as the declaring
                    // activity of the context activity passed in.
                    if (Helpers.IsActivityLocked(currentActivity))//.IsLocked)
                    {
                        if (!IsDeclaringActivityMatchesContext(currentActivity, parsingContext))
                            return null;
                    }

                    return currentActivity;
                }
            }

            return null;
        }

        private static Activity GetActivity(Activity containerActivity, string id)
        {
            if (containerActivity != null)
            {
                Queue activities = new Queue();
                activities.Enqueue(containerActivity);
                while (activities.Count > 0)
                {
                    Activity activity = (Activity)activities.Dequeue();
                    if (activity.Enabled)
                    {
                        if (activity.QualifiedName == id)
                            return activity;

                        if (activity is CompositeActivity)
                        {
                            foreach (Activity child in ((CompositeActivity)activity).Activities)
                                activities.Enqueue(child);
                        }
                    }
                }
            }
            return null;
        }

        private static bool IsDeclaringActivityMatchesContext(Activity currentActivity, Activity context)
        {
            CompositeActivity declaringContext = context as CompositeActivity;

            CompositeActivity declaringActivityOfCurrent = Helpers.GetDeclaringActivity(currentActivity);

            // If the context activity is locked and it is a primitive activity
            // or NOT a custom activity then we need to find its enclosing 
            // custom activity.
            if (Helpers.IsActivityLocked(context) &&
                    (declaringContext == null || !Helpers.IsCustomActivity(declaringContext))
                )
                declaringContext = Helpers.GetDeclaringActivity(context);

            if (declaringContext == declaringActivityOfCurrent)
                return true;
            else
                return false;
        }
    }
}
