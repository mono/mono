//
// System.Web.UI.StateItem.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;

namespace System.Web.UI
{
        public sealed class StateItem
        {
                private bool _isDirty = false;
                private object _value = null;
                public bool IsDirty
                {
                        get
                        {
                                return _isDirty;
                        }
                        set
                        {
                                _isDirty = value;
                        }
                }
                public object Value
                {
                        get
                        {
                                return _value;
                        }
                        set
                        {
                                _value = value;
                        }
                }
                public StateItem() {}
                public StateItem(Object value)
                {
                        _value = value;
                }
        }
}
