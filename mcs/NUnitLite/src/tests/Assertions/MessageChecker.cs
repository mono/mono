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

using System;

namespace NUnit.Framework.Assertions
{
	/// <summary>
	/// MessageCheckingTest is an abstract base for tests
	/// that check for an expected message in the exception
	/// handler.
	/// </summary>
	public abstract class MessageChecker : IExpectException
	{
		protected string expectedMessage;
		protected MessageMatch matchType = MessageMatch.Exact;
        protected readonly string NL = NUnit.Env.NewLine;

		[SetUp]
		public void SetUp()
		{
			expectedMessage = null;
		}

		public void HandleException( Exception ex )
		{
			if ( expectedMessage != null )
            {
                switch(matchType)
                {
                    default:
                    case MessageMatch.Exact:
                                        Assert.AreEqual( expectedMessage, ex.Message );
                        break;
                    case MessageMatch.Contains:
                        Assert.That(ex.Message, Is.StringContaining(expectedMessage));
                        break;
                    case MessageMatch.StartsWith:
                        Assert.That(ex.Message, Is.StringStarting(expectedMessage));
                        break;
#if !NETCF
                    case MessageMatch.Regex:
                        Assert.That(ex.Message, Is.StringMatching(expectedMessage));
                        break;
#endif
                }
            }
		}
	}
}
