//------------------------------------------------------------------------------
// <copyright file="ObjectListItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls 
{
    /*
     * Object List Item class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListItem.uex' path='docs/doc[@for="ObjectListItem"]/*' />
    [
        ToolboxItem(false)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListItem : MobileListItem
    {
        private String[] _fields;
        private bool _dirty;
        private ObjectList _owner;
            
        internal ObjectListItem(ObjectList owner) : this(owner, null)
        {
        }

        internal ObjectListItem(ObjectList owner, Object dataItem) : base(dataItem, null, null)
        {
            _owner = owner;
            _fields = new String[owner.AllFields.Count];
        }

        private int FieldIndexFromKey(String key)
        {
            int index = _owner.AllFields.IndexOf (key);
            if (index == -1)
            {
                throw new ArgumentException(
                    SR.GetString(SR.ObjectList_FieldNotFound, key));
            }
            return index;
        }

        /// <include file='doc\ObjectListItem.uex' path='docs/doc[@for="ObjectListItem.this"]/*' />
        public String this[String key]
        {
            get
            {
                return this[FieldIndexFromKey (key)];
            }

            set
            {
                this[FieldIndexFromKey (key)] = value;
            }
        }

        /// <include file='doc\ObjectListItem.uex' path='docs/doc[@for="ObjectListItem.this1"]/*' />
        public String this[int index]
        {
            get
            {
                String s = _fields[index];
                return (s != null) ? s : String.Empty;
            }

            set
            {
                _fields[index] = value;
                if (IsTrackingViewState)
                {
                    _dirty = true;
                }
            }
        }

        /// <include file='doc\ObjectListItem.uex' path='docs/doc[@for="ObjectListItem.Equals"]/*' />
        public override bool Equals(Object obj) 
        {
            ObjectListItem other = obj as ObjectListItem;
            
            if (other == null)
            {
                return false;
            }

            if (_fields == null && other._fields == null)
            {
                return true;
            }
            else if (_fields == null || other._fields == null)
            {
                return false;
            }

            if (_fields.Length != other._fields.Length)
            {
                return false;
            }

            for (int i = 0; i < _fields.Length; i++)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            if(!Value.Equals(other.Value) || !Text.Equals(other.Text))
            {
                return false;
            }
            
            return true;
        }

        /// <include file='doc\ObjectListItem.uex' path='docs/doc[@for="ObjectListItem.GetHashCode"]/*' />
        public override int GetHashCode()
        {
            if (_fields.Length > 0)
            {
                return _fields[0].GetHashCode();
            }
            else
            {
                return Value.GetHashCode();
            }
        }

        
        /////////////////////////////////////////////////////////////////////////
        //  STATE MANAGEMENT, FOR ITEM'S DATA (NON-CONTROL) STATE.
        /////////////////////////////////////////////////////////////////////////

        internal override Object SaveDataState()
        {
            Object baseState = base.SaveDataState ();
            if (_dirty && _fields != null)
            {
                int fieldCount = _fields.Length;
                Object[] itemState = new Object[fieldCount + 1];
                itemState[0] = baseState;
                for (int i = 0; i < fieldCount; i++)
                {
                    itemState[i + 1] = _fields[i];
                }
                return itemState;
            }
            else if (baseState != null)
            {
                return new Object[1] { baseState };
            }
            else
            {
                return null;
            }
        }

        internal override void LoadDataState(Object state)
        {
            if (state != null)
            {
                Object[] itemState = (Object[])state;
                int fieldCount = itemState.Length;
                base.LoadDataState (itemState[0]);
                _fields = new String[fieldCount - 1];
                for (int i = 1; i < fieldCount; i++)
                {
                    _fields[i - 1] = (String)itemState[i];
                }
            }
        }

        internal override bool Dirty 
        {
            get 
            { 
                return _dirty || base.Dirty;
            }
            set { 
                _dirty = true;
                base.Dirty = value;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  EVENT BUBBLING
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\ObjectListItem.uex' path='docs/doc[@for="ObjectListItem.OnBubbleEvent"]/*' />
        protected override bool OnBubbleEvent(Object source, EventArgs e) 
        {
            if (e is CommandEventArgs) 
            {
                ObjectListCommandEventArgs args = new ObjectListCommandEventArgs(this, source, (CommandEventArgs)e);
                RaiseBubbleEvent (this, args);
                return true;
            }
            return false;
        }

    } 

}

