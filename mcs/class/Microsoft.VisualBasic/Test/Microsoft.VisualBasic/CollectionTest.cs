// Collection.cs - NUnit Test Cases for Microsoft.VisualBasic.Collection
//
// Authors:
//   Chris J. Breisch (cjbreisch@altavista.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Chris J. Breisch
// (C) Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;
using System.Collections;

namespace MonoTests.Microsoft.VisualBasic
{
        [TestFixture]
	public class CollectionTest : Assertion
	{
		
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}

		// Test Constructor
		[Test]
		public void New ()
		{
			Collection c;

			c = new Collection();

			AssertNotNull("#N01", c);
			AssertEquals("#N02", 0, c.Count);
		}

		// Test Add method with Key == null
		[Test]
		public void AddNoKey ()
		{
			Collection c;

			c = new Collection();

			c.Add(typeof(int), null, null, null);
			c.Add(typeof(double), null, null, null);
			c.Add(typeof(string), null, null, null);
			
			AssertEquals("#ANK01", 3, c.Count);

			// Collection class is 1-based
			AssertEquals("#ANK02", typeof(string), c[3]);

		}

		// Test Add method with Key specified
		[Test]
		public void AddKey ()
		{
			Collection c;

			c = new Collection();

			c.Add("Baseball", "Base", null, null);
			c.Add("Football", "Foot", null, null);
			c.Add("Basketball", "Basket", null, null);
			c.Add("Volleyball", "Volley", null, null);

			AssertEquals("#AK01", 4, c.Count);

			// Collection class is 1-based
			AssertEquals("#AK02", "Baseball", c[1]);
			AssertEquals("#AK03", "Volleyball", c["Volley"]);

		}

		// Test Add method with Before specified and Key == null
		[Test]
		public void AddBeforeNoKey ()
		{
			Collection c;

			c = new Collection();

			c.Add(typeof(int), null, null, null);
			c.Add(typeof(double), null, 1, null);
			c.Add(typeof(string), null, 2, null);
			c.Add(typeof(object), null, 2, null);

			AssertEquals("#ABNK01", 4, c.Count);

			// Collection class is 1-based
			AssertEquals("#ABNK02", typeof(int), c[4]);
			AssertEquals("#ABNK03", typeof(double), c[1]);
			AssertEquals("#ABNK04", typeof(object), c[2]);

		}

		// Test Add method with Before and Key
		[Test]
		public void AddBeforeKey ()
		{
			Collection c;

			c = new Collection();

			c.Add("Baseball", "Base", null, null);
			c.Add("Football", "Foot", 1, null);
			c.Add("Basketball", "Basket", 1, null);
			c.Add("Volleyball", "Volley", 3, null);

			AssertEquals("#ABK01", 4, c.Count);
			AssertEquals("#ABK02", "Basketball", c[1]);
			AssertEquals("#ABK03", "Baseball", c[4]);
			AssertEquals("#ABK04", "Volleyball", c["Volley"]);
			AssertEquals("#ABK05", "Football", c["Foot"]);

		}

		// Test Add method with After specified and Key == null
		[Test]
		public void AddAfterNoKey ()
		{
			Collection c;

			c = new Collection();

			c.Add(typeof(int), null, null, 0);
			c.Add(typeof(double), null, null, 1);
			c.Add(typeof(string), null, null, 1);
			c.Add(typeof(object), null, null, 3);

			AssertEquals("#AANK01", 4, c.Count);

			// Collection class is 1-based
			AssertEquals("#AANK02", typeof(object), c[4]);
			AssertEquals("#AANK03", typeof(int), c[1]);
			AssertEquals("#AANK04", typeof(string), c[2]);

		}

		// Test Add method with After and Key
		[Test]
		public void AddAfterKey ()
		{
			Collection c;

			c = new Collection();

			c.Add("Baseball", "Base", null, 0);
			c.Add("Football", "Foot", null, 1);
			c.Add("Basketball", "Basket", null, 1);
			c.Add("Volleyball", "Volley", null, 2);

			AssertEquals("#AAK01", 4, c.Count);

			// Collection class is 1-based
			AssertEquals("#AAK02", "Baseball", c[1]);
			AssertEquals("#AAK03", "Football", c[4]);
			AssertEquals("#AAK04", "Basketball", c["Basket"]);
			AssertEquals("#AAK05", "Volleyball", c["Volley"]);
		}

