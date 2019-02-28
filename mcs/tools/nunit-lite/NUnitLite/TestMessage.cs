
using System;
using System.Runtime.Serialization;

using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;

namespace Xamarin
{
	[Serializable]
	class TestStartedMessage
	{
		public ITest Test;
	}

	[Serializable]
	class TestFinishedMessage
	{
		public ITestResult TestResult;
	}

	[Serializable]
	class ExceptionMessage
	{
		public Exception Exception;
	}

	[Serializable]
	class ResultMessage
	{
		public ITestResult TestResult;
	}
}
