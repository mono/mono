// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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
// ***********************************************************************

using System;
using System.Globalization;
using System.Text;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// ExceptionHelper provides static methods for working with exceptions
    /// </summary>
    public class ExceptionHelper
    {
        // TODO: Move to a utility class
        /// <summary>
        /// Builds up a message, using the Message field of the specified exception
        /// as well as any InnerExceptions.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>A combined message string.</returns>
        public static string BuildMessage(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.CurrentCulture, "{0} : {1}", exception.GetType().ToString(), exception.Message);

            Exception inner = exception.InnerException;
            while (inner != null)
            {
                sb.Append(NUnit.Env.NewLine);
                sb.AppendFormat(CultureInfo.CurrentCulture, "  ----> {0} : {1}", inner.GetType().ToString(), inner.Message);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds up a message, using the Message field of the specified exception
        /// as well as any InnerExceptions.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>A combined stack trace.</returns>
        public static string BuildStackTrace(Exception exception)
        {
            StringBuilder sb = new StringBuilder(GetStackTrace(exception));

            Exception inner = exception.InnerException;
            while (inner != null)
            {
                sb.Append(NUnit.Env.NewLine);
                sb.Append("--");
                sb.Append(inner.GetType().Name);
                sb.Append(NUnit.Env.NewLine);
                sb.Append(GetStackTrace(inner));

                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        private static string GetStackTrace(Exception exception)
        {
            try
            {
                return exception.StackTrace;
            }
            catch (Exception)
            {
                return "No stack trace available";
            }
        }
    }
}
