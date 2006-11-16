//------------------------------------------------------------------------------
// 
// System.IO.CheckArgument.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 25, 2001 
//
// NOTE: All contributors can freely add to this class or make modifications
//       that do not break existing usage of methods 
//------------------------------------------------------------------------------

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;

namespace System.IO
{
	/// <summary>
	/// A utility class to assist with various argument validations in System.IO
	/// </summary>
	internal sealed class CheckArgument
	{
		/// <summary>
		/// Generates and exception if arg contains whitepace only
		/// </summary>
		public static void WhitespaceOnly (string arg, string desc)
		{
			if (arg != null && arg.Length > 0)
			{
				string temp = arg.Trim ();
				if (temp.Length == 0)
				{
					throw new ArgumentException (desc);
				}
			}
		}
		
		/// <summary>
		/// Generates and exception if arg contains whitepace only
		/// </summary>
		public static void WhitespaceOnly (string arg)
		{
			WhitespaceOnly (arg, "Argument string consists of whitespace characters only.");
		}
		
		/// <summary>
		/// Generates and exception if arg is empty
		/// </summary>
		public static void Empty (string arg, string desc)
		{
			if (arg != null && arg.Length == 0)
			{
				throw new ArgumentException (desc);
			}
		}
		
		/// <summary>
		/// Generates and exception if arg is empty
		/// </summary>
		public static void Empty (string arg)
		{
			Empty (arg, "Argument string is empty.");
		}
		
		/// <summary>
		/// Generates and exception if arg is null
		/// </summary>
		public static void Null (Object arg, string desc)
		{
			if (arg == null)
			{
				throw new ArgumentNullException (desc);
			}
		}
		
		/// <summary>
		/// Generates and exception if arg is null
		/// </summary>
		public static void Null (Object arg)
		{
			if (arg == null)
			{
				throw new ArgumentNullException ();
			}
		}
		
		/// <summary>
		/// Generates and exception if path contains invalid path characters
		/// </summary>
		public static void PathChars (string path, string desc)
		{
			if (path != null)
			{
				if (path.IndexOfAny (System.IO.Path.InvalidPathChars) > -1)
				{
					throw new ArgumentException (desc);
				}
			}
		}
		
		/// <summary>
		/// Generates and exception if path contains invalid path characters
		/// </summary>
		public static void PathChars (string path)
		{
			PathChars (path, "Path contains invalid characters");
		}
		
		/// <summary>
		/// Generates and exception if path too long
		/// </summary>
		[MonoTODO ("Not implemented")]
		public static void PathLength (string path, string desc)
		{
		 	//TODO: find out how long is too long
		}
		
		/// <summary>
		/// Generates and exception if path too long
		/// </summary>
		public static void PathLength (string path)
		{
			PathLength (path, "Path is too long");
		}
		
		/// <summary>
		/// Generates and exception if path is illegal
		/// </summary>
		public static void Path (string path, bool bAllowNull, bool bLength)
		{
			if (path != null) //allow null
			{
				Empty (path, "Path cannot be the empty string");	// path can't be empty
				WhitespaceOnly (path, "Path cannot be all whitespace");	// path can't be all whitespace
				PathChars (path);	// path can't contain invalid characters
				if (bLength)
				{
					PathLength ("Path too long");
				}
			}
			else if (!bAllowNull)
			{
				throw new ArgumentNullException ("Parameter name: path");
			}
		}
		
		/// <summary>
		/// Generates and exception if path is illegal
		/// </summary>
		public static void Path (string path, bool bAllowNull)
		{
			Path (path, bAllowNull, false);
		}
		
		/// <summary>
		/// Generates and exception if path is illegal
		/// </summary>
		public static void Path (string path)
		{
			Path (path, false, false);
		}

	}
}	// namespace System.IO.Private
