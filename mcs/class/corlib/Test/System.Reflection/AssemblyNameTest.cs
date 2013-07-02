//
// AssemblyNameTest.cs - NUnit Test Cases for AssemblyName
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;
#if !TARGET_JVM && !MOBILE
using System.Reflection.Emit;
#endif
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Reflection {

[TestFixture]
public class AssemblyNameTest {
	private AssemblyName an;

	private string tempDir = Path.Combine (Path.GetTempPath (), "MonoTests.System.Reflection.AssemblyNameTest");

#if !TARGET_JVM // Thread.GetDomain is not supported for TARGET_JVM.
	private AppDomain domain;
#endif // TARGET_JVM

	// created with "sn -o test.snk test.txt"
	static byte[] keyPair = {
		0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53,
		0x41, 0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
		0x3D, 0xBD, 0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0,
		0x58, 0x07, 0x2B, 0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22,
		0xDB, 0x20, 0xB2, 0xA9, 0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64,
		0x2F, 0x5D, 0x8D, 0xEB, 0x78, 0x02, 0xF7, 0xA5, 0x36, 0x77,
		0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68, 0xDB, 0xEB, 0x24, 0x09,
		0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E, 0x59, 0x54, 0x4A,
		0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F, 0xA8, 0x3E,
		0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D, 0x10,
		0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB,
		0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F,
		0xF5, 0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D,
		0xD3, 0x56, 0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07,
		0x93, 0x44, 0x00, 0x4A, 0xF8, 0xFF, 0xA4, 0xCB, 0x73, 0xC0,
		0x6A, 0x62, 0xB4, 0xB7, 0xC8, 0x92, 0x58, 0x87, 0xCD, 0x07,
		0x0C, 0x7D, 0x6C, 0xC1, 0x4A, 0xFC, 0x82, 0x57, 0x0E, 0x43,
		0x85, 0x09, 0x75, 0x98, 0x51, 0xBB, 0x35, 0xF5, 0x64, 0x83,
		0xC7, 0x79, 0x89, 0x5C, 0x55, 0x36, 0x66, 0xAB, 0x27, 0xA4,
		0xD9, 0xD4, 0x7E, 0x6B, 0x67, 0x64, 0xC1, 0x54, 0x4E, 0x37,
		0xF1, 0x4E, 0xCA, 0xB3, 0xE5, 0x63, 0x91, 0x57, 0x12, 0x14,
		0xA6, 0xEA, 0x8F, 0x8F, 0x2B, 0xFE, 0xF3, 0xE9, 0x16, 0x08,
		0x2B, 0x86, 0xBC, 0x26, 0x0D, 0xD0, 0xC6, 0xC4, 0x1A, 0x72,
		0x43, 0x76, 0xDC, 0xFF, 0x28, 0x52, 0xA1, 0xDE, 0x8D, 0xFA,
		0xD5, 0x1F, 0x0B, 0xB5, 0x4F, 0xAF, 0x06, 0x79, 0x11, 0xEE,
		0xA8, 0xEC, 0xD3, 0x74, 0x55, 0xA2, 0x80, 0xFC, 0xF8, 0xD9,
		0x50, 0x69, 0x48, 0x01, 0xC2, 0x5A, 0x04, 0x56, 0xB4, 0x3E,
		0x24, 0x32, 0x20, 0xB5, 0x2C, 0xDE, 0xBB, 0xBD, 0x13, 0xFD,
		0x13, 0xF7, 0x03, 0x3E, 0xE3, 0x37, 0x84, 0x74, 0xE7, 0xD0,
		0x5E, 0x9E, 0xB6, 0x26, 0xAE, 0x6E, 0xB0, 0x55, 0x6A, 0x52,
		0x63, 0x6F, 0x5A, 0x9D, 0xF2, 0x67, 0xD6, 0x61, 0x4F, 0x7A,
		0x45, 0xEE, 0x5C, 0x3D, 0x2B, 0x7C, 0xB2, 0x40, 0x79, 0x54,
		0x84, 0xD1, 0xBE, 0x61, 0x3E, 0x5E, 0xD6, 0x18, 0x8E, 0x14,
		0x98, 0xFC, 0x35, 0xBF, 0x5F, 0x1A, 0x20, 0x2E, 0x1A, 0xD8,
		0xFF, 0xC4, 0x6B, 0xC0, 0xC9, 0x7D, 0x06, 0xEF, 0x09, 0xF9,
		0xF3, 0x69, 0xFC, 0xBC, 0xA2, 0xE6, 0x80, 0x22, 0xB9, 0x79,
		0x7E, 0xEF, 0x57, 0x9F, 0x49, 0xE1, 0xBC, 0x0D, 0xB6, 0xA1,
		0xFE, 0x8D, 0xBC, 0xBB, 0xA3, 0x05, 0x02, 0x6B, 0x04, 0x45,
		0xF7, 0x5D, 0xEE, 0x43, 0x06, 0xD6, 0x9C, 0x94, 0x48, 0x1A,
		0x0B, 0x9C, 0xBC, 0xB4, 0x4E, 0x93, 0x60, 0x87, 0xCD, 0x58,
		0xD6, 0x9A, 0x39, 0xA6, 0xC0, 0x7F, 0x8E, 0xFF, 0x25, 0xC1,
		0xD7, 0x2C, 0xF6, 0xF4, 0x6F, 0x24, 0x52, 0x0B, 0x39, 0x42,
		0x1B, 0x0D, 0x04, 0xC1, 0x93, 0x2A, 0x19, 0x1C, 0xF0, 0xB1,
		0x9B, 0xC1, 0x24, 0x6D, 0x1B, 0x0B, 0xDA, 0x1C, 0x8B, 0x72,
		0x48, 0xF0, 0x3E, 0x52, 0xBF, 0x0A, 0x84, 0x3A, 0x9B, 0xC8,
		0x6D, 0x13, 0x1E, 0x72, 0xF4, 0x46, 0x93, 0x88, 0x1A, 0x5F,
		0x4C, 0x3C, 0xE5, 0x9D, 0x6E, 0xBB, 0x4E, 0xDD, 0x5D, 0x1F,
		0x11, 0x40, 0xF4, 0xD7, 0xAF, 0xB3, 0xAB, 0x9A, 0x99, 0x15,
		0xF0, 0xDC, 0xAA, 0xFF, 0x9F, 0x2D, 0x9E, 0x56, 0x4F, 0x35,
		0x5B, 0xBA, 0x06, 0x99, 0xEA, 0xC6, 0xB4, 0x48, 0x51, 0x17,
		0x1E, 0xD1, 0x95, 0x84, 0x81, 0x18, 0xC0, 0xF1, 0x71, 0xDE,
		0x44, 0x42, 0x02, 0x06, 0xAC, 0x0E, 0xA8, 0xE2, 0xF3, 0x1F,
		0x96, 0x1F, 0xBE, 0xB6, 0x1F, 0xB5, 0x3E, 0xF6, 0x81, 0x05,
		0x20, 0xFA, 0x2E, 0x40, 0x2E, 0x4D, 0xA0, 0x0E, 0xDA, 0x42,
		0x9C, 0x05, 0xAA, 0x9E, 0xAF, 0x5C, 0xF7, 0x3A, 0x3F, 0xBB,
		0x91, 0x73, 0x45, 0x27, 0xA8, 0xA2, 0x07, 0x4A, 0xEF, 0x59,
		0x1E, 0x97, 0x9D, 0xE0, 0x30, 0x5A, 0x83, 0xCE, 0x1E, 0x57,
		0x32, 0x89, 0x43, 0x41, 0x28, 0x7D, 0x14, 0x8D, 0x8B, 0x41,
		0x1A, 0x56, 0x76, 0x43, 0xDB, 0x64, 0x86, 0x41, 0x64, 0x8D,
		0x4C, 0x91, 0x83, 0x4E, 0xF5, 0x6C };

#if !NET_2_0
	static byte [] kp_token = { 0xff, 0xef, 0x94, 0x53, 0x67, 0x69, 0xda, 0x06 };
#endif

	static byte [] publicKey1 = {
		0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00,
		0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00,
		0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00,
		0x01, 0x00, 0x3d, 0xbd, 0x72, 0x08, 0xc6, 0x2b, 0x0e, 0xa8,
		0xc1, 0xc0, 0x58, 0x07, 0x2b, 0x63, 0x5f, 0x7c, 0x9a, 0xbd,
		0xcb, 0x22, 0xdb, 0x20, 0xb2, 0xa9, 0xda, 0xda, 0xef, 0xe8,
		0x00, 0x64, 0x2f, 0x5d, 0x8d, 0xeb, 0x78, 0x02, 0xf7, 0xa5,
		0x36, 0x77, 0x28, 0xd7, 0x55, 0x8d, 0x14, 0x68, 0xdb, 0xeb,
		0x24, 0x09, 0xd0, 0x2b, 0x13, 0x1b, 0x92, 0x6e, 0x2e, 0x59,
		0x54, 0x4a, 0xac, 0x18, 0xcf, 0xc9, 0x09, 0x02, 0x3f, 0x4f,
		0xa8, 0x3e, 0x94, 0x00, 0x1f, 0xc2, 0xf1, 0x1a, 0x27, 0x47,
		0x7d, 0x10, 0x84, 0xf5, 0x14, 0xb8, 0x61, 0x62, 0x1a, 0x0c,
		0x66, 0xab, 0xd2, 0x4c, 0x4b, 0x9f, 0xc9, 0x0f, 0x3c, 0xd8,
		0x92, 0x0f, 0xf5, 0xff, 0xce, 0xd7, 0x6e, 0x5c, 0x6f, 0xb1,
		0xf5, 0x7d, 0xd3, 0x56, 0xf9, 0x67, 0x27, 0xa4, 0xa5, 0x48,
		0x5b, 0x07, 0x93, 0x44, 0x00, 0x4a, 0xf8, 0xff, 0xa4, 0xcb };

	static byte [] pk_token1 = { 0xce, 0x52, 0x76, 0xd8, 0x68, 0x7e, 0Xc6, 0xdc };

	static byte [] publicKey2 = {
		0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00,
		0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00,
		0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00,
		0x01, 0x00, 0x6d, 0xfd, 0xba, 0xb9, 0x9d, 0x43, 0xf1, 0xef,
		0x33, 0xe2, 0xbd, 0x2d, 0x7c, 0x26, 0xe2, 0x9d, 0x37, 0x4a,
		0xdf, 0xb5, 0x63, 0x12, 0x90, 0x35, 0x90, 0x24, 0x8a, 0xe7,
		0x5a, 0xc5, 0xa3, 0x3a, 0x84, 0xee, 0x9b, 0xd1, 0xac, 0x3a,
		0x59, 0x2b, 0x91, 0x97, 0x83, 0x01, 0x4f, 0x92, 0x01, 0xc6,
		0x3b, 0x96, 0x20, 0x19, 0xeb, 0xdc, 0x2c, 0x6f, 0x1f, 0xbb,
		0x04, 0x9b, 0x62, 0x39, 0xc0, 0xff, 0x58, 0x64, 0x17, 0x48,
		0xc2, 0x5b, 0x94, 0x98, 0x35, 0x50, 0x1f, 0x27, 0xbc, 0xea,
		0x91, 0x92, 0x3f, 0x5c, 0x33, 0x12, 0x17, 0x65, 0x56, 0x3e,
		0x40, 0x44, 0x27, 0x1d, 0xef, 0x0e, 0x72, 0xab, 0xd4, 0xf0,
		0x49, 0xa3, 0x95, 0x1a, 0x61, 0xb4, 0x47, 0x90, 0x20, 0xcc,
		0x50, 0xa4, 0x4d, 0x8b, 0x8a, 0x58, 0x17, 0x70, 0xa4, 0x53,
		0xe4, 0xdc, 0x73, 0x5d, 0x8c, 0x4e, 0xb8, 0xd3, 0xa9, 0xbf };

#if !NET_2_0
	static byte [] pk_token2 = { 0x22, 0x7c, 0x9c, 0x2c, 0x3c, 0x00, 0x63, 0xe9 };
#endif

	static byte [] publicKey3 = {
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	static byte [] pk_token3 = { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };

	[SetUp]
	public void SetUp () 
	{
		try {
			if (Directory.Exists (tempDir))
				Directory.Delete (tempDir, true);
		}
		catch (Exception) {
		}

		Directory.CreateDirectory (tempDir);

#if !TARGET_JVM // Thread.GetDomain is not supported for TARGET_JVM.
		domain = Thread.GetDomain ();
#endif // TARGET_JVM
	}

	[TearDown]
	public void TearDown () 
	{
		try {
			if (Directory.Exists (tempDir))
				Directory.Delete (tempDir, true);
		}
		catch (Exception) {
			// This can happen on windows when the directory contains
			// files opened by the CLR
		}
	}

	[Test] // ctor ()
	public void Constructor0 ()
	{
		an = new AssemblyName ();
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
#if NET_2_0
		Assert.AreEqual (String.Empty, an.FullName, "FullName");
#else
		Assert.IsNull (an.FullName, "FullName");
#endif
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.IsNull (an.Name, "Name");
#if NET_2_0
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
#endif
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.IsNull (an.GetPublicKeyToken (), "GetPublicKeyToken");
#if NET_2_0
		Assert.AreEqual (string.Empty, an.ToString (), "ToString");
#else
		Assert.AreEqual (typeof (AssemblyName).FullName, an.ToString (), "ToString");
#endif
	}

	[Test]
	public void SetPublicKey () 
	{
		an = new AssemblyName ();
		Assert.IsNull (an.GetPublicKey (), "#A1");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#A2");
		Assert.IsNull (an.KeyPair, "#A3");
		Assert.IsNull (an.GetPublicKeyToken (), "#A4");

		an.SetPublicKey (publicKey1);

		Assert.AreEqual (publicKey1, an.GetPublicKey (), "#B1");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#B2");
		Assert.IsNull (an.KeyPair, "#B3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#B4");
		an.SetPublicKey (keyPair);
		Assert.AreEqual (keyPair, an.GetPublicKey (), "#B5");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#B6");

		an.SetPublicKey ((byte []) null);

		Assert.IsNull (an.GetPublicKey (), "#C1");
#if NET_2_0
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#C2");
#else
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#C2");
#endif
		Assert.IsNull (an.KeyPair, "#C3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#C4");

		an.SetPublicKey (publicKey1);
		an.SetPublicKeyToken (pk_token1);
		an.SetPublicKey ((byte []) null);

		Assert.IsNull (an.GetPublicKey (), "#D1");
#if NET_2_0
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#D2");
#else
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#D2");
#endif
		Assert.IsNull (an.KeyPair, "#D3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#D4");

		an.SetPublicKey ((byte []) null);
		an.SetPublicKeyToken (pk_token1);
		an.SetPublicKey ((byte []) null);

		Assert.IsNull (an.GetPublicKey (), "#E1");
#if NET_2_0
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#E2");
#else
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#E2");
#endif
		Assert.IsNull (an.KeyPair, "#E3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#E4");

		an = new AssemblyName ();
		an.SetPublicKey (publicKey1);
		an.SetPublicKey ((byte []) null);
		an.SetPublicKeyToken (pk_token1);
		an.SetPublicKey ((byte []) null);

		Assert.IsNull (an.GetPublicKey (), "#F1");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#F2");
		Assert.IsNull (an.KeyPair, "#F3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#F4");

		an = new AssemblyName ();
		an.SetPublicKey (publicKey1);
		an.SetPublicKey ((byte []) null);
		an.SetPublicKeyToken (pk_token1);

		Assert.IsNull (an.GetPublicKey (), "#G1");
#if NET_2_0
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#G2");
#else
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#G2");
#endif
		Assert.IsNull (an.KeyPair, "#G3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#G4");

		an = new AssemblyName ();
		an.SetPublicKey (new byte [0]);

		Assert.IsNotNull (an.GetPublicKey (), "#H1");
		Assert.AreEqual (0, an.GetPublicKey ().Length, "#H2");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#H3");
		Assert.IsNull (an.KeyPair, "#H4");
#if NET_2_0
		Assert.IsNotNull (an.GetPublicKeyToken (), "#H5");
		Assert.AreEqual (0, an.GetPublicKeyToken ().Length, "#H6");
#else
		Assert.IsNull (an.GetPublicKeyToken (), "#H5");
#endif

		an = new AssemblyName ();
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#I1");
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#I2");

		an = new AssemblyName ();
		an.SetPublicKey ((byte []) null);
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#J1");
		an.SetPublicKey ((byte []) null);
#if NET_2_0
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#J2");
#else
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#J2");
#endif
		an.SetPublicKey ((byte []) null);
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#J3");
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#J4");
		Assert.AreEqual (publicKey1, an.GetPublicKey (), "#J5");
		an.SetPublicKey (publicKey2);
		Assert.AreEqual (publicKey2, an.GetPublicKey (), "#J6");
	}

	[Test]
	public void SetPublicKey_PublicKey_Invalid ()
	{
		an = new AssemblyName ();
		an.SetPublicKey (new byte [] { 0x0b, 0x0a });
		Assert.AreEqual (new byte [] { 0x0b, 0x0a }, an.GetPublicKey (), "#1");
	}

	[Test]
	public void SetPublicKeyToken ()
	{
		an = new AssemblyName ();
		an.SetPublicKeyToken (pk_token1);

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#A1");
		Assert.IsNull (an.KeyPair, "#A2");
		Assert.IsNull (an.GetPublicKey (), "#A3");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#A4");

		an.SetPublicKeyToken ((byte []) null);

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#B1");
		Assert.IsNull (an.KeyPair, "#B2");
		Assert.IsNull (an.GetPublicKey (), "#B3");
		Assert.IsNull (an.GetPublicKeyToken (), "#B4");

		an.SetPublicKeyToken (new byte [0]);

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#C1");
		Assert.IsNull (an.KeyPair, "#C2");
		Assert.IsNull (an.GetPublicKey (), "#C3");
		Assert.IsNotNull (an.GetPublicKeyToken (), "#C4");
		Assert.AreEqual (0, an.GetPublicKeyToken ().Length, "#C5");
	}

	[Test]
	public void KeyPair () 
	{
		an = new AssemblyName ();
		an.Name = "test";
		an.KeyPair = new StrongNameKeyPair (keyPair);

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#A1");
		Assert.IsNotNull (an.KeyPair, "#A2");
		Assert.IsNull (an.GetPublicKey (), "#A3");
		Assert.IsNull (an.GetPublicKeyToken (), "#A4");
		Assert.AreEqual ("test", an.FullName, "#A5");

		an.KeyPair = null;

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#B1");
		Assert.IsNull (an.KeyPair, "#B2");
		Assert.IsNull (an.GetPublicKey (), "#B3");
		Assert.IsNull (an.GetPublicKeyToken (), "#B4");
		Assert.AreEqual ("test", an.FullName, "#B5");
	}

	// !!! this assembly MUST NOT use a StrongName !!!
	[Test]
	public void Self ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		an = a.GetName ();

		Assert.AreEqual (CultureInfo.InvariantCulture, an.CultureInfo, "CultureInfo");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "Flags");
		Assert.AreEqual (AssemblyHashAlgorithm.SHA1, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.IsNotNull (an.Name, "Name");
#if NET_2_0
		//Assert.AreEqual (ProcessorArchitecture.MSIL, an.ProcessorArchitecture, "PA");
#endif
		Assert.AreEqual (new Version (0, 0, 0, 0), an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine,
			an.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (new byte [0], an.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}

	[Test]
	public void Flags_Corlib ()
	{
		an = typeof (int).Assembly.GetName ();
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags);
	}

	[Test]
	public void Flags_Self ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		an = a.GetName ();
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags);
	}

	[Test]
	public void FullName_Corlib ()
	{
		an = typeof(int).Assembly.GetName ();
		Assert.IsNotNull (an.FullName, "#1");

		string AssemblyCorlib;
#if MOBILE
		AssemblyCorlib = "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
#elif NET_4_0
		AssemblyCorlib = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
#else
		AssemblyCorlib = "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
#endif
		Assert.AreEqual (AssemblyCorlib, an.FullName, "#2");
	}

	[Test]
	public void FullName_Self ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		an = a.GetName ();

		Assert.IsNotNull (an.FullName, "#1");
		Assert.IsTrue (an.FullName.IndexOf ("Version=0.0.0.0") != -1, "#2");
		Assert.IsTrue (an.FullName.IndexOf ("Culture=neutral") != -1, "#3");
		Assert.IsTrue (an.FullName.IndexOf ("PublicKeyToken=null") != -1, "#4");
	}

	[Test]
	public void FullName_Flags ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with only name
		an = new AssemblyName ();
