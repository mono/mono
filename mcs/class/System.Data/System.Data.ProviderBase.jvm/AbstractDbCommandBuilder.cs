//
// System.Data.ProviderBase.AbstractDbCommand
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;

namespace System.Data.ProviderBase {
	public abstract class AbstractDbCommandBuilder : DbCommandBuilder {
		protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause) {
			throw new NotImplementedException();
		}

		protected override string GetParameterName(int parameterOrdinal) {
			throw new NotImplementedException();
		}

		protected override string GetParameterName(String parameterName) {
			throw new NotImplementedException();
		}

		protected override string GetParameterPlaceholder(int parameterOrdinal) {
			return "?";
		}

		protected override void SetRowUpdatingHandler(DbDataAdapter adapter) {
			throw new NotImplementedException();
		}

		protected static void DeriveParameters (AbstractDbCommand command) {
			if (command.Connection.State != ConnectionState.Open) {
				throw new InvalidOperationException("DeriveParameters requires an open and available Connection. The connection's current state is Closed.");
			}
			
			if(command.CommandType != CommandType.StoredProcedure) {
				throw ExceptionHelper.DeriveParametersNotSupported(command.GetType(),command.CommandType);
			}

			ArrayList parameters = command.DeriveParameters(command.CommandText, true);
			command.Parameters.Clear();
			foreach (AbstractDbParameter param in parameters) {
				command.Parameters.Add(param.Clone());
			}
		}
	}
}