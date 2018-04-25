//------------------------------------------------------------------------------
// <copyright file="StyleConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;

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
    internal class StyleConverter: StringConverter
    {
        protected virtual Object [] GetStyles(Object instance)
        {
            // We do not support anything other than a single styleSheet
            if (!(instance is System.Web.UI.MobileControls.StyleSheet))
            {
                return null;
            }

            StyleSheet _styleSheet = (StyleSheet)instance;
            ICollection styles = _styleSheet.Styles;

            ArrayList _styleArray = new ArrayList();

            foreach (String key in styles)
            {
                System.Web.UI.MobileControls.Style style = (System.Web.UI.MobileControls.Style) _styleSheet[key];
                if (style.Name != null && style.Name.Length > 0)
                {
                    _styleArray.Add(style.Name);
                }
            }

            if (0 == _styleArray.Count)
            {
                // add (None) entry for CurrentStyle == null
                _styleArray.Add(SR.GetString(SR.StyleSheet_PropNotSet));
            }

            _styleArray.Sort();
            return _styleArray.ToArray();
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
