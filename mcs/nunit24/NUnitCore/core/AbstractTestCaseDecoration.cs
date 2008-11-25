// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;
using System.Collections.Specialized;

namespace NUnit.Core
{
	/// <summary>
	/// TestCaseDecorator is used to add functionality to
	/// another TestCase, which it aggregates.
	/// </summary>
	public abstract class AbstractTestCaseDecoration : TestCase
	{
		protected TestCase testCase;

		public AbstractTestCaseDecoration( TestCase testCase )
			: base( (TestName)testCase.TestName.Clone() )
		{
			this.testCase = testCase;
			this.RunState = testCase.RunState;
			this.IgnoreReason = testCase.IgnoreReason;
            this.Description = testCase.Description;
            this.Categories = new System.Collections.ArrayList(testCase.Categories);
            if (testCase.Properties != null)
            {
                this.Properties = new ListDictionary();
                foreach (DictionaryEntry entry in testCase.Properties)
                    this.Properties.Add(entry.Key, entry.Value);
            }
        }

		public override int TestCount
		{
			get { return testCase.TestCount; }
		}
	}
}
