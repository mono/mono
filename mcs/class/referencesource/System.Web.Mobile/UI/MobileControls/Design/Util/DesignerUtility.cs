//------------------------------------------------------------------------------
// <copyright file="DesignerUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Reflection;
    using System.Web.UI.MobileControls;
    using System.Globalization;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerUtility
    {
        /* Removed for DCR 4240
        internal static bool IsValidName(String name)
        {
            if (name == null || name.Length == 0)
            {
                return false;
            }

            for (int pos = 0; pos < name.Length; pos++)
            {
                Char ch = name[pos];
                if (Char.IsWhiteSpace(ch) || ch.Equals('"') || ch.Equals('<') || 
                    ch.Equals('>') || ch.Equals('\'') || ch.Equals('&'))
                {
                    return false;
                }
            }
            return true;
        }
        */

        internal static bool TopLevelControl(MobileControl control)
        {
            if (control is Form || control is StyleSheet)
            {
                return true;
            }
            return false;
        }

        internal static String ChoiceToUniqueIdentifier(DeviceSpecificChoice choice)
        {
            Debug.Assert(choice != null);
            return  ChoiceToUniqueIdentifier(choice.Filter, choice.Argument);
        }

        internal static String ChoiceToUniqueIdentifier(String filter, String argument)
        {
            if (String.IsNullOrEmpty(filter))
            {
                filter = SR.GetString(SR.DeviceFilter_DefaultChoice);
            }
            if (argument == null)
            {
                argument = String.Empty;
            }
            return String.Format(CultureInfo.InvariantCulture, "{0} (\"{1}\")", filter, argument);
        }
        
        // NOTE: Throws CheckoutException.Canceled if user cancels checkout.
        internal static void EnsureFileCheckedOut(ISite site, String fileName)
        {
            Type serviceType = Type.GetType("Microsoft.VisualStudio.Shell.VsCheckoutService, " + AssemblyRef.MicrosoftVisualStudio);

            // Do nothing if service cannot be found.
            if (serviceType == null) {
//                Debug.Fail("type Microsoft.VisualStudio.Shell.VsCheckoutService cannot be found in DesignerUtility.EnsureFileCheckedOut.");
                return;
            }

            Object serviceInstance = System.Activator.CreateInstance(
                serviceType, new Object[] { site });

            try
            {
                Object[] args = new Object[] { fileName };
                bool fileNeedsCheckout = (bool) serviceType.InvokeMember(
                    "DoesFileNeedCheckout",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    serviceInstance,
                    args, CultureInfo.InvariantCulture
                );
                if(fileNeedsCheckout)
                {
                    serviceType.InvokeMember(
                        "CheckoutFile",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        serviceInstance,
                        args, CultureInfo.InvariantCulture
                    );                
                }
            }
            finally
            {
                serviceType.InvokeMember(
                    "Dispose",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    serviceInstance,
                    new Object[] {}, CultureInfo.InvariantCulture
                );                
                serviceInstance = null;
            }
        }

        // Ideally we would refactor the rest of the common code in the ADF/PO/DFE
        // dialogs into a separate manager object.
        internal static StringCollection GetDuplicateChoiceTreeNodes(ICollection treeNodes)
        {
            HybridDictionary visitedChoices = new HybridDictionary();
            StringCollection duplicateChoices = new StringCollection();
            
            foreach(ChoiceTreeNode node in treeNodes)
            {
                String key = ChoiceToUniqueIdentifier(node.Name, node.Argument);
                if(visitedChoices[key] == null)
                {
                    visitedChoices[key] = 1;
                }
                else
                {
                    if((int)visitedChoices[key] == 1)
                    {
                        duplicateChoices.Add(key);

                    }
                    visitedChoices[key] = ((int)visitedChoices[key]) + 1;
                }
            }
            return duplicateChoices;
        }        
    }
}
