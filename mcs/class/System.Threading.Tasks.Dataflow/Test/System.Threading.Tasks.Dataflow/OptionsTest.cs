// OptionsTest.cs
//  
// Copyright (c) 2012 Petr Onderka
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class OptionsTest {
		private static IEnumerable<IDataflowBlock> CreateBlocksWithNameFormat(string nameFormat)
		{
			var dataflowBlockOptions = new DataflowBlockOptions { NameFormat = nameFormat };
			var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions { NameFormat = nameFormat };
			var groupingDataflowBlockOptions = new GroupingDataflowBlockOptions { NameFormat = nameFormat };

			yield return new ActionBlock<int> (i => { }, executionDataflowBlockOptions);
			yield return new BatchBlock<double> (10, dataflowBlockOptions);
			yield return new BatchedJoinBlock<dynamic, object> (
				10, groupingDataflowBlockOptions);
			yield return new BatchedJoinBlock<dynamic, object, char> (
				10, groupingDataflowBlockOptions);
			yield return new BroadcastBlock<byte> (x => x, dataflowBlockOptions);
			yield return new BufferBlock<int> (dataflowBlockOptions);
			yield return new JoinBlock<double, dynamic> (groupingDataflowBlockOptions);
			yield return new JoinBlock<object, char, byte> (
				groupingDataflowBlockOptions);
			yield return new TransformBlock<int, int> (
				i => i, executionDataflowBlockOptions);
			yield return new TransformManyBlock<double, dynamic> (
				x => null, executionDataflowBlockOptions);
			yield return new WriteOnceBlock<object> (x => x, dataflowBlockOptions);
		}

		[Test]
		public void NameFormatTest ()
		{
			var constant = "constant";
			foreach (var block in CreateBlocksWithNameFormat (constant))
				Assert.AreEqual (constant, block.ToString ());

			foreach (var block in CreateBlocksWithNameFormat ("{0}"))
				Assert.AreEqual (block.GetType ().ToString (), block.ToString ());

			foreach (var block in CreateBlocksWithNameFormat ("{1}"))
				Assert.AreEqual (block.Completion.Id.ToString (), block.ToString ());
		}
	}
}