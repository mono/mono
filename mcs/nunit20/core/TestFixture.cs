using System;
using System.Collections;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// A TestSuite that wraps a class marked with TestFixtureAttribute
	/// </summary>
	public class TestFixture : TestSuite
	{
		private const string FIXTURE_SETUP_FAILED = "Fixture setup failed";

		#region Constructors

		public TestFixture( object fixture ) : base( fixture, 0 )
		{
			Initialize();
		}

		public TestFixture( object fixture, int assemblyKey ) : base( fixture, assemblyKey )
		{
			Initialize();
		}

		public TestFixture( Type fixtureType ) : base( fixtureType, 0 )
		{
			Initialize();
		}

		public TestFixture( Type fixtureType, int assemblyKey ) : base( fixtureType, assemblyKey )
		{
			Initialize();
		}

		private void Initialize()
		{
			try
			{
				Reflect.CheckFixtureType( fixtureType );

				IList categories = Reflect.GetCategories( fixtureType );
				CategoryManager.Add( categories );
				this.Categories = categories;

				this.fixtureSetUp = Reflect.GetFixtureSetUpMethod( fixtureType );
				this.fixtureTearDown = Reflect.GetFixtureTearDownMethod( fixtureType );

				this.IsExplicit = Reflect.HasExplicitAttribute( fixtureType );

				if ( Reflect.HasIgnoreAttribute( fixtureType ) )
				{
					this.ShouldRun = false;
					this.IgnoreReason = Reflect.GetIgnoreReason( fixtureType );
				}
		
				this.Description = Reflect.GetDescription( fixtureType );

				MethodInfo [] methods = fixtureType.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static|BindingFlags.NonPublic);
				foreach(MethodInfo method in methods)
				{
					TestCase testCase = TestCaseBuilder.Make( fixtureType, method );
					if(testCase != null)
					{
						testCase.AssemblyKey = this.AssemblyKey;
						this.Add( testCase );
					}
				}

				if( this.CountTestCases() == 0 )
				{
					this.ShouldRun = false;
					this.IgnoreReason = this.Name + " does not have any tests";
				}
			}
			catch( InvalidTestFixtureException exception )
			{
				this.ShouldRun = false;
				this.IgnoreReason = exception.Message;
			}
		}

		#endregion

		#region Static Methods

		public static bool IsValidType( Type type )
		{
			return !type.IsAbstract && Reflect.HasTestFixtureAttribute( type );
		}

		#endregion
		
		public override void DoSetUp( TestResult suiteResult )
		{
			try 
			{
				if ( Fixture == null )
					Fixture = Reflect.Construct( fixtureType );

				if (this.fixtureSetUp != null)
					Reflect.InvokeMethod(fixtureSetUp, Fixture);
				IsSetUp = true;
			} 
			catch (Exception ex) 
			{
				// Error in TestFixtureSetUp causes the suite and
				// all contained suites to be ignored.
				// TODO: Change this to be a failure?
				NunitException nex = ex as NunitException;
				if (nex != null)
					ex = nex.InnerException;

				if ( ex is NUnit.Framework.IgnoreException )
				{
					this.ShouldRun = false;
					suiteResult.NotRun(ex.Message);
					suiteResult.StackTrace = ex.StackTrace;
					this.IgnoreReason = ex.Message;
				}
				else
				{
					suiteResult.Failure( ex.Message, ex.StackTrace, true );
				}
			}
			finally
			{
				suiteResult.AssertCount = NUnit.Framework.Assert.Counter;
			}
		}

		public override void DoTearDown( TestResult suiteResult )
		{
			if (this.ShouldRun) 
			{
				try 
				{
					IsSetUp = false;
					if (this.fixtureTearDown != null)
						Reflect.InvokeMethod(fixtureTearDown, Fixture);
				} 
				catch (Exception ex) 
				{
					// Error in TestFixtureTearDown causes the
					// suite to be marked as a failure, even if
					// all the contained tests passed.
					NunitException nex = ex as NunitException;
					if (nex != null)
						ex = nex.InnerException;

					suiteResult.Failure( ex.Message, ex.StackTrace);
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
	}
}
