#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.IO;

namespace DbLinq.Util
{
    internal static class Output
    {
        /// <summary>
        /// Writes a warning
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteWarningLine(this TextWriter textWriter, string format, params object[] arg)
        {
            WriteLine(textWriter, OutputLevel.Warning, format, arg);
        }

        /// <summary>
        /// Writes an error
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteErrorLine(this TextWriter textWriter, string format, params object[] arg)
        {
            WriteLine(textWriter, OutputLevel.Error, format, arg);
        }

        /// <summary>
        /// Internal main write method.
        /// Depending on the output type, we may want to 
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="outputLevel"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteLine(TextWriter textWriter, OutputLevel outputLevel, string format, params object[] arg)
        {
            if (IsConsole(textWriter))
                WriteConsoleLine(outputLevel, format, arg);
            else
                textWriter.WriteLine(format, arg);
        }

        /// <summary>
        /// Writes to console, using specified information level
        /// </summary>
        /// <param name="outputLevel"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteConsoleLine(OutputLevel outputLevel, string format, params object[] arg)
        {
            ConsoleColor? consoleForegroundColor;
            TextWriter consoleOutput;
            // depending on output level, color and textwriter are set
            switch (outputLevel)
            {
            case OutputLevel.Debug:
                consoleForegroundColor = ConsoleColor.Blue;
                consoleOutput = Console.Out;
                break;
            case OutputLevel.Information:
                consoleForegroundColor = null;
                consoleOutput = Console.Out;
                break;
            case OutputLevel.Warning:
                consoleForegroundColor = ConsoleColor.Yellow;
                consoleOutput = Console.Out;
                break;
            case OutputLevel.Error:
                consoleForegroundColor = ConsoleColor.Red;
                consoleOutput = Console.Error;
                break;
            default:
                throw new ArgumentOutOfRangeException("outputLevel");
            }
            try
            {
                // optionnaly set the color
                if (consoleForegroundColor.HasValue)
                    Console.ForegroundColor = consoleForegroundColor.Value;
                // and write
                consoleOutput.WriteLine(format, arg);
            }
            finally
            {
                // since the WriteLine may return exceptions if the format is invalid
                // we reset the color here
                if (consoleForegroundColor.HasValue)
                    Console.ResetColor();
            }
        }

        /// <summary>
        /// Tells if we're writing on the console.
        /// </summary>
        /// <param name="textWriter"></param>
        /// <returns></returns>
        private static bool IsConsole(TextWriter textWriter)
        {
            return textWriter == Console.Out || textWriter == Console.Error;
        }
    }
}
