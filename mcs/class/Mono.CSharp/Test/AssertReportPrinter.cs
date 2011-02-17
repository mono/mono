using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CSharp;
using NUnit.Framework;

namespace MonoTests
{
	class AssertReportPrinter : ReportPrinter
	{
		public override void Print (AbstractMessage msg)
		{
			Assert.Fail (msg.Text);
		}
	}
}
