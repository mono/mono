//------------------------------------------------------------------------------
// 
// System.IO.FileAttributes.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------


namespace System.IO
{
	[Flags]
	[Serializable]
	public enum FileAttributes : int
	{
		Archive = 0x00020,
		Compressed = 0x00800, 
		Device = 0x00040, // Reserved for future use (NOT the w32 value). 
		Directory = 0x00010,
		Encrypted = 0x04000, // NOT the w32 value
		Hidden = 0x00002,
		Normal = 0x00080,
		NotContentIndexed = 0x02000,
		Offline = 0x01000,
		ReadOnly = 0x00001,
		ReparsePoint = 0x00400,
		SparseFile = 0x00200,
		System = 0x00004,
		Temporary = 0x00100

		//
		// This flag is used internall by Mono to make it Executable
		//
		// Executable = 0x80000000
	}

}
