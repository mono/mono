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
	public enum FileAttributes
	{
		Archive,
		Compressed, 
		Device, // Reserved for future use. 
		Directory,
		Encrypted,
		Hidden,
		Normal,
		NotContentIndexed,
		Offline,
		ReadOnly,
		ReparsePoint,
		SparseFile,
		System,
		Temporary 
	}

}