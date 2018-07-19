//------------------------------------------------------------------------------
// <copyright file="DeviceSpecificChoiceCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Collection of DeviceSpecificChoice objects.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DeviceSpecificChoiceCollection : ArrayListCollectionBase
    {
        DeviceSpecific _owner;

        internal DeviceSpecificChoiceCollection(DeviceSpecific owner)
        {
            _owner = owner;
        }

        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.this"]/*' />
        public DeviceSpecificChoice this[int index]
        {
            get
            {
                return (DeviceSpecificChoice)Items[index];
            }
        }

        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.Add"]/*' />
        public void Add(DeviceSpecificChoice choice)
        {
            AddAt(-1, choice);
        }

        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.AddAt"]/*' />
        public void AddAt(int index, DeviceSpecificChoice choice)
        {
            choice.Owner = _owner;
            if (index == -1)
            {
                Items.Add(choice);
            }
            else
            {
                Items.Insert(index, choice);
            }
        }

        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.Clear"]/*' />
        public void Clear()
        {
            Items.Clear();
        }

        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.RemoveAt"]/*' />
        public void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Items.RemoveAt(index);
            }
        }
        
        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.Remove"]/*' />
        public void Remove(DeviceSpecificChoice choice)
        {
            int index = Items.IndexOf(choice, 0, Count);
            if (index != -1)
            {
                Items.RemoveAt(index);
            }
        }

        ///////////////////////////////////////////////////////////
        ///  DESIGNER PROPERTY
        ///////////////////////////////////////////////////////////
        /// <include file='doc\DeviceSpecificChoiceCollection.uex' path='docs/doc[@for="DeviceSpecificChoiceCollection.All"]/*' />
        [
            Browsable(false),
            PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public ArrayList All
        {
            get 
            {
                return base.Items;
            }
        }
    }
}
