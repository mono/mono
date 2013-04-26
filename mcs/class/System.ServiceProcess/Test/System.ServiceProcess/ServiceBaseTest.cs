//
// ServiceBaseTest.cs -
//	NUnit Test Cases for ServiceBase
//
// Author:
//	Andres G. Aragoneses  (andres@7digital.com)
//
// Copyright (C) 2013 7digital Media, Ltd (http://www.7digital.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ServiceProcess;
using NUnit.Framework;

namespace Test
{
	[TestFixture]
	public class ServiceBaseTest
	{
		const int SOME_ERROR_CODE = 1;

		public partial class ServiceFoo : ServiceBase
		{
			public ServiceFoo ()
			{
				InitializeComponent ();
			}

			protected override void OnStart (string[] args)
			{
			}

			protected override void OnStop ()
			{
				ExitCode = SOME_ERROR_CODE;
			}

			public void StartHook ()
			{
				OnStart (new string [] { });
			}
		}

		[Test]
		public void StopCallsOnStop ()
		{
			var s = new ServiceFoo ();
			Assert.AreEqual (0, s.ExitCode);
			s.Stop ();
			Assert.AreEqual (SOME_ERROR_CODE, s.ExitCode);
		}

		[Test]
		public void ExitCodeIsNotResetByBaseClassServiceBaseBetweenRuns ()
		{
			var s = new ServiceFoo ();
			Assert.AreEqual (0, s.ExitCode);
			s.Stop ();
			Assert.AreEqual (SOME_ERROR_CODE, s.ExitCode);
			s.StartHook ();
			Assert.AreEqual (SOME_ERROR_CODE, s.ExitCode);
		}

		partial class ServiceFoo
		{
			/// <summary>
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.IContainer components = null;

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
			protected override void Dispose(bool disposing)
			{
				if (disposing && (components != null))
				{
					components.Dispose();
				}
				base.Dispose(disposing);
			}

			#region Component Designer generated code

			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			private void InitializeComponent()
			{
				components = new System.ComponentModel.Container();
				this.ServiceName = "ServiceFoo";
			}

			#endregion
		}
	}
}

