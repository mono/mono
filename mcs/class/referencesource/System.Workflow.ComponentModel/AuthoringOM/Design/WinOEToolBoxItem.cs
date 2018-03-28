namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Design;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Reflection;

    [Serializable]
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityToolboxItem : ToolboxItem
    {
        private const String ActivitySuffix = "Activity";

        public ActivityToolboxItem()
        {
        }
        public ActivityToolboxItem(Type type)
            : base(type)
        {
            // 
            if (type != null)
            {
                if (type.Name != null)
                {
                    string name = type.Name;
                    if ((type.Assembly == Assembly.GetExecutingAssembly() ||
                        type.Assembly != null && type.Assembly.FullName != null &&
                        type.Assembly.FullName.Equals(AssemblyRef.ActivitiesAssemblyRef, StringComparison.OrdinalIgnoreCase)) &&
                        type.Name.EndsWith(ActivitySuffix, StringComparison.Ordinal) &&
                        !type.Name.Equals(ActivitySuffix, StringComparison.Ordinal))
                    {
                        name = type.Name.Substring(0, type.Name.Length - ActivitySuffix.Length);
                    }

                    base.DisplayName = name;
                }

                base.Description = ActivityDesigner.GetActivityDescription(type);
            }
        }

        protected ActivityToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }

        public virtual IComponent[] CreateComponentsWithUI(IDesignerHost host)
        {
            return CreateComponentsCore(host);
        }

        // 

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            Type typeOfComponent = GetType(host, AssemblyName, TypeName, true);

            if (typeOfComponent == null && host != null)
                typeOfComponent = host.GetType(TypeName);

            if (typeOfComponent == null)
            {
                ITypeProviderCreator tpc = null;
                if (host != null)
                    tpc = (ITypeProviderCreator)host.GetService(typeof(ITypeProviderCreator));
                if (tpc != null)
                {
                    System.Reflection.Assembly assembly = tpc.GetTransientAssembly(this.AssemblyName);
                    if (assembly != null)
                        typeOfComponent = assembly.GetType(this.TypeName);
                }

                if (typeOfComponent == null)
                    typeOfComponent = GetType(host, AssemblyName, TypeName, true);
            }

            ArrayList comps = new ArrayList();
            if (typeOfComponent != null)
            {
                if (typeof(IComponent).IsAssignableFrom(typeOfComponent))
                    comps.Add(TypeDescriptor.CreateInstance(null, typeOfComponent, null, null));
            }

            IComponent[] temp = new IComponent[comps.Count];
            comps.CopyTo(temp, 0);
            return temp;
        }

        public static Image GetToolboxImage(Type activityType)
        {
            if (activityType == null)
                throw new ArgumentNullException("activityType");

            Image toolBoxImage = null;
            if (activityType != null)
            {
                object[] attribs = activityType.GetCustomAttributes(typeof(ToolboxBitmapAttribute), false);
                if (attribs != null && attribs.GetLength(0) == 0)
                    attribs = activityType.GetCustomAttributes(typeof(ToolboxBitmapAttribute), true);

                ToolboxBitmapAttribute toolboxBitmapAttribute = (attribs != null && attribs.GetLength(0) > 0) ? attribs[0] as ToolboxBitmapAttribute : null;
                if (toolboxBitmapAttribute != null)
                    toolBoxImage = toolboxBitmapAttribute.GetImage(activityType);
            }

            return toolBoxImage;
        }

        public static string GetToolboxDisplayName(Type activityType)
        {
            if (activityType == null)
                throw new ArgumentNullException("activityType");

            string displayName = activityType.Name;
            object[] toolboxItemAttributes = activityType.GetCustomAttributes(typeof(ToolboxItemAttribute), true);
            if (toolboxItemAttributes != null && toolboxItemAttributes.Length > 0)
            {
                ToolboxItemAttribute toolboxItemAttrib = toolboxItemAttributes[0] as ToolboxItemAttribute;
                if (toolboxItemAttrib != null && toolboxItemAttrib.ToolboxItemType != null)
                {
                    try
                    {
                        ToolboxItem item = Activator.CreateInstance(toolboxItemAttrib.ToolboxItemType, new object[] { activityType }) as ToolboxItem;
                        if (item != null)
                            displayName = item.DisplayName;
                    }
                    catch
                    {
                    }
                }
            }

            if (activityType.Assembly != null && activityType.Assembly.FullName != null)
            {
                if ((activityType.Assembly.FullName.Equals(AssemblyRef.ActivitiesAssemblyRef, StringComparison.OrdinalIgnoreCase) ||
                    activityType.Assembly.FullName.Equals(Assembly.GetExecutingAssembly().FullName, StringComparison.OrdinalIgnoreCase)) &&
                    displayName.EndsWith(ActivitySuffix, StringComparison.Ordinal) &&
                    !displayName.Equals(ActivitySuffix, StringComparison.Ordinal))
                {
                    displayName = displayName.Substring(0, displayName.Length - ActivitySuffix.Length);
                }
            }

            return displayName;
        }
    }
}
