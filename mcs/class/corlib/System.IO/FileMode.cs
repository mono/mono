//------------------------------------------------------------------------------
// 
// System.IO.FileMode.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Thu 07/18/2001 
//
//------------------------------------------------------------------------------

namespace System.IO
{

public enum FileMode
{
    CreateNew = 1,
    Create,
    Open,
    OpenOrCreate,
    Truncate,
    Append,
}

} // Namespace
