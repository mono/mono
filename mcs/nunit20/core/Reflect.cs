using System;
using System.Reflection;
using System.Collections;
using NUnit.Framework;

namespace NUnit.Core
{
	/// <summary>
	/// Helper methods for inspecting a type by reflection.
	/// 
	/// Many of these methods take a MemberInfo as an argument to avoid
	/// duplication, even though certain attributes can only appear on
	/// specific types of members, like MethodInfo or Type.
	/// 
	/// Generally, these methods perform simple utility functions like
	/// checking for a given attribute. However, some of the methdods
	/// actually implement policies, which might change at some later
	/// time. The intent is that policies that may vary among different
	/// types of test cases or suites should be handled by those types,
	/// while common decisions are handled here.
	/// </summary>
	public class Reflect
	{
		#region Attribute types used by reflect
 
		public static readonly Type TestFixtureType = typeof( TestFixtureAttribute );
		public static readonly Type TestType = typeof( TestAttribute );
		public static readonly Type SetUpType = typeof( SetUpAttribute );
		public static readonly Type TearDownType = typeof( TearDownAttribute );
		public static readonly Type FixtureSetUpType = typeof( TestFixtureSetUpAttribute );
		public static readonly Type FixtureTearDownType = typeof( TestFixtureTearDownAttribute );
		public static readonly Type ExplicitType = typeof( ExplicitAttribute );
		public static readonly Type CategoryType = typeof( CategoryAttribute );
		public static readonly Type IgnoreType = typeof( IgnoreAttribute );
		public static readonly Type ExpectedExceptionType = typeof( ExpectedExceptionAttribute );
		public static readonly Type SuiteType = typeof( SuiteAttribute );
		
		#endregion

		#region Binding flags used by reflect

		private static readonly BindingFlags InstanceMethods =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		private static readonly BindingFlags StaticMethods =
			BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;

		private static readonly BindingFlags AllMethods = 
			BindingFlags.Public |  BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.Static;

		private static readonly BindingFlags AllDeclaredMethods = 
			AllMethods | BindingFlags.DeclaredOnly; 

		#endregion

		#region Check for presence of an attribute

		public static bool HasTestFixtureAttribute(Type type)
		{
			return type.IsDefined( TestFixtureType, true );	// Inheritable
		}

		public static bool HasTestAttribute(MethodInfo method)
		{
			return method.IsDefined( TestType, false );
		}
		
		public static bool HasExplicitAttribute(MemberInfo member)
		{
			return member.IsDefined( ExplicitType, false );
		}

		public static bool HasCategoryAttribute(MemberInfo member) 
		{
			return member.IsDefined( CategoryType, false );
		}

		public static bool HasExpectedExceptionAttribute(MethodInfo method)
		{
			return method.IsDefined( ExpectedExceptionType, false );
		}

		public static bool HasIgnoreAttribute( MemberInfo member )
		{
			return member.IsDefined( IgnoreType, false );
		}

		public static bool HasSuiteAttribute( PropertyInfo property )
		{
			return property.IsDefined( SuiteType, false );
		}

		#endregion

		#region Legacy Checks on Names

		public static bool IsObsoleteTestMethod(MethodInfo methodToCheck)
		{
			if ( methodToCheck.Name.ToLower().StartsWith("test") )
			{
				object[] attributes = methodToCheck.GetCustomAttributes( false );

				foreach( Attribute attribute in attributes )
					if( attribute is SetUpAttribute ||
						attribute is TestFixtureSetUpAttribute ||
						attribute is TearDownAttribute || 
						attribute is TestFixtureTearDownAttribute )
					{
						return false;
					}

				return true;	
			}

			return false;
		}

		#endregion

		#region Get Attributes 

		public static TestFixtureAttribute GetTestFixtureAttribute( Type type )
		{
			object[] attributes = type.GetCustomAttributes( TestFixtureType, true );
			return attributes.Length > 0 ? (TestFixtureAttribute) attributes[0] : null;
		}

