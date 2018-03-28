//------------------------------------------------------------------------------
// <copyright file="listitem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using AttributeCollection = System.Web.UI.AttributeCollection;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Interacts with the parser to build a <see cref='System.Web.UI.WebControls.ListItem'/> control.</para>
    /// </devdoc>
    public class ListItemControlBuilder : ControlBuilder {


        public override bool AllowWhitespaceLiterals() {
            return false;
        }


        public override bool HtmlDecodeLiterals() {
            // ListItem text gets rendered as an encoded attribute value.

            // At parse time text specified as an attribute gets decoded, and so text specified as a
            // literal needs to go through the same process.

            return true;
        }
    }


    /// <devdoc>
    ///    <para>Constructs a list item control and defines
    ///       its properties. This class cannot be inherited.</para>
    /// </devdoc>
    [
    ControlBuilderAttribute(typeof(ListItemControlBuilder)),
    TypeConverterAttribute(typeof(ExpandableObjectConverter)),
    ParseChildren(true, "Text"),
    ]
    public sealed class ListItem : IStateManager, IParserAccessor, IAttributeAccessor {

        private bool selected;
        private bool marked;
        private bool textisdirty;
        private bool valueisdirty;
        private bool enabled;
        private bool enabledisdirty;

        private string text;
        private string value;
        private AttributeCollection _attributes;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ListItem'/> class.</para>
        /// </devdoc>
        public ListItem() : this(null, null) {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ListItem'/> class with the specified text data.</para>
        /// </devdoc>
        public ListItem(string text) : this(text, null) {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ListItem'/> class with the specified text data.</para>
        /// </devdoc>
        public ListItem(string text, string value) : this(text, value, true)  {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ListItem'/> class with the
        ///    specified text and value data.</para>
        /// </devdoc>
        public ListItem(string text, string value, bool enabled) {
            this.text = text;
            this.value = value;
            this.enabled = enabled;
        }


        /// <devdoc>
        ///    <para>Gets the collection of attribute name/value pairs expressed on the list item
        ///       control but not supported by the control's strongly typed properties.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public AttributeCollection Attributes {
            get {
                if (_attributes == null)
                    _attributes = new AttributeCollection(new StateBag(true));

                return _attributes;
            }
        }


        /// <devdoc>
        ///  Internal property used to manage dirty state of ListItem.
        /// </devdoc>
        internal bool Dirty {
            get {
                return (textisdirty || valueisdirty || enabledisdirty);
            }
            set {
                textisdirty = value;
                valueisdirty = value;
                enabledisdirty = value;
            }
        }


        [
        DefaultValue(true)
        ]
        public bool Enabled  {
            get  {
                return enabled;
            }
            set  {
                enabled = value;
                if (((IStateManager)this).IsTrackingViewState)
                    enabledisdirty = true;
            }
        }

        internal bool HasAttributes {
            get {
                return _attributes != null && _attributes.Count > 0;
            }
        }


        /// <devdoc>
        ///    <para>Specifies a value indicating whether the
        ///       item is selected.</para>
        /// </devdoc>
        [
        DefaultValue(false),
        TypeConverter(typeof(MinimizableAttributeTypeConverter))
        ]
        public bool Selected {
            get {
                return selected;
            }
            set {
                selected = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the display text of the list
        ///       item control.</para>
        /// </devdoc>
        [
        Localizable(true),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty)
        ]
        public string Text {
            get {
                if (text != null)
                    return text;
                if (value != null)
                    return value;
                return String.Empty;
            }
            set {
                text = value;
                if (((IStateManager)this).IsTrackingViewState)
                    textisdirty = true;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the value content of the list item control.</para>
        /// </devdoc>
        [
        Localizable(true),
        DefaultValue("")
        ]
        public string Value {
            get {
                if (value != null)
                    return value;
                if (text != null)
                    return text;
                return String.Empty;
            }
            set {
                this.value = value;
                if (((IStateManager)this).IsTrackingViewState)
                    valueisdirty =true;
            }
        }


        /// <internalonly/>
        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(Value.GetHashCode(), Text.GetHashCode());
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object o) {
            ListItem other = o as ListItem;

            if (other != null) {
                return Value.Equals(other.Value) && Text.Equals(other.Text);
            }
            return false;
        }


        /// <devdoc>
        /// <para>Creates a <see cref='System.Web.UI.WebControls.ListItem'/> from the specified string.</para>
        /// </devdoc>
        public static ListItem FromString(string s) {
            return new ListItem(s);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override string ToString() {
            return this.Text;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Return true if tracking state changes.
        /// Method of private interface, IStateManager.
        /// </devdoc>
        bool IStateManager.IsTrackingViewState {
            get {
                return marked;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        internal void LoadViewState(object state) {
            if (state != null) {
                if (state is Triplet)  {
                    Triplet t = (Triplet) state;
                    if (t.First != null) {
                        Text = (string) t.First;
                    }
                    if (t.Second != null)  {
                        Value = (string) t.Second;
                    }
                    if (t.Third != null)  {
                        try  {
                            Enabled = (bool) t.Third;
                        }
                        catch {
                        }
                    }
                }
                else if (state is Pair) {
                    Pair p = (Pair) state;
                    if (p.First != null)
                        Text = (string) p.First;
                    Value = (string) p.Second;
                }
                else
                    Text = (string) state;
            }
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        internal void TrackViewState() {
            marked = true;
        }

        internal void RenderAttributes(HtmlTextWriter writer) {
            if (_attributes != null) {
                _attributes.AddAttributes(writer);
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        internal object SaveViewState() {
            string text = null;
            string value = null;

            if(textisdirty)  {
                text = Text;
            }
            if(valueisdirty)  {
                value = Value;
            }
            if(enabledisdirty)  {
                return new Triplet(text, value, Enabled);
            }
            else if(valueisdirty)  {
                return new Pair(text, value);
            }
            else if(textisdirty)  {
                return text;
            }

            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Returns the attribute value of the list item control
        /// having the specified attribute name.
        /// </devdoc>
        string IAttributeAccessor.GetAttribute(string name) {
            return Attributes[name];
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Sets an attribute of the list
        /// item control with the specified name and value.</para>
        /// </devdoc>
        void IAttributeAccessor.SetAttribute(string name, string value) {
            Attributes[name] = value;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para> Allows the <see cref='System.Web.UI.WebControls.ListItem.Text'/>
        /// property to be persisted as inner content.</para>
        /// </devdoc>
        void IParserAccessor.AddParsedSubObject(object obj) {
            if (obj is LiteralControl) {
                Text = ((LiteralControl)obj).Text;
            }
            else {
                if (obj is DataBoundLiteralControl) {
                    throw new HttpException(SR.GetString(SR.Control_Cannot_Databind, "ListItem"));
                }
                else {
                    throw new HttpException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "ListItem", obj.GetType().Name.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }


        /// <devdoc>
        /// Do not change the signature or remove this method. It is called via reflection
        /// to support correct serialization behavior for Text and Value which are
        /// implemented as dependent properties.
        /// </devdoc>
        private void ResetText() {
            Text = null;
        }


        /// <devdoc>
        /// Do not change the signature or remove this method. It is called via reflection
        /// to support correct serialization behavior for Text and Value which are
        /// implemented as dependent properties.
        /// </devdoc>
        private void ResetValue() {
            Value = null;
        }


        /// <devdoc>
        /// Do not change the signature or remove this method. It is called via reflection
        /// to support correct serialization behavior for Text and Value which are
        /// implemented as dependent properties.
        /// </devdoc>
        private bool ShouldSerializeText() {
            return (text != null) && (text.Length != 0);
        }


        /// <devdoc>
        /// Do not change the signature or remove this method. It is called via reflection
        /// to support correct serialization behavior for Text and Value which are
        /// implemented as dependent properties.
        /// </devdoc>
        private bool ShouldSerializeValue() {
            return (value != null) && (value.Length != 0);
        }
    }
}
