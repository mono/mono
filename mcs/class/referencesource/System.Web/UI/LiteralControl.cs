//------------------------------------------------------------------------------
// <copyright file="LiteralControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Control that holds a literal string
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.IO;

    /// <devdoc>
    /// <para>Defines the properties and methods of the LiteralControl class. A 
    ///    literal control is usually rendered as HTML text on a page. </para>
    /// <para>
    ///    LiteralControls behave as text holders, i.e., the parent of a LiteralControl may decide
    ///    to extract its text, and remove the control from its Control collection (typically for
    ///    performance reasons).
    ///    Therefore a control derived from LiteralControl must do any preprocessing of its Text
    ///    when it hands it out, that it would otherwise have done in its Render implementation.
    /// </para>
    /// </devdoc>
    [
    ToolboxItem(false)
    ]
    public class LiteralControl : Control, ITextControl {
        internal string _text;


        /// <devdoc>
        ///    <para>Creates a control that holds a literal string.</para>
        /// </devdoc>
        public LiteralControl() {
            PreventAutoID();
            SetEnableViewStateInternal(false);
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the LiteralControl class with
        ///    the specified text.</para>
        /// </devdoc>
        public LiteralControl(string text) : this() {
            _text = (text != null) ? text : String.Empty;
        }


        /// <devdoc>
        ///    <para>Gets or sets the text content of the literal control.</para>
        /// </devdoc>
        public virtual string Text {
            get {
                return _text;
            }
            set {
                _text = (value != null) ? value : String.Empty;
            }
        }


        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }


        /// <devdoc>
        ///    <para>Saves any state that was modified after mark.</para>
        /// </devdoc>
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected internal override void Render(HtmlTextWriter output) {
            output.Write(_text);
        }

        internal override void InitRecursive(Control namingContainer) {
            ResolveAdapter();
            if (AdapterInternal != null) {
                AdapterInternal.OnInit(EventArgs.Empty);
            }
            else {
                OnInit(EventArgs.Empty);
            }

        }

        internal override void LoadRecursive() {
            if (AdapterInternal != null) {
                AdapterInternal.OnLoad(EventArgs.Empty);
            }
            else {
                OnLoad(EventArgs.Empty);
            }
        }

        internal override void PreRenderRecursiveInternal() {
            if (AdapterInternal != null) {
                AdapterInternal.OnPreRender(EventArgs.Empty);
            }
            else {
                OnPreRender(EventArgs.Empty);
            }
        }

        internal override void UnloadRecursive(bool dispose) {
            if (AdapterInternal != null) {
                AdapterInternal.OnUnload(EventArgs.Empty);
            }
            else {
                OnUnload(EventArgs.Empty);
            }

            // 
            if (dispose)
                Dispose();
        }


    }


    /*
     * Class used to access literal strings stored in a resource (perf optimization).
     * This class is only public because it needs to be used by the generated classes.
     * Users should not use directly.
     */
    internal sealed class ResourceBasedLiteralControl : LiteralControl {
        private TemplateControl _tplControl;
        private int _offset;    // Offset of the start of this string in the resource
        private int _size;      // Size of this string in bytes
        private bool _fAsciiOnly;    // Does the string contain only 7-bit ascii characters

        internal ResourceBasedLiteralControl(TemplateControl tplControl, int offset, int size, bool fAsciiOnly) {

            // Make sure we don't access invalid data
            if (offset < 0 || offset+size > tplControl.MaxResourceOffset)
                throw new ArgumentException();

            _tplControl = tplControl;
            _offset = offset;
            _size = size;
            _fAsciiOnly = fAsciiOnly;

            PreventAutoID();
            EnableViewState = false;
        }

        public override string Text {
            get {
                // If it's just a normal string, call the base
                if (_size == 0)
                    return base.Text;
                    
                return StringResourceManager.ResourceToString(
                    _tplControl.StringResourcePointer, _offset, _size);
            }
            set {
                // From now on, this will behave like a normal LiteralControl
                _size = 0;
                base.Text = value;
            }
        }

        protected internal override void Render(HtmlTextWriter output) {

            // If it's just a normal string, call the base
            if (_size == 0) {
                base.Render(output);
                return;
            }

            output.WriteUTF8ResourceString(_tplControl.StringResourcePointer, _offset, _size, _fAsciiOnly);
        }
    }
}
