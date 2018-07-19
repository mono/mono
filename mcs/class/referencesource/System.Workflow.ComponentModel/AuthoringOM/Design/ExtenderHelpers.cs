#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //

    #region Class ExtenderHelpers
    internal static class ExtenderHelpers
    {
        internal static void FilterDependencyProperties(IServiceProvider serviceProvider, Activity activity)
        {
            IExtenderListService extenderListService = serviceProvider.GetService(typeof(IExtenderListService)) as IExtenderListService;
            if (extenderListService != null)
            {
                Dictionary<string, DependencyProperty> dependencyProperyies = new Dictionary<string, DependencyProperty>();
                foreach (DependencyProperty property in activity.MetaDependencyProperties)
                    dependencyProperyies.Add(property.Name, property);

                List<string> disallowedProperties = new List<string>();
                foreach (IExtenderProvider extenderProvider in extenderListService.GetExtenderProviders())
                {
                    if (!extenderProvider.CanExtend(activity))
                    {
                        ProvidePropertyAttribute[] propertyAttributes = extenderProvider.GetType().GetCustomAttributes(typeof(ProvidePropertyAttribute), true) as ProvidePropertyAttribute[];
                        foreach (ProvidePropertyAttribute propertyAttribute in propertyAttributes)
                            disallowedProperties.Add(propertyAttribute.PropertyName);
                    }
                }

                foreach (string propertyName in disallowedProperties)
                {
                    if (dependencyProperyies.ContainsKey(propertyName))
                        activity.RemoveProperty(dependencyProperyies[propertyName]);
                }
            }
        }
    }
    #endregion
}
