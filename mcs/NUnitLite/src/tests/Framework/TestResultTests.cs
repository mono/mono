// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

namespace NUnitLite.Tests
{
    [TestFixture]
    public class TestResultTests
    {
        private static readonly string MESSAGE = "my message";
        private static readonly string STACKTRACE = "stack trace";

        private TestResult result;

        [SetUp]
        public void SetUp()
        {
            result = new TestCaseResult(null);
        }

        void VerifyResultState(ResultState expectedState, string message )
        {
            Assert.That( result.ResultState , Is.EqualTo( expectedState ) );
            //if ( expectedState == ResultState.Error )
            //    Assert.That(result.Message, Is.EqualTo("System.Exception : " + message));
            //else
                Assert.That(result.Message, Is.EqualTo(message));
        }

        [Test]
        public void DefaultStateIsInconclusive()
        {
            VerifyResultState(ResultState.Inconclusive, null);
        }

        [Test]
        public void CanMarkAsSuccess()
        {
            result.SetResult(ResultState.Success);
            VerifyResultState(ResultState.Success, null);
        }

        [Test]
        public void CanMarkAsFailure()
        {
            result.SetResult(ResultState.Failure, MESSAGE, STACKTRACE);
            VerifyResultState(ResultState.Failure, MESSAGE);
            Assert.That( result.StackTrace, Is.EqualTo( STACKTRACE ) );
        }

        [Test]
        public void CanMarkAsError()
        {
            Exception caught;
            try
            {
                throw new Exception(MESSAGE);
            }
            catch(Exception ex)
            {
                caught = ex;          
            }

            result.SetResult(ResultState.Error, caught.Message, caught.StackTrace);
            VerifyResultState(ResultState.Error, MESSAGE);
            Assert.That( result.StackTrace, Is.EqualTo( caught.StackTrace ) );
        }

        [Test]
        public void CanMarkAsIgnored()
        {
            result.SetResult(ResultState.Ignored, MESSAGE);
            VerifyResultState(ResultState.Ignored, MESSAGE);
        }
    }
}
