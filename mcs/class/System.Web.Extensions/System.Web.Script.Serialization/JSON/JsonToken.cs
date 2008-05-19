#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies the type of Json token.
	/// </summary>
	enum JsonToken
	{
		/// <summary>
		/// This is returned by the <see cref="JsonReader"/> if a <see cref="JsonReader.Read"/> method has not been called. 
		/// </summary>
		None,
		/// <summary>
		/// An object start token.
		/// </summary>
		StartObject,
		/// <summary>
		/// An array start token.
		/// </summary>
		StartArray,
		/// <summary>
		/// An object property name.
		/// </summary>
		PropertyName,
		/// <summary>
		/// A comment.
		/// </summary>
		Comment,
		/// <summary>
		/// An interger.
		/// </summary>
		Integer,
		/// <summary>
		/// A float.
		/// </summary>
		Float,
		/// <summary>
		/// A string.
		/// </summary>
		String,
		/// <summary>
		/// A boolean.
		/// </summary>
		Boolean,
		/// <summary>
		/// A null token.
		/// </summary>
		Null,
		/// <summary>
		/// An undefined token.
		/// </summary>
		Undefined,
		/// <summary>
		/// An object end token.
		/// </summary>
		EndObject,
		/// <summary>
		/// An array end token.
		/// </summary>
		EndArray,
		/// <summary>
		/// A JavaScript object constructor.
		/// </summary>
		Constructor,
		/// <summary>
		/// A Date.
		/// </summary>
		Date
	}
}