#if NET_2_0
		an.Flags = AssemblyNameFlags.EnableJITcompileOptimizer |
			AssemblyNameFlags.EnableJITcompileTracking |
			AssemblyNameFlags.PublicKey |
			AssemblyNameFlags.Retargetable;
#else
		an.Flags = AssemblyNameFlags.PublicKey |
			AssemblyNameFlags.Retargetable;
#endif
		an.Name = assemblyName;
		Assert.AreEqual (assemblyName + ", Retargetable=Yes", an.FullName, "#1");
		an.Flags = AssemblyNameFlags.None;
		Assert.AreEqual (assemblyName, an.FullName, "#2");
	}

	[Test]
	public void FullName_Name ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with only name
		an = new AssemblyName ();
		an.Name = assemblyName;
		Assert.IsNotNull (an.FullName, "FullName2#1");
		Assert.AreEqual (an.Name, an.FullName, "FullName2#2");
		Assert.AreEqual (-1, an.FullName.IndexOf ("Culture="), "FullName2#3");
		Assert.AreEqual (-1, an.FullName.IndexOf ("PublicKeyToken="), "FullName2#4");
	}

	[Test]
	public void FullName_Version ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2";

		// tests for AssemblyName with name and version
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.Version = new Version (assemblyVersion);
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion, an.FullName, "FullName3#1");
	}

	[Test]
	public void FullName_Culture ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name and neutral culture
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.CultureInfo = CultureInfo.InvariantCulture;
		Assert.AreEqual (assemblyName + ", Culture=neutral", an.FullName, "#1");
		an.CultureInfo = new CultureInfo ("nl-BE");
		Assert.AreEqual (assemblyName + ", Culture=nl-BE", an.FullName, "#2");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#3");
