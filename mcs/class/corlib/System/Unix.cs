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
		public static readonly char AltDirectorySeparatorChar = '\\'; // TODO: verify this
		public static readonly char DirectorySeparatorChar = '/';
		public static readonly char[] InvalidPathChars = { '/' };	// TODO: Research this further
		public static readonly char PathSeparator = ';';	// might be a space for unix/linux
		public static readonly char VolumeSeparatorChar = '/';
			
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
        private unsafe static extern int getPid();
				
		[ DllImport("glib", EntryPoint="g_get_cur_directory", CharSet=CharSet.Ansi) ]
        private unsafe static extern IntPtr GetCurrentDirectory();
				
		[ DllImport("glib", EntryPoint="g_free", CharSet=CharSet.Ansi) ]
        private unsafe static extern void GFree(IntPtr p);
				
		[ DllImport("libc", EntryPoint="chdir", CharSet=CharSet.Ansi) ]
        private unsafe static extern int SetCurrentDirectory(string path);

		public static string getMachineName()
		{	// TODO: determine if there is a better way than this
			return Environment.GetEnvironmentVariable("HOSTNAME");
		}
		
		private static char[] getPidFileContents(string fileName, ref int length)
		{	// TODO: Use a special folder define probably for the proc folder
			string path = Path.Combine(Path.Combine("/proc", getPid().ToString()), fileName);
			StreamReader stream = File.OpenText(path);
			length = (int)stream.Length;
			char[] buffer = new char[length];
			stream.Read(buffer, 0, length);
			return buffer;
		}
		
		public static string getCurrentDirectory()
		{
			IntPtr p = GetCurrentDirectory();
			string str = System.Runtime.InteropServices.Marshall.PtrToStringAnsi(p);
			GFree(p);
			return str;
		}
		
		public static void setCurrentDirectory(string path)
		{
			if(SetCurrentDirectory(path) != 0) 
			{	// TODO: figure out how we'll get to errno and
				//       so we can the appropriate exception
				throw new IOException();
			}
		}

		public static string getCommandLine()
		{
			int notused = 0;
			return new string(getPidFileContents("cmdline", ref notused));
		}
	}
}