// created on 30/7/2002 at 00:31

// Npgsql.NpgsqlMediator.cs
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
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Npgsql
{
    ///<summary>
    /// This class is responsible for serving as bridge between the backend
    /// protocol handling and the core classes. It is used as the mediator for
    /// exchanging data generated/sent from/to backend.
    /// </summary>
    ///
    internal sealed class NpgsqlMediator
    {
        //
        // Expectations that depend on context.
        // Non-default values must be set before collecting responses.
        //
        // Some kinds of messages only get one response, and do not
        // expect a ready_for_query response.
        private bool                  _require_ready_for_query;

        //
        // Responses collected from the backend.
        //
        private ArrayList				_errors;
        private ArrayList				_notices;
        private	ArrayList				_resultSets;
        private ArrayList				_responses;
        private ArrayList               _notifications;
        private ListDictionary          _parameters;
        private NpgsqlBackEndKeyData    _backend_key_data;
        private NpgsqlRowDescription	_rd;
        private ArrayList				_rows;
        private String                  _sqlSent;
        private Int32                   _commandTimeout;
        

        public NpgsqlMediator()
        {
            _require_ready_for_query = true;

            _errors = new ArrayList();
            _notices = new ArrayList();
            _resultSets = new ArrayList();
            _responses = new ArrayList();
            _notifications = new ArrayList();
            _parameters = new ListDictionary(CaseInsensitiveComparer.Default);
            _backend_key_data = null;
            _sqlSent = String.Empty;
            _commandTimeout = 20;
        }

        public void ResetExpectations()
        {
            _require_ready_for_query = true;
        }

        public void ResetResponses()
        {
            _errors.Clear();
            _notices.Clear();
            _resultSets.Clear();
            _responses.Clear();
            _notifications.Clear();
            _parameters.Clear();
            _backend_key_data = null;
            _sqlSent = String.Empty;
            _commandTimeout = 20;
        }




        public Boolean RequireReadyForQuery
        {
            get
            {
                return _require_ready_for_query;
            }
            set
            {
                _require_ready_for_query = value;
            }
        }



        public NpgsqlRowDescription LastRowDescription
        {
            get
            {
                return _rd;
            }
        }

        public ArrayList ResultSets
        {
            get
            {
                return _resultSets;
            }
        }

        public ArrayList CompletedResponses
        {
            get
            {
                return _responses;
            }
        }

        public ArrayList Errors
        {
            get
            {
                return _errors;
            }
        }

        public ArrayList Notices
        {
            get
            {
                return _notices;
            }
        }

        public ArrayList Notifications
        {
            get
            {
                return _notifications;
            }
        }

        public ListDictionary Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public NpgsqlBackEndKeyData BackendKeyData
        {
            get
            {
                return _backend_key_data;
            }
        }
        
        public String SqlSent
        {
            set
            {
                _sqlSent = value;
            }
            
            get
            {
                return _sqlSent;
            }
        }
        
        public Int32 CommandTimeout
        {
            set
            {
                _commandTimeout = value;
            }
            
            get
            {
                return _commandTimeout;
            }
        
        }

        public void AddNotification(NpgsqlNotificationEventArgs data)
        {
            _notifications.Add(data);
        }

        public void AddCompletedResponse(String response)
        {
            if (_rd != null)
            {
                // Finished receiving the resultset. Add it to the buffer.
                _resultSets.Add(new NpgsqlResultSet(_rd, _rows));

                // Add a placeholder response.
                _responses.Add(null);

                // Discard the RowDescription.
                _rd = null;
            }
            else
            {
                // Add a placeholder resultset.
                _resultSets.Add(null);
                // It was just a non query string. Just add the response.
                _responses.Add(response);
            }

        }

        public void AddRowDescription(NpgsqlRowDescription rowDescription)
        {
            _rd = rowDescription;
            _rows = new ArrayList();
        }

        public void AddAsciiRow(NpgsqlAsciiRow asciiRow)
        {
            _rows.Add(asciiRow);
        }

        public void AddBinaryRow(NpgsqlBinaryRow binaryRow)
        {
            _rows.Add(binaryRow);
        }


        public void SetBackendKeydata(NpgsqlBackEndKeyData keydata)
        {
            _backend_key_data = keydata;
        }

        public void AddParameterStatus(String Key, NpgsqlParameterStatus PS)
        {
            _parameters[Key] = PS;
        }
    }
}
