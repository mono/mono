//------------------------------------------------------------------------------
// 
// System.Threading.ThreadState.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Thu 07/18/2001 
//
//------------------------------------------------------------------------------

namespace System.Threading
{

public enum ThreadState
{
    Aborted,
    AbortRequested,
    Background,
    Running,
    Unstarted,
    WaitSleepJoin,
}

} // Namespace
