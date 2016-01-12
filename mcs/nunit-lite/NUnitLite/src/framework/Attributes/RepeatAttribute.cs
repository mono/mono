// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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

#if false
// TODO: Rework this
// RepeatAttribute should either
//  1) Apply at load time to create the exact number of tests, or
//  2) Apply at run time, generating tests or results dynamically
//
// #1 is feasible but doesn't provide much benefit
// #2 requires infrastructure for dynamic test cases first
using System;
using NUnit.Framework.Api;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework
{
	/// <summary>
	/// RepeatAttribute may be applied to test case in order
	/// to run it multiple times.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
	public class RepeatAttribute : PropertyAttribute, ICommandDecorator
	{
        /// <summary>
        /// Construct a RepeatAttribute
        /// </summary>
        /// <param name="count">The number of times to run the test</param>
        public RepeatAttribute(int count) : base(count) { }

        //private int count;

        ///// <summary>
        ///// Construct a RepeatAttribute
        ///// </summary>
        ///// <param name="count">The number of times to run the test</param>
        //public RepeatAttribute(int count)
        //{
        //    this.count = count;
        //}

        ///// <summary>
        ///// Gets the number of times to run the test.
        ///// </summary>
        //public int Count
        //{
        //    get { return count; }
        //}

        #region ICommandDecorator Members

        CommandStage ICommandDecorator.Stage
        {
            get { return CommandStage.Repeat; }
        }

        int ICommandDecorator.Priority
        {
            get { return 0; }
        }

        TestCommand ICommandDecorator.Decorate(TestCommand command)
        {
            return new RepeatedTestCommand(command);
        }

        #endregion
    }
}
#endif