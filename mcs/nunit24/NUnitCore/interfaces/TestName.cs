// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core
{
	/// <summary>
	/// TestName encapsulates all info needed to identify and
	/// locate a test that has been loaded by a runner. It consists
	/// of a three components: the simple name of the test, an int
	/// id that is unique to a given tree of tests and an int 
	/// runner id that identifies the particular runner that
	/// holds the test instance.
	/// </summary>
	[Serializable]
	public class TestName : ICloneable
	{
		#region Fields
		/// <summary>
		/// ID that uniquely identifies the test
		/// </summary>
		private TestID testID;

		private int runnerID;

		/// <summary>
		/// The simple name of the test, without qualification
		/// </summary>
		private string name;

		/// <summary>
		/// The fully qualified name of the test
		/// </summary>
		private string fullName;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the TestID that uniquely identifies this test
		/// </summary>
		public TestID TestID
		{
			get { return testID; }
			set { testID = value; }
		}

		/// <summary>
		/// Gets the ID for the runner that created the test from
		/// the TestID, or returns -1 if the TestID is null.
		/// </summary>
		public int RunnerID
		{
			get { return runnerID; }
			set { runnerID = value; }
		}

		/// <summary>
		/// Gets or sets the simple name of the test
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets the full (qualified) name of the test
		/// </summary>
		public string FullName
		{
			get { return fullName; }
			set { fullName = value; }
		}

		/// <summary>
		/// Get the string representation of this test name, incorporating all
		/// the components of the name.
		/// </summary>
		public string UniqueName
		{
			get
			{
				if ( this.testID == null )
					return string.Format( "[{0}]{1}", this.runnerID, this.fullName );
				else
					return string.Format( "[{0}-{1}]{2}", this.RunnerID, this.testID, this.fullName );
			}
		}
		#endregion

		#region Static Methods
        /// <summary>
        /// Parse a string representation of a TestName,
        /// returning a TestName.
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>A TestName</returns>
		public static TestName Parse( string s )
		{
			if ( s == null ) throw new ArgumentNullException( "s", "Cannot parse a null string" );

			TestName testName = new TestName();
			testName.FullName = testName.Name = s;

			if ( s.StartsWith( "[" ) )
			{
				int rbrack = s.IndexOf( "]" );
				if ( rbrack < 0 || rbrack == s.Length - 1 )
					throw new FormatException( "Invalid TestName format: " + s );

				testName.FullName = testName.Name = s.Substring( rbrack + 1 );

				int dash = s.IndexOf( "-" );
				if ( dash < 0 || dash > rbrack )
					testName.RunnerID = Int32.Parse( s.Substring( 1, rbrack - 1 ) );
				else
				{
					testName.RunnerID = Int32.Parse( s.Substring( 1, dash - 1 ) );
					testName.TestID = TestID.Parse( s.Substring( dash + 1, rbrack - dash - 1 ) );
				}
			}

			return testName;
		}
		#endregion

		#region Object Overrides
		/// <summary>
		/// Compares two TestNames for equality
		/// </summary>
		/// <param name="obj">the other TestID</param>
		/// <returns>True if the two TestIDs are equal</returns>
		public override bool Equals(object obj)
		{
			TestName other = obj as TestName;
			if ( other == null )
				return base.Equals (obj);

			return this.TestID == other.testID
				&& this.runnerID == other.runnerID 
				&& this.fullName == other.fullName;
		}

		/// <summary>
		/// Calculates a hashcode for this TestID
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return unchecked( this.testID.GetHashCode() + this.fullName.GetHashCode() );
		}

		/// <summary>
		/// Override ToString() to display the UniqueName
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.UniqueName;
		}
		#endregion

		#region Operator Overrides
        /// <summary>
        /// Override the == operator
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
		public static bool operator ==( TestName name1, TestName name2 )
		{
			if ( Object.Equals( name1, null ) )
				return Object.Equals( name2, null );

			return name1.Equals( name2 );
		}

        /// <summary>
        /// Override the != operator
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
		public static bool operator !=( TestName name1, TestName name2 )
		{
			return name1 == name2 ? false : true;
		}
		#endregion

		#region ICloneable Implementation
		/// <summary>
		/// Returns a duplicate of this TestName
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion
	}
}
