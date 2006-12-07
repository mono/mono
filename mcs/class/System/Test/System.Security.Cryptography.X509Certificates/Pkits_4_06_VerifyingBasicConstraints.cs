//
// Pkits_4_06_VerifyingBasicConstraints.cs -
//	NUnit tests for Pkits 4.6 : Verifying Basic Constraints
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	/*
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_06_VerifyingBasicConstraints: PkitsTest {

		public X509Certificate2 MissingbasicConstraintsCACert {
			get { return GetCertificate ("MissingbasicConstraintsCACert.crt"); }
		}

		public X509Certificate2 BasicConstraintsCriticalcAFalseCACert {
			get { return GetCertificate ("basicConstraintsCriticalcAFalseCACert.crt"); }
		}

		public X509Certificate2 BasicConstraintsNotCriticalCACert {
			get { return GetCertificate ("basicConstraintsNotCriticalCACert.crt"); }
		}

		public X509Certificate2 BasicConstraintsNotCriticalcAFalseCACert {
			get { return GetCertificate ("basicConstraintsNotCriticalcAFalseCACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint0CACert {
			get { return GetCertificate ("pathLenConstraint0CACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint0subCACert {
			get { return GetCertificate ("pathLenConstraint0subCACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint0subCA2Cert {
			get { return GetCertificate ("pathLenConstraint0subCA2Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6CACert {
			get { return GetCertificate ("pathLenConstraint6CACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subCA0Cert {
			get { return GetCertificate ("pathLenConstraint6subCA0Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subsubCA00Cert {
			get { return GetCertificate ("pathLenConstraint6subsubCA00Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subCA1Cert {
			get { return GetCertificate ("pathLenConstraint6subCA1Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subsubCA11Cert {
			get { return GetCertificate ("pathLenConstraint6subsubCA11Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subsubsubCA11XCert {
			get { return GetCertificate ("pathLenConstraint6subsubsubCA11XCert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subCA4Cert {
			get { return GetCertificate ("pathLenConstraint6subCA4Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subsubCA41Cert {
			get { return GetCertificate ("pathLenConstraint6subsubCA41Cert.crt"); }
		}

		public X509Certificate2 PathLenConstraint6subsubsubCA41XCert {
			get { return GetCertificate ("pathLenConstraint6subsubsubCA41XCert.crt"); }
		}

		public X509Certificate2 PathLenConstraint0SelfIssuedCACert {
			get { return GetCertificate ("pathLenConstraint0SelfIssuedCACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint1CACert {
			get { return GetCertificate ("pathLenConstraint1CACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint1SelfIssuedCACert {
			get { return GetCertificate ("pathLenConstraint1SelfIssuedCACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint1subCACert {
			get { return GetCertificate ("pathLenConstraint1subCACert.crt"); }
		}

		public X509Certificate2 PathLenConstraint1SelfIssuedsubCACert {
			get { return GetCertificate ("pathLenConstraint1SelfIssuedsubCACert.crt"); }
		}

		[Test]
		public void T01_InvalidMissingBasicConstaints ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidMissingbasicConstraintsTest1EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (MissingbasicConstraintsCACert, chain.ChainElements[1].Certificate, "MissingbasicConstraintsCACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[1].ChainElementStatus, "MissingbasicConstraintsCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T02_InvalidCAFalse ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidcAFalseTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicConstraintsCriticalcAFalseCACert, chain.ChainElements[1].Certificate, "BasicConstraintsCriticalcAFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[1].ChainElementStatus, "BasicConstraintsCriticalcAFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T03_InvalidCAFalse ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidcAFalseTest3EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicConstraintsNotCriticalcAFalseCACert, chain.ChainElements[1].Certificate, "basicConstraintsNotCriticalcAFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[1].ChainElementStatus, "basicConstraintsNotCriticalcAFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T04_ValidBasicConstraintsNotCritical ()
		{
			X509Certificate2 ee = GetCertificate ("ValidbasicConstraintsNotCriticalTest4EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicConstraintsNotCriticalCACert, chain.ChainElements[1].Certificate, "BasicConstraintsNotCriticalCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicConstraintsNotCriticalCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T05_InvalidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidpathLenConstraintTest5EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0subCACert, chain.ChainElements[1].Certificate, "PathLenConstraint0subCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint0subCACert.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[2].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T06_InvalidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidpathLenConstraintTest6EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0subCACert, chain.ChainElements[1].Certificate, "pathLenConstraint0subCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "pathLenConstraint0subCACert.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[2].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T07_ValidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("ValidpathLenConstraintTest7EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[1].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T08_ValidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("ValidpathLenConstraintTest8EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[1].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T09_InvalidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidpathLenConstraintTest9EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint6subsubCA00Cert, chain.ChainElements[1].Certificate, "PathLenConstraint6subsubCA00Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint6subsubCA00Cert.Status");
			Assert.AreEqual (PathLenConstraint6subCA0Cert, chain.ChainElements[2].Certificate, "PathLenConstraint6subCA0Cert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint6subCA0Cert.Status");
			Assert.AreEqual (PathLenConstraint6CACert, chain.ChainElements[3].Certificate, "PathLenConstraint6CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint6CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[4].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T10_InvalidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidpathLenConstraintTest10EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint6subsubCA00Cert, chain.ChainElements[1].Certificate, "PathLenConstraint6subsubCA00Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint6subsubCA00Cert.Status");
			Assert.AreEqual (PathLenConstraint6subCA0Cert, chain.ChainElements[2].Certificate, "PathLenConstraint6subCA0Cert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint6subCA0Cert.Status");
			Assert.AreEqual (PathLenConstraint6CACert, chain.ChainElements[3].Certificate, "PathLenConstraint6CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint6CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[4].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T11_InvalidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidpathLenConstraintTest11EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint6subsubsubCA11XCert, chain.ChainElements[1].Certificate, "PathLenConstraint6subsubsubCA11XCert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint6subsubsubCA11XCert.Status");
			Assert.AreEqual (PathLenConstraint6subsubCA11Cert, chain.ChainElements[2].Certificate, "PathLenConstraint6subsubCA11Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint6subsubCA11Cert.Status");
			Assert.AreEqual (PathLenConstraint6subCA1Cert, chain.ChainElements[3].Certificate, "PathLenConstraint6subCA1Cert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint6subCA1Cert.Status");
			Assert.AreEqual (PathLenConstraint6CACert, chain.ChainElements[4].Certificate, "PathLenConstraint6CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "PathLenConstraint6CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[5].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[5].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T12_InvalidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidpathLenConstraintTest12EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint6subsubsubCA11XCert, chain.ChainElements[1].Certificate, "PathLenConstraint6subsubsubCA11XCert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint6subsubsubCA11XCert.Status");
			Assert.AreEqual (PathLenConstraint6subsubCA11Cert, chain.ChainElements[2].Certificate, "PathLenConstraint6subsubCA11Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint6subsubCA11Cert.Status");
			Assert.AreEqual (PathLenConstraint6subCA1Cert, chain.ChainElements[3].Certificate, "PathLenConstraint6subCA1Cert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint6subCA1Cert.Status");
			Assert.AreEqual (PathLenConstraint6CACert, chain.ChainElements[4].Certificate, "PathLenConstraint6CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "PathLenConstraint6CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[5].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[5].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T13_ValidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("ValidpathLenConstraintTest13EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint6subsubsubCA41XCert, chain.ChainElements[1].Certificate, "PathLenConstraint6subsubsubCA11XCert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint6subsubsubCA11XCert.Status");
			Assert.AreEqual (PathLenConstraint6subsubCA41Cert, chain.ChainElements[2].Certificate, "PathLenConstraint6subsubCA11Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint6subsubCA11Cert.Status");
			Assert.AreEqual (PathLenConstraint6subCA4Cert, chain.ChainElements[3].Certificate, "PathLenConstraint6subCA1Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint6subCA1Cert.Status");
			Assert.AreEqual (PathLenConstraint6CACert, chain.ChainElements[4].Certificate, "PathLenConstraint6CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "PathLenConstraint6CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[5].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[5].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T14_ValidPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("ValidpathLenConstraintTest14EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint6subsubsubCA41XCert, chain.ChainElements[1].Certificate, "PathLenConstraint6subsubsubCA11XCert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint6subsubsubCA11XCert.Status");
			Assert.AreEqual (PathLenConstraint6subsubCA41Cert, chain.ChainElements[2].Certificate, "PathLenConstraint6subsubCA11Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint6subsubCA11Cert.Status");
			Assert.AreEqual (PathLenConstraint6subCA4Cert, chain.ChainElements[3].Certificate, "PathLenConstraint6subCA1Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint6subCA1Cert.Status");
			Assert.AreEqual (PathLenConstraint6CACert, chain.ChainElements[4].Certificate, "PathLenConstraint6CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "PathLenConstraint6CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[5].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[5].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T15_ValidSelfIssuedPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("ValidSelfIssuedpathLenConstraintTest15EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0SelfIssuedCACert, chain.ChainElements[1].Certificate, "PathLenConstraint0SelfIssuedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint0SelfIssuedCACert.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[2].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T15_ValidSelfIssuedPathLenConstraint_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidSelfIssuedpathLenConstraintTest15EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this IS valid wrt RFC3280
			// The problem seems that the Self Issued CA certificates 
			// from the test suite don't have any, even empty, CRL

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0SelfIssuedCACert, chain.ChainElements[1].Certificate, "PathLenConstraint0SelfIssuedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint0SelfIssuedCACert.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[2].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T16_InvalidSelfIssuedPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidSelfIssuedpathLenConstraintTest16EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0subCA2Cert, chain.ChainElements[1].Certificate, "pathLenConstraint0subCA2Cert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "pathLenConstraint0subCA2Cert.Status");
			Assert.AreEqual (PathLenConstraint0SelfIssuedCACert, chain.ChainElements[2].Certificate, "PathLenConstraint0SelfIssuedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint0SelfIssuedCACert.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[3].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[4].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T16_InvalidSelfIssuedPathLenConstraint_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidSelfIssuedpathLenConstraintTest16EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			// note again the RevocationStatusUnknown because of the CRL-less self-issued CA
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint0subCA2Cert, chain.ChainElements[1].Certificate, "pathLenConstraint0subCA2Cert");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[1].ChainElementStatus, "pathLenConstraint0subCA2Cert.Status");
			Assert.AreEqual (PathLenConstraint0SelfIssuedCACert, chain.ChainElements[2].Certificate, "PathLenConstraint0SelfIssuedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint0SelfIssuedCACert.Status");
			Assert.AreEqual (PathLenConstraint0CACert, chain.ChainElements[3].Certificate, "PathLenConstraint0CACert");
			CheckChainStatus (X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint0CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[4].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T17_ValidSelfIssuedPathLenConstraint ()
		{
			X509Certificate2 ee = GetCertificate ("ValidSelfIssuedpathLenConstraintTest17EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint1SelfIssuedsubCACert, chain.ChainElements[1].Certificate, "PathLenConstraint1SelfIssuedsubCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint1SelfIssuedsubCACert.Status");
			Assert.AreEqual (PathLenConstraint1subCACert, chain.ChainElements[2].Certificate, "PathLenConstraint1subCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint1subCACert.Status");
			Assert.AreEqual (PathLenConstraint1SelfIssuedCACert, chain.ChainElements[3].Certificate, "PathLenConstraint1SelfIssuedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint1SelfIssuedCACert.Status");
			Assert.AreEqual (PathLenConstraint1CACert, chain.ChainElements[4].Certificate, "PathLenConstraint1CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "PathLenConstraint1CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[5].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[5].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T17_ValidSelfIssuedPathLenConstraint_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidSelfIssuedpathLenConstraintTest17EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this IS valid wrt RFC3280
			// The problem seems that the Self Issued CA certificates 
			// from the test suite don't have any, even empty, CRL

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (PathLenConstraint1SelfIssuedsubCACert, chain.ChainElements[1].Certificate, "PathLenConstraint1SelfIssuedsubCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "PathLenConstraint1SelfIssuedsubCACert.Status");
			Assert.AreEqual (PathLenConstraint1subCACert, chain.ChainElements[2].Certificate, "PathLenConstraint1subCACert");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[2].ChainElementStatus, "PathLenConstraint1subCACert.Status");
			Assert.AreEqual (PathLenConstraint1SelfIssuedCACert, chain.ChainElements[3].Certificate, "PathLenConstraint1SelfIssuedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "PathLenConstraint1SelfIssuedCACert.Status");
			Assert.AreEqual (PathLenConstraint1CACert, chain.ChainElements[4].Certificate, "PathLenConstraint1CACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[4].ChainElementStatus, "PathLenConstraint1CACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[5].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[5].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
