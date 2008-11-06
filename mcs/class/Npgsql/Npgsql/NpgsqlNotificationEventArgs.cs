// Npgsql.NpgsqlNotificationEventArgs.cs
//
// Author:
//  Wojtek Wierzbicki
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


using System;

namespace Npgsql
{
    /// <summary>
    /// EventArgs class to send Notification parameters.
    /// </summary>
    public class NpgsqlNotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Process ID of the PostgreSQL backend that sent this notification.
        /// </summary>
        public Int32 PID = 0;

        /// <summary>
        /// Condition that triggered that notification.
        /// </summary>
        public String Condition = null;

        internal NpgsqlNotificationEventArgs(Int32 nPID, String nCondition)
        {
            PID = nPID;
            Condition = nCondition;
        }
    }
}
