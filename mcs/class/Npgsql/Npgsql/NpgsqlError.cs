// created on 12/7/2003 at 18:36

// Npgsql.NpgsqlError.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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

using System;
using System.IO;
using System.Text;

namespace Npgsql
{
	
	/// <summary>
	/// This class represents the ErrorResponse message sent from PostgreSQL
	/// server.
	/// </summary>
	/// 
	internal sealed class NpgsqlError
	{
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlError";
	  
	  private Int32 _protocolVersion;
	  private String _severity;
	  private String _code;
	  private String _message;
	  private String _detail;
	  private String _hint;
	  private String _position;
	  private String _where;
	  private String _file;
	  private String _line;
	  private String _routine;
	  
	  
	  
	  public String Message
	  {
	    get
	    {
	      return _message;
	    }
	  }
	  
	  
	  
	  
	  public NpgsqlError(Int32 protocolVersion)
	  {
	    _protocolVersion = protocolVersion;
	    
	  }
	  
	  public void ReadFromStream(Stream inputStream, Encoding encoding)
	  {
	    
	    if (_protocolVersion == ProtocolVersion.Version2)
	    {
	      _message = PGUtil.ReadString(inputStream, encoding);
	      
	    }
	    else
	    {
	      Int32 messageLength = PGUtil.ReadInt32(inputStream, new Byte[4]);
	      
	      //[TODO] Would this be the right way to do?
	      // Check the messageLength value. If it is 1178686529, this would be the
	      // "FATA" string, which would mean a protocol 2.0 error string.
	      
	      if (messageLength == 1178686529)
	      {
	        _message = "FATA" + PGUtil.ReadString(inputStream, encoding);
	        return;
	      }
	      	      	      	      
	      Char field;
	      String fieldValue;
	      
	      field = (Char) inputStream.ReadByte();
	      
	      // Now start to read fields.
	      while (field != 0)
	      {
  	      
	        fieldValue = PGUtil.ReadString(inputStream, encoding);
	        
  	      switch (field)
  	      {
  	        case 'S':
  	          _severity = fieldValue;
  	          break;
  	        case 'C':
  	          _code = fieldValue;
  	          break;
  	        case 'M':
  	          _message = fieldValue;
  	          break;
  	        case 'D':
  	          _detail = fieldValue;
  	          break;
  	        case 'H':
  	          _hint = fieldValue;
  	          break;
  	        case 'P':
  	          _position = fieldValue;
  	          break;
  	        case 'W':
  	          _where = fieldValue;
  	          break;
  	        case 'F':
  	          _file = fieldValue;
  	          break;
  	        case 'L':
  	          _line = fieldValue;
  	          break;
  	        case 'R':
  	          _routine = fieldValue;
  	          break;
  	          	        
  	      }
  	      
  	      field = (Char) inputStream.ReadByte();
	        
	      }
	      
	      // Read 0 byte terminator.
	      //inputStream.ReadByte();
	    }
	    
	  }
	}
}
