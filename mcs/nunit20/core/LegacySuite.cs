using System;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// Represents a test suite constructed from a type that has a static Suite property
	/// </summary>
	public class LegacySuite : TestSuite
	{
		private PropertyInfo suiteProperty;

		#region Constructors

		public LegacySuite( Type fixtureType ) : base( fixtureType, 0 )
		{
			Initialize();
		}

		public LegacySuite( Type fixtureType, int assemblyKey ) : base( fixtureType, assemblyKey ) 
		{
			Initialize();
		}

		public LegacySuite( object fixture ) : base( fixture, 0 ) 
		{
			Initialize();
		}

		public LegacySuite( object fixture, int assemblyKey ) : base( fixture, assemblyKey ) 
		{
			Initialize();
		}

		private void Initialize()
		{
			suiteProperty = Reflect.GetSuiteProperty( this.fixtureType );

			MethodInfo method = suiteProperty.GetGetMethod(true);
			if(method.ReturnType!=typeof(NUnit.Core.TestSuite) || method.GetParameters().Length>0)
			{
				this.ShouldRun = false;
				this.IgnoreReason = "Invalid suite property method signature";
			}
			else
			{
				TestSuite suite = (TestSuite)suiteProperty.GetValue(null, new Object[0]);		
				foreach( Test test in suite.Tests )
					this.Add( test );
			}
		}

		#endregion

		#region Static methods

		public static bool IsValidType( Type type )
		{
			return Reflect.GetSuiteProperty( type ) != null;
		}

		#endregion
	}
}
