//
// VersionTest.cs - NUnit Test Cases for the System.Version class
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{

public class VersionTest : TestCase
{
	public VersionTest () {}

	public void TestCtors ()
	{
		Version v1;
		bool exception;
		
		/*
		v1 = new Version ();
		AssertEquals ("A1", 0, v1.Major);
		AssertEquals ("A2", 0, v1.Minor);
		AssertEquals ("A3", -1, v1.Build);
		AssertEquals ("A4", -1, v1.Revision);
	*/

		v1 = new Version (23, 7);
		AssertEquals ("A5", 23, v1.Major);
		AssertEquals ("A6", 7, v1.Minor);
		AssertEquals ("A7", -1, v1.Build);
		AssertEquals ("A8", -1, v1.Revision);

		v1 = new Version (23, 7, 99);
		AssertEquals ("A9", 23, v1.Major);
		AssertEquals ("A10", 7, v1.Minor);
		AssertEquals ("A11", 99, v1.Build);
		AssertEquals ("A12", -1, v1.Revision);

		v1 = new Version (23, 7, 99, 42);
		AssertEquals ("A13", 23, v1.Major);
		AssertEquals ("A14", 7, v1.Minor);
		AssertEquals ("A15", 99, v1.Build);
		AssertEquals ("A16", 42, v1.Revision);
		
		try {	
			v1 = new Version (23, -1);
			exception = false;
		}
		catch (ArgumentOutOfRangeException) {
			exception = true;
		}
		Assert ("A17", exception);

		try {	
			v1 = new Version (23, 7, -1);
			exception = false;
		}
		catch (ArgumentOutOfRangeException) {
			exception = true;
		}
		Assert ("A18", exception);

		try {	
			v1 = new Version (23, 7, 99, -1);
			exception = false;
		}
		catch (ArgumentOutOfRangeException) {
			exception = true;
		}
		Assert ("A19", exception);
		
	}

	public void TestStringCtor () 
	{
		Version v1;
		bool exception;
		
		v1 = new Version("1.42.79");
		AssertEquals ("A1", 1, v1.Major);
		AssertEquals ("A2", 42, v1.Minor);
		AssertEquals ("A3", 79, v1.Build);
		AssertEquals ("A4", -1, v1.Revision);
		

		try {	
			v1 = new Version ("1.42.-79");
			exception = false;
		}
		catch (ArgumentOutOfRangeException) {
			exception = true;
		}
		Assert ("A5", exception);

		try {	
			v1 = new Version ("1");
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A6", exception);

		try {	
			v1 = new Version ("1.2.3.4.5");
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A7", exception);

		try {	
			v1 = new Version ((string) null);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A6", exception);


	}

	public void TestClone () 
	{
		Version v1 = new Version (1, 2, 3, 4);
		Version v2 = (Version) v1.Clone ();

		Assert ("A1", v1.Equals (v2));
		Assert ("A2", !ReferenceEquals (v1, v2));

		Version v3 = new Version (); // 0.0
		v2 = (Version) v3.Clone ();

		Assert ("A3", v3.Equals (v2));
		Assert ("A4", !ReferenceEquals (v3, v2));
	}

	public void TestCompareTo ()
	{
		Assert ("A1", new Version (1, 2).CompareTo (new Version (1, 1)) > 0);
		Assert ("A2", new Version (1, 2, 3).CompareTo (new Version (2, 2, 3)) < 0);
		Assert ("A3", new Version (1, 2, 3, 4).CompareTo (new Version (1, 2, 2, 1)) > 0);

		Assert ("A4", new Version (1, 3).CompareTo (new Version (1, 2, 2, 1)) > 0);
		Assert ("A5", new Version (1, 2, 2).CompareTo (new Version (1, 2, 2, 1)) < 0);

		Assert ("A6", new Version (1, 2).CompareTo (new Version (1, 2)) == 0);
		Assert ("A7", new Version (1, 2, 3).CompareTo (new Version (1, 2, 3)) == 0);

		Assert ("A8", new Version (1, 1) < new Version (1, 2));
		Assert ("A9", new Version (1, 2) <= new Version (1, 2));
		Assert ("A10", new Version (1, 2) <= new Version (1, 3));
		Assert ("A11", new Version (1, 2) >= new Version (1, 2));
		Assert ("A12", new Version (1, 3) >= new Version (1, 2));
		Assert ("A13", new Version (1, 3) > new Version (1, 2));

		Version v1 = new Version(1, 2);
		bool exception;

		// LAMESPEC: Docs say this should throw a ArgumentNullException,
		// but it simply works. Seems any version is subsequent to null
		Assert ("A14:", v1.CompareTo (null) > 0);

		try {
			v1.CompareTo ("A string is not a version");
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A15", exception);
		
	}
	

	public void TestEquals ()
	{
		Version v1 = new Version (1, 2);
		Version v2 = new Version (1, 2, 3, 4);
		Version v3 = new Version (1, 2, 3, 4);

		AssertEquals ("A1", true, v2.Equals (v3));
		AssertEquals ("A2", true, v2.Equals (v2));
		AssertEquals ("A3", false, v1.Equals (v3));

		AssertEquals ("A4", true, v2 == v3);
		AssertEquals ("A5", true, v2 == v2);
		AssertEquals ("A6", false, v1 == v3);

		AssertEquals ("A7", false, v2.Equals ((Version) null));
		AssertEquals ("A8", false, v2.Equals ("A string"));
		
	}

	public void TestToString ()
	{
		Version v1 = new Version (1,2);
		Version v2 = new Version (1,2,3);
		Version v3 = new Version (1,2,3,4);
		
		AssertEquals ("A1", "1.2", v1.ToString ());
		AssertEquals ("A2", "1.2.3", v2.ToString ());
		AssertEquals ("A3", "1.2.3.4", v3.ToString ());

		AssertEquals ("A4", "" , v3.ToString (0));
		AssertEquals ("A5", "1" , v3.ToString (1));
		AssertEquals ("A6", "1.2" , v3.ToString (2));
		AssertEquals ("A7", "1.2.3" , v3.ToString (3));
		AssertEquals ("A8", "1.2.3.4" , v3.ToString (4));

		bool exception;

		try {
			v2.ToString (4);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A9", exception);

		try {
			v3.ToString (42);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A10", exception);
	}

}

}
