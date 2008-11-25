// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Text;
using System.Collections;

namespace NUnit.Framework
{
    /// <summary>
    /// Static methods used in creating messages
    /// </summary>
    public class MsgUtils
    {
        /// <summary>
        /// Static string used when strings are clipped
        /// </summary>
        public static readonly string ELLIPSIS = "...";

        /// <summary>
        /// Returns the representation of a type as used in NUnitLite.
        /// This is the same as Type.ToString() except for arrays,
        /// which are displayed with their declared sizes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetTypeRepresentation(object obj)
        {
            Array array = obj as Array;
            if ( array == null )
                return string.Format( "<{0}>", obj.GetType() );

            StringBuilder sb = new StringBuilder();
            Type elementType = array.GetType();
            int nest = 0;
            while (elementType.IsArray)
            {
                elementType = elementType.GetElementType();
                ++nest;
            }
            sb.Append(elementType.ToString());
            sb.Append('[');
            for (int r = 0; r < array.Rank; r++)
            {
                if (r > 0) sb.Append(',');
                sb.Append(array.GetLength(r));
            }
            sb.Append(']');

            while (--nest > 0)
                sb.Append("[]");

            return string.Format( "<{0}>", sb.ToString() );
        }
        /// <summary>
        /// Converts any control characters in a string 
        /// to their escaped representation.
        /// </summary>
        /// <param name="s">The string to be converted</param>
        /// <returns>The converted string</returns>
        public static string ConvertWhitespace(string s)
        {
			if( s != null )
			{
				s = s.Replace( "\\", "\\\\" );
				s = s.Replace( "\r", "\\r" );
				s = s.Replace( "\n", "\\n" );
				s = s.Replace( "\t", "\\t" );
			}
			return s;
        }

        /// <summary>
        /// Return the a string representation for a set of indices into an array
        /// </summary>
        /// <param name="indices">Array of indices for which a string is needed</param>
        public static string GetArrayIndicesAsString(int[] indices)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            for (int r = 0; r < indices.Length; r++)
            {
                if (r > 0) sb.Append(',');
                sb.Append(indices[r].ToString());
            }
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Get an array of indices representing the point in a collection or
        /// array corresponding to a single int index into the collection.
        /// </summary>
        /// <param name="collection">The collection to which the indices apply</param>
        /// <param name="index">Index in the collection</param>
        /// <returns>Array of indices</returns>
        public static int[] GetArrayIndicesFromCollectionIndex(ICollection collection, int index)
        {
            Array array = collection as Array;

            if ( array == null || array.Rank == 1)
                return new int[] { index };

            int[] result = new int[array.Rank];

            for (int r = array.Rank; --r > 0; )
            {
                int l = array.GetLength(r);
                result[r] = index % l;
                index /= l;
            }

            result[0] = index;
            return result;
        }

        /// <summary>
        /// Clip a string to a given length, starting at a particular offset, returning the clipped
        /// string with ellipses representing the removed parts
        /// </summary>
        /// <param name="s">The string to be clipped</param>
        /// <param name="maxStringLength">The maximum permitted length of the result string</param>
        /// <param name="clipStart">The point at which to start clipping</param>
        /// <returns>The clipped string</returns>
        public static string ClipString(string s, int maxStringLength, int clipStart)
        {
            int clipLength = maxStringLength;
            StringBuilder sb = new StringBuilder();

            if (clipStart > 0)
            {
                clipLength -= ELLIPSIS.Length;
                sb.Append( ELLIPSIS );
            }

            if (s.Length - clipStart > clipLength)
            {
                clipLength -= ELLIPSIS.Length;
                sb.Append( s.Substring( clipStart, clipLength ));
                sb.Append(ELLIPSIS);
            }
            else if (clipStart > 0)
                sb.Append( s.Substring(clipStart));
            else
                sb.Append( s );
 
            return sb.ToString();
        }

        /// <summary>
        /// Clip the expected and actual strings in a coordinated fashion, 
        /// so that they may be displayed together.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="maxDisplayLength"></param>
        /// <param name="mismatch"></param>
        public static void ClipExpectedAndActual(ref string expected, ref string actual, int maxDisplayLength, int mismatch)
        {
            // Case 1: Both strings fit on line
            int maxStringLength = Math.Max(expected.Length, actual.Length);
            if (maxStringLength <= maxDisplayLength)
                return;

            // Case 2: Assume that the tail of each string fits on line
            int clipLength = maxDisplayLength - ELLIPSIS.Length;
            int tailLength = clipLength - mismatch;
            int clipStart = maxStringLength - clipLength;

            // Case 3: If it doesn't, center the mismatch position
            if ( clipStart > mismatch )
                clipStart = Math.Max( 0, mismatch - clipLength / 2 );

            expected = ClipString(expected, maxDisplayLength, clipStart);
            actual = ClipString(actual, maxDisplayLength, clipStart);
        }

        /// <summary>
        /// Shows the position two strings start to differ.  Comparison 
        /// starts at the start index.
        /// </summary>
        /// <param name="expected">The expected string</param>
        /// <param name="actual">The actual string</param>
        /// <param name="istart">The index in the strings at which comparison should start</param>
        /// <param name="ignoreCase">Boolean indicating whether case should be ignored</param>
        /// <returns>-1 if no mismatch found, or the index where mismatch found</returns>
        static public int FindMismatchPosition(string expected, string actual, int istart, bool ignoreCase)
        {
            int length = Math.Min(expected.Length, actual.Length);

            string s1 = ignoreCase ? expected.ToLower() : expected;
            string s2 = ignoreCase ? actual.ToLower() : actual;

            for (int i = istart; i < length; i++)
            {
                if (s1[i] != s2[i])
                    return i;
            }

            //
            // Strings have same content up to the length of the shorter string.
            // Mismatch occurs because string lengths are different, so show
            // that they start differing where the shortest string ends
            //
            if (expected.Length != actual.Length)
                return length;

            //
            // Same strings : We shouldn't get here
            //
            return -1;
        }
    }
}
