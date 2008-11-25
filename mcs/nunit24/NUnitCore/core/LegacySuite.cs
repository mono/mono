// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// Represents a test suite constructed from a type that has a static Suite property
	/// </summary>
	public class LegacySuite : TestSuite
	{
		#region Static Methods

		public static PropertyInfo GetSuiteProperty( Type testClass )
		{
			if( testClass == null )
				return null;

			PropertyInfo property = Reflect.GetPropertyWithAttribute( 
				testClass, 
				NUnitFramework.SuiteAttribute,
				BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly );

			return property;
		}

		#endregion

		#region Constructors

		public LegacySuite( Type fixtureType ) : base( fixtureType )
		{
            PropertyInfo suiteProperty = GetSuiteProperty( fixtureType );

            if (suiteProperty == null)
                throw new ArgumentException( "Invalid argument to LegacySuite constructor", "fixtureType" );

            this.fixtureSetUp = NUnitFramework.GetFixtureSetUpMethod(fixtureType);
            this.fixtureTearDown = NUnitFramework.GetFixtureTearDownMethod(fixtureType);

            MethodInfo method = suiteProperty.GetGetMethod(true);

            if (method.GetParameters().Length == 0)
            {
                Type returnType = method.ReturnType;

                if (returnType.FullName == "NUnit.Core.TestSuite")
                {
                    TestSuite suite = (TestSuite)suiteProperty.GetValue(null, new Object[0]);
                    foreach (Test test in suite.Tests)
                        this.Add(test);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(returnType))
                {
                    foreach (object obj in (IEnumerable)suiteProperty.GetValue(null, new object[0]))
                    {
                        Type type = obj as Type;
						if ( type != null && TestFixtureBuilder.CanBuildFrom(type) )
							this.Add( TestFixtureBuilder.BuildFrom(type) );
						else
							this.Add(obj);
                    }
                }
                else
                {
                    this.RunState = RunState.NotRunnable;
                    this.IgnoreReason = "Suite property must return either TestSuite or IEnumerable";
                }
            }
            else
            {
                this.RunState = RunState.NotRunnable;
                this.IgnoreReason = "Suite property may not be indexed";
            }
		}

		#endregion
	}
}
