//------------------------------------------------------------------------------
// <copyright file="DoubleLink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * DoubleLink
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Util {
    using System.Runtime.Serialization.Formatters;

    internal class DoubleLink {
        internal DoubleLink    _next, _prev;
        internal Object           Item;

        internal DoubleLink() {
            _next = _prev = this;
        }
        internal DoubleLink(Object item)  : this() {
            this.Item = item;
        }
        internal DoubleLink Next {get {return _next;}}

        internal void InsertAfter(DoubleLink after) {
            this._prev = after;
            this._next = after._next;
            after._next = this;
            this._next._prev = this;
        }

        internal void InsertBefore(DoubleLink before) {
            this._prev = before._prev;
            this._next = before;
            before._prev = this;
            this._prev._next = this;
        }

        internal void Remove() {
            this._prev._next = this._next;
            this._next._prev = this._prev;
            _next = _prev = this;
        }

#if DBG
        internal virtual void DebugValidate() {
            Debug.CheckValid(this._next != this || this._prev == this, "Invalid link");
        }

        internal virtual string DebugDescription(string indent) {
            string desc;

            desc = indent + "_next=" + _next + ", _prev=" + _prev + "\nItem=";
            desc += Debug.GetDescription(Item, indent + "    ");

            return desc;
        }
#endif
    }

}