#else
		Assert.IsNull (an.FullName, "#4");
#endif
	}

	[Test]
	public void FullName_PublicKey ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name and public key
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#A1");
		an.SetPublicKey ((byte []) null);
		Assert.AreEqual (assemblyName, an.FullName, "#A2");
		an.SetPublicKey (new byte [0]);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#A3");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#A4");
#else
		Assert.IsNull (an.FullName, "#A4");
#endif

		an = new AssemblyName ();
		an.HashAlgorithm = AssemblyHashAlgorithm.MD5;
		an.Name = assemblyName;
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#B1");
		an.SetPublicKeyToken (new byte [] { 0x0a, 0x22 });
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=0a22", an.FullName, "#B2");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#B2");
#endif
		an.SetPublicKeyToken ((byte []) null);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#B3");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#B4");
#else
		Assert.IsNull (an.FullName, "#B4");
#endif

		an = new AssemblyName ();
		an.HashAlgorithm = AssemblyHashAlgorithm.None;
		an.Name = assemblyName;
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#C1");
		an.SetPublicKeyToken (new byte [] { 0x0a, 0x22 });
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=0a22", an.FullName, "#C2");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#C2");
#endif
		an.SetPublicKeyToken ((byte []) null);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#C3");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#C4");
#else
		Assert.IsNull (an.FullName, "#C4");
#endif

		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (new byte [0]);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#D1");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#D2");
#else
		Assert.IsNull (an.FullName, "#D2");
