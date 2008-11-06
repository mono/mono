// Npgsql.NpgsqlCancelRequest.cs
//
// Author:
//  Francisco Jr. (fxjrlists@yahoo.com.br)
//
//  Copyright (C) 2002-2006 The Npgsql Development Team
//  http://pgfoundry.org/projects/npgsql
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
    /// This class represents the CancelRequest message sent to PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class NpgsqlCancelRequest
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlCancelRequest";


        private static Int32 CancelRequestMessageSize = 16;
        private static Int32 CancelRequestCode = 1234 << 16 | 5678;

        private NpgsqlBackEndKeyData BackendKeydata;
        
        
        public NpgsqlCancelRequest(NpgsqlBackEndKeyData BackendKeydata)
        {
            this.BackendKeydata = BackendKeydata;
            
        }

        public void WriteToStream(Stream outputStream, Encoding encoding)
        {
            PGUtil.WriteInt32(outputStream, CancelRequestMessageSize);
            PGUtil.WriteInt32(outputStream, CancelRequestCode);
            PGUtil.WriteInt32(outputStream, BackendKeydata.ProcessID);
            PGUtil.WriteInt32(outputStream, BackendKeydata.SecretKey);
            
            outputStream.Flush();

        }

    }
}