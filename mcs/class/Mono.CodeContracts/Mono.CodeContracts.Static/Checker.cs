// 
// Checker.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.Analysis.NonNull;
using Mono.CodeContracts.Static.Analysis.Numerical;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static {
	public class Checker {
		private readonly CheckOptions options;
		private CodeContractsAnalysisDriver<IMethodResult<SymbolicValue>> analysis_driver;
		private Dictionary<string, IMethodAnalysis> analyzers;

		private Checker (CheckOptions options)
		{
			this.options = options;
		}

		public static CheckResults Check (CheckOptions options)
		{
			var checker = new Checker (options);
			return checker.Analyze ();
		}

		private CheckResults Analyze ()
		{
			if (this.options.Assembly == null)
				return CheckResults.Error ("No assembly given to check");

			DebugOptions.Debug = this.options.ShowDebug;

		        this.analyzers = new Dictionary<string, IMethodAnalysis> {
		                {"non-null", new NonNullAnalysisFacade ()},
		                {"arithmetic", new Analysers.Arithmetic ()}
		        };

			this.analysis_driver = new CodeContractsAnalysisDriver<IMethodResult<SymbolicValue>> (
				new BasicAnalysisDriver (MetaDataProvider.Instance, CodeContractDecoder.Instance));

			return AnalyzeAssembly (this.options.Assembly);
		}

		private CheckResults AnalyzeAssembly (string assemblyPath)
		{
			IMetaDataProvider metadataDecoder = this.analysis_driver.MetaDataProvider;
			AssemblyNode assembly;
			string reason;
			if (!metadataDecoder.TryLoadAssembly (assemblyPath, out assembly, out reason))
				return CheckResults.Error (string.Format ("Cannot load assembly: {0}", reason));

			var proofResults = new Dictionary<string, ICollection<string>> ();
			foreach (Method method in metadataDecoder.Methods (assembly))
				AnalyzeMethod (method, proofResults);
			if (proofResults.Count == 0)
				return CheckResults.Error ("No methods found.");

			return new CheckResults (null, null, proofResults);
		}

		private void AnalyzeMethod (Method method, Dictionary<string, ICollection<string>> proofResults)
		{
			IMetaDataProvider metadataDecoder = this.analysis_driver.MetaDataProvider;
			if (!metadataDecoder.HasBody (method))
				return;
			if (this.options.Method != null && !metadataDecoder.FullName (method).Contains (this.options.Method))
				return;

			var results = new List<string> ();
			proofResults.Add (method.FullName, results);
			try {
				AnalyzeMethodInternal (method, results);
			} catch (Exception e) {
				results.Add ("Exception: " + e.Message);
				return;
			}

			results.Add (string.Format ("Checked {0} assertions", results.Count));
		}

		private void AnalyzeMethodInternal (Method method, List<string> proofResults)
		{
			string fullMethodName = method.FullName;
			IMethodDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> methodDriver = this.analysis_driver.CreateMethodDriver (method);

			methodDriver.RunHeapAndExpressionAnalyses ();

			var results = new List<IMethodResult<SymbolicValue>> (this.analyzers.Values.Count);
			foreach (IMethodAnalysis analysis in this.analyzers.Values) {
				IMethodResult<SymbolicValue> result = analysis.Analyze (fullMethodName, methodDriver);
				results.Add (result);
			}

			ComposedFactQuery<SymbolicValue> facts = CreateFactQuery (methodDriver.BasicFacts.IsUnreachable, results);
			foreach (var methodResult in results)
				methodResult.ValidateImplicitAssertions (facts, proofResults);

			AssertionFinder.ValidateAssertions (facts, methodDriver, proofResults);
		}

		private ComposedFactQuery<Variable> CreateFactQuery<Variable> (Predicate<APC> isUnreachable, IEnumerable<IMethodResult<Variable>> results)
		{
			var res = new ComposedFactQuery<Variable> (isUnreachable);
			res.Add (new ConstantPropagationFactQuery<Variable> ());
			foreach (var methodResult in results)
				res.Add (methodResult.FactQuery);
			return res;
		}
	}
}
