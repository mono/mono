// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using XAML3 = System.Windows.Markup;
using System.Xaml;
using System.Xaml.MS.Impl;
using System.Windows.Markup;

namespace MS.Internal.Xaml.Context
{
    [DebuggerDisplay("{ToString()}")]
    internal class ObjectWriterFrame : XamlCommonFrame
    {
        private ObjectWriterFrameFlags _flags;
        private Dictionary<XamlMember, object> _preconstructionPropertyValues;
        private HashSet<XamlMember> _assignedProperties;
        private Object _key;

        public ObjectWriterFrame()
            : base()
        { }


        public ObjectWriterFrame(ObjectWriterFrame source)
            : base(source)
        {
            // Calling the getter will instantiate new Dictionaries. 
            // So we just check the field instead to verify that it isn't
            // being used.
            if (source._preconstructionPropertyValues != null)
            {
                _preconstructionPropertyValues = new Dictionary<XamlMember, object>(source.PreconstructionPropertyValues);
            }
            if (source._assignedProperties != null)
            {
                _assignedProperties = new HashSet<XamlMember>(source.AssignedProperties);
            }
            _key = source._key;
            _flags = source._flags;
            Instance = source.Instance;
            Collection = source.Collection;
            NameScopeDictionary = source.NameScopeDictionary;
            PositionalCtorArgs = source.PositionalCtorArgs;
            InstanceRegisteredName = source.InstanceRegisteredName;
        }

        public override void Reset()
        {
            base.Reset();
            _preconstructionPropertyValues = null;
            _assignedProperties = null;
            Instance = null;
            Collection = null;
            NameScopeDictionary = null;
            PositionalCtorArgs = null;
            InstanceRegisteredName = null;
            _flags = ObjectWriterFrameFlags.None;
            _key = null;
        }

        // Must be able to clone the ObjectWriterFrame as
        // context for Templates
        public override XamlFrame Clone()
        {
            return new ObjectWriterFrame(this);
        }

        public override string ToString()
        {
            string type = (this.XamlType == null) ? String.Empty : this.XamlType.Name;
            string prop = (this.Member == null) ? "-" : this.Member.Name;
            string inst = (Instance == null) ? "-" : ((Instance is String) ? Instance.ToString() : "*");
            string coll = (Collection == null) ? "-" : "*";
            string res = KS.Fmt("{0}.{1} inst={2} coll={3}",
                                 type, prop, inst, coll);
            return res;
        }

        public Object Instance { get; set; }
        public Object Collection { get; set; }
        
        public bool WasAssignedAtCreation
        {
            get { return GetFlag(ObjectWriterFrameFlags.WasAssignedAtCreation); }
            set { SetFlag(ObjectWriterFrameFlags.WasAssignedAtCreation, value); }
        }

        public bool IsObjectFromMember
        {
            get { return GetFlag(ObjectWriterFrameFlags.IsObjectFromMember); }
            set { SetFlag(ObjectWriterFrameFlags.IsObjectFromMember, value); }
        }

        public bool IsPropertyValueSet
        {
            get { return GetFlag(ObjectWriterFrameFlags.IsPropertyValueSet); }
            set { SetFlag(ObjectWriterFrameFlags.IsPropertyValueSet, value); }
        }

        public bool IsKeySet
        {
            get { return GetFlag(ObjectWriterFrameFlags.IsKeySet); }
            private set { SetFlag(ObjectWriterFrameFlags.IsKeySet, value); }
        }

        public bool IsTypeConvertedObject
        {
            get { return GetFlag(ObjectWriterFrameFlags.IsTypeConvertedObject); }
            set { SetFlag(ObjectWriterFrameFlags.IsTypeConvertedObject, value); }
        }

        // The following three flags are interrelated. See XamlObjectWritr.Logic_ShouldConvertKey
        // for more details.
        public bool KeyIsUnconverted
        {
            get { return GetFlag(ObjectWriterFrameFlags.KeyIsUnconverted); }
            set { SetFlag(ObjectWriterFrameFlags.KeyIsUnconverted, value); }
        }

        public bool ShouldConvertChildKeys
        {
            get { return GetFlag(ObjectWriterFrameFlags.ShouldConvertChildKeys); }
            set { SetFlag(ObjectWriterFrameFlags.ShouldConvertChildKeys, value); }
        }

        public bool ShouldNotConvertChildKeys
        {
            get { return GetFlag(ObjectWriterFrameFlags.ShouldNotConvertChildKeys); }
            set { SetFlag(ObjectWriterFrameFlags.ShouldNotConvertChildKeys, value); }
        }

        public INameScopeDictionary NameScopeDictionary { get; set; }
        public object[] PositionalCtorArgs { get; set; }
        public object Key
        {
            get
            {
                // We use a special KeyHolder in some x:Reference scenarios.
                // We need to unwrap this when returning.
                FixupTargetKeyHolder ftkh = _key as FixupTargetKeyHolder;
                if (ftkh != null)
                {
                    return ftkh.Key;
                }
                return _key;
            }
            set
            {
                _key = value;
                IsKeySet = true;
            }
        }

        /// <summary>
        /// The x:Name of this.Instance.
        /// Used to trigger forward ref resolution.
        /// </summary>
        public string InstanceRegisteredName { get; set; }

        /// <summary>
        /// Collection of directives set on this object, because
        /// directives can't be stored on the object.
        /// </summary>
        public Dictionary<XamlMember, object> PreconstructionPropertyValues
        {
            get
            {
                if (_preconstructionPropertyValues == null)
                {
                    _preconstructionPropertyValues = new Dictionary<XamlMember, object>();
                }
                return _preconstructionPropertyValues;
            }
        }

        public bool HasPreconstructionPropertyValuesDictionary
        {
            get { return _preconstructionPropertyValues != null; }
        }

        /// <summary>
        /// All Properties that are set so far.
        /// </summary>
        public HashSet<XamlMember> AssignedProperties
        {
            get
            {
                if (_assignedProperties == null)
                {
                    _assignedProperties = new HashSet<XamlMember>();
                }
                return _assignedProperties;
            }
        }

        private bool GetFlag(ObjectWriterFrameFlags flag)
        {
            return (_flags & flag) != ObjectWriterFrameFlags.None;
        }

        private void SetFlag(ObjectWriterFrameFlags flag, bool value)
        {
            if (value)
            {
                _flags = _flags | flag;
            }
            else
            {
                _flags = _flags & ~flag;
            }
        }

        [Flags]
        private enum ObjectWriterFrameFlags : byte
        {
            None = 0,
            WasAssignedAtCreation = 0x01,
            IsObjectFromMember    = 0x02,
            IsPropertyValueSet    = 0x04,
            IsKeySet              = 0x08,
            IsTypeConvertedObject = 0x10,
            KeyIsUnconverted      = 0x20,
            ShouldConvertChildKeys     = 0x40,
            ShouldNotConvertChildKeys  = 0x80
        }
    }
}
