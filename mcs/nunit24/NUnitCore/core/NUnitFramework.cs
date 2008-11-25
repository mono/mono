// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using NUnit.Core.Extensibility;

namespace NUnit.Core
{
	/// <summary>
	/// Static methods that implement aspects of the NUnit framework that cut 
	/// across individual test types, extensions, etc. Some of these use the 
	/// methods of the Reflect class to implement operations specific to the 
	/// NUnit Framework.
	/// </summary>
	public class NUnitFramework
	{
		private static Type assertType;
        //private static Hashtable frameworkByAssembly = new Hashtable();

        #region Constants

		#region Attribute Names
		// NOTE: Attributes used in switch statements must be const

        // Attributes that apply to Assemblies, Classes and Methods
        public const string IgnoreAttribute = "NUnit.Framework.IgnoreAttribute";
		public const string PlatformAttribute = "NUnit.Framework.PlatformAttribute";
		public const string CultureAttribute = "NUnit.Framework.CultureAttribute";
		public const string ExplicitAttribute = "NUnit.Framework.ExplicitAttribute";
        public const string CategoryAttribute = "NUnit.Framework.CategoryAttribute";
        public const string PropertyAttribute = "NUnit.Framework.PropertyAttribute";
		public const string DescriptionAttribute = "NUnit.Framework.DescriptionAttribute";

        // Attributes that apply only to Classes
        public const string TestFixtureAttribute = "NUnit.Framework.TestFixtureAttribute";
        public const string SetUpFixtureAttribute = "NUnit.Framework.SetUpFixtureAttribute";

        // Attributes that apply only to Methods
        public const string TestAttribute = "NUnit.Framework.TestAttribute";
        public static readonly string SetUpAttribute = "NUnit.Framework.SetUpAttribute";
        public static readonly string TearDownAttribute = "NUnit.Framework.TearDownAttribute";
        public static readonly string FixtureSetUpAttribute = "NUnit.Framework.TestFixtureSetUpAttribute";
        public static readonly string FixtureTearDownAttribute = "NUnit.Framework.TestFixtureTearDownAttribute";
        public static readonly string ExpectedExceptionAttribute = "NUnit.Framework.ExpectedExceptionAttribute";

        // Attributes that apply only to Properties
        public static readonly string SuiteAttribute = "NUnit.Framework.SuiteAttribute";
        #endregion

        #region Other Framework Types
        public static readonly string AssertException = "NUnit.Framework.AssertionException";
        public static readonly string IgnoreException = "NUnit.Framework.IgnoreException";
        public static readonly string AssertType = "NUnit.Framework.Assert";
		public static readonly string ExpectExceptionInterface = "NUnit.Framework.IExpectException";
        #endregion

        #region Core Types
        public static readonly string SuiteBuilderAttribute = typeof(SuiteBuilderAttribute).FullName;
        public static readonly string SuiteBuilderInterface = typeof(ISuiteBuilder).FullName;

        public static readonly string TestCaseBuilderAttributeName = typeof(TestCaseBuilderAttribute).FullName;
        public static readonly string TestCaseBuilderInterfaceName = typeof(ITestCaseBuilder).FullName;

        public static readonly string TestDecoratorAttributeName = typeof(TestDecoratorAttribute).FullName;
        public static readonly string TestDecoratorInterfaceName = typeof(ITestDecorator).FullName;
        #endregion

        #endregion

        #region Identify SetUp and TearDown Methods
        public static bool IsSetUpMethod(MethodInfo method)
        {
            return Reflect.HasAttribute(method, NUnitFramework.SetUpAttribute, false);
        }

        public static bool IsTearDownMethod(MethodInfo method)
        {
            return Reflect.HasAttribute(method, NUnitFramework.TearDownAttribute, false);
        }

        public static bool IsFixtureSetUpMethod(MethodInfo method)
        {
            return Reflect.HasAttribute(method, NUnitFramework.FixtureSetUpAttribute, false);
        }

        public static bool IsFixtureTearDownMethod(MethodInfo method)
        {
            return Reflect.HasAttribute(method, NUnitFramework.FixtureTearDownAttribute, false);
        }

        #endregion

        #region Locate SetUp and TearDown Methods
        public static MethodInfo GetSetUpMethod(Type fixtureType)
		{
			return Reflect.GetMethodWithAttribute(fixtureType, SetUpAttribute,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				true);
		}

        public static MethodInfo GetTearDownMethod(Type fixtureType)
		{
			return Reflect.GetMethodWithAttribute(fixtureType, TearDownAttribute,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				true);
		}

		public static MethodInfo GetFixtureSetUpMethod(Type fixtureType)
		{
			return Reflect.GetMethodWithAttribute(fixtureType, FixtureSetUpAttribute,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				true);
		}

        public static MethodInfo GetFixtureTearDownMethod(Type fixtureType)
		{
			return Reflect.GetMethodWithAttribute(fixtureType, FixtureTearDownAttribute,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				true);
		}
		#endregion

