//------------------------------------------------------------------------------
// 
// System.Private.Windows.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Tuesday, August 21, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System
{
	public sealed class PlatformSpecific
	{
		public static readonly char AltDirectorySeparatorChar = '/'; // TODO: verify this
		public static readonly char DirectorySeparatorChar = '\\';
		public static readonly char[] InvalidPathChars = { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
		public static readonly char PathSeparator = ';';	// might be a space for unix/linux
		public static readonly char VolumeSeparatorChar = ':';	

 		/// <summary>
		/// Gets the standard new line value
		/// </summary>
		public static string NewLine
		{
			get
			{
				return "\r\n";
			}
		}
		
		// TODO: verify the "W" versions are available on 95/98/ME
		// or whatever other windoze platforms we'll target
		[ DllImport("kernel32", EntryPoint="GetEnvironmentStringsW") ]
		private extern static IntPtr getEnvironStrings();
		[ DllImport("kernel32", EntryPoint="FreeEnvironmentStringsW") ]
		private extern static int freeEnvironStrings(IntPtr p);	

		public static string getEnvironment()
		{
			IntPtr pEnv = getEnvironStrings();
			int start = 0;
			int offset = 0;
			int length = 0;
			int consecutive_nulls = 0;
			int null_count = 0;
			char ch;
			string strEnv = null;

			try
			{
				// the first string is just the process identifer, etc.
				// the array of strings ends with double (perhaps) triple null
				// if we read too far before we reach the end an exception will
				// surely be thrown
				while(length == 0)
				{
					ch = (char)System.Runtime.InteropServices.Marshal.ReadInt16(pEnv, offset);

					offset += 2;

					if(ch == '\0')
					{
						null_count++;
						consecutive_nulls++;
						if(start == 0 && null_count > 1)
						{	// we skip the first two windows strings
							// because they aren't environment variables
							start = offset;
						}
						if(consecutive_nulls > 1)
						{	// TODO: verify this length is exactly correct
							// this was a quickie calculation that worked
							length = offset - (consecutive_nulls + start);
						}
					}
					else
					{
						consecutive_nulls = 0;
					}
				}

				char[] arEnv = new char[length];
				int index;
				for(offset = start, index = 0; index < length; offset += 2, index++)
				{
					ch = (char)System.Runtime.InteropServices.Marshal.ReadInt16(pEnv, offset);
					arEnv[index] = ch == '\0' ? '\n' : ch;
				}
				strEnv = new string(arEnv);
				//Console.WriteLine(str);
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				strEnv = null;
			}
			freeEnvironStrings(pEnv);
			return strEnv;
		}
	}
}