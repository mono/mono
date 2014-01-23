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

#if !MOBILE

using NUnit.Framework;
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


	[TestFixture]
	public class LicenseManagerTests
	{
		[SetUp]
		public void GetReady ()
		{
		}

		[Test]
		public void Test () 
		{
			object lockObject = new object ();
			//**DEFAULT CONTEXT & LicenseUsageMode**
			//Get CurrentContext, check default type
			Assert.AreEqual ("System.ComponentModel.Design.RuntimeLicenseContext", LicenseManager.CurrentContext.GetType().ToString(), "LicenseManager #1");
			//Read default LicenseUsageMode, check against CurrentContext (LicCont).UsageMode
			Assert.AreEqual (LicenseManager.CurrentContext.UsageMode, LicenseManager.UsageMode, "LicenseManager #2");

			//**CHANGING CONTEXT**
			//Change the context and check it changes
			LicenseContext oldcontext = LicenseManager.CurrentContext;
			LicenseContext newcontext = new DesigntimeLicenseContext();
			LicenseManager.CurrentContext = newcontext;
			Assert.AreEqual (newcontext, LicenseManager.CurrentContext, "LicenseManager #3");
			//Check the UsageMode changed too
			Assert.AreEqual (newcontext.UsageMode, LicenseManager.UsageMode, "LicenseManager #4");
			//Set Context back to original
			LicenseManager.CurrentContext = oldcontext;
			//Check it went back
			Assert.AreEqual (oldcontext, LicenseManager.CurrentContext, "LicenseManager #5");
			//Check the UsageMode changed too
			Assert.AreEqual (oldcontext.UsageMode, LicenseManager.UsageMode, "LicenseManager #6");

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
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(), "LicenseManager #7");
				exceptionThrown = true;
			} 
			//Check the exception was thrown
			Assert.AreEqual (true, exceptionThrown, "LicenseManager #8");
			//Check the context didn't change
			Assert.AreEqual (oldcontext, LicenseManager.CurrentContext, "LicenseManager #9");
			//Unlock it
			LicenseManager.UnlockContext(lockObject);
			//Now's unlocked, change it
			LicenseManager.CurrentContext=newcontext;
			Assert.AreEqual (newcontext, LicenseManager.CurrentContext, "LicenseManager #10");
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
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "LicenseManager #11");
				exceptionThrown = true;
			} 
			Assert.AreEqual (true, exceptionThrown, "LicenseManager #12");
			//Unlock it
			LicenseManager.UnlockContext(lockObject);

			//** bool IsValid(Type);
			Assert.AreEqual (true, LicenseManager.IsLicensed (typeof (UnlicensedObject)), "LicenseManager #13");
			Assert.AreEqual (true, LicenseManager.IsLicensed (typeof (LicensedObject)), "LicenseManager #14");
			Assert.AreEqual (false, LicenseManager.IsLicensed (typeof (InvalidLicensedObject)), "LicenseManager #15");

			Assert.AreEqual (true, LicenseManager.IsValid (typeof (UnlicensedObject)), "LicenseManager #16");
			Assert.AreEqual (true, LicenseManager.IsValid (typeof (LicensedObject)), "LicenseManager #17");
			Assert.AreEqual (false, LicenseManager.IsValid (typeof (InvalidLicensedObject)), "LicenseManager #18");

			UnlicensedObject unlicensedObject = new UnlicensedObject ();
			LicensedObject licensedObject = new LicensedObject ();
			InvalidLicensedObject invalidLicensedObject = new InvalidLicensedObject ();

			//** bool IsValid(Type, object, License);
			License license=null;
			Assert.AreEqual (true, LicenseManager.IsValid (unlicensedObject.GetType (), unlicensedObject, out license), "LicenseManager #19");
			Assert.AreEqual (null, license, "LicenseManager #20");

			license=null;
			Assert.AreEqual (true, LicenseManager.IsValid (licensedObject.GetType (), licensedObject,out license), "LicenseManager #21");
			Assert.AreEqual ("TestLicense", license.GetType ().Name, "LicenseManager #22");

			license=null;
			Assert.AreEqual (false, LicenseManager.IsValid (invalidLicensedObject.GetType (), invalidLicensedObject, out license), "LicenseManager #23");
			Assert.AreEqual (null, license, "LicenseManager #24");

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
				Assert.AreEqual (typeof(LicenseException), e.GetType(), "LicenseManager #25");
				exceptionThrown=true;
			} 
			//Check the exception was thrown
			Assert.AreEqual (true, exceptionThrown, "LicenseManager #26");

			//** License Validate(Type, object);
			//Shouldn't throw exception, returns null license
			license=LicenseManager.Validate (typeof (UnlicensedObject), unlicensedObject);
			Assert.AreEqual (null, license, "LicenseManager #27");

			//Shouldn't throw exception, returns TestLicense license
			license=LicenseManager.Validate(typeof(LicensedObject), licensedObject);
			Assert.AreEqual ("TestLicense", license.GetType ().Name, "LicenseManager #28");

			//Should throw exception, returns null license
			exceptionThrown=false;
			try 
			{
				license=null;
				license=LicenseManager.Validate (typeof (InvalidLicensedObject), invalidLicensedObject);
			} 
			catch (Exception e) 
			{
				Assert.AreEqual (typeof (LicenseException), e.GetType (), "LicenseManager #29");
				exceptionThrown=true;
			} 
			//Check the exception was thrown
			Assert.AreEqual (true, exceptionThrown, "LicenseManager #30");
			Assert.AreEqual (null, license, "LicenseManager #31");


			//** object CreateWithContext (Type, LicenseContext);
			object cwc = null;
			//Test we can create an unlicensed object with no context
			cwc = LicenseManager.CreateWithContext (typeof (UnlicensedObject), null);
			Assert.AreEqual ("UnlicensedObject", cwc.GetType ().Name, "LicenseManager #32");
			//Test we can create RunTime with CurrentContext (runtime)
			cwc = null;
			cwc = LicenseManager.CreateWithContext (typeof (RuntimeLicensedObject),
				LicenseManager.CurrentContext);
			Assert.AreEqual ("RuntimeLicensedObject", cwc.GetType ().Name, "LicenseManager #33");
			//Test we can't create DesignTime with CurrentContext (runtime)
			exceptionThrown=false;
			try 
			{
				cwc = null;
				cwc = LicenseManager.CreateWithContext (typeof (DesigntimeLicensedObject), LicenseManager.CurrentContext);
			} 
			catch (Exception e) 
			{
				Assert.AreEqual (typeof (LicenseException), e.GetType (), "LicenseManager #34");
				exceptionThrown=true;
			} 
			//Check the exception was thrown
			Assert.AreEqual (true, exceptionThrown, "LicenseManager #35");
			//Test we can create DesignTime with A new DesignTimeContext 
			cwc = null;
			cwc = LicenseManager.CreateWithContext (typeof (DesigntimeLicensedObject),
				new DesigntimeLicenseContext ());
			Assert.AreEqual ("DesigntimeLicensedObject", cwc.GetType ().Name, "LicenseManager #36");

			//** object CreateWithContext(Type, LicenseContext, object[]);
			//Test we can create RunTime with CurrentContext (runtime)
			cwc = null;
			cwc = LicenseManager.CreateWithContext (typeof (RuntimeLicensedObject),
				LicenseManager.CurrentContext, new object [] { 7 });
			Assert.AreEqual ("RuntimeLicensedObject", cwc.GetType ().Name, "LicenseManager #37");

		}
	}
}

#endif