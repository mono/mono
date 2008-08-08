/*
 * Strings.cs - Implementation of the "I18N.Common.Strings" class.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace I18N.Common
{

using System;
using System.Reflection;
using System.Resources;

// This class provides string resource support to the rest
// of the I18N library assemblies.

public sealed class Strings
{
	// Cached copy of the resources for this assembly.
	// private static ResourceManager resources = null;

	// Helper for obtaining string resources for this assembly.
	public static String GetString(String tag)
	{
		switch (tag) {
		case "ArgRange_Array":
			return "Argument index is out of array range.";
		case "Arg_InsufficientSpace":
			return "Insufficient space in the argument array.";
		case "ArgRange_NonNegative":
			return "Non-negative value is expected.";
		case "NotSupp_MissingCodeTable":
			return "This encoding is not supported. Code table is missing.";
		case "ArgRange_StringIndex":
			return "String index is out of range.";
		case "ArgRange_StringRange":
			return "String length is out of range.";
		default:
			throw new ArgumentException (String.Format ("Unexpected error tag name:  {0}", tag));
		}
	}

}; // class Strings

}; // namespace I18N.Common
