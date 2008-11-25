// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Fixtures
{
	/// <summary>
	/// TestTree provides a simple, character-based representation of
	/// a loaded tree of tests and is used for comparing actual and
	/// expected tree values.
	/// </summary>
	public class TestTree
	{
		string display;
		string signature;

		public static TestTree Parse( string display )
		{
			return new TestTree( display );
		}

		public TestTree( string display )
		{
			this.display = display;
			this.signature = display.Trim().Replace( Environment.NewLine, "+" ).Replace( " ", "+" );
		}

		public override string ToString()
		{
			return this.display;
		}

		public override bool Equals(object obj)
		{
			bool ok = obj is TestTree && ((TestTree)obj).signature == this.signature;
//			System.Diagnostics.Debug.Assert( ok );
			return ok;
		}

		public override int GetHashCode()
		{
			return signature.GetHashCode ();
		}



	}
}
