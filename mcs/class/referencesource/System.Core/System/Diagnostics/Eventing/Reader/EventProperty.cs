// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: EventProperty
**
** Purpose: 
** This public class defines the methods / properties for the 
** individual Data Values of an EventRecord.  Instances of this
** class are obtained from EventRecord.
** 
============================================================*/
using System;

namespace System.Diagnostics.Eventing.Reader {
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class EventProperty {
        private object value;

        internal EventProperty(object value) {
            this.value = value;
        }

        public object Value {
            get {
                return value;
            }
        }
    }
}