		public static TestAttribute GetTestAttribute( MemberInfo member )
		{
			object[] attributes = member.GetCustomAttributes( TestType, false );
			return attributes.Length > 0 ? (TestAttribute)attributes[0] : null;
		}

		public static IgnoreAttribute GetIgnoreAttribute( MemberInfo member )
		{
			object[] attributes = member.GetCustomAttributes( IgnoreType, false );
			return attributes.Length > 0 ? (IgnoreAttribute) attributes[0] : null;
		}

		public static ExpectedExceptionAttribute GetExpectedExceptionAttribute( MethodInfo method )
		{
			object[] attributes = method.GetCustomAttributes( ExpectedExceptionType, false);
			return attributes.Length > 0 ? (ExpectedExceptionAttribute) attributes[0] : null;
		}

		#endregion

		#region Get Properties of Attributes

		public static string GetIgnoreReason( MemberInfo member )
		{
			IgnoreAttribute attribute = GetIgnoreAttribute( member );
			return attribute == null ? "no reason" : attribute.Reason;
		}

		public static string GetDescription( MethodInfo method )
		{
			TestAttribute attribute = GetTestAttribute( method );
			return attribute == null ? null : attribute.Description;
		}

		public static string GetDescription( Type fixtureType )
		{
			TestFixtureAttribute attribute = GetTestFixtureAttribute( fixtureType );
			return attribute == null ? null : attribute.Description;
		}

		#endregion

		#region Methods to check validity of a type and its members

		/// <summary>
		/// Method to validate that a type is a valid test fixture
		/// </summary>
		/// <param name="fixtureType">The type to be checked</param>
		public static void CheckFixtureType( Type fixtureType )
		{
			if ( fixtureType.GetConstructor( Type.EmptyTypes ) == null )
				throw new InvalidTestFixtureException(fixtureType.FullName + " does not have a valid constructor");
			
			CheckSetUpTearDownMethod( fixtureType, SetUpType );
			CheckSetUpTearDownMethod( fixtureType, TearDownType );
			CheckSetUpTearDownMethod( fixtureType, FixtureSetUpType );
			CheckSetUpTearDownMethod( fixtureType, FixtureTearDownType );
		}

		/// <summary>
		/// This method verifies that a type has no more than one method of a particular
		/// SetUp or TearDown type and that the method has a correct signature.
		/// </summary>
		/// <param name="fixtureType">The type to be checked</param>
		/// <param name="attributeType">The attribute to check for</param>
		private static void CheckSetUpTearDownMethod( Type fixtureType, Type attributeType )
		{
			int count = 0;
			MethodInfo theMethod = null;

			foreach(MethodInfo method in fixtureType.GetMethods( AllDeclaredMethods ))
			{
				if( method.IsDefined( attributeType, false ) )
				{
					theMethod = method;
					count++;
				}
			}

			if ( count > 1 )
			{
				string attributeName = attributeType.Name;
				if ( attributeName.EndsWith( "Attribute" ) )
					attributeName = attributeName.Substring( 
						0, attributeName.Length - 9 );

				throw new InvalidTestFixtureException( 
					string.Format( "{0} has multiple {1} methods",
					fixtureType.Name, attributeName ) );
			}

			CheckSetUpTearDownSignature( theMethod );
		} 

		private static void CheckSetUpTearDownSignature( MethodInfo method )
		{
			if ( method != null )
			{
				if ( !method.IsPublic && !method.IsFamily || method.IsStatic || method.ReturnType != typeof(void) || method.GetParameters().Length > 0 )
					throw new InvalidTestFixtureException("Invalid SetUp or TearDown method signature");
			}
		}

		/// <summary>
		/// Check the signature of a test method
		/// </summary>
		/// <param name="methodToCheck">The method signature to check</param>
		/// <returns>True if the signature is correct, otherwise false</returns>
		public static bool IsTestMethodSignatureCorrect(MethodInfo methodToCheck)
		{
			return 
				!methodToCheck.IsStatic
				&& !methodToCheck.IsAbstract
				&& methodToCheck.IsPublic
				&& methodToCheck.GetParameters().Length == 0
				&& methodToCheck.ReturnType.Equals(typeof(void));
		}
		
