//------------------------------------------------------------------------------
// <copyright file="MobileListItem.cs" company="Microsoft">
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
     * List Item type - enumeration.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItemType"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum MobileListItemType
    {
        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItemType.HeaderItem"]/*' />
        HeaderItem,
        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItemType.ListItem"]/*' />
        ListItem,
        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItemType.FooterItem"]/*' />
        FooterItem,
        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItemType.SeparatorItem"]/*' />
        SeparatorItem
    }

    /*
     * Mobile List Item class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem"]/*' />
    [
        PersistName("Item"),
        ToolboxItem(false)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileListItem : TemplateContainer, IStateManager
    {
        private const int SELECTED = 0;
        private const int MARKED = 1;
        private const int TEXTISDIRTY = 2;
        private const int VALUEISDIRTY = 3;
        private const int SELECTIONISDIRTY = 4;

        private int _index;
        private MobileListItemType _itemType;
        private Object _dataItem;
        private String _text;
        private String _value;
        private BitArray _flags;

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.MobileListItem"]/*' />
        public MobileListItem() : this(null, null, null) 
        {
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.MobileListItem1"]/*' />
        public MobileListItem(String text) : this(null, text, null) 
        {
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.MobileListItem2"]/*' />
        public MobileListItem(String text, String value) : this(null, text, value)
        {
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.MobileListItem3"]/*' />
        public MobileListItem(MobileListItemType itemType) : this (null, null, null)
        {
            _itemType = itemType;
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.MobileListItem4"]/*' />
        public MobileListItem(Object dataItem, String text, String value)
        {
            _index = -1;
            _dataItem = dataItem;
            _text = text;
            _value = value;
            _flags = new BitArray(5);
            _itemType = MobileListItemType.ListItem;
        }

        internal MobileListItemType ItemType
        {
            get
            {
                return _itemType;
            }
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.Index"]/*' />
        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int Index
        {
            get
            {
                return _index;
            }
        }

        internal void SetIndex(int value)
        {
            _index = value;
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.operatorMobileListItem"]/*' />
        public static implicit operator MobileListItem(String s) 
        {
            return new MobileListItem(s);
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.Text"]/*' />
        [
            DefaultValue("")
        ]        
        public String Text 
        {
            get 
            {
                String s;
                if (_text != null)
                {
                    s = _text;
                }
                else if (_value != null)
                {
                    s = _value;
                }
                else
                {
                    s = String.Empty;
                }
                return s;
            }

            set 
            {
                _text = value;
                if (((IStateManager)this).IsTrackingViewState)
                {
                    _flags.Set (TEXTISDIRTY,true);
                }
            }
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.Value"]/*' />
        [
            DefaultValue("")
        ]
        public String Value 
        {
            get 
            {
                String s;
                if (_value != null)
                {
                    s = _value;
                }
                else if (_text != null)
                {
                    s = _text;
                }
                else
                {
                    s = String.Empty;
                }
                return s;
            }

            set 
            {
                _value = value;
                if (_flags.Get (MARKED))
                {
                    _flags.Set (VALUEISDIRTY,true);
                }
            }
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.DataItem"]/*' />
        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Object DataItem
        {
            get
            {
                return _dataItem;
            }
            set
            {
                _dataItem = value;
            }
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.Equals"]/*' />
        public override bool Equals(Object o) 
        {
            MobileListItem other = o as MobileListItem;
            if (other != null) 
            {
                return Value.Equals(other.Value) && Text.Equals(other.Text);
            }
            return false;
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.GetHashCode"]/*' />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.FromString"]/*' />
        public static MobileListItem FromString(String s) 
        {
            return new MobileListItem(s);
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.ToString"]/*' />
        public override String ToString() 
        {
            return Text;
        }

        /////////////////////////////////////////////////////////////////////////
        //  STATE MANAGEMENT, FOR ITEM'S DATA (NON-CONTROL) STATE.
        /////////////////////////////////////////////////////////////////////////

        internal virtual Object SaveDataState()
        {
            String sa0 = _flags.Get(TEXTISDIRTY) ? _text : null;
            String sa1 = _flags.Get(VALUEISDIRTY) ? _value : null;

            if (sa0 == null && sa1 == null)
            {
                return null;
            }
            else
            {
                return new String[2] { sa0, sa1 };
            }
        }

        internal virtual void LoadDataState(Object state)
        {
            if (state != null) 
            {
                String[] sa = (String[])state;
                if (sa[0] != null)
                {
                    Text = sa[0];
                }
                if (sa[1] != null)
                {
                    Value = sa[1];
                }
            }
        }

        /// <internalonly/>
        protected new bool IsTrackingViewState
        {   
            get
            {
                return _flags.Get (MARKED);
            }
        }

        /// <internalonly/>
        protected new void LoadViewState(Object state) 
        {
            LoadDataState (state);
        }

        /// <internalonly/>
        protected new void TrackViewState() 
        {
            _flags.Set (MARKED, true);
        }

        /// <internalonly/>
        protected new Object SaveViewState() 
        {
            return SaveDataState ();
        }

        internal virtual bool Dirty 
        {
            get 
            { 
                return (_flags.Get(TEXTISDIRTY) || _flags.Get(VALUEISDIRTY)); 
            }
            set 
            { 
                _flags.Set (TEXTISDIRTY, value); 
                _flags.Set (VALUEISDIRTY, value); 
            }
        }

        internal bool SelectionDirty 
        {
            get 
            { 
                return _flags.Get(SELECTIONISDIRTY);
            }
            set 
            { 
                _flags.Set (SELECTIONISDIRTY, value); 
            }
        }

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="ListItem.Selected"]/*' />
        /// <devdoc>
        ///    <para>Specifies a value indicating whether the
        ///       item is selected.</para>
        /// </devdoc>
        [
            DefaultValue(false)
        ]
        public bool Selected {
            get { 
                return _flags.Get(SELECTED); 
            }
            set { 
                _flags.Set(SELECTED,value); 
                if (((IStateManager)this).IsTrackingViewState)
                {
                    SelectionDirty = true;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  EVENT BUBBLING
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileListItem.uex' path='docs/doc[@for="MobileListItem.OnBubbleEvent"]/*' />
        protected override bool OnBubbleEvent(Object source, EventArgs e) 
        {
            if (e is CommandEventArgs) 
            {
                ListCommandEventArgs args = new ListCommandEventArgs(this, source, (CommandEventArgs)e);
                RaiseBubbleEvent (this, args);
                return true;
            }
            return false;
        }

        #region Implementation of IStateManager
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }
        #endregion
    } 
}
