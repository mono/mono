//
// System.ComponentModel.LicenseManagerTests test cases
//
// Authors:
//	Ivan Hamilton (ivan@chimerical.com.au)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Martin Willemoes Hansen
// (c) 2004 Ivan Hamilton

#define NUNIT // Comment out this one if you wanna play with the test without using NUnit

#if NUNIT
using NUnit.Framework;
#else
using System.Reflection;
#endif

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace MonoTests.System.ComponentModel 
{
	public class UnlicensedObject 
	{
	}

	[LicenseProvider (typeof (TestLicenseProvider))]
	public class LicensedObject
	{
	}

	[LicenseProvider (typeof (TestLicenseProvider))]
	public class InvalidLicensedObject
	{
	}

	[LicenseProvider (typeof (TestLicenseProvider))]
	public class RuntimeLicensedObject
	{
		public RuntimeLicensedObject () 
		{
			LicenseManager.Validate (typeof (RuntimeLicensedObject));
		}
		public RuntimeLicensedObject (int a): this () {}
	}

	[LicenseProvider (typeof (TestLicenseProvider))]
	public class DesigntimeLicensedObject
	{
		public DesigntimeLicensedObject () 
		{
			LicenseManager.Validate (typeof (DesigntimeLicensedObject));
		}
	}

	public class TestLicenseProvider : LicenseProvider 
	{

		private class TestLicense : License 
		{
			public override void Dispose ()
			{
			}

			public override string LicenseKey 
			{
				get { return "YourLicenseKey"; }
			}
		}

		public TestLicenseProvider () : base ()
		{
		}

		public override License GetLicense (LicenseContext context,
			Type type,
			object instance,
			bool allowExceptions)
		{
			if (type.Name.Equals ("RuntimeLicensedObject")) {
				if (context.UsageMode != LicenseUsageMode.Runtime)
					if (allowExceptions)
						throw new LicenseException (type, instance, "License fails because this is a Runtime only license");
					else
						return null;
				return new TestLicense ();
			}

			if (type.Name.Equals ("DesigntimeLicensedObject")) 
			{
				if (context.UsageMode != LicenseUsageMode.Designtime)
					if (allowExceptions)
						throw new LicenseException (type, instance, "License fails because this is a Designtime only license");
					else
						return null;
				return new TestLicense ();
			}

			if (type.Name.Equals ("LicensedObject"))
				return new TestLicense ();

			if (allowExceptions)
				throw new LicenseException (type, instance, "License fails because of class name.");
			else
				return null;
		}
	}


#if NUNIT
	[TestFixture]
	public class LicenseManagerTests : Assertion
	{
		[SetUp]
		public void GetReady ()
		{
#else
	public class LicenseManagerTests
	{
		static LicenseManagerTests ()
		{
#endif
		}

#if NUNIT
		[Test]
#endif
		public void Test () 
		{
			object lockObject = new object ();
			//**DEFAULT CONTEXT & LicenseUsageMode**
			//Get CurrentContext, check default type
			AssertEquals ("LicenseManager #1", "System.ComponentModel.Design.RuntimeLicenseContext", LicenseManager.CurrentContext.GetType().ToString());
			//Read default LicenseUsageMode, check against CurrentContext (LicCont).UsageMode
			AssertEquals ("LicenseManager #2", LicenseManager.CurrentContext.UsageMode, LicenseManager.UsageMode);

			//**CHANGING CONTEXT**
			//Change the context and check it changes
			LicenseContext oldcontext = LicenseManager.CurrentContext;
			LicenseContext newcontext = new DesigntimeLicenseContext();
			LicenseManager.CurrentContext = newcontext;
			AssertEquals ("LicenseManager #3", newcontext, LicenseManager.CurrentContext);
			//Check the UsageMode changed too
			AssertEquals ("LicenseManager #4", newcontext.UsageMode, LicenseManager.UsageMode);
			//Set Context back to original
			LicenseManager.CurrentContext = oldcontext;
			//Check it went back
			AssertEquals ("LicenseManager #5", oldcontext, LicenseManager.CurrentContext);
			//Check the UsageMode changed too
			AssertEquals ("LicenseManager #6", oldcontext.UsageMode, LicenseManager.UsageMode);

			//**CONTEXT LOCKING**
			//Lock the context
			LicenseManager.LockContext(lockObject);
			//Try and set new context again, should throw System.InvalidOperationException: The CurrentContext property of the LicenseManager is currently locked and cannot be changed.
			bool exceptionThrown = false;
			try 
			{
				LicenseManager.CurrentContext=newcontext;
			} 
			catch (Exception e) 
			{
				AssertEquals ("LicenseManager #7",typeof(InvalidOperationException), e.GetType());
				exceptionThrown = true;
			} 
			//Check the exception was thrown
			AssertEquals ("LicenseManager #8", true, exceptionThrown);
			//Check the context didn't change
			AssertEquals ("LicenseManager #9", oldcontext, LicenseManager.CurrentContext);
			//Unlock it
			LicenseManager.UnlockContext(lockObject);
			//Now's unlocked, change it
			LicenseManager.CurrentContext=newcontext;
			AssertEquals ("LicenseManager #10", newcontext, LicenseManager.CurrentContext);
			//Change it back
			LicenseManager.CurrentContext = oldcontext;


			//Lock the context
			LicenseManager.LockContext(lockObject);
			//Unlock with different "user" should throw System.ArgumentException: The CurrentContext property of the LicenseManager can only be unlocked with the same contextUser.
			object wrongLockObject = new object();
			exceptionThrown = false;
			try 
			{
				LicenseManager.UnlockContext(wrongLockObject);
			} 
			catch (Exception e) 
			{
				AssertEquals ("LicenseManager #11",typeof(ArgumentException), e.GetType());
				exceptionThrown = true;
			} 
			AssertEquals ("LicenseManager #12",true, exceptionThrown);
			//Unlock it
			LicenseManager.UnlockContext(lockObject);

			//** bool IsValid(Type);
			AssertEquals ("LicenseManager #13", true, LicenseManager.IsLicensed (typeof (UnlicensedObject)));
			AssertEquals ("LicenseManager #14", true, LicenseManager.IsLicensed (typeof (LicensedObject)));
			AssertEquals ("LicenseManager #15", false, LicenseManager.IsLicensed (typeof (InvalidLicensedObject)));

			AssertEquals ("LicenseManager #16", true, LicenseManager.IsValid (typeof (UnlicensedObject)));
			AssertEquals ("LicenseManager #17", true, LicenseManager.IsValid (typeof (LicensedObject)));
			AssertEquals ("LicenseManager #18", false, LicenseManager.IsValid (typeof (InvalidLicensedObject)));

			UnlicensedObject unlicensedObject = new UnlicensedObject ();
			LicensedObject licensedObject = new LicensedObject ();
			InvalidLicensedObject invalidLicensedObject = new InvalidLicensedObject ();

			//** bool IsValid(Type, object, License);
			License license=null;
			AssertEquals ("LicenseManager #19", true, LicenseManager.IsValid (unlicensedObject.GetType (), unlicensedObject, out license));
			AssertEquals ("LicenseManager #20", null, license);

			license=null;
			AssertEquals ("LicenseManager #21",true, LicenseManager.IsValid (licensedObject.GetType (), licensedObject,out license));
			AssertEquals ("LicenseManager #22", "TestLicense", license.GetType ().Name);

			license=null;
			AssertEquals ("LicenseManager #23",false, LicenseManager.IsValid (invalidLicensedObject.GetType (), invalidLicensedObject, out license));
			AssertEquals ("LicenseManager #24",null, license);

			//** void Validate(Type);
			//Shouldn't throw exception
			LicenseManager.Validate (typeof (UnlicensedObject));
			//Shouldn't throw exception
			LicenseManager.Validate (typeof (LicensedObject));
			//Should throw exception
			exceptionThrown=false;
			try 
			{
				LicenseManager.Validate(typeof(InvalidLicensedObject));
			} 
			catch (Exception e) 
			{
				AssertEquals ("LicenseManager #25",typeof(LicenseException), e.GetType());
				exceptionThrown=true;
			} 
			//Check the exception was thrown
			AssertEquals ("LicenseManager #26",true, exceptionThrown);

			//** License Validate(Type, object);
			//Shouldn't throw exception, returns null license
			license=LicenseManager.Validate (typeof (UnlicensedObject), unlicensedObject);
			AssertEquals ("LicenseManager #27", null, license);

			//Shouldn't throw exception, returns TestLicense license
			license=LicenseManager.Validate(typeof(LicensedObject), licensedObject);
			AssertEquals ("LicenseManager #28", "TestLicense", license.GetType ().Name);

			//Should throw exception, returns null license
			exceptionThrown=false;
			try 
			{
				license=null;
				license=LicenseManager.Validate (typeof (InvalidLicensedObject), invalidLicensedObject);
			} 
			catch (Exception e) 
			{
				AssertEquals ("LicenseManager #29",typeof (LicenseException), e.GetType ());
				exceptionThrown=true;
			} 
			//Check the exception was thrown
			AssertEquals ("LicenseManager #30",true, exceptionThrown);
			AssertEquals ("LicenseManager #31",null, license);


			//** object CreateWithContext (Type, LicenseContext);
			object cwc = null;
			//Test we can create an unlicensed object with no context
			cwc = LicenseManager.CreateWithContext (typeof (UnlicensedObject), null);
			AssertEquals ("LicenseManager #32", "UnlicensedObject", cwc.GetType ().Name);
			//Test we can create RunTime with CurrentContext (runtime)
			cwc = null;
			cwc = LicenseManager.CreateWithContext (typeof (RuntimeLicensedObject),
				LicenseManager.CurrentContext);
			AssertEquals ("LicenseManager #33", "RuntimeLicensedObject", cwc.GetType ().Name);
			//Test we can't create DesignTime with CurrentContext (runtime)
			exceptionThrown=false;
			try 
			{
				cwc = null;
				cwc = LicenseManager.CreateWithContext (typeof (DesigntimeLicensedObject), LicenseManager.CurrentContext);
			} 
			catch (Exception e) 
			{
				AssertEquals ("LicenseManager #34",typeof (LicenseException), e.GetType ());
				exceptionThrown=true;
			} 
			//Check the exception was thrown
			AssertEquals ("LicenseManager #35",true, exceptionThrown);
			//Test we can create DesignTime with A new DesignTimeContext 
			cwc = null;
			cwc = LicenseManager.CreateWithContext (typeof (DesigntimeLicensedObject),
				new DesigntimeLicenseContext ());
			AssertEquals ("LicenseManager #36", "DesigntimeLicensedObject", cwc.GetType ().Name);

			//** object CreateWithContext(Type, LicenseContext, object[]);
			//Test we can create RunTime with CurrentContext (runtime)
			cwc = null;
			cwc = LicenseManager.CreateWithContext (typeof (RuntimeLicensedObject),
				LicenseManager.CurrentContext, new object [] { 7 });
			AssertEquals ("LicenseManager #37", "RuntimeLicensedObject", cwc.GetType ().Name);

		}
		
#if !NUNIT
		void Assert (string msg, bool result)
		{
			if (!result)
				Console.WriteLine (msg);
		}

		void AssertEquals (string msg, object expected, object real)
		{
			if (expected == null && real == null)
				return;

			if (expected != null && expected.Equals (real))
				return;

			Console.WriteLine ("{0}: expected: '{1}', got: '{2}'", msg, expected, real);
		}

		void Fail (string msg)
		{
			Console.WriteLine ("Failed: {0}", msg);
		}

		static void Main ()
		{
			LicenseManagerTests p = new LicenseManagerTests ();
			Type t = p.GetType ();
			MethodInfo [] methods = t.GetMethods ();
			foreach (MethodInfo m in methods) 
			{
				if (m.Name.Substring (0, 4) == "Test") 
				{
					m.Invoke (p, null);
				}
			}
		}
#endif
	}
}