#endif
		an.SetPublicKey (publicKey3);
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#D3");
#else
		Assert.IsNull (an.FullName, "#D3");
#endif
		an.Name = assemblyName;
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token3), an.FullName, "#D4");
	}

	[Test]
	public void FullName_PublicKey_KeyPair ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name and public key
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (keyPair);
#if NET_2_0
		try {
			Assert.Fail ("#A1: " + an.FullName);
		} catch (SecurityException ex) {
			// Invalid assembly public key
			Assert.AreEqual (typeof (SecurityException), ex.GetType (), "#A2");
			Assert.AreEqual ((SecurityAction) 0, ex.Action, "#A3");
			Assert.IsNull (ex.InnerException, "#A4");
			Assert.IsNotNull (ex.Message, "#A5");
		}
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (kp_token), an.FullName, "#A1");
#endif

		an.SetPublicKeyToken (new byte [0]);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#B1");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (kp_token), an.FullName, "#B1");
#endif

		an.SetPublicKeyToken ((byte []) null);
#if NET_2_0
		try {
			Assert.Fail ("#C1: " + an.FullName);
		} catch (SecurityException ex) {
			// Invalid assembly public key
			Assert.AreEqual (typeof (SecurityException), ex.GetType (), "#C2");
			Assert.AreEqual ((SecurityAction) 0, ex.Action, "#C3");
			Assert.IsNull (ex.InnerException, "#C4");
			Assert.IsNotNull (ex.Message, "#C5");
		}
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (kp_token), an.FullName, "#C1");
#endif

		an.SetPublicKeyToken (new byte [0]);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#D1");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (kp_token), an.FullName, "#D1");
#endif
		an.SetPublicKey (publicKey1);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#D2");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#D2");
#endif
		an.SetPublicKeyToken ((byte []) null);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#D3");
	}

	[Test]
	public void FullName_PublicKeyToken ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKeyToken (pk_token1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#A1");
		an.SetPublicKeyToken ((byte []) null);
		Assert.AreEqual (assemblyName, an.FullName, "#A2");
		an.SetPublicKeyToken (new byte [0]);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#A3");
		an.SetPublicKeyToken (pk_token1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#A4");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#A5");
#else
		Assert.IsNull (an.FullName, "#A5");
#endif

		an = new AssemblyName ();
		an.HashAlgorithm = AssemblyHashAlgorithm.MD5;
		an.Name = assemblyName;
		an.SetPublicKeyToken (pk_token1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#B1");
		an.SetPublicKeyToken (new byte [] { 0x0a, 0x22 });
		Assert.AreEqual (assemblyName + ", PublicKeyToken=0a22", an.FullName, "#B2");
		an.SetPublicKeyToken ((byte []) null);
		Assert.AreEqual (assemblyName, an.FullName, "#B3");
		an.Name = null;
#if NET_2_0
		Assert.AreEqual (string.Empty, an.FullName, "#B4");
#else
		Assert.IsNull (an.FullName, "#B4");
#endif

		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (publicKey1);
		an.SetPublicKeyToken (pk_token1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#C1");
		an.SetPublicKey ((byte []) null);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#C2");
#else
		Assert.AreEqual (assemblyName, an.FullName, "#C2");
#endif
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#C3");
		an.SetPublicKey (new byte [0]);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#C4");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#C4");
#endif
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#C5");

		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#D1");
		an.SetPublicKey (new byte [0]);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#D2");
		an.SetPublicKeyToken (pk_token1);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#D3");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#D3");
#endif
		an.SetPublicKey ((byte []) null);
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#D4");
#else
		Assert.AreEqual (assemblyName, an.FullName, "#D4");
#endif
	}

	[Test]
	public void FullName_VersionCulture ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2";

		// tests for AssemblyName with name, version and neutral culture
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.Version = new Version (assemblyVersion);
		an.CultureInfo = CultureInfo.InvariantCulture;
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion
			+ ", Culture=neutral", an.FullName, "#1");
		an.CultureInfo = new CultureInfo ("en-US");
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion
			+ ", Culture=en-US", an.FullName, "#2");
		an.CultureInfo = new CultureInfo ("en");
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion
			+ ", Culture=en", an.FullName, "#3");
	}

	[Test]
	public void FullName_VersionPublicKey ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2";

		// tests for AssemblyName with name, version and public key
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.Version = new Version (assemblyVersion);
		an.SetPublicKey (publicKey1);
		an.SetPublicKeyToken (pk_token1);
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion
			+ ", PublicKeyToken=" + GetTokenString (pk_token1), 
			an.FullName, "#1");
	}

	[Test]
	public void FullName_CulturePublicKey ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name, culture and public key
		an = new AssemblyName ();
		an.Name = assemblyName;
		an.CultureInfo = CultureInfo.InvariantCulture;
		an.SetPublicKey (publicKey1);
		an.SetPublicKeyToken (pk_token1);
		Assert.AreEqual (assemblyName + ", Culture=neutral"
			+ ", PublicKeyToken=" + GetTokenString (pk_token1),
			an.FullName, "#1");
	}

	[Test]
	public void GetPublicKeyToken ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName ();
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#A1");
		an.SetPublicKey (publicKey2);
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#A2");
		an.SetPublicKeyToken (new byte [] { 0x0a });
		Assert.AreEqual (new byte [] { 0x0a }, an.GetPublicKeyToken (), "#A3");
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (new byte [] { 0x0a }, an.GetPublicKeyToken (), "#A4");
		an.SetPublicKeyToken (new byte [0]);
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#A5");
		an.Name = assemblyName;
		an.SetPublicKey (publicKey2);
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#A6");
#if NET_2_0
		Assert.AreEqual (assemblyName + ", PublicKeyToken=null", an.FullName, "#A7");
#else
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token2), an.FullName, "#A7");
#endif

		an = new AssemblyName ();
		an.HashAlgorithm = AssemblyHashAlgorithm.MD5;
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#B1");

		an = new AssemblyName ();
		an.HashAlgorithm = AssemblyHashAlgorithm.None;
		an.SetPublicKey (publicKey1);
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#C1");

		an = new AssemblyName ();
		an.SetPublicKeyToken (new byte [0]);
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#D1");
		an.SetPublicKeyToken (new byte [] { 0x0b, 0xff });
		Assert.AreEqual (new byte [] { 0x0b, 0xff }, an.GetPublicKeyToken (), "#D2");
		an.SetPublicKeyToken ((byte []) null);
		Assert.IsNull (an.GetPublicKeyToken (), "#D3");

		an = new AssemblyName ();
		an.SetPublicKey (keyPair);
#if NET_2_0
		try {
			an.GetPublicKeyToken ();
			Assert.Fail ("#E1");
		} catch (SecurityException ex) {
			// Invalid assembly public key
			Assert.AreEqual (typeof (SecurityException), ex.GetType (), "#E2");
			Assert.AreEqual ((SecurityAction) 0, ex.Action, "#E3");
			Assert.IsNull (ex.InnerException, "#E4");
			Assert.IsNotNull (ex.Message, "#E5");
		}
#else
		Assert.AreEqual (kp_token, an.GetPublicKeyToken (), "#E1");
#endif
	}

	[Test]
	public void GetPublicKeyToken_Corlib ()
	{
		an = typeof (int).Assembly.GetName ();
		Assert.IsNotNull (an.GetPublicKeyToken (), "#1");
		Assert.IsTrue (an.GetPublicKeyToken ().Length > 0, "#2");
	}

	[Test]
	public void GetPublicGetToken_Ecma ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (publicKey3);
		Assert.IsNotNull (an.GetPublicKeyToken (), "#1");
		Assert.AreEqual (pk_token3, an.GetPublicKeyToken (), "#2");
	}

	[Test]
	public void GetPublicKeyToken_Self ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		an = a.GetName ();
#if NET_2_0
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken ());
#else
		Assert.IsNull (an.GetPublicKeyToken ());
#endif
	}

	static int nameIndex = 0;

	private AssemblyName GenAssemblyName () 
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.AssemblyNameTest" + (nameIndex ++);
		return assemblyName;
	}

