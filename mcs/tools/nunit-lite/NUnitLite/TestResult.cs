
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;

namespace Xamarin
{
	[Serializable]
	class TestResult : ITestResult
	{
		public TestResult (ITestResult testResult)
		{
			ResultState = testResult.ResultState;
			Test = new Test (testResult.Test);
		}

		public ResultState ResultState
		{
			get;
		}

		public string Name
		{
			get { throw new NotImplementedException (); }
		}

		public string FullName
		{
			get { throw new NotImplementedException (); }
		}

		public TimeSpan Duration
		{
			get { throw new NotImplementedException (); }
		}

		public string Message
		{
			get { throw new NotImplementedException (); }
		}

		public string StackTrace
		{
			get { throw new NotImplementedException (); }
		}

		public string ExceptionType
		{
			get { throw new NotImplementedException (); }
		}

		public int AssertCount
		{
			get { throw new NotImplementedException (); }
		}


		public int FailCount
		{
			get { throw new NotImplementedException (); }
		}

		public int PassCount
		{
			get { throw new NotImplementedException (); }
		}

		public int SkipCount
		{
			get { throw new NotImplementedException (); }
		}

		public int InconclusiveCount
		{
			get { throw new NotImplementedException (); }
		}

		public bool HasChildren
		{
			get { throw new NotImplementedException (); }
		}

		public IList<ITestResult> Children
		{
			get { throw new NotImplementedException (); }
		}

		public ITest Test
		{
			get;
		}

		public XmlNode ToXml(bool recursive)
		{
			throw new NotImplementedException ();
		}

		public XmlNode AddToXml(XmlNode parentNode, bool recursive)
		{
			throw new NotImplementedException ();
		}
	}
}
