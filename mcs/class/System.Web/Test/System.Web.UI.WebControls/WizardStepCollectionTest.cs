//
// Tests for System.Web.UI.WebControls.WizardStepCollectionTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


#if NET_2_0

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class WizardStepCollectionTest
	{
		
		[Test]
		public void WizardStepCollection_DefaultProperty ()
		{
			Wizard w = new Wizard();
			Assert.AreEqual (typeof (WizardStepCollection), w.WizardSteps.GetType (), "WizardStepCollection");
			Assert.AreEqual (0,w.WizardSteps.Count, "Count");
			Assert.AreEqual (false, w.WizardSteps.IsReadOnly, "IsReadOnly");
			Assert.AreEqual (false, w.WizardSteps.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (w.WizardSteps, w.WizardSteps.SyncRoot, "SyncRoot");
		}

		[Test]
		public void WizardStepCollection_Add ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			try {
				w.WizardSteps.Add (step1);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (1, w.WizardSteps.Count, "Add step fail");
		}

		[Test]
		public void WizardStepCollection_AddAt ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();

			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.AddAt (0, step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count fail");
			Assert.AreEqual (step2, w.WizardSteps[0], "Step index fail");
		}

		public void WizardStepCollection_Clear ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();

			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.Add (step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count fail");
			w.WizardSteps.Clear ();
			Assert.AreEqual (0, w.WizardSteps.Count, "Step clear fail");
		}

		[Test]
		public void WizardStepCollection_Contains ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();

			try {
				w.WizardSteps.Add (step1);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (1, w.WizardSteps.Count, "Step count fail");
			Assert.AreEqual (true, w.WizardSteps.Contains (step1), "Step contains fail#1");
			Assert.AreEqual (false, w.WizardSteps.Contains (step2), "Step contains fail#2");
		}

		[Test]
		public void WizardStepCollection_CopyTo ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();
			

			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.Add (step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count fail");
			WizardStep[] steps = new WizardStep [w.WizardSteps.Count] ;
			w.WizardSteps.CopyTo (steps, 0);
			Assert.AreEqual (2, steps.GetLength (0), "Copyto elements count");
			Assert.AreEqual (step1, steps[0], "Copyto elements equal#1");
			Assert.AreEqual (step2, steps[1], "Copyto elements equal#2");
		}

		[Test]
		public void WizardStepCollection_GetEnumerator ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();


			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.Add (step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count fail");
			IEnumerator numerator = w.WizardSteps.GetEnumerator ();
			numerator.Reset();
			numerator.MoveNext ();
			Assert.AreEqual (step1, numerator.Current, "Enumerator item value#1");
			numerator.MoveNext ();
			Assert.AreEqual (step2, numerator.Current, "Enumerator item value#2");
		}

		[Test]
		public void WizardStepCollection_Insert ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();

			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.Insert (0, step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count fail");
			Assert.AreEqual (step2, w.WizardSteps[0], "Step index fail");
		}

		[Test]
		public void WizardStepCollection_Remove ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();

			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.Add (step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count before remove fail");
			try {
				w.WizardSteps.Remove (step1);
				w.WizardSteps.Remove (step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (0, w.WizardSteps.Count, "Step count after remove fail");
		}

		[Test]
		public void WizardStepCollection_RemoveAt ()
		{
			Wizard w = new Wizard ();
			WizardStep step1 = new WizardStep ();
			WizardStep step2 = new WizardStep ();

			try {
				w.WizardSteps.Add (step1);
				w.WizardSteps.Add (step2);
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (2, w.WizardSteps.Count, "Step count before removeat fail");
			try {
				w.WizardSteps.RemoveAt (0);
				
			}
			catch (Exception e) {
				Assert.Fail (e.Message);
			}
			Assert.AreEqual (1, w.WizardSteps.Count, "Step count after removeat fail");
			Assert.AreEqual (step2, w.WizardSteps[0], "Item value after remove");
		}
	}
}
#endif