		#region Locate ExceptionHandler
		public static MethodInfo GetDefaultExceptionHandler( Type fixtureType )
		{
			return Reflect.HasInterface( fixtureType, ExpectExceptionInterface )
				? GetExceptionHandler( fixtureType, "HandleException" )
				: null;
		}

		public static MethodInfo GetExceptionHandler( Type fixtureType, string name )
		{
			return Reflect.GetNamedMethod( 
				fixtureType, 
				name,
				new string[] { "System.Exception" },
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
		}
		#endregion

		#region Get Special Properties of Attributes

		#region IgnoreReason
		public static string GetIgnoreReason( System.Attribute attribute )
		{
			return Reflect.GetPropertyValue( attribute, "Reason" ) as string;
		}
		#endregion

		#region Description
		/// <summary>
		/// Method to return the description from an attribute
		/// </summary>
		/// <param name="attribute">The attribute to check</param>
		/// <returns>The description, if any, or null</returns>
		public static string GetDescription(System.Attribute attribute)
		{
			return Reflect.GetPropertyValue( attribute, "Description" ) as string;
		}
		#endregion

		#region ExpectedException Attrributes
		public static string GetHandler(System.Attribute attribute)
		{
			return Reflect.GetPropertyValue( attribute, "Handler" ) as string;
		}

		public static Type GetExceptionType(System.Attribute attribute)
		{
			return Reflect.GetPropertyValue( attribute, "ExceptionType" ) as Type;
		}

		public static string GetExceptionName(System.Attribute attribute)
		{
			return Reflect.GetPropertyValue( attribute, "ExceptionName" ) as string;
		}

		public static string GetExpectedMessage(System.Attribute attribute)
		{
			return Reflect.GetPropertyValue( attribute, "ExpectedMessage" ) as string;
		}

		public static string GetMatchType(System.Attribute attribute)
		{
			object matchEnum = Reflect.GetPropertyValue( attribute, "MatchType" );
			return matchEnum != null ? matchEnum.ToString() : null;
		}

		public static string GetUserMessage(System.Attribute attribute)
		{
			return Reflect.GetPropertyValue( attribute, "UserMessage" ) as string;
		}
		#endregion

		#endregion

		#region ApplyCommonAttributes
        /// <summary>
        /// Modify a newly constructed test based on a type or method by 
        /// applying any of NUnit's common attributes.
        /// </summary>
        /// <param name="member">The type or method from which the test was constructed</param>
        /// <param name="test">The test to which the attributes apply</param>
        public static void ApplyCommonAttributes(MemberInfo member, Test test)
        {
            ApplyCommonAttributes( Reflect.GetAttributes( member, false ), test );
        }

        /// <summary>
        /// Modify a newly constructed test based on an assembly by applying 
        /// any of NUnit's common attributes.
        /// </summary>
        /// <param name="assembly">The assembly from which the test was constructed</param>
        /// <param name="test">The test to which the attributes apply</param>
        public static void ApplyCommonAttributes(Assembly assembly, Test test)
        {
            ApplyCommonAttributes( Reflect.GetAttributes( assembly, false ), test );
        }

        /// <summary>
        /// Modify a newly constructed test by applying any of NUnit's common
        /// attributes, based on an input array of attributes. This method checks
        /// for all attributes, relying on the fact that specific attributes can only
        /// occur on those constructs on which they are allowed.
        /// </summary>
        /// <param name="attributes">An array of attributes possibly including NUnit attributes
        /// <param name="test">The test to which the attributes apply</param>
        public static void ApplyCommonAttributes(Attribute[] attributes, Test test)
        {
			IList categories = new ArrayList();
			ListDictionary properties = new ListDictionary();

            foreach (Attribute attribute in attributes)
            {
				Type attributeType = attribute.GetType();
				string attributeName = attributeType.FullName;
                bool isValid = test.RunState != RunState.NotRunnable;

                switch (attributeName)
                {
					case TestFixtureAttribute:
					case TestAttribute:
						if ( test.Description == null )
							test.Description = GetDescription( attribute );
						break;
					case DescriptionAttribute:
						test.Description = GetDescription( attribute );
						break;
					case ExplicitAttribute:
                        if (isValid)
                        {
                            test.RunState = RunState.Explicit;
                            test.IgnoreReason = GetIgnoreReason(attribute);
                        }
                        break;
                    case IgnoreAttribute:
                        if (isValid)
                        {
                            test.RunState = RunState.Ignored;
                            test.IgnoreReason = GetIgnoreReason(attribute);
                        }
                        break;
                    case PlatformAttribute:
                        PlatformHelper pHelper = new PlatformHelper();
                        if (isValid && !pHelper.IsPlatformSupported(attribute))
                        {
                            test.RunState = RunState.Skipped;
                            test.IgnoreReason = GetIgnoreReason(attribute);
							if ( test.IgnoreReason == null )
								test.IgnoreReason = pHelper.Reason;
                        }
                        break;
					case CultureAttribute:
						CultureDetector cultureDetector = new CultureDetector();
						if (isValid && !cultureDetector.IsCultureSupported(attribute))
						{
							test.RunState = RunState.Skipped;
							test.IgnoreReason = cultureDetector.Reason;
						}
						break;
					default:
						if ( Reflect.InheritsFrom( attributeType, CategoryAttribute ) )
						{	
							categories.Add( Reflect.GetPropertyValue( attribute, "Name" ) );
						}
						else if ( Reflect.InheritsFrom( attributeType, PropertyAttribute ) )
						{
							string name = (string)Reflect.GetPropertyValue( attribute, "Name" );
							if ( name != null && name != string.Empty )
							{
								object val = Reflect.GetPropertyValue( attribute, "Value" );
								properties[name] = val;
							}
						}
						break;
                }
            }

			test.Categories = categories;
			test.Properties = properties;
        }
		#endregion

		#region ApplyExpectedExceptionAttribute
		// TODO: Handle this with a separate ExceptionProcessor object
		public static void ApplyExpectedExceptionAttribute(MethodInfo method, TestMethod testMethod)
		{
			Attribute attribute = Reflect.GetAttribute(
                method, NUnitFramework.ExpectedExceptionAttribute, false );

			if (attribute != null)
			{
				testMethod.ExceptionExpected = true;

				Type expectedExceptionType = GetExceptionType( attribute );
				string expectedExceptionName = GetExceptionName( attribute );
				if ( expectedExceptionType != null )
					testMethod.ExpectedExceptionType = expectedExceptionType;
				else if ( expectedExceptionName != null )
					testMethod.ExpectedExceptionName = expectedExceptionName;
				
				testMethod.ExpectedMessage = GetExpectedMessage( attribute );
				testMethod.MatchType = GetMatchType( attribute );
				testMethod.UserMessage = GetUserMessage( attribute );

				string handlerName = GetHandler( attribute );
				if ( handlerName == null )
					testMethod.ExceptionHandler = GetDefaultExceptionHandler( testMethod.FixtureType );
				else
				{
					MethodInfo handler = GetExceptionHandler( testMethod.FixtureType, handlerName );
					if ( handler != null )
						testMethod.ExceptionHandler = handler;
					else
					{
						testMethod.RunState = RunState.NotRunnable;
						testMethod.IgnoreReason = string.Format( 
							"The specified exception handler {0} was not found", handlerName );
					}
				}
			}
		}
		#endregion

		#region GetAssertCount
		public static int GetAssertCount()
		{
			if ( assertType == null )
				foreach( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
					if ( assembly.GetName().Name == "nunit.framework" )
					{
						assertType = assembly.GetType( AssertType );
						break;
					}

			if ( assertType == null )
				return 0;

			PropertyInfo property = Reflect.GetNamedProperty( 
				assertType,
				"Counter", 
				BindingFlags.Public | BindingFlags.Static );

			if ( property == null )
				return 0;
		
			return (int)property.GetValue( null, new object[0] );
		}
		#endregion

		#region IsSuiteBuilder
		public static bool IsSuiteBuilder( Type type )
		{
			return Reflect.HasAttribute( type, SuiteBuilderAttribute, false )
				&& Reflect.HasInterface( type, SuiteBuilderInterface );
		}
		#endregion

		#region IsTestCaseBuilder
		public static bool IsTestCaseBuilder( Type type )
		{
			return Reflect.HasAttribute( type, TestCaseBuilderAttributeName, false )
				&& Reflect.HasInterface( type, TestCaseBuilderInterfaceName );
		}
		#endregion

		#region IsTestDecorator
		public static bool IsTestDecorator( Type type )
		{
			return Reflect.HasAttribute( type, TestDecoratorAttributeName, false )
				&& Reflect.HasInterface( type, TestDecoratorInterfaceName );
		}
		#endregion

		#region AllowOldStyleTests
		public static bool AllowOldStyleTests
		{
			get
			{
				try
				{
					NameValueCollection settings = (NameValueCollection)
						ConfigurationSettings.GetConfig("NUnit/TestCaseBuilder");
					if (settings != null)
					{
						string oldStyle = settings["OldStyleTestCases"];
						if (oldStyle != null)
							return Boolean.Parse(oldStyle);
					}
				}
				catch( Exception e )
				{
					Debug.WriteLine( e );
				}

				return false;
			}
		}
		#endregion

		#region BuildConfiguration
		public static string BuildConfiguration
		{
			get
			{
#if DEBUG
				if (Environment.Version.Major == 2)
					return "Debug2005";
				else
					return "Debug";
#else
				if (Environment.Version.Major == 2)
					return "Release2005";
				else
					return "Release";
#endif
			}
		}
		#endregion
	}
}
