using System;
using NUnit.Framework;
using Mono.CSharp;

namespace MonoTests.EvaluatorTest
{
	public class EvaluatorFixture
	{	
		[SetUp]
		public void Setup ()
		{
			Evaluator.Init (new string[0]);
		}
	}
}