#if !TARGET_JVM && !MOBILE // Reflection.Emit is not supported for TARGET_JVM.
	private Assembly GenerateAssembly (AssemblyName name) 
	{
		AssemblyBuilder ab = domain.DefineDynamicAssembly (
			name,
			AssemblyBuilderAccess.RunAndSave,
			tempDir);
		ab.DefineDynamicModule ("def_module");
		ab.Save (name.Name + ".dll");

		return Assembly.LoadFrom (Path.Combine (tempDir, name.Name + ".dll"));
	}

	private AssemblyBuilder GenerateDynamicAssembly (AssemblyName name)
	{
		AssemblyBuilder ab = domain.DefineDynamicAssembly (
				name,
				AssemblyBuilderAccess.Run);

		return ab;
	}

	[Test]
	public void TestCultureInfo ()
	{
		AssemblyName name;
		Assembly a;
		CultureInfo culture;

		name = GenAssemblyName ();
		name.CultureInfo = CultureInfo.CreateSpecificCulture ("ar-DZ");
		a = GenerateAssembly (name);
		culture = a.GetName ().CultureInfo;
		Assert.IsFalse (culture.IsNeutralCulture, "#A1");
		Assert.IsFalse (culture.IsReadOnly, "#A2");
		Assert.AreEqual (5121, culture.LCID, "#A3");
		Assert.AreEqual ("ar-DZ", culture.Name, "#A4");
		Assert.IsTrue (culture.UseUserOverride, "#A5");

		name = GenAssemblyName ();
		name.CultureInfo = new CultureInfo ("en");
		a = GenerateAssembly (name);
		culture = a.GetName ().CultureInfo;
		Assert.IsTrue (culture.IsNeutralCulture, "#B1");
		Assert.IsFalse (culture.IsReadOnly, "#B2");
		Assert.AreEqual (9, culture.LCID, "#B3");
		Assert.AreEqual ("en", culture.Name, "#B4");
		Assert.IsTrue (culture.UseUserOverride, "#B5");

		name = GenAssemblyName ();
		name.CultureInfo = CultureInfo.InvariantCulture;
		a = GenerateAssembly (name);
		culture = a.GetName ().CultureInfo;
		Assert.IsFalse (culture.IsNeutralCulture, "#C1");
#if NET_2_0
		Assert.IsFalse (culture.IsReadOnly, "#C2");
#else
		Assert.IsTrue (culture.IsReadOnly, "#C2");
#endif
		Assert.AreEqual (127, culture.LCID, "#C3");
		Assert.AreEqual (string.Empty, culture.Name, "#C4");
		Assert.IsFalse (culture.UseUserOverride, "#C5");

		a = typeof (int).Assembly;
		name = a.GetName ();
		culture = name.CultureInfo;
		Assert.IsFalse (culture.IsNeutralCulture, "#D1");
#if NET_2_0
		Assert.IsFalse (culture.IsReadOnly, "#D2");
#else
		Assert.IsTrue (culture.IsReadOnly, "#D2");
#endif
		Assert.AreEqual (127, culture.LCID, "#D3");
		Assert.AreEqual (string.Empty, culture.Name, "#D4");
		Assert.IsFalse (culture.UseUserOverride, "#D5");

		a = Assembly.GetExecutingAssembly ();
		name = a.GetName ();
		culture = name.CultureInfo;
		Assert.IsFalse (culture.IsNeutralCulture, "#E1");
#if NET_2_0
		Assert.IsFalse (culture.IsReadOnly, "#E2");
#else
		Assert.IsTrue (culture.IsReadOnly, "#E2");
#endif
		Assert.AreEqual (127, culture.LCID, "#E3");
		Assert.AreEqual (string.Empty, culture.Name, "#E4");
		Assert.IsFalse (culture.UseUserOverride, "#E5");

		AssemblyName [] names = a.GetReferencedAssemblies ();
		foreach (AssemblyName an in names) {
			culture = an.CultureInfo;
			Assert.IsFalse (culture.IsNeutralCulture, "#F1:" + an.Name);
			Assert.IsFalse (culture.IsReadOnly, "#F2:" + an.Name);
			Assert.AreEqual (127, culture.LCID, "#F3:" + an.Name);
			Assert.AreEqual (string.Empty, culture.Name, "#F4:" + an.Name);
#if NET_2_0
			Assert.IsFalse (culture.UseUserOverride, "#F5:" + an.Name);
#else
			Assert.IsTrue (culture.UseUserOverride, "#F5:" + an.Name);
#endif
		}
	}

	[Test]
	public void Version ()
	{
		AssemblyName name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3, 4);

		Assembly a = GenerateAssembly (name);
		Assert.AreEqual ("1.2.3.4", a.GetName ().Version.ToString (), "1.2.3.4 normal");

		name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3);

		a = GenerateAssembly (name);
		Assert.AreEqual ("1.2.3.0", a.GetName ().Version.ToString (), "1.2.3.0 normal");

		name = GenAssemblyName ();
		name.Version = new Version (1, 2);

		a = GenerateAssembly (name);
		Assert.AreEqual ("1.2.0.0", a.GetName ().Version.ToString (), "1.2.0.0 normal");
	}

	[Test]
	[Category ("NotWorking")]
	public void Version_Dynamic ()
	{
		AssemblyName name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3, 4);

		AssemblyBuilder ab = GenerateDynamicAssembly (name);
		Assert.AreEqual ("1.2.3.4", ab.GetName ().Version.ToString (), "1.2.3.4 dynamic");

		name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3);

		ab = GenerateDynamicAssembly (name);
#if NET_2_0
		Assert.AreEqual ("1.2.3.0", ab.GetName ().Version.ToString (), "1.2.3.0 dynamic");
#else
		Assert.AreEqual ("1.2.3.65535", ab.GetName ().Version.ToString (), "1.2.3.0 dynamic");
#endif

		name = GenAssemblyName ();
		name.Version = new Version (1, 2);

		ab = GenerateDynamicAssembly (name);
#if NET_2_0
		Assert.AreEqual ("1.2.0.0", ab.GetName ().Version.ToString (), "1.2.0.0 dynamic");
#else
		Assert.AreEqual ("1.2.65535.65535", ab.GetName ().Version.ToString (), "1.2.0.0 dynamic");
#endif
	}
