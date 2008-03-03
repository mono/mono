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
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.Data.Linq.Provider;
using System.Reflection;

namespace System.Data.Linq
{
    public abstract class Schema
    {
        protected Schema(DataContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
        }

        #region Fields
        private DataContext context;
        #endregion

        #region Properties
        public DataContext Context
        {
            get { return context; }
        }
        #endregion

        #region Protected Methods
        protected IExecuteResult ExecuteMethodCall(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            //TODO:
            throw new NotImplementedException();
        }

        protected IQueryResults<T> ExecuteMethodCall<T>(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            //TODO:
            throw new NotImplementedException();
        }

        protected IMultipleResults ExecuteMethodCallWithMultipleResults(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            //TODO:
            throw new NotImplementedException();
        }
        #endregion
    }
}
