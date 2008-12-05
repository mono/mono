//
// StringBuilderExtensions.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2008 Novell, Inc.  http://novell.com/
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Text;

namespace System.Web.Script.Serialization
{
	internal static class StringBuilderExtensions 
	{
		static void CheckCount (StringBuilder sb, int maxCount)
		{
			if (sb.Length > maxCount)
				throw new InvalidOperationException ("Maximum length exceeded.");
		}
		
		public static StringBuilder AppendCount (StringBuilder sb, int maxCount, char[] value) 
                {
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                } 
                
                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, string value) 
                {
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, bool value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }
                
                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, byte value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, decimal value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, double value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, short value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, int value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

		public static StringBuilder AppendCount (StringBuilder sb, int maxCount, long value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, object value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, sbyte value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, float value) 
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, ushort value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }       
                
                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, uint value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, ulong value)
		{
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, char value) 
                {
			StringBuilder ret = sb.Append (value);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, char value, int repeatCount) 
                {
			StringBuilder ret = sb.Append (value, repeatCount);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, char[] value, int startIndex, int charCount ) 
                {
			StringBuilder ret = sb.Append (value, startIndex, charCount);
			CheckCount (sb, maxCount);
			return ret;
                }

                public static StringBuilder AppendCount (StringBuilder sb, int maxCount, string value, int startIndex, int count) 
                {
			StringBuilder ret = sb.Append (value, startIndex, count);
			CheckCount (sb, maxCount);
			return ret;
                }
	}
}
