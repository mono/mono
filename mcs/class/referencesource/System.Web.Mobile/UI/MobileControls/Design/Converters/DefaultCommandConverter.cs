//------------------------------------------------------------------------------
// <copyright file="DefaultCommandConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
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
    internal class DefaultCommandConverter : StringConverter
    {
        private Object[] GetCommands(ObjectList objectList)
        {
            ObjectListCommandCollection commands = objectList.Commands;
            if (commands.Count == 0)
            {
                return null;
            }

            ArrayList commandList = new ArrayList(commands.Count);
            foreach(ObjectListCommand command in commands)
            {
                commandList.Add(command.Name);
            }

            commandList.Sort();
            return commandList.ToArray();
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

            ObjectList objectList = null;
            if (context.Instance is IDeviceSpecificChoiceDesigner)
            {
                objectList = ((IDeviceSpecificChoiceDesigner)context.Instance).UnderlyingObject as ObjectList;
            }
            else if (context.Instance is ObjectList)
            {
                objectList = (ObjectList)context.Instance;
            }
            else
            {
                return null;
            }
            
            Debug.Assert(objectList != null);

            Object [] objValues = GetCommands(objectList);
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
            if (context.Instance is IComponent) 
            {
                // We only support the dropdown in single-select mode.
                return true;
            }
            return false;
        }        
    }    
}
