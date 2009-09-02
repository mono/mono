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

using DbLinq.Util;
using DbLinq.Data.Linq.Database;
using System.Collections.Generic;

using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace DbLinq.Data.Linq.Sugar
{
    internal class ParameterizedQuery : AbstractQuery
    {
        public ParameterizedQuery(DataContext dataContext, SqlStatement sql, IList<ObjectInputParameterExpression> inputParameters)
            : base(dataContext, sql)
        {
            this.InputParameters = inputParameters;
        }

        /// <summary>
        /// Parameters to be sent as SQL parameters
        /// </summary>
        public IList<ObjectInputParameterExpression> InputParameters { get; protected set; }

        public ITransactionalCommand GetCommandTransactional(bool createTransaction)
        {
            ITransactionalCommand command = base.GetCommand(createTransaction);
            foreach (var inputParameter in InputParameters)
            {
                var dbParameter = command.Command.CreateParameter();
                dbParameter.ParameterName = DataContext.Vendor.SqlProvider.GetParameterName(inputParameter.Alias);
                object value = NormalizeDbType(inputParameter.GetValue(Target));
                dbParameter.SetValue(value, inputParameter.ValueType);
                command.Command.Parameters.Add(dbParameter);
            }
            return command;
        }

        public override ITransactionalCommand GetCommand()
        {
            return GetCommandTransactional(true);
        }

        private object NormalizeDbType(object value)
        {
            System.Data.Linq.Binary b = value as System.Data.Linq.Binary;
            if (b != null)
                return b.ToArray();
            return value;
        }

        public object Target { get; set; }
    }
}