#endif // TARGET_JVM

	[Test]
	public void HashAlgorithm ()
	{
		Assert.AreEqual (AssemblyHashAlgorithm.SHA1, 
			typeof (int).Assembly.GetName ().HashAlgorithm);
	}

	[Test]
	public void Serialization ()
	{
		an = new AssemblyName ();
		an.CodeBase = "http://www.test.com/test.dll";
		an.CultureInfo = CultureInfo.InvariantCulture;
		an.Flags = AssemblyNameFlags.PublicKey;
		an.HashAlgorithm = AssemblyHashAlgorithm.MD5;
		an.KeyPair = new StrongNameKeyPair (publicKey1);
		an.Name = "TestAssembly";
		an.Version = new Version (1, 5);
		an.VersionCompatibility = AssemblyVersionCompatibility.SameProcess;

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, an);

		// reset position of memorystream
		ms.Position = 0;

		// deserialze assembly name
		AssemblyName dsAssemblyName = (AssemblyName) bf.Deserialize (ms);

		// close the memorystream
		ms.Close ();

		// compare orginal and deserialized assembly name
		Assert.AreEqual (an.CodeBase, dsAssemblyName.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, dsAssemblyName.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.Flags, dsAssemblyName.Flags, "Flags");
		Assert.AreEqual (an.HashAlgorithm, dsAssemblyName.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.Name, dsAssemblyName.Name, "Name");
		Assert.AreEqual (an.Version, dsAssemblyName.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, dsAssemblyName.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (an.EscapedCodeBase, dsAssemblyName.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.FullName, dsAssemblyName.FullName, "FullName");
		Assert.AreEqual (an.ToString (), dsAssemblyName.ToString (), "ToString");
		Assert.AreEqual (an.GetPublicKey (), dsAssemblyName.GetPublicKey (), "PublicKey");
		Assert.AreEqual (an.GetPublicKeyToken (), dsAssemblyName.GetPublicKeyToken (), "PublicToken");
	}

	[Test]
	public void Serialization_WithoutStrongName ()
	{
		an = new AssemblyName ();
		an.CodeBase = "http://www.test.com/test.dll";
		an.CultureInfo = CultureInfo.InvariantCulture;
		an.Flags = AssemblyNameFlags.None;
		an.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
		an.KeyPair = null;
		an.Name = "TestAssembly2";
		an.Version = new Version (1, 5, 0, 0);
		an.VersionCompatibility = AssemblyVersionCompatibility.SameMachine;

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, an);

		// reset position of memorystream
		ms.Position = 0;

		// deserialze assembly name
		AssemblyName dsAssemblyName = (AssemblyName) bf.Deserialize (ms);

		// close the memorystream
		ms.Close ();

		// compare orginal and deserialized assembly name
		Assert.AreEqual (an.CodeBase, dsAssemblyName.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, dsAssemblyName.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.Flags, dsAssemblyName.Flags, "Flags");
		Assert.AreEqual (an.HashAlgorithm, dsAssemblyName.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.Name, dsAssemblyName.Name, "Name");
		Assert.AreEqual (an.Version, dsAssemblyName.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, dsAssemblyName.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (an.EscapedCodeBase, dsAssemblyName.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.FullName, dsAssemblyName.FullName, "FullName");
		Assert.AreEqual (an.ToString (), dsAssemblyName.ToString (), "ToString");
		Assert.AreEqual (an.GetPublicKey (), dsAssemblyName.GetPublicKey (), "PublicKey");
		Assert.AreEqual (an.GetPublicKeyToken (), dsAssemblyName.GetPublicKeyToken (), "PublicToken");
	}

#if !TARGET_JVM // Assemblyname.GetObjectData not implemented yet for TARGET_JVM
	[Test]
	public void GetObjectData_Info_Null ()
	{
		an = new AssemblyName ();
		try {
			an.GetObjectData (null, new StreamingContext (StreamingContextStates.All));
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("info", ex.ParamName, "#6");
		}
	}
