#if WMLSUPPORT
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Web.UI
{
    /*
     * EmptyTextWriter class. Like the Null text writer, but keeps track of whether
     * anything was written or not.
     */
    internal class EmptyTextWriter : HtmlTextWriter
    {
        internal EmptyTextWriter() : base(null)
        {
        }

        public override void Write(string s) 
        {
        }

        public override void Write(bool value) 
        {
        }

        public override void Write(char value) 
        {
        }

        public override void Write(char[] buffer) 
        {
        }

        public override void Write(char[] buffer, int index, int count) 
        {
        }

        public override void Write(double value)
        {
        }

        public override void Write(float value)
        {
        }

        public override void Write(int value)
        {
        }

        public override void Write(long value)
        {
        }

        public override void Write(Object value)
        {
        }

        public override void Write(String format, Object arg0)
        {
        }

        public override void Write(String format, Object arg0, Object arg1)
        {
        }

        public override void Write(String format, params object[] arg)
        {
        }

        // Inherited method delegates to inner writer, which is null.  Override to avoid this.
        public override void WriteLine() {
        }

        public override void WriteLine(string s) 
        {
        }

        public override void WriteLine(bool value) 
        {
        }

        public override void WriteLine(char value) 
        {
        }

        public override void WriteLine(char[] buffer) 
        {
        }

        public override void WriteLine(char[] buffer, int index, int count) 
        {
        }

        public override void WriteLine(double value)
        {
        }

        public override void WriteLine(float value)
        {
        }

        public override void WriteLine(int value)
        {
        }

        public override void WriteLine(long value)
        {
        }

        public override void WriteLine(Object value)
        {
        }

        public override void WriteLine(String format, Object arg0)
        {
        }

        public override void WriteLine(String format, Object arg0, Object arg1)
        {
        }

        public override void WriteLine(String format, params object[] arg)
        {
        }

        public override void WriteLine(UInt32 value) 
        {
        }

        public override Encoding Encoding 
        {
            get 
            {
                return Encoding.UTF8;
            }
        }
    }
}
#endif