		// Test GetEnumerator method
		[Test]
		public void GetEnumerator ()
		{
			Collection c;
			IEnumerator e;
			object[] o = new object[4] {typeof(int), 
				typeof(double), typeof(string), typeof(object)};
			int i = 0;

			c = new Collection();

			c.Add(typeof(int), null, null, null);
			c.Add(typeof(double), null, null, null);
			c.Add(typeof(string), null, null, null);
			c.Add(typeof(object), null, null, null);

			e = c.GetEnumerator();

			AssertNotNull("#GE01", e);

			while (e.MoveNext()) {
				AssertEquals("#GE02." + i.ToString(), o[i], e.Current);
				i++;
			}

			e.Reset();
			e.MoveNext();

			AssertEquals("#GE03", o[0], e.Current);

		}

		// Test GetEnumerator method again, this time using foreach
		[Test]
		public void Foreach ()
		{
			Collection c;
			object[] o = new object[4] {typeof(int), 
				typeof(double), typeof(string), typeof(object)};
			int i = 0;
			
			c = new Collection();

			c.Add(typeof(int), null, null, null);
			c.Add(typeof(double), null, null, null);
			c.Add(typeof(string), null, null, null);
			c.Add(typeof(object), null, null, null);

			
			foreach (object item in c) {
				AssertEquals("#fe01." + i.ToString(), o[i], item);
				i++;
			}
			
		}

		// Test Remove method with Index
		[Test]
		public void RemoveNoKey ()
		{
			Collection c;

			c = new Collection();

			c.Add(typeof(int), null, null, null);
			c.Add(typeof(double), null, null, null);
			c.Add(typeof(string), null, null, null);
			c.Add(typeof(object), null, null, null);

			AssertEquals("#RNK01", 4, c.Count);

			c.Remove(3);

			AssertEquals("#RNK02", 3, c.Count);

			// Collection class is 1-based
			AssertEquals("#RNK03", typeof(object), c[3]);

			c.Remove(1);

			AssertEquals("#RNK04", 2, c.Count);
			AssertEquals("#RNK05", typeof(double), c[1]);
			AssertEquals("#RNK06", typeof(object), c[2]);

			c.Remove(2);

			AssertEquals("#RNK07", 1, c.Count);
			AssertEquals("#RNK08", typeof(double), c[1]);

			c.Remove(1);

			AssertEquals("#RNK09", 0, c.Count);
		
		}

		// Test Remove method with Key
		[Test]
		public void RemoveKey ()
		{
			Collection c;

			c = new Collection();

			c.Add("Baseball", "Base", null, null);
			c.Add("Football", "Foot", null, null);
			c.Add("Basketball", "Basket", null, null);
			c.Add("Volleyball", "Volley", null, null);

			AssertEquals("#RK01", 4, c.Count);

			c.Remove("Foot");

			AssertEquals("#RK02", 3, c.Count);
			AssertEquals("#RK03", "Basketball", c["Basket"]);

			// Collection class is 1-based
			AssertEquals("#RK04", "Volleyball", c[3]);

			c.Remove("Base");

			AssertEquals("#RK05", 2, c.Count);
			AssertEquals("#RK06", "Basketball", c[1]);
			AssertEquals("#RK07", "Volleyball", c["Volley"]);

			c.Remove(2);

			AssertEquals("#RK08", 1, c.Count);
			AssertEquals("#RK09", "Basketball", c[1]);
			AssertEquals("#RK10", "Basketball", c["Basket"]);

			c.Remove(1);

			AssertEquals("#RK11", 0, c.Count);
		}

