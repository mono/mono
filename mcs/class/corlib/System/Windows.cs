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
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace System
{
	public sealed class PlatformSpecific
	{
		// TODO: Get more complete error code list
		public const int ERROR_FILE_NOT_FOUND = 2;
		public const int ERROR_PATH_NOT_FOUND = 3;
		public const int ERROR_TOO_MANY_OPEN_FILES = 4;
		public const int ERROR_ACCESS_DENIED = 5;
		public const int ERROR_INVALID_DRIVE = 15;
		
		public const int MAX_COMPUTERNAME_LENGTH = 31;
		public const int MAX_PATH = 260;

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
		private unsafe static extern IntPtr getEnvironStrings();
		
		[ DllImport("kernel32", EntryPoint="FreeEnvironmentStringsW") ]
		private unsafe static extern int freeEnvironStrings(IntPtr p);
		
		[ DllImport("kernel32", EntryPoint="GetCommandLineW") ]
		public unsafe static extern string getCommandLine();

		[DllImport("kernel32", SetLastError=true)]
		private unsafe static extern int GetCurrentDirectory(int bufSize, StringBuilder buf);
	   
		[DllImport("kernel32", SetLastError=true)]
		private unsafe static extern bool SetCurrentDirectory(string name);
	   
		[DllImport("kernel32")]
		private unsafe static extern int GetLastError();
		 
		[DllImport("libmono")]
		private unsafe static extern bool os_version(ref int platform, ref IntPtr version);
		
		public static OperatingSystem getOSVersion()
		{
			int platform = 0;
			IntPtr version = new IntPtr();
			if(os_version(ref platform, ref version))
			{
				string str = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(version);
				System.Runtime.InteropServices.Marshal.FreeBSTR(version);
				return new OperatingSystem((PlatformID)platform, new Version(str));
			}
			return null;
		}
		
		[DllImport("kernel32", SetLastError=true)]
		private unsafe static extern bool GetComputerName(StringBuilder buf, ref int bufSize);
		
		public static string getMachineName()
		{
			int capacity = MAX_COMPUTERNAME_LENGTH + 1; 
			StringBuilder buf = new StringBuilder(capacity);
			if(!GetComputerName(buf, ref capacity))
			{
				Debug.Assert(false);
			   // TODO: Determine if we should return error and if so
			   //       what error to return.	
			}
			return buf.ToString();
		}
	
		public static string getCurrentDirectory()
		{
			int capacity = MAX_PATH; 
			StringBuilder buf = new StringBuilder(capacity);
			int needed = GetCurrentDirectory(capacity, buf);
			if(needed > capacity)
			{
				capacity = needed;
				buf.Capacity = needed;
				needed = GetCurrentDirectory(capacity, buf);
			}
			if(needed > capacity || needed == 0)
			{
				Debug.Assert(false);
				return String.Empty;
			}
			return buf.ToString();
		}
		
		public static void setCurrentDirectory(string path)
		{
			if(!SetCurrentDirectory(path)) 
			{
				bool bNotFound;
				switch(GetLastError())
				{		// TODO: Get more complete error code list
				case ERROR_FILE_NOT_FOUND:
				case ERROR_PATH_NOT_FOUND:
				case ERROR_TOO_MANY_OPEN_FILES:
				case ERROR_INVALID_DRIVE:
					bNotFound = true;
					break;
				default:
					bNotFound = false;
					break;
				}
				if(bNotFound)
				{
					throw new DirectoryNotFoundException();
				}
				throw new IOException();
			}
		}
		
		public static string getEnvironment()
		{
			int start = 0;
			int offset = 0;
			int length = 0;
			int consecutive_nulls = 0;
			int null_count = 0;
			char ch;
			string strEnv = null;
			IntPtr pEnv = getEnvironStrings();

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