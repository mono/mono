//------------------------------------------------------------------------------
// <copyright file="ChoiceConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;

    /// <summary>
    ///    <para>
    ///       Can filter and retrieve several types of values from controls.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ChoiceConverter: StringConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        {
            if (context != null && context.Instance is Array)
            {
                return value;
            }

            if (value is String) 
            {
                return MatchFilterName((string)value, context);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// </summary>
        protected virtual Object [] GetChoices(Object instance)
        {
            bool defaultAdded = false;
            DeviceSpecific deviceSpecific;

            if (instance is System.Web.UI.MobileControls.StyleSheet)
            {
                StyleSheet ss = (StyleSheet) instance;
                ISite componentSite = ss.Site;
                Debug.Assert(componentSite != null, "Expected the component to be sited.");
                IDesignerHost designerHost = (IDesignerHost) componentSite.GetService(typeof(IDesignerHost));
                Debug.Assert(designerHost != null, "Expected a designer host.");
                Object designer = designerHost.GetDesigner(ss);
                Debug.Assert(designer != null, "Expected a designer for the stylesheet.");
                Debug.Assert(designer is StyleSheetDesigner, "Expected a StyleSheet designer.");
                StyleSheetDesigner ssd = (StyleSheetDesigner) designer;
                Style style = (Style) ssd.CurrentStyle;
                if (null != style)
                {
                    deviceSpecific = style.DeviceSpecific;
                }
                else
                {
                    deviceSpecific = null;
                }
            }
            else if (instance is System.Web.UI.MobileControls.DeviceSpecific)
            {
                deviceSpecific = (DeviceSpecific) instance;
            }
            else if (instance is MobileControl)
            {
                MobileControl mc = (MobileControl) instance;
                deviceSpecific = mc.DeviceSpecific;
            }
            else
            {
                // Simply return null if the instance is not recognizable.
                return null;
            }

            ArrayList returnArray = new ArrayList();

            // entry that corresponds to null CurrentChoice.
            returnArray.Add(SR.GetString(SR.DeviceFilter_NoChoice));

            if (null == deviceSpecific)
            {
                return returnArray.ToArray();
            }

            Debug.Assert(deviceSpecific.Choices != null);
            foreach(DeviceSpecificChoice choice in deviceSpecific.Choices)
            {
                // Choice must have a Name
                if (choice.Filter != null && choice.Filter.Length == 0)
                {
                    if (!defaultAdded)
                    {
                        returnArray.Add(SR.GetString(SR.DeviceFilter_DefaultChoice));
                        defaultAdded = true;
                    }
                }
                else
                {
                    if (!choice.Filter.Equals(SR.GetString(SR.DeviceFilter_NoChoice)))
                    {
                        returnArray.Add(DesignerUtility.ChoiceToUniqueIdentifier(choice));
                    }
                }
            }
            returnArray.Sort();
            return returnArray.ToArray();
        }

        /// <summary>
        ///    <para>
        ///       Returns a collection of standard values retrieved from the context specified
        ///       by the specified type descriptor.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    A type descriptor that specifies the location of the context to convert from.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A StandardValuesCollection that represents the standard values collected from
        ///       the specified context.
        ///    </para>
        /// </returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null || context.Instance == null)
            {
                return null;
            }

            Object [] objValues = GetChoices(context.Instance);
            if (objValues != null)
            {
                return new StandardValuesCollection(objValues);
            }
            else
            {
                return null;
            }            
        }

        /// <summary>
        ///    <para>
        ///       Gets whether
        ///       or not the context specified contains exclusive standard values.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    A type descriptor that indicates the context to convert from.
        /// </param>
        /// <returns>
        ///    <para>
        ///    <see langword='true'/> if the specified context contains exclusive standard 
        ///       values, otherwise <see langword='false'/>.
        ///    </para>
        /// </returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        /// <summary>
        ///    <para>
        ///       Gets whether or not the specified context contains supported standard
        ///       values.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    A type descriptor that indicates the context to convert from.
        /// </param>
        /// <returns>
        ///    <para>
        ///    <see langword='true'/> if the specified context conatins supported standard 
        ///       values, otherwise <see langword='false'/>.
        ///    </para>
        /// </returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private String MatchFilterName(String name, ITypeDescriptorContext context) 
        {
            Debug.Assert(name != null, "Expected an actual device filter name to match.");

            // Try a partial match
            //
            String bestMatch = null;

            StandardValuesCollection standardValues = GetStandardValues(context);
            if (standardValues == null)
            {
                return null;
            }

            IEnumerator e = standardValues.GetEnumerator();
            while (e.MoveNext()) 
            {
                string filterName = e.Current.ToString();
                if (String.Equals(filterName, name, StringComparison.OrdinalIgnoreCase)) {
                    // For an exact match, return immediately
                    //
                    return filterName;
                }
                else if (e.Current.ToString().StartsWith(name, StringComparison.OrdinalIgnoreCase)) 
                {
                    if (bestMatch == null || filterName.Length <= bestMatch.Length) 
                    {
                        bestMatch = filterName;
                    }
                }
            }
                
            if (bestMatch == null) 
            {
                // no match... use NoChoice
                bestMatch = SR.GetString(SR.DeviceFilter_NoChoice);
            }
            return bestMatch;
        }
    }    
}
