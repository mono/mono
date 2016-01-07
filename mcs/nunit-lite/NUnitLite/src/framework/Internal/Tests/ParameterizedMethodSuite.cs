// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
// ***********************************************************************

using System.Reflection;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// ParameterizedMethodSuite holds a collection of individual
    /// TestMethods with their arguments applied.
    /// </summary>
    public class ParameterizedMethodSuite : TestSuite
    {
        private MethodInfo _method;
        private bool _isTheory;

        /// <summary>
        /// Construct from a MethodInfo
        /// </summary>
        /// <param name="method"></param>
        public ParameterizedMethodSuite(MethodInfo method)
            : base(method.ReflectedType.FullName, method.Name)
        {
            _method = method;
            _isTheory = method.IsDefined(typeof(TheoryAttribute), true);
            this.maintainTestOrder = true;
        }

        /// <summary>
        /// Gets the MethodInfo for which this suite is being built.
        /// </summary>
        public MethodInfo Method
        {
            get { return _method; }
        }

        /// <summary>
        /// Gets a string representing the type of test
        /// </summary>
        /// <value></value>
        public override string TestType
        {
            get
            {
                if (_isTheory)
                    return "Theory";

#if CLR_2_0 || CLR_4_0
                if (this.Method.ContainsGenericParameters)
                    return "GenericMethod";
#endif
                
                return "ParameterizedMethod";
            }
        }

        /// <summary>
        /// Gets the command to be executed after all the child
        /// tests are run. Overridden in ParameterizedMethodSuite
        /// to set the result to failure if all the child tests
        /// were inconclusive.
        /// </summary>
        /// <returns></returns>
        public override TestCommand GetOneTimeTearDownCommand()
        {
            TestCommand command = base.GetOneTimeTearDownCommand();

            if (_isTheory) 
                command = new TheoryResultCommand(command);

            return command;
        }
    }
}
