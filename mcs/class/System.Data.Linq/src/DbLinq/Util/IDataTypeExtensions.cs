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
using System.Text.RegularExpressions;
using DbLinq.Vendor;

namespace DbLinq.Util
{
#if !MONO_STRICT
    public // used by external vendors
#endif
    static class IDataTypeExtensions
    {
        private static readonly Regex rawTypeEx = new Regex(@"(?<type>\w+)(\((?<length>\d+)(,(?<scale>\d+))?\))?( (?<qualifier>\w+))?", RegexOptions.Compiled);
        /// <summary>
        /// unpacks a raw db type
        /// </summary>
        /// <param name="rawType"></param>
        /// <param name="dataType"></param>
        public static void UnpackRawDbType(this IDataType dataType, string rawType)
        {
            var match = rawTypeEx.Match(rawType);
            if (match.Success)
            {
                dataType.Type = match.Result("${type}");
                var rawLength = match.Result("${length}");
                int length;
                if (int.TryParse(rawLength, out length))
                    dataType.Length = length;
                else
                    dataType.Length = null;
                dataType.Precision = (int?)dataType.Length;
                var rawScale = match.Result("${scale}");
                int scale;
                if (int.TryParse(rawScale, out scale))
                    dataType.Scale = scale;
                else
                    dataType.Scale = null;
                var qualifier = match.Result("${qualifier}");
                dataType.Unsigned = qualifier.ToLower().Contains("unsigned");
            }
            else
            {
                dataType.Type = rawType;
                dataType.Length = null;
                dataType.Precision = null;
                dataType.Scale = null;
                dataType.Unsigned = null;
            }
        }

    }
}
