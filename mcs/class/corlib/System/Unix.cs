//------------------------------------------------------------------------------
// 
// System.Private.Unix.cs 
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
using System.Runtime.InteropServices;

namespace System
{
	public sealed class PlatformSpecific
	{	
 		/// <summary>
		/// Gets the standard new line value
		/// </summary>
		public static string NewLine
		{
			get
			{
				return "\n";
			}
		}
		
		[ DllImport("libc", EntryPoint="getpid") ]
        private extern static int getPid();

		private static char[] getPidFileContents(string fileName, ref int length)
		{
			Int32 pid = getPid();
			// TODO: Use a special folder define probably for the proc folder
			string path = Path.Combine("/proc", pid.ToString());
			path = Path.Combine(path, fileName);
			StreamReader stream = File.OpenText(path);
			FileInfo finfo = new FileInfo(path);
			length = (int)finfo.Length;
			char[] buffer = new char[length];
			stream.Read(buffer, 0, length);
			return buffer;
		}

		public static string getEnvironment()
		{
			// couldn't DllImport environ from libc because it was a 
			// global variable not a method couldn't find another
			// way to access unsafe globals
			// definitely a candidate for "libmono" 

			string strEnv = null;

			try
			{ 
				int length = 0;
				char[] arEnv = getPidFileContents("environ", ref length);
				
				for(int i = 0; i < length - 1; i++)
				{
					if(arEnv[i] == '\0')
					{
						arEnv[i] = '\t';  // safer delimeter
					}
				}
				strEnv = new string(arEnv);
				//Console.WriteLine(str);
			}
			catch(Exception e)
			{
				Debug.WriteLine(e.ToString());
				strEnv = null;
			}
			return strEnv;
		}
	}
}