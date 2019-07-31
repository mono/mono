// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.Build.Locator;

// from https://raw.githubusercontent.com/Microsoft/MSBuildSdks/master/src/UnitTest.Common/MSBuildSdkTestBase.cs
namespace Mono.WebAssembly.SdkTests
{
	public abstract class MSBuildTestBase
	{
		//public static readonly VisualStudioInstance CurrentVisualStudioInstance = MSBuildLocator.RegisterDefaults ();

		protected MSBuildTestBase ()
		{
			MSBuildPath = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild/15.0/bin";
			MSBuildLocator.RegisterMSBuildPath (MSBuildPath);
			//MSBuildPath = CurrentVisualStudioInstance.MSBuildPath;
		}

		protected string MSBuildPath { get; }
	}

	public abstract class MSBuildSdkTestBase : MSBuildTestBase, IDisposable
	{
		static readonly string TestAssemblyPathValue = typeof (MSBuildSdkTestBase).Assembly.ManifestModule.FullyQualifiedName;

		readonly string _testRootPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());

		public string TestAssemblyPath => TestAssemblyPathValue;

		public string TestRootPath {
			get {
				Directory.CreateDirectory (_testRootPath);
				return _testRootPath;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (Directory.Exists (TestRootPath)) {
					Directory.Delete (TestRootPath, recursive: true);
				}
			}
		}

		protected string GetTempFile (string name)
		{
			if (name == null) {
				throw new ArgumentNullException (nameof (name));
			}

			return Path.Combine (TestRootPath, name);
		}

		protected string GetTempFileWithExtension (string extension = null)
		{
			return Path.Combine (TestRootPath, $"{Path.GetRandomFileName ()}{extension ?? string.Empty}");
		}
	}
}
