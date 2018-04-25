//------------------------------------------------------------------------------
// <copyright file="StyleReferenceConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Diagnostics;
    using System.Collections;
    using System.Globalization;
    using System.ComponentModel;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Util;

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
    internal class StyleReferenceConverter: StringConverter
    {
        protected virtual Object [] GetStyles(Object instance)
        {
            StyleSheet styleSheet = null;
            Style instanceStyle = null;

            // Remember, ChoicePropertyFilter is a MobileControl, so we must
            // check for ChoicePropertyFilter first...
            if (instance is IDeviceSpecificChoiceDesigner)
            {
                instance = ((IDeviceSpecificChoiceDesigner)instance).UnderlyingObject;
            }
            
            if (instance is System.Web.UI.MobileControls.Style)
            {
                instanceStyle = (Style) instance;
                if (instanceStyle.Control is StyleSheet)
                {
                    styleSheet = (StyleSheet) instanceStyle.Control;
                }
                else if ((instanceStyle.Control is Form && instanceStyle is PagerStyle) ||
                    (instanceStyle.Control is ObjectList))
                {
                    if (instanceStyle.Control.MobilePage != null)
                    {
                        styleSheet = instanceStyle.Control.MobilePage.StyleSheet;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.Fail("Unsupported objects passed in");
                }
            }
            else if (instance is System.Web.UI.MobileControls.MobileControl)
            {
                MobileControl control = (MobileControl)instance;
                if (control.MobilePage == null)
                {
                    return null;
                }
                styleSheet = control.MobilePage.StyleSheet;
            }
            else if (instance is Array)
            {
                Array array = (Array)instance;
                Debug.Assert(array.Length > 0);

                return GetStyles(array.GetValue(0));
            }
            else
            {
                Debug.Fail("Unsupported type passed in");
                return null;
            }
            Debug.Assert(null != styleSheet);

            ICollection styles = styleSheet.Styles;
            ArrayList styleArray = new ArrayList();
            foreach (String key in styles)
            {
                System.Web.UI.MobileControls.Style style = styleSheet[key];
                if (style.Name != null && style.Name.Length > 0)
                {
                    if (null == instanceStyle || 0 != String.Compare(instanceStyle.Name, style.Name, StringComparison.Ordinal))
                    {
                        styleArray.Add(style.Name);
                    }
                }
            }

            if (styleSheet == StyleSheet.Default)
            {
                styleArray.Sort();
                return styleArray.ToArray();
            }

            styles = StyleSheet.Default.Styles;
            foreach (String key in styles)
            {
                System.Web.UI.MobileControls.Style style = StyleSheet.Default[key];
                if (style.Name != null && style.Name.Length > 0)
                {
                    if (null == instanceStyle || 0 != String.Compare(instanceStyle.Name, style.Name, StringComparison.Ordinal))
                    {
                        styleArray.Add(style.Name);
                    }
                }
            }

            if (styleArray.Count <= 1)
            {
                return styleArray.ToArray();
            }

            styleArray.Sort();
            String preID = ((String)styleArray[0]).ToLower(CultureInfo.InvariantCulture);

            int i = 1;
            while (i < styleArray.Count)
            {
                if (String.Equals((String)styleArray[i], preID, StringComparison.OrdinalIgnoreCase)) {
                    styleArray.RemoveAt(i);
                }
                else
                {
                    preID = ((String)styleArray[i]).ToLower(CultureInfo.InvariantCulture);
                    i++;
                }
            }

            return styleArray.ToArray();
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

            Object [] objValues = GetStyles(context.Instance);
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
    }    
}
