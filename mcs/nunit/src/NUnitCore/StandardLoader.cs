namespace NUnit.Runner
{
	using System;
	using System.Collections;
	using System.Reflection;
	using NUnit.Framework;
	
	/// <summary>
	/// Same as StandardLoader.
	/// TODO: Clean up "Unloading" concepts in framework
	/// </summary>
	public class UnloadingLoader : StandardLoader{}

	/// <summary>
	/// TestLoader that 
	/// </summary>
	public class StandardLoader : ITestLoader
	{
		#region Overidable loader implementatioons
		/// <summary>
		/// Attempts by all means possible to return a test for the given type.
		/// Check in the following order:
		/// 1. For a static Suite property, that implments ITest.
		/// 2. Tries to dynamically create a suite for the type.
		/// </summary>
		/// <param name="testClass"></param>
		/// <returns></returns>
		protected virtual ITest CoerceTestFrom(Type testClass)
		{
			try
			{
				ITest test = GetStaticSuiteProperty(testClass);
				if (test == null )
				{
					// try to extract a test suite automatically
					test = new TestSuite(testClass);
				}
				return test;
			}
			catch (Exception e)
			{
				throw new NUnitException("Error building test for class: "
					+ testClass.FullName,e);
			}
		}
		
		/// <summary>
		/// Searches for the type specified by the testClassName in the 
		/// specified assembly, and if found, attempts to coerce a test
		/// from the type.
		/// </summary>
		/// <param name="testClassName"></param>
		/// <param name="assemblyFileName"></param>
		/// <returns></returns>
		public virtual ITest LoadTest(string testClassName,
			string assemblyFileName)
		{
			try
			{
				return this.CoerceTestFrom(
					getAssembly(assemblyFileName).GetType(testClassName));
			}
			catch (Exception e)
			{
				throw new LoaderException("Error loading test class: "
					+ testClassName + "," + assemblyFileName, e);
			}
		}
		/// <summary>
		/// Determines if a Type is a test.
		/// </summary>
		/// <param name="typeToCheck"></param>
		protected virtual bool IsTestClass(Type typeToCheck) 
		{
			if(typeToCheck!=null)
			{
				if( typeToCheck.IsClass
					&& typeToCheck.IsPublic
					&& !typeToCheck.IsAbstract)
				{
					try 
					{
						if( (typeof(ITest).IsAssignableFrom(typeToCheck)
							// Has public single string constructor
							&& (typeToCheck.GetConstructor(new Type[]{typeof(string)})!=null))
							|| GetStaticSuiteProperty(typeToCheck)!= null)
						{
							return true;
						}
					} 
					catch(System.Security.SecurityException)
					{
						// eat security exceptions, since shouldn't 
						// have errors on classes we can't access
					}
				}
				return false;
			}
			else
			{
				throw new ArgumentNullException("typeToCheck");
			}
		}
		/// <summary>
		/// Uses reflection to obtain the suite property for the Type
		/// </summary>
		/// <param name="testClass"></param>
		/// <returns>The Suite property of the Type, or null if the property 
		/// does not exist</returns>
		protected virtual TestSuite GetStaticSuiteProperty(Type testClass) 
		{
			if(testClass!=null)
			{
				TestSuite test = null;
				PropertyInfo suiteProperty = testClass.GetProperty("Suite"
					,BindingFlags.Static|BindingFlags.Public
					,Type.DefaultBinder			// unknown
					,typeof(ITest)				// Itest return type
					,Type.EmptyTypes			// no parameters
					,new ParameterModifier[0]	// unknown
					);
				if (suiteProperty != null )
				{
					test = (TestSuite)suiteProperty.GetValue(null, new Object[0]);
				}
				return test;
			}
			else
			{
				throw new ArgumentNullException ("testClass");
			}
		}
		private Assembly getAssembly(string assemblyFileName)
		{
			try
			{
				return Assembly.LoadFrom(assemblyFileName);
			}
			catch(ArgumentNullException)
			{
				throw new ArgumentNullException("assemblyFileName");
			}
		}
		#endregion
		
		#region ILoader Methods
		/// <summary>
		/// Implements ILoader.GetLoadName().
		/// </summary>
		/// <param name="test"></param>
		/// <returns></returns>
		public virtual string GetLoadName(ITest test)
		{
			Type testType = test.GetType();
			if(testType.Equals(typeof(TestSuite)))
			{
				string tname = test.ToString();
				testType = Type.GetType(tname);
			}
			if(testType != null)
				return testType.FullName+","+testType.Assembly.CodeBase;
			else
				return string.Empty;
		}
		/// <summary>
		/// Implements ILoader.LoadTest().
		/// Loads an instance of the test class specified by the 
		/// AssemblyQualifiedName. The assembly qualified name
		/// contains the Full Clas Name, followed by the CodeBase
		/// (file or url) of the assembly. If the class is found,
		/// the loader will attempt to return a TestSuite for the
		/// class. Trying first the static Suite property, followed 
		/// by trying to dynamically create a suite for the class.
		/// </summary>
		/// <param name="assemblyQualifiedName">The qualified name
		/// for the class taking the form 
		/// "Namespace.ClassName,AssemblyFileName" without the quotes.
		/// Assembly file name can be a fulied qualified path name, or
		/// a URL.</param>
		/// <returns></returns>
		public virtual ITest LoadTest(string assemblyQualifiedName)
		{
			if(assemblyQualifiedName==null)
				throw new ArgumentNullException("assemblyQualifiedName");

			string[] nameParts = assemblyQualifiedName.Split(new Char[]{','});
			if(nameParts.Length >= 1)
			{
				return this.LoadTest(nameParts[0].Trim(),nameParts[1].Trim());
			}
			else
			{
				throw new ArgumentException("Expected an Assembly Qualified Class"
					+ " Name, containing the file name of the assembly",
					"assemblyQualifiedName");
			}
		}
		#endregion

		/// <summary>
		/// Examies all types in the specified assembly and returns a list of those
		/// types that can be coerced into tests.
		/// </summary>
		/// <param name="assemblyFileName"></param>
		/// <returns></returns>
		public virtual Type[] GetTestTypes(string assemblyFileName)
		{
			Assembly assembly = getAssembly(assemblyFileName);
			ArrayList Tests = new ArrayList(assembly.GetExportedTypes().Length);
			foreach(Type typeToCheck in assembly.GetExportedTypes())
			{
				if(this.IsTestClass(typeToCheck))
				{
					Tests.Add(typeToCheck);
				}
			}
			Type[] ret = new Type[Tests.Count];
			Tests.CopyTo(ret);
			return ret;
		}
	}
}