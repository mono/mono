//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	PerlTest.cs
//
// Authors:	
//   Dan Lewis (dlewis@gmx.co.uk)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (c) 2002 Dan Lewis
// (c) 2003 Martin Willemoes Hansen

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
	
       
	[TestFixture]
	public class PerlTest {

		[Test]
		public void Trials () {
			string msg = "";
			foreach (RegexTrial trial in PerlTrials.trials) {
				string actual = trial.Execute ();
				if (actual != trial.Expected) {
					msg += "\t" + trial.ToString () +
						"Expected " + trial.Expected +
						" but got " + actual + "\n";
						
					if ( trial.Error != "" ) 
						msg += "\n" + trial.Error;

						
					//Assertion.Fail (
					//	trial.ToString () +
					//	"Expected " + trial.Expected +
					//	" but got " + actual
					//);
				}
			}
			if (msg != "" ) 
				Assertion.Fail("\n" + msg);
		}
	}
}

