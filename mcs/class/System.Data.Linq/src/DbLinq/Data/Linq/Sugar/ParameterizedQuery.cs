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

#if MONO_STRICT
using System.Data.Linq.Sql;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    internal abstract class ParameterizedQuery : AbstractQuery
    {
        protected ParameterizedQuery(DataContext dataContext, SqlStatement sql, IList<ObjectInputParameterExpression> inputParameters)
            : base(dataContext, sql)
        {
            this.InputParameters = inputParameters;
        }

        /// <summary>
        /// Parameters to be sent as SQL parameters
        /// </summary>
        public IList<ObjectInputParameterExpression> InputParameters { get; protected set; }

        public override ITransactionalCommand GetCommand()
        {
            ITransactionalCommand transactionalCommand = base.GetCommand(true);
            foreach (var inputParameter in InputParameters)
            {
                var dbParameter = transactionalCommand.Command.CreateParameter();
                dbParameter.ParameterName = DataContext.Vendor.SqlProvider.GetParameterName(inputParameter.Alias);
                dbParameter.SetValue(inputParameter.GetValue(Target), inputParameter.ValueType);
                transactionalCommand.Command.Parameters.Add(dbParameter);
            }
            return transactionalCommand;
        }

        public object Target { get; set; }
    }
}