#endif // TARGET_JVM

	[Test]
	public void Clone_Corlib ()
	{
		an = typeof (int).Assembly.GetName ();
		AssemblyName clone = (AssemblyName) an.Clone ();

		Assert.AreEqual (an.CodeBase, clone.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, clone.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.EscapedCodeBase, clone.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.Flags, clone.Flags, "Flags");
		Assert.AreEqual (an.FullName, clone.FullName, "FullName");
		Assert.AreEqual (an.HashAlgorithm, clone.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.KeyPair, clone.KeyPair, "KeyPair");
		Assert.AreEqual (an.Name, clone.Name, "Name");
#if NET_2_0
		Assert.AreEqual (an.ProcessorArchitecture, clone.ProcessorArchitecture, "PA");
#endif
		Assert.AreEqual (an.Version, clone.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, clone.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (an.GetPublicKey (), clone.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (an.GetPublicKeyToken (), clone.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.ToString (), clone.ToString (), "ToString");
	}

	[Test]
	public void Clone_Empty ()
	{
		an = new AssemblyName ();
		AssemblyName clone = (AssemblyName) an.Clone ();

		Assert.IsNull (clone.CodeBase, "CodeBase");
		Assert.IsNull (clone.CultureInfo, "CultureInfo");
		Assert.IsNull (clone.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, clone.Flags, "Flags");
#if NET_2_0
		Assert.AreEqual (String.Empty, clone.FullName, "FullName");
#else
		Assert.IsNull (clone.FullName, "FullName");
#endif
		Assert.AreEqual (AssemblyHashAlgorithm.None, clone.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (clone.KeyPair, "KeyPair");
		Assert.IsNull (clone.Name, "Name");
#if NET_2_0
		Assert.AreEqual (ProcessorArchitecture.None, clone.ProcessorArchitecture, "PA");
#endif
		Assert.IsNull (clone.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			clone.VersionCompatibility, "VersionCompatibility");
	}

	[Test]
	public void Clone_Self ()
	{
		an = Assembly.GetExecutingAssembly ().GetName ();
		AssemblyName clone = (AssemblyName) an.Clone ();

		Assert.AreEqual (an.CodeBase, clone.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, clone.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.EscapedCodeBase, clone.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.Flags, clone.Flags, "Flags");
		Assert.AreEqual (an.FullName, clone.FullName, "FullName");
		Assert.AreEqual (an.HashAlgorithm, clone.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.KeyPair, clone.KeyPair, "KeyPair");
		Assert.AreEqual (an.Name, clone.Name, "Name");
#if NET_2_0
		//Assert.AreEqual (ProcessorArchitecture.MSIL, clone.ProcessorArchitecture, "PA");
#endif
		Assert.AreEqual (an.Version, clone.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, clone.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (an.GetPublicKey (), clone.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (an.GetPublicKeyToken (), clone.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.ToString (), clone.ToString (), "ToString");
	}

	[Test]
	[ExpectedException (typeof (FileNotFoundException))]
	public void GetAssemblyName_AssemblyFile_DoesNotExist ()
	{
		AssemblyName.GetAssemblyName (Path.Combine (tempDir, "doesnotexist.dll"));
	}

	[Test]
	[Category ("NotWorking")]
	public void GetAssemblyName_AssemblyFile_ReadLock ()
	{
		string file = Path.Combine (tempDir, "loadfailure.dll");
		using (FileStream fs = File.Open (file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None)) {
			try {
				AssemblyName.GetAssemblyName (file);
				Assert.Fail ("#1");
			} catch (FileLoadException ex) {
				// Could not load file or assembly '...' or one
				// of its dependencies. The process cannot access
				// the file because it is being used by another
				// process
				Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.FileName, "#3");
#if NET_2_0
				Assert.AreEqual (file, ex.FileName, "#4");
#else
				Assert.IsTrue (ex.FileName.IndexOf ("loadfailure.dll") != -1, "#4");
#endif
				Assert.IsNull (ex.InnerException, "#5");
				Assert.IsNotNull (ex.Message, "#6");
			}
		}
		File.Delete (file);
	}

	[Test]
	public void GetAssemblyName_AssemblyFile_BadImage ()
	{
		string file = Path.Combine (tempDir, "badimage.dll");
		using (StreamWriter sw = File.CreateText (file)) {
			sw.WriteLine ("somegarbage");
		}
		try {
			AssemblyName.GetAssemblyName (file);
			Assert.Fail ("#1");
		} catch (BadImageFormatException ex) {
			Assert.AreEqual (typeof (BadImageFormatException), ex.GetType (), "#2");
			Assert.IsNotNull (ex.FileName, "#3");
#if NET_2_0
			Assert.AreEqual (file, ex.FileName, "#4");
#else
			Assert.IsTrue (ex.FileName.IndexOf ("badimage.dll") != -1, "#4");
#endif
			Assert.IsNull (ex.InnerException, "#5");
			Assert.IsNotNull (ex.Message, "#6");
		} finally {
			File.Delete (file);
		}
	}

	[Test]
	public void GetAssemblyName_CodeBase ()
	{
		Assembly execAssembly = Assembly.GetExecutingAssembly ();

		AssemblyName aname = AssemblyName.GetAssemblyName (execAssembly.Location);
		Assert.IsNotNull (aname.CodeBase, "#1");
		Assert.AreEqual (execAssembly.CodeBase, aname.CodeBase, "#2");
	}

	// helpers

	private string GetTokenString (byte[] value)
	{
		string tokenString = "";
		for (int i = 0; i < value.Length; i++) {
			tokenString += value[i].ToString ("x2");
		}
		return tokenString;
	}

#if NET_2_0
	[Test] // ctor (String)
	public void Constructor1_Name ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName (assemblyName);
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly", an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.IsNull (an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual ("TestAssembly", an.ToString (), "ToString");
	}

	[Test] // ctor (String)
	[Category("TargetJvmNotWorking")] // Not yet supported for TARGET_JVM.
	public void Constructor1_Full ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyCulture = "neutral";
		const string assemblyVersion = "1.2.3.4";

		an = new AssemblyName (assemblyName + ", Version=" + assemblyVersion + 
				", Culture=" + assemblyCulture + ", PublicKeyToken=" + GetTokenString (pk_token1) + ",ProcessorArchitecture=X86");
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.AreEqual (CultureInfo.InvariantCulture, an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly, Version=1.2.3.4, Culture=neutral, PublicKeyToken=" +
			GetTokenString (pk_token1), an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.X86, an.ProcessorArchitecture, "PA");
		Assert.AreEqual (new Version (assemblyVersion), an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}

	[Test] // ctor (String)
	public void Constructor1_AssemblyName_Empty ()
	{
		try {
			new AssemblyName (string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// String cannot have zero length
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_AssemblyName_Invalid ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", =1.2.4.5");
			Assert.Fail ("#A1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}

		try {
			new AssemblyName (assemblyName + ", OtherAttribute");
			Assert.Fail ("#B1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_AssemblyName_Null ()
	{
		try {
			new AssemblyName (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("assemblyName", ex.ParamName, "#6");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_Culture ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyCulture = "en-US";

		an = new AssemblyName (assemblyName + ", Culture=" + assemblyCulture);
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.AreEqual (new CultureInfo (assemblyCulture), an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly, Culture=en-US", an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.IsNull (an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}

	[Test] // ctor (String)
	public void Constructor1_Culture_Incomplete ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", Culture=");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_Culture_NotSupported ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", Culture=aa-AA");
			Assert.Fail ("#1");
#if NET_4_0
		} catch (CultureNotFoundException ex) {
		}
#else
		} catch (ArgumentException ex) {
			// Culture name 'aa-aa' is not supported
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("name", ex.ParamName, "#6");
		}
#endif
	}

	[Test] // ctor (String)
	[Category ("NotWorking")] // bug #351708
	public void Constructor1_ProcessorArchitecture ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName (assemblyName + ", ProcessorArchitecture=X86");
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly", an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.X86, an.ProcessorArchitecture, "PA");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.IsNull (an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual ("TestAssembly", an.ToString (), "ToString");

		an = new AssemblyName (assemblyName + ", ProcessorArchitecture=mSiL");
		Assert.AreEqual (ProcessorArchitecture.MSIL, an.ProcessorArchitecture, "PA: MSIL");

		an = new AssemblyName (assemblyName + ", ProcessorArchitecture=AmD64");
		Assert.AreEqual (ProcessorArchitecture.Amd64, an.ProcessorArchitecture, "PA: Amd64");

		an = new AssemblyName (assemblyName + ", ProcessorArchitecture=iA64");
		Assert.AreEqual (ProcessorArchitecture.IA64, an.ProcessorArchitecture, "PA: IA64");
	}

	[Test] // ctor (String)
	[Category ("NotWorking")] // bug #351708
	public void Constructor1_ProcessorArchitecture_Incomplete ()
	{
		const string assemblyName = "TestAssembly";
		try {
			new AssemblyName (assemblyName + ", ProcessorArchitecture=");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	[Category ("NotWorking")] // bug #351708
	public void Constructor1_ProcessorArchitecture_Invalid ()
	{
		const string assemblyName = "TestAssembly";
		try {
			new AssemblyName (assemblyName + ", ProcessorArchitecture=XXX");
			Assert.Fail ("#A1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#A2");
			Assert.IsNull (ex.FileName, "#A3");
			Assert.IsNull (ex.InnerException, "#A4");
			Assert.IsNotNull (ex.Message, "#A5");
		}

		try {
			new AssemblyName (assemblyName + ", ProcessorArchitecture=None");
			Assert.Fail ("#B1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#B2");
			Assert.IsNull (ex.FileName, "#B3");
			Assert.IsNull (ex.InnerException, "#B4");
			Assert.IsNotNull (ex.Message, "#B5");
		}
	}

	[Test] // ctor (String)
	[Category ("NotDotNet")] // MS only sets the public key token not the public key: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=321088
	public void Constructor1_PublicKey_Mono ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName (assemblyName + ", PublicKey=" + GetTokenString (publicKey1));
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly, PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (publicKey1, an.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}

	[Test]
	[Category ("NotWorking")] // MS only sets the public key token not the public key: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=321088
	public void Constructor1_PublicKey_MS ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName (assemblyName + ", PublicKey=" + GetTokenString (publicKey1));
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly, PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}

	[Test] // ctor (String)
	[Category ("NotWorking")] // bug #351725
	public void Constructor1_PublicKey_Ecma ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName (assemblyName + ", PublicKey=" + GetTokenString (publicKey3));
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly, PublicKeyToken=" + GetTokenString (pk_token3), an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.AreEqual (pk_token3, an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}

	[Test] // ctor (String)
	public void Constructor1_PublicKey_Incomplete ()
	{
		const string assemblyName = "TestAssembly";
		
		try {
			new AssemblyName (assemblyName + ", PublicKey=");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_PublicKey_Invalid ()
	{
		const string assemblyName = "TestAssembly";
		
		try {
			new AssemblyName (assemblyName + ", PublicKey=0024000004800000940000000602000000240000525341310004000011000000e39d99616f48cf7d6d59f345e485e713e89b8b1265a31b1a393e9894ee3fbddaf382dcaf4083dc31ee7a40a2a25c69c6d019fba9f37ec17fd680e4f6fe3b5305f71ae9e494e3501d92508c2e98ca1e22991a217aa8ce259c9882ffdfff4fbc6fa5e6660a8ff951cd94ed011e5633651b64e8f4522519b6ec84921ee22e4840e");
			Assert.Fail ("#A1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#A2");
			Assert.IsNull (ex.FileName, "#A3");
			Assert.IsNull (ex.InnerException, "#A4");
			Assert.IsNotNull (ex.Message, "#A5");
		}

		try {
			new AssemblyName (assemblyName + ", PublicKey=null");
			Assert.Fail ("#B1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#B2");
			Assert.IsNull (ex.FileName, "#B3");
			Assert.IsNull (ex.InnerException, "#B4");
			Assert.IsNotNull (ex.Message, "#B5");
		}
	}

	[Test] // ctor (String)
	[Category ("NotWorking")] // bug #351756
	public void Constructor1_PublicKey_KeyPair ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", PublicKey=" + GetTokenString (keyPair));
			Assert.Fail ("#1");
		} catch (SecurityException ex) {
			// Invalid assembly public key
			Assert.AreEqual (typeof (SecurityException), ex.GetType (), "#2");
			Assert.AreEqual ((SecurityAction) 0, ex.Action, "#3");
			Assert.IsNull (ex.InnerException, "#4");
			Assert.IsNotNull (ex.Message, "#5");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_PublicKeyToken ()
	{
		const string assemblyName = "TestAssembly";

		an = new AssemblyName (assemblyName + ", PublicKeyToken=" + GetTokenString (pk_token1));
		Assert.IsNull (an.CodeBase, "#A:CodeBase");
		Assert.IsNull (an.CultureInfo, "#A:CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "#A:EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#A:Flags");
		Assert.AreEqual ("TestAssembly, PublicKeyToken=" + GetTokenString (pk_token1), an.FullName, "#A:FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "#A:HashAlgorithm");
		Assert.IsNull (an.KeyPair, "#A:KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "#A:Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "#A:PA");
		Assert.IsNull (an.Version, "#A:Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "#A:VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "#A:GetPublicKey");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#A:GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "#A:ToString");

		an = new AssemblyName (assemblyName + ", PublicKeyToken=null");
		Assert.IsNull (an.CodeBase, "#B:CodeBase");
		Assert.IsNull (an.CultureInfo, "#B:CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "#B:EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#B:Flags");
		//Assert.AreEqual ("TestAssembly, PublicKeyToken=null", an.FullName, "#B:FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "#B:HashAlgorithm");
		Assert.IsNull (an.KeyPair, "#B:KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "#B:Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "#B:PA");
		Assert.IsNull (an.Version, "#B:Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "#B:VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "#B:GetPublicKey");
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#B:GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "#B:ToString");
	}

	[Test] // ctor (String)
	public void Constructor1_PublicKeyToken_Incomplete ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", PublicKeyToken=");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_PublicKeyToken_Invalid ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", PublicKeyToken=27576a8182a188");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_Retargetable ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", Retargetable=Yes");
			Assert.Fail ("#A1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#A2");
			Assert.IsNull (ex.FileName, "#A3");
			Assert.IsNull (ex.InnerException, "#A4");
			Assert.IsNotNull (ex.Message, "#A5");
		}

		try {
			new AssemblyName (assemblyName + ", Retargetable=No");
			Assert.Fail ("#B1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#B2");
			Assert.IsNull (ex.FileName, "#B3");
			Assert.IsNull (ex.InnerException, "#B4");
			Assert.IsNotNull (ex.Message, "#B5");
		}

		try {
			new AssemblyName (assemblyName + ", Version=1.0.0.0, Retargetable=Yes");
			Assert.Fail ("#C1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#C2");
			Assert.IsNull (ex.FileName, "#C3");
			Assert.IsNull (ex.InnerException, "#C4");
			Assert.IsNotNull (ex.Message, "#C5");
		}

		try {
			new AssemblyName (assemblyName + ", Version=1.0.0.0, Culture=neutral, Retargetable=Yes");
			Assert.Fail ("#D1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#D2");
			Assert.IsNull (ex.FileName, "#D3");
			Assert.IsNull (ex.InnerException, "#D4");
			Assert.IsNotNull (ex.Message, "#D5");
		}

		try {
			new AssemblyName (assemblyName + ", Version=1.0.0.0, PublicKeyToken=null, Retargetable=Yes");
			Assert.Fail ("#E1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#E2");
			Assert.IsNull (ex.FileName, "#E3");
			Assert.IsNull (ex.InnerException, "#E4");
			Assert.IsNotNull (ex.Message, "#E5");
		}

		an = new AssemblyName (assemblyName + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null, Retargetable=yEs");
		Assert.IsNull (an.CodeBase, "F:CodeBase");
		Assert.AreEqual (CultureInfo.InvariantCulture, an.CultureInfo, "#F:CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "#F:EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.Retargetable, an.Flags, "#F:Flags");
		Assert.AreEqual ("TestAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null, Retargetable=Yes", an.FullName, "#F:FullName");
		Assert.IsNull (an.GetPublicKey (), "#F:GetPublicKey");
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#F:GetPublicKeyToken");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "#F:HashAlgorithm");
		Assert.IsNull (an.KeyPair, "#F:KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "#F:Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "#F:PA");
		Assert.AreEqual (an.FullName, an.ToString (), "#F:ToString");
		Assert.AreEqual (new Version (1, 0, 0, 0), an.Version, "#F:Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "#F:VersionCompatibility");

		an = new AssemblyName (assemblyName + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null, Retargetable=nO");
		Assert.IsNull (an.CodeBase, "G:CodeBase");
		Assert.AreEqual (CultureInfo.InvariantCulture, an.CultureInfo, "#G:CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "#G:EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "#G:Flags");
		Assert.AreEqual ("TestAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", an.FullName, "#G:FullName");
		Assert.IsNull (an.GetPublicKey (), "#G:GetPublicKey");
		Assert.AreEqual (new byte [0], an.GetPublicKeyToken (), "#G:GetPublicKeyToken");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "#G:HashAlgorithm");
		Assert.IsNull (an.KeyPair, "#G:KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "#G:Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "#G:PA");
		Assert.AreEqual (an.FullName, an.ToString (), "#G:ToString");
		Assert.AreEqual (new Version (1, 0, 0, 0), an.Version, "#G:Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "#G:VersionCompatibility");

		an = new AssemblyName (assemblyName + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=" + GetTokenString (pk_token1) + ", Retargetable=yes");
		Assert.IsNull (an.CodeBase, "H:CodeBase");
		Assert.AreEqual (CultureInfo.InvariantCulture, an.CultureInfo, "#H:CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "#H:EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.Retargetable, an.Flags, "#H:Flags");
		Assert.AreEqual ("TestAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=" + GetTokenString (pk_token1) + ", Retargetable=Yes", an.FullName, "#H:FullName");
		Assert.IsNull (an.GetPublicKey (), "#H:GetPublicKey");
		Assert.AreEqual (pk_token1, an.GetPublicKeyToken (), "#H:GetPublicKeyToken");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "#H:HashAlgorithm");
		Assert.IsNull (an.KeyPair, "#H:KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "#H:Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "#H:PA");
		Assert.AreEqual (an.FullName, an.ToString (), "#H:ToString");
		Assert.AreEqual (new Version (1, 0, 0, 0), an.Version, "#H:Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "#H:VersionCompatibility");
	}

	[Test] // ctor (String)
	public void Constructor1_Retargetable_Incomplete ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", Retargetable=");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#4");
			Assert.IsNotNull (ex.Message, "#5");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_Retargetable_Invalid ()
	{
		const string assemblyName = "TestAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

		try {
			new AssemblyName (assemblyName + ", Retargetable=False");
			Assert.Fail ("#A1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#A2");
			Assert.IsNull (ex.FileName, "#A3");
			Assert.IsNull (ex.InnerException, "#A4");
			Assert.IsNotNull (ex.Message, "#A5");
		}

		try {
			new AssemblyName (assemblyName + ", Retargetable=1");
			Assert.Fail ("#B1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#B2");
			Assert.IsNull (ex.FileName, "#B3");
			Assert.IsNull (ex.InnerException, "#B4");
			Assert.IsNotNull (ex.Message, "#B5");
		}

		try {
			new AssemblyName (assemblyName + ", Retargetable=True");
			Assert.Fail ("#C1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#C2");
			Assert.IsNull (ex.FileName, "#C3");
			Assert.IsNull (ex.InnerException, "#C4");
			Assert.IsNotNull (ex.Message, "#C5");
		}
	}

	[Test] // ctor (String)
	[Category("TargetJvmNotWorking")] // Not yet supported for TARGET_JVM.
	public void Constructor1_Version ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2.3.4";

		an = new AssemblyName (assemblyName + ", Version=" + assemblyVersion);
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.AreEqual ("TestAssembly, Version=1.2.3.4", an.FullName, "FullName");
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.AreEqual (assemblyName, an.Name, "Name");
		Assert.AreEqual (ProcessorArchitecture.None, an.ProcessorArchitecture, "PA");
		Assert.AreEqual (new Version (assemblyVersion), an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
		Assert.IsNull (an.GetPublicKey (), "GetPublicKey");
		Assert.IsNull (an.GetPublicKeyToken (), "GetPublicKeyToken");
		Assert.AreEqual (an.FullName, an.ToString (), "ToString");
	}


	[Test] // ctor (String)
	public void Constructor1_Version_Incomplete ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", Version=, Culture=neutral");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // ctor (String)
	public void Constructor1_Version_Invalid ()
	{
		const string assemblyName = "TestAssembly";

		try {
			new AssemblyName (assemblyName + ", Version=a.b");
			Assert.Fail ("#1");
		} catch (FileLoadException ex) {
			// The given assembly name or codebase was invalid
			Assert.AreEqual (typeof (FileLoadException), ex.GetType (), "#2");
			Assert.IsNull (ex.FileName, "#3");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test (Description="Xamarin bug #99 - whitespaces in key=value")]
	public void WhiteSpaceInKeyValue ()
	{
		string nameWithSpaces = String.Format ("MySql.Data.Tests, PublicKey      = \t  {0},  Culture   =\tneutral, Version=\t1.2.3.4", GetTokenString (publicKey1));
		string fullName = "MySql.Data.Tests, Version=1.2.3.4, Culture=neutral, PublicKeyToken=ce5276d8687ec6dc";
		var an = new AssemblyName (nameWithSpaces);

		Assert.AreEqual (fullName, an.FullName);
	}
#endif
}

}
