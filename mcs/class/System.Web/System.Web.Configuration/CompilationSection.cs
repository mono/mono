//
// System.Web.Configuration.CompilationSection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
//

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
using System;
using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class CompilationSection : InternalSection
	{
		static ConfigurationPropertyCollection props;
		static ConfigurationProperty compilers;
		static ConfigurationProperty tempDirectory;
		static ConfigurationProperty debug;
		static ConfigurationProperty strict;
		static ConfigurationProperty _explicit;
		static ConfigurationProperty batch;
		static ConfigurationProperty batchTimeout;
		static ConfigurationProperty maxBatchSize;
		static ConfigurationProperty maxBatchGeneratedFileSize;
		static ConfigurationProperty numRecompilesBeforeAppRestart;
		static ConfigurationProperty defaultLanguage;
		static ConfigurationProperty assemblies;
		static ConfigurationProperty buildProviders;
		static ConfigurationProperty expressionBuilders;
		static ConfigurationProperty urlLinePragmas;
		static ConfigurationProperty codeSubDirectories;

		static CompilationSection ()
		{
			props = new ConfigurationPropertyCollection ();
			Type strType = typeof (string);
			TypeConverter strTypeConv = new StringConverter ();
			Type boolType = typeof (bool);
			TypeConverter boolTypeConv = new BooleanConverter ();
			Type intType = typeof (int);
			TypeConverter intTypeConv = new Int32Converter ();

			assemblies = new ConfigurationProperty ("assemblies", typeof (AssemblyCollection), 0);
			props.Add (assemblies);
			batch = new ConfigurationProperty ("batch", boolType, true, boolTypeConv, null, 0);
			props.Add (batch);
			buildProviders = new ConfigurationProperty ("buidProviders", typeof (BuildProviderCollection), 0);
			props.Add (buildProviders);
			batchTimeout = new ConfigurationProperty ("batchTimeout", typeof (TimeSpan), new TimeSpan (0, 15, 0),
							new TimeSpanConverter (), null, 0);
			props.Add (batchTimeout);
			codeSubDirectories = new ConfigurationProperty ("codeSubDirectories", typeof (CodeSubDirectoriesCollection), 0);
			props.Add (codeSubDirectories);
			compilers = new ConfigurationProperty ("compilers", typeof (CompilerCollection), 0);
			props.Add (compilers);
			debug = new ConfigurationProperty ("debug", boolType, false, boolTypeConv, null, 0);
			props.Add (debug);
			defaultLanguage = new ConfigurationProperty ("defaultLanguage", strType, "c#", strTypeConv, null, 0);
			props.Add (defaultLanguage);
			expressionBuilders = new ConfigurationProperty ("expressionBuilders", typeof (ExpressionBuilderCollection), 0);
			props.Add (expressionBuilders);
			_explicit = new ConfigurationProperty ("explicit", boolType, true, boolTypeConv, null, 0);
			props.Add (_explicit);
			maxBatchSize = new ConfigurationProperty ("maxBatchSize", intType, 1000, intTypeConv, null, 0);
			props.Add (maxBatchSize);
			maxBatchGeneratedFileSize = new ConfigurationProperty ("maxBatchGeneratedFileSize", intType, 3000, intTypeConv, null, 0);
			props.Add (maxBatchGeneratedFileSize);
			numRecompilesBeforeAppRestart = new ConfigurationProperty ("numRecompilesBeforeAppRestart", intType, 15, intTypeConv, null, 0);
			props.Add (numRecompilesBeforeAppRestart);
			strict = new ConfigurationProperty ("strict", boolType, false, boolTypeConv, null, 0);
			props.Add (strict);
			tempDirectory = new ConfigurationProperty ("tempDirectory", strType, "", strTypeConv, null, 0);
			props.Add (tempDirectory);
			urlLinePragmas = new ConfigurationProperty ("urlLinePragmas", boolType, false, boolTypeConv, null, 0);
			props.Add (urlLinePragmas);
		}

		public CompilationSection ()
		{
		}

		public AssemblyCollection Assemblies {
			get { return (AssemblyCollection) this [assemblies]; }
		}

		public bool Batch {
			get { return (bool) this [batch]; }
			set { this [batch] = value; }
		}

		public TimeSpan BatchTimeout {
			get { return (TimeSpan) this [batchTimeout]; }
			set { this [batchTimeout] = value; }
		}

		public BuildProviderCollection BuildProviders {
			get { return (BuildProviderCollection) this [buildProviders]; }
		}

		public CodeSubDirectoriesCollection CodeSubDirectories {
			get { return (CodeSubDirectoriesCollection) this [codeSubDirectories]; }
		}

		public CompilerCollection Compilers {
			get { return (CompilerCollection) this [compilers]; }
		}

		public bool Debug {
			get { return (bool) this [debug]; }
			set { this [debug] = value; }
		}

		public string DefaultLanguage {
			get { return (string) this [defaultLanguage]; }
			set { this [defaultLanguage] = value; }
		}

		public bool Explicit {
			get { return (bool) this [_explicit]; }
			set { this [_explicit] = value; }
		}

		public ExpressionBuilderCollection ExpressionBuilders {
			get { return (ExpressionBuilderCollection) this [expressionBuilders]; }
		}

		public int MaxBatchGeneratedFileSize {
			get { return (int) this [maxBatchGeneratedFileSize]; }
			set { this [maxBatchGeneratedFileSize] = value; }
		}

		public int MaxBatchSize {
			get { return (int) this [maxBatchSize]; }
			set { this [maxBatchSize] = value; }
		}

		public int NumRecompilesBeforeAppRestart {
			get { return (int) this [numRecompilesBeforeAppRestart]; }
			set { this [numRecompilesBeforeAppRestart] = value; }
		}

		public bool Strict {
			get { return (bool) this [strict]; }
			set { this [strict] = value; }
		}

		public string TempDirectory {
			get { return (string) this [tempDirectory]; }
			set { this [tempDirectory] = value; }
		}

		public bool UrlLinePragmas {
			get { return (bool) this [urlLinePragmas]; }
			set { this [urlLinePragmas] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return props; }
		}
	}
}
#endif // NET_2_0

