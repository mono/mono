#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Text;

	/// <summary>
	/// Summary description for TestSuite.
	/// </summary>
	/// 
	[Serializable]
	public class TestSuite : Test
	{
		private ArrayList tests = new ArrayList();
		private MethodInfo fixtureSetUp;
		private MethodInfo fixtureTearDown;
		private object fixture;
		private const string FIXTURE_SETUP_FAILED = "Fixture setup failed";
		private const string EXPLICIT_SELECTION_REQUIRED = "Explicit selection required";
		private bool isSetUp;

		public TestSuite( string name ) : this( name, 0 ) { }

		public TestSuite( string name, int assemblyKey ) 
			: base( name, assemblyKey )
		{
			ShouldRun = true;
		}

		public void Sort()
		{
			this.Tests.Sort();

			foreach( Test test in Tests )
			{
				TestSuite suite = test as TestSuite;
				if ( suite != null )
					suite.Sort();
			}		
		}

		public TestSuite( string parentSuiteName, string name ) 
			: this( parentSuiteName, name, 0 ) { }

		public TestSuite( string parentSuiteName, string name, int assemblyKey ) 
			: base( parentSuiteName, name, assemblyKey )
		{
			ShouldRun = true;
		}

		private IList GetCategories(object fixture) 
		{
			IList names = new ArrayList();
			object[] attributes = fixture.GetType().GetCustomAttributes(typeof(NUnit.Framework.CategoryAttribute), true);
			foreach(NUnit.Framework.CategoryAttribute attribute in attributes) 
			{
				names.Add(attribute.Name);
			}
			return names;
		}

		protected internal virtual void Add(Test test) 
		{
			if(test.ShouldRun)
			{
				test.ShouldRun = ShouldRun;
				test.IgnoreReason = IgnoreReason;
			}
			test.Parent = this;
			tests.Add(test);
		}

		protected internal virtual TestSuite CreateNewSuite(Type type)
		{
			int index = type.FullName.LastIndexOf( "." ) + 1;
			string name = type.FullName.Substring( index );
			return new TestSuite( type.Namespace, name, this.AssemblyKey);
		}

		public void Add(object fixture) 
		{
			TestSuite testSuite = CreateNewSuite(fixture.GetType());
			testSuite.fixture = fixture;
			IList categories = GetCategories(fixture);
			CategoryManager.Add(categories);
			testSuite.Categories = categories;
			testSuite.fixtureSetUp = this.FindMethodByAttribute(fixture, typeof(NUnit.Framework.TestFixtureSetUpAttribute));
			testSuite.fixtureTearDown = this.FindMethodByAttribute(fixture, typeof(NUnit.Framework.TestFixtureTearDownAttribute));
			Add(testSuite);

			Type explicitAttribute = typeof(NUnit.Framework.ExplicitAttribute);
			object[] attributes = fixture.GetType().GetCustomAttributes( explicitAttribute, false );
			testSuite.IsExplicit = attributes.Length > 0;

			Type ignoreMethodAttribute = typeof(NUnit.Framework.IgnoreAttribute);
			attributes = fixture.GetType().GetCustomAttributes(ignoreMethodAttribute, false);
			if(attributes.Length == 1)
			{
				NUnit.Framework.IgnoreAttribute attr = 
					(NUnit.Framework.IgnoreAttribute)attributes[0];
				testSuite.ShouldRun = false;
				testSuite.IgnoreReason = attr.Reason;
			}

			Type fixtureAttribute = typeof(NUnit.Framework.TestFixtureAttribute);
			attributes = fixture.GetType().GetCustomAttributes(fixtureAttribute, false);
			if(attributes.Length == 1)
			{
				NUnit.Framework.TestFixtureAttribute fixtureAttr = 
					(NUnit.Framework.TestFixtureAttribute)attributes[0];
				testSuite.Description = fixtureAttr.Description;
			}

			////////////////////////////////////////////////////////////////////////
			// Uncomment the following code block to allow including Suites in the
			// tree of tests. This causes a problem when the same test is added
			// in multiple suites so we need to either fix it or prevent it.
			//
			// See also a line to change in TestSuiteBuilder.cs
			////////////////////////////////////////////////////////////////////////

			PropertyInfo [] properties = fixture.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
			foreach(PropertyInfo property in properties)
			{
				object[] attrributes = property.GetCustomAttributes(typeof(NUnit.Framework.SuiteAttribute),false);
				if(attrributes.Length>0)
				{
					MethodInfo method = property.GetGetMethod(true);
					if(method.ReturnType!=typeof(NUnit.Core.TestSuite) || method.GetParameters().Length>0)
					{
						testSuite.ShouldRun = false;
						testSuite.IgnoreReason = "Invalid suite property method signature";
					}
//					else
//					{
//						TestSuite suite = (TestSuite)property.GetValue(null, new Object[0]);
//						foreach( Test test in suite.Tests )
//							testSuite.Add( test );
//					}
				}
			}

			MethodInfo [] methods = fixture.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static|BindingFlags.NonPublic);
			foreach(MethodInfo method in methods)
			{
				TestCase testCase = TestCaseBuilder.Make(fixture, method);
				if(testCase != null)
				{
					testCase.AssemblyKey = testSuite.AssemblyKey;
					testSuite.Add(testCase);
				}
			}

			if(testSuite.CountTestCases() == 0)
			{
				testSuite.ShouldRun = false;
				testSuite.IgnoreReason = testSuite.Name + " does not have any tests";
			}
		}

		private void CheckSuiteProperty(PropertyInfo property)
		{
			MethodInfo method = property.GetGetMethod(true);
			if(method.ReturnType!=typeof(NUnit.Core.TestSuite))
				throw new InvalidSuiteException("Invalid suite property method signature");
			if(method.GetParameters().Length>0)
				throw new InvalidSuiteException("Invalid suite property method signature");
		}

		public override ArrayList Tests 
		{
			get { return tests; }
		}

		public override bool IsSuite
		{
			get { return true; }
		}

		public override bool IsTestCase
		{
			get { return false; }
		}

		public bool IsSetUp
		{
			get { return isSetUp; }
		}

		/// <summary>
		/// True if this is a fixture. May populate the test's
		/// children as a side effect.
		/// TODO: An easier way to tell this?
		/// </summary>
		public override bool IsFixture
		{
			get
			{
				// We have no way of constructing an empty suite unless it's a fixture
				if ( Tests.Count == 0 ) return true;
				
				// Any suite with children is a fixture if the children are test cases
				Test firstChild = (Test)Tests[0];
				return !firstChild.IsSuite;
			}
		}

		public override int CountTestCases()
		{
			int count = 0;

			foreach(Test test in Tests)
			{
				count += test.CountTestCases();
			}
			return count;
		}

		public override int CountTestCases(IFilter filter)
		{
			int count = 0;

			if(this.Filter(filter)) 
			{
				foreach(Test test in Tests)
				{
					count += test.CountTestCases(filter);
				}
			}
			return count;
		}

		public override TestResult Run(EventListener listener)
		{
			return Run( listener, null );
		}
			
		public override TestResult Run(EventListener listener, IFilter filter)
		{
			TestSuiteResult suiteResult = new TestSuiteResult(this, Name);

			listener.SuiteStarted(this);
			long startTime = DateTime.Now.Ticks;

			if ( ShouldRun )
			{
				suiteResult.Executed = true;	
				doFixtureSetup( suiteResult );

				RunAllTests( suiteResult, listener, filter );

				doFixtureTearDown( suiteResult );
			}
			else
				suiteResult.NotRun(this.IgnoreReason);

			long stopTime = DateTime.Now.Ticks;
			double time = ((double)(stopTime - startTime)) / (double)TimeSpan.TicksPerSecond;
			suiteResult.Time = time;

			listener.SuiteFinished(suiteResult);
			return suiteResult;
		}

		public void DoSetUp( TestResult suiteResult )
		{
			doFixtureSetup( suiteResult );
		}

		public void DoTearDown( TestResult suiteResult )
		{
			doFixtureTearDown( suiteResult );
		}

		public void InvokeFixtureTearDown() 
		{
			if (this.fixtureTearDown != null)
				this.InvokeMethod(fixtureTearDown, fixture);		}

		private void doFixtureTearDown(TestResult suiteResult)
		{
			if (this.ShouldRun) 
			{
				try 
				{
					isSetUp = false;
					InvokeFixtureTearDown();
				} 
				catch (Exception ex) 
				{
					handleFixtureException(suiteResult, ex);
				}
				finally
				{
					suiteResult.AssertCount += NUnit.Framework.Assert.Counter;
				}
			}
			if (this.IgnoreReason == FIXTURE_SETUP_FAILED) 
			{
				this.ShouldRun = true;
				this.IgnoreReason = null;
			}
		}

		private void doFixtureSetup(TestResult suiteResult)
		{
			try 
			{
				InvokeFixtureSetUp();
				isSetUp = true;
			} 
			catch (Exception ex) 
			{
				handleFixtureException(suiteResult, ex);
				this.ShouldRun = false;
				this.IgnoreReason = FIXTURE_SETUP_FAILED;
			}
			finally
			{
				suiteResult.AssertCount = NUnit.Framework.Assert.Counter;
			}
		}

		private void InvokeFixtureSetUp()
		{
			if (this.fixtureSetUp != null)
				this.InvokeMethod(fixtureSetUp, fixture);
		}

		private void handleFixtureException(TestResult result, Exception ex) 
		{
			NunitException nex = ex as NunitException;
			if (nex != null)
				ex = nex.InnerException;

			result.Executed = false;
			result.NotRun(ex.ToString());
			result.StackTrace = ex.StackTrace;
		}

		protected virtual void RunAllTests(
			TestSuiteResult suiteResult, EventListener listener, IFilter filter )
		{
			foreach(Test test in ArrayList.Synchronized(Tests))
			{
				bool saveShouldRun = test.ShouldRun;

				if (test.ShouldRun)
				{
					if (this.ShouldRun == false)
					{
						test.ShouldRun = false;
						test.IgnoreReason = FIXTURE_SETUP_FAILED;
					}
					else if ( test.IsExplicit && filter == null )
					{
						test.ShouldRun = false;
						test.IgnoreReason = EXPLICIT_SELECTION_REQUIRED;
					}
				}
					
				if ( filter == null || test.Filter( filter ) )
					suiteResult.AddResult( test.Run( listener, filter ) );
				
				if ( saveShouldRun && !test.ShouldRun ) 
				{
					test.ShouldRun = true;
					test.IgnoreReason = null;
				}
			}
		}

		public override bool Filter(IFilter filter) 
		{
			return filter.Pass(this);
		}
	}
}
