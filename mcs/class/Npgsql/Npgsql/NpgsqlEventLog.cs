// created on 07/06/2002 at 09:34

// Npgsql.NpgsqlEventLog.cs
// 
// Author:
//	Dave Page (dpage@postgresql.org)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System.IO;
using System.Text;
using System.Diagnostics;
using System;

namespace Npgsql
{

  public enum LogLevel 
  {
    None = 0,
    Normal = 1,
    Debug = 2
  }
  
	/// <summary>
	/// This class handles all the Npgsql event & debug logging
	/// </summary>
	public class NpgsqlEventLog
	{
	  
    // Logging related values
    private static readonly String CLASSNAME = "NpgsqlEventLog";
    private static   String       logfile;
    private static   LogLevel     level;
    private static   Boolean echomessages;

    // Constructor
    // By marking this private, it should not be possible to create
    // instances of this class.
    private NpgsqlEventLog() {}
    
    ///<summary>
    /// Sets/Returns the level of information to log to the logfile.
    /// 0 - None
    /// 1 - Normal
    /// 2 - Complete
    /// </summary>	
    public static LogLevel Level
    {
      get
      {
        return level;
      }
      set
      {
        level = value;
        LogMsg("Set " + CLASSNAME + ".Level = " + value, LogLevel.Normal);
      }
    }
    
    ///<summary>
    /// Sets/Returns the filename to use for logging.
    /// </summary>	
    public static String LogName
    {
      get
      {
        return logfile;
      }
      set
      {
        logfile = value;
        LogMsg("Set " + CLASSNAME + ".LogFile = " + value, LogLevel.Normal);
      }
    }

    ///<summary>
    /// Sets/Returns whether Log messages should be echoed to the console
    /// </summary>	
    public static Boolean EchoMessages
    {
      get
      {
        return echomessages;
      }
      set
      {
        echomessages = value;
        LogMsg("Set " + CLASSNAME + ".EchoMessages = " + value, LogLevel.Normal);
      }
    }
    
    // Event/Debug Logging
    public static void LogMsg(String message, LogLevel msglevel) 
    {
      if (msglevel > level)
        return;
        
      Process proc = Process.GetCurrentProcess();
      
      if (echomessages)
      {
        Console.WriteLine(message);
      }
      
      if (logfile != null)
      {
        if (logfile != "")
        {
          
          StreamWriter writer = new StreamWriter(logfile, true);
          
          // The format of the logfile is
          // [Date] [Time]  [PID]  [Level]  [Message]
          writer.WriteLine(System.DateTime.Now + "  " + proc.Id + "  " + msglevel + "  " + message);
          writer.Close(); 
        }
      }
    }
    
	}
}