		// Test all the Exceptions we're supposed to throw
		[Test]
		public void Exception ()
		{
			Collection c;
			bool caughtException = false;

			c = new Collection();

			try {
				// nothing in Collection yet
				object o = c[0];
			}
			catch (Exception e) {
				AssertEquals("#E01", typeof(IndexOutOfRangeException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E02", true, caughtException);
                
			c.Add("Baseball", "Base", null, null);
			c.Add("Football", "Foot", null, null);
			c.Add("Basketball", "Basket", null, null);
			c.Add("Volleyball", "Volley", null, null);

			caughtException = false;

			try {
				// only 4 elements
				object o = c[5];
			}
			catch (Exception e) {
				AssertEquals("#E03", typeof(IndexOutOfRangeException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E04", true, caughtException);
            
			caughtException = false;
			
			try {
				// Collection class is 1-based
				object o = c[0];
			}
			catch (Exception e) {
				AssertEquals("#E05", typeof(IndexOutOfRangeException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E06", true, caughtException);
            
			caughtException = false;
			
			try {
				// no member with Key == "Kick"
				object o = c["Kick"];
			}
			catch (Exception e) {
				// FIXME
				// VB Language Reference says IndexOutOfRangeException 
				// here, but MS throws ArgumentException
				// AssertEquals("#E07", typeof(IndexOutOfRangeException), e.GetType());
				AssertEquals("#E07", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E08", true, caughtException);
         
			caughtException = false;
			
			try {
				// Even though Indexer is an object, really it's a string
				object o = c[typeof(int)];
			}
			catch (Exception e) {
				AssertEquals("#E09", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E10", true, caughtException);
         
			caughtException = false;
			
			try {
				// can't specify both Before and After
				c.Add("Kickball", "Kick", "Volley", "Foot");
			}
			catch (Exception e) {
				AssertEquals("#E11", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E12", true, caughtException);
         
			caughtException = false;
			
			try {
				// Key "Foot" already exists
				c.Add("Kickball", "Foot", null, null);
			}
			catch (Exception e) {
				AssertEquals("#E13", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E14", true, caughtException);

			caughtException = false;
			
			try {
				// Even though Before is object, it's really a string
				c.Add("Dodgeball", "Dodge", typeof(int), null);
			}
			catch (Exception e) {
				AssertEquals("#E15", typeof(InvalidCastException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E16", true, caughtException);
        
			caughtException = false;
			
			try {
				// Even though After is object, it's really a string
				c.Add("Wallyball", "Wally", null, typeof(int));
			}
			catch (Exception e) {
				AssertEquals("#E17", typeof(InvalidCastException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E18", true, caughtException);
        
			caughtException = false;
			
			try {
				// have to pass a legitimate value to remove
				c.Remove(null);
			}
			catch (Exception e) {
				AssertEquals("#E19", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E20", true, caughtException);
        
			caughtException = false;
			
			try {
				// no Key "Golf" exists
				c.Remove("Golf");
			}
			catch (Exception e) {
				AssertEquals("#E21", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E22", true, caughtException);
        
			caughtException = false;
			
			try {
				// no Index 10 exists
				c.Remove(10);
			}
			catch (Exception e) {
				AssertEquals("#E23", typeof(IndexOutOfRangeException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E24", true, caughtException);
        
			caughtException = false;
			
			try {
				IEnumerator e = c.GetEnumerator();
				
				// Must MoveNext before Current
				object item = e.Current;
			}
			catch (Exception e) {
				// FIXME
				// On-line help says InvalidOperationException here, 
				// but MS throws IndexOutOfRangeException
				// AssertEquals("#E25", typeof(InvalidOperationException), e.GetType());
				AssertEquals("#E25", typeof(IndexOutOfRangeException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#E26", true, caughtException);
        
			caughtException = false;
			
			try {
				IEnumerator e = c.GetEnumerator();
				e.MoveNext();

				c.Add("Paintball", "Paint", null, null);

				// Can't MoveNext if Collection has been modified
				e.MoveNext();
			}
			catch (Exception e) {
				// FIXME
				// On-line help says this should throw an error. MS doesn't.
				AssertEquals("#E27", typeof(InvalidOperationException), e.GetType());
				caughtException = true;
			}

			// FIXME
			// What to do about this?  MS doesn't throw an error
			// AssertEquals("#E28", true, caughtException);
			AssertEquals("#E28", false, caughtException);
        
			caughtException = false;
			
			try {
				IEnumerator e = c.GetEnumerator();
				e.MoveNext();

				c.Add("Racketball", "Racket", null, null);

				// Can't Reset if Collection has been modified
				e.Reset();
			}
			catch (Exception e) {
				// FIXME
				// On-line help says this should throw an error.  MS doesn't.
				AssertEquals("#E29", typeof(InvalidOperationException), e.GetType());
				caughtException = true;
			}

			// FIXME
			// What to do about this?  MS doesn't throw an error
			// AssertEquals("#E30", true, caughtException);
			AssertEquals("#E30", false, caughtException);

			caughtException = false;
		}
	}
}
