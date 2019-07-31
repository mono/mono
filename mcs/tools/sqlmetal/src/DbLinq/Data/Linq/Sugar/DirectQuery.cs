#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using DbLinq.Data.Linq.Database;
using System.Data;

using DbLinq.Data.Linq.Sql;
using DbLinq.Util;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace DbLinq.Data.Linq.Sugar
{
    internal class DirectQuery : AbstractQuery
    {
        public IList<object> parameterValues { get; set; }
        public IList<string> Parameters { get; private set; }

        public DirectQuery(DataContext dataContext, SqlStatement sql, IList<string> parameters)
            : base(dataContext, sql)
        {
            Parameters = parameters;
        }

        public override ITransactionalCommand GetCommand()
        {
            ITransactionalCommand command = base.GetCommand(false);
            FeedParameters(command);
            return command;
        }

        /// <summary>
        /// Fills dbCommand parameters, given names and values
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="parameterNames"></param>
        /// <param name="parameterValues"></param>
        private void FeedParameters(ITransactionalCommand command)
        {
            IDbCommand dbCommand = command.Command;
            for (int parameterIndex = 0; parameterIndex < Parameters.Count; parameterIndex++)
            {
                var dbParameter = dbCommand.CreateParameter();
                dbParameter.ParameterName = Parameters[parameterIndex];

                var value = parameterValues[parameterIndex];
                if (value == null)
                    dbParameter.Value = DBNull.Value;
                else
                    dbParameter.Value = value;

                dbCommand.Parameters.Add(dbParameter);
            }

        }
    }

}
