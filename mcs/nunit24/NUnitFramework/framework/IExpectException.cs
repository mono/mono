// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Framework
{
    /// <summary>
    /// Interface implemented by a user fixture in order to
    /// validate any expected exceptions. It is only called
    /// for test methods marked with the ExpectedException
    /// attribute.
    /// </summary>
	public interface IExpectException
    {
		/// <summary>
		/// Method to handle an expected exception
		/// </summary>
		/// <param name="ex">The exception to be handled</param>
        void HandleException(Exception ex);
    }
}
