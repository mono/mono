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
namespace DbLinq.Vendor
{
    /// <summary>
    /// Represents a database data type
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface IDataType
    {
        /// <summary>
        /// The base type, like 'number', 'varchar'
        /// </summary>
        string SqlType { get; set; }
        /// <summary>
        /// The managed type to use, like System.Int32
        /// </summary>
        string ManagedType { get; set; }
        /// <summary>
        /// For all types, the possibility to have a NULL
        /// </summary>
        bool Nullable { get; set; }
        /// <summary>
        /// On non numeric data types, the length (for strings or blobs)
        /// </summary>
        long? Length { get; set; }
        /// <summary>
        /// On numeric data types, the number of digits in the integer part
        /// </summary>
        int? Precision { get; set; }
        /// <summary>
        /// On numeric data types, the number of digits in the decimal part
        /// </summary>
        int? Scale { get; set; }
        /// <summary>
        /// On numeric data types, if there is a sign
        /// </summary>
        bool? Unsigned { get; set; }

        /// <summary>
        /// The original (or domain) type, returned raw by column information.
        /// Is also used to generated the database.
        /// </summary>
        string FullType { get; set; }
    }
}