		#endregion

		#region Get Methods of a type

		// These methods all take an object and assume that the type of the
		// object was pre-checked so that there are no duplicate methods,
		// statics, private methods, etc.

		public static ConstructorInfo GetConstructor( Type fixtureType )
		{
			return fixtureType.GetConstructor( Type.EmptyTypes );
		}

		public static MethodInfo GetSetUpMethod( Type fixtureType )
		{
			return GetMethod( fixtureType, SetUpType );
		}

		public static MethodInfo GetTearDownMethod(Type fixtureType)
		{			
			return GetMethod(fixtureType, TearDownType );
		}

		public static MethodInfo GetFixtureSetUpMethod( Type fixtureType )
		{
			return GetMethod( fixtureType, FixtureSetUpType );
		}

		public static MethodInfo GetFixtureTearDownMethod( Type fixtureType )
		{
			return GetMethod( fixtureType, FixtureTearDownType );
		}

		public static MethodInfo GetMethod( Type fixtureType, Type attributeType )
		{
			foreach(MethodInfo method in fixtureType.GetMethods( InstanceMethods ) )
			{
				if( method.IsDefined( attributeType, true ) ) 
					return method;
			}

			return null;
		}

		public static MethodInfo GetMethod( Type fixtureType, string methodName )
		{
			foreach(MethodInfo method in fixtureType.GetMethods( InstanceMethods ) )
			{
				if( method.Name == methodName ) 
					return method;
			}

			return null;
		}

		#endregion

		#region Get Suite Property

		public static PropertyInfo GetSuiteProperty( Type testClass )
		{
			if( testClass != null )
			{
				PropertyInfo[] properties = testClass.GetProperties( StaticMethods );
				foreach( PropertyInfo property in properties )
				{
					if( Reflect.HasSuiteAttribute( property ) )
					{
						try 
						{
							CheckSuiteProperty(property);
						}
						catch( InvalidSuiteException )
						{
							return null;
						}
						return property;
					}
				}
			}
			return null;
		}

		private static void CheckSuiteProperty(PropertyInfo property)
		{
			MethodInfo method = property.GetGetMethod(true);
			if(method.ReturnType!=typeof(NUnit.Core.TestSuite))
				throw new InvalidSuiteException("Invalid suite property method signature");
			if(method.GetParameters().Length>0)
				throw new InvalidSuiteException("Invalid suite property method signature");
		}

		#endregion

		#region Categories

		public static IList GetCategories( MemberInfo member )
		{
			object[] attributes = member.GetCustomAttributes( CategoryType, false );
			IList names = new ArrayList();

			foreach(CategoryAttribute attribute in attributes) 
				names.Add(attribute.Name);
			
			return names;
		}

		#endregion

		#region Invoke Methods

		public static object Construct( Type type )
		{
			ConstructorInfo ctor = GetConstructor( type );
			if ( ctor == null )
				throw new InvalidTestFixtureException(type.FullName + " does not have a valid constructor");
			
			return ctor.Invoke( Type.EmptyTypes );
		}

		public static void InvokeMethod( MethodInfo method, object fixture ) 
		{
			if(method != null)
			{
				try
				{
					method.Invoke( fixture, null );
				}
				catch(TargetInvocationException e)
				{
					Exception inner = e.InnerException;
					throw new NunitException("Rethrown",inner);
				}
			}
		}

		public static void InvokeSetUp( object fixture )
		{
			MethodInfo method = GetSetUpMethod( fixture.GetType() );
			if(method != null)
			{
				InvokeMethod(method, fixture);
			}
		}

		public static void InvokeTearDown( object fixture )
		{
			MethodInfo method = GetTearDownMethod( fixture.GetType() );
			if(method != null)
			{
				InvokeMethod(method, fixture);
			}
		}

		#endregion

		#region Private Constructor for static-only class

		private Reflect() { }

		#endregion
	}
}
