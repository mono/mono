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

	/// <summary>
	///		Test Class.
	/// </summary>
	public abstract class Test : LongLivingMarshalByRefObject, ITest, IComparable
	{
		private string fullName;
		private string testName;
		private int assemblyKey;
		private bool shouldRun;
		private string ignoreReason;
		private string description;

		public Test( string name ) : this( name, 0 ) { }

		public Test( string name, int assemblyKey )
		{
			fullName = testName = name;
			this.assemblyKey = assemblyKey;
		}

		protected Test( string pathName, string testName ) 
			: this( pathName, testName, 0 ) { }

		protected Test( string pathName, string testName, int assemblyKey ) 
		{ 
			fullName = pathName + "." + testName;
			this.testName = testName;
			this.assemblyKey = assemblyKey;
			shouldRun = true;
		}

		public string IgnoreReason
		{
			get { return ignoreReason; }
			set { ignoreReason = value; }
		}

		public virtual bool ShouldRun
		{
			get { return shouldRun; }
			set { shouldRun = value; }
		}

		public String Description
		{
			get { return description; }
			set { description = value; }
		}

		public string FullName 
		{
			get { return fullName; }
		}

		public string Name
		{
			get { return testName; }
		}

		/// <summary>
		/// If the name is a path, this just returns the file part
		/// </summary>
		public string ShortName
		{
			get
			{
				string name = Name;
				int val = name.LastIndexOf("\\");
				if(val != -1)
					name = name.Substring(val+1);
				return name;
			}
		}

		public int AssemblyKey
		{
			get { return assemblyKey; }
			set { assemblyKey = value; }
		}

		public string UniqueName
		{
			get { return string.Format( "[{0}]{1}", assemblyKey, fullName ); }
		}

		public abstract int CountTestCases { get; }
		public abstract bool IsSuite { get; }
		public abstract bool IsFixture{ get; }
		public abstract bool IsTestCase{ get; }
		public abstract ArrayList Tests { get; }
		
		public abstract TestResult Run(EventListener listener);

		protected MethodInfo FindMethodByAttribute(object fixture, Type type)
		{
			foreach(MethodInfo method in fixture.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic))
			{
				if(method.IsDefined(type,true)) 
				{
					return method;
				}
			}
			return null;
		}

		protected void InvokeMethod(MethodInfo method, object fixture) 
		{
			if(method != null)
			{
				try
				{
					method.Invoke(fixture, null);
				}
				catch(TargetInvocationException e)
				{
					Exception inner = e.InnerException;
					throw new NunitException("Rethrown",inner);
				}
			}
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			Test other = obj as Test;
			
			if ( other == null )
				return -1;

			return this.FullName.CompareTo( other.FullName );
		}

		#endregion
	}
}
