#region License
// Copyright 2006 James Newton-King
// http://www.newtonsoft.com
//
// This work is licensed under the Creative Commons Attribution 2.5 License
// http://creativecommons.org/licenses/by/2.5/
//
// You are free:
//    * to copy, distribute, display, and perform the work
//    * to make derivative works
//    * to make commercial use of the work
//
// Under the following conditions:
//    * For any reuse or distribution, you must make clear to others the license terms of this work.
//    * Any of these conditions can be waived if you get permission from the copyright holder.
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
