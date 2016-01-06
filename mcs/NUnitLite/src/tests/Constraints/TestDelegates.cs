// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnitLite.Tests
{
    public class TestDelegates
    {
        public static void ThrowsArgumentException()
        {
            throw new ArgumentException("myMessage", "myParam");
        }

        public static void ThrowsSystemException()
        {
            throw new Exception();
        }

        public static void ThrowsNothing()
        {
        }

        public static void ThrowsDerivedException()
        {
            throw new DerivedException();
        }

        public class DerivedException : Exception
        {
        }
    }
}
