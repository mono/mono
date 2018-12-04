// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

namespace System.Xaml
{
    // list of the different kinds of "built-in" type functionality
    // recognized by the XAML language system.
    [Flags]
    internal enum BoolTypeBits
    {
        Constructible                       = 0x0001,
        XmlData                             = 0x0002,
        MarkupExtension                     = 0x0004,
        Nullable                            = 0x0008,
        NameScope                           = 0x0010,
        ConstructionRequiresArguments       = 0x0020,
        Public                              = 0x0040,
        Unknown                             = 0x0100,
        TrimSurroundingWhitespace           = 0x1000,
        WhitespaceSignificantCollection     = 0x2000,
        UsableDuringInitialization          = 0x4000,
        Ambient                             = 0x8000,
        Default                             = Constructible | Nullable | Public,
        AllValid                            = 0xFFFF << 16
    }

    internal enum BoolMemberBits
    {
        ReadOnly     = 0x0001,
        WriteOnly    = 0x0002,
        Event        = 0x0004,
        Unknown      = 0x0008,
        Ambient      = 0x0010,
        ReadPublic   = 0x0020,
        WritePublic  = 0x0040,
        Default      = ReadPublic | WritePublic,
        Directive    = Default,
        AllValid     = 0xFFFF << 16
    }

    // Use this instead of a Nullable<bool> when a single-word read is needed for thread safety
    internal enum ThreeValuedBool : byte
    {
        NotSet,
        False,
        True
    }
	
    // Thread safety: it's important that this structure remain word-sized, so that reads and
    // writes to it are atomic
    internal struct NullableReference<T> where T : class
    {
        private static object s_NullSentinel = new object();
        private static object s_NotPresentSentinel = new object();

        private object _value;

        public bool IsNotPresent
        {
            get { return object.ReferenceEquals(_value, s_NotPresentSentinel); }
            set { _value = value ? s_NotPresentSentinel : null; }
        }

        public bool IsSet
        {
            get { return !object.ReferenceEquals(_value, null); }
        }

        public bool IsSetVolatile
        {
            get
            {
                object value = Thread.VolatileRead(ref _value);
                return !object.ReferenceEquals(value, null);
            }
        }

        public T Value
        {
            get
            {
                object value = _value;
                return object.ReferenceEquals(value, s_NullSentinel) ? null : (T)value;
            }
            set { _value = object.ReferenceEquals(value, null) ? s_NullSentinel : value; }
        }

        public void SetIfNull(T value)
        {
            object newValue = object.ReferenceEquals(value, null) ? s_NullSentinel : value;
            Interlocked.CompareExchange(ref _value, newValue, null);
        }

        public void SetVolatile(T value)
        {
            object newValue = object.ReferenceEquals(value, null) ? s_NullSentinel : value;
            Thread.VolatileWrite(ref _value, newValue);
        }
    }
}
