//------------------------------------------------------------------------------
// <copyright file="EmptyTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * EmptyTextWriter class. Like the Null text writer, but keeps track of whether
     * anything was written or not.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class EmptyTextWriter : TextWriter
    {
#if UNUSED_CODE
        bool _writeCalled = false;
#endif
        bool _nonWhiteSpaceWritten = false;

        internal EmptyTextWriter() : base(CultureInfo.CurrentCulture)
        {
        }

#if UNUSED_CODE
        internal /*public*/ bool WriteCalled
        {
            get
            {
                return _writeCalled;
            }
        }
#endif

        internal /*public*/ bool NonWhiteSpaceWritten
        {
            get
            {
                return _nonWhiteSpaceWritten;
            }
        }

        internal /*public*/ void Reset()
        {
#if UNUSED_CODE
            _writeCalled = false;
#endif
            _nonWhiteSpaceWritten = false;
        }

        public override void Write(string s) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(s))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void Write(bool value) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void Write(char value) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void Write(char[] buffer) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(buffer))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void Write(char[] buffer, int index, int count) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(buffer, index, count))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void Write(double value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void Write(float value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void Write(int value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void Write(long value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void Write(Object value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (value != null && !IsWhiteSpace(value.ToString()))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void Write(String format, Object arg0)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(format) && !IsWhiteSpace(String.Format(CultureInfo.CurrentCulture, format, arg0)))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void Write(String format, Object arg0, Object arg1)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(format) && !IsWhiteSpace(String.Format(CultureInfo.CurrentCulture, format, arg0, arg1)))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void Write(String format, params object[] arg)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(format) && !IsWhiteSpace(String.Format(CultureInfo.CurrentCulture, format, arg)))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(string s) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(s))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(bool value) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void WriteLine(char value) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void WriteLine(char[] buffer) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(buffer))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(char[] buffer, int index, int count) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(buffer, index, count))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(double value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void WriteLine(float value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void WriteLine(int value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void WriteLine(long value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override void WriteLine(Object value)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (value != null && !IsWhiteSpace(value.ToString()))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(String format, Object arg0)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(format) && !IsWhiteSpace(String.Format(CultureInfo.CurrentCulture, format, arg0)))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(String format, Object arg0, Object arg1)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(format) && !IsWhiteSpace(String.Format(CultureInfo.CurrentCulture, format, arg0, arg1)))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(String format, params object[] arg)
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            if (!IsWhiteSpace(format) && !IsWhiteSpace(String.Format(CultureInfo.CurrentCulture, format, arg)))
            {
                _nonWhiteSpaceWritten = true;
            }
        }

        public override void WriteLine(UInt32 value) 
        {
#if UNUSED_CODE
            _writeCalled = true;
#endif
            _nonWhiteSpaceWritten = true;
        }

        public override Encoding Encoding 
        {
            get 
            {
                return Encoding.UTF8;
            }
        }

        private static bool IsWhiteSpace(String s)
        {
            if (s == null)
            {
                return true;
            }

            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                if (c != '\r' && c != '\n' && !Char.IsWhiteSpace(c))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsWhiteSpace(char[] buffer)
        {
            if (buffer == null)
            {
                return true;
            }

            return IsWhiteSpace(buffer, 0, buffer.Length);
        }

        private static bool IsWhiteSpace(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                return true;
            }

            for (int i = 0; i < count; i++)
            {
                char c = buffer[index + i];
                if (c != '\r' && c != '\n' && !Char.IsWhiteSpace(c))
                {
                    return false;
                }
            }

            return true;
        }
    }

}
