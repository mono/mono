// Blocks.cs
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
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace MonoTests {
	public static class Blocks {
		public static IEnumerable<IDataflowBlock> CreateBlocksWithOptions (
			DataflowBlockOptions dataflowBlockOptions,
			ExecutionDataflowBlockOptions executionDataflowBlockOptions,
			GroupingDataflowBlockOptions groupingDataflowBlockOptions)
		{
			yield return new ActionBlock<int> (i => { }, executionDataflowBlockOptions);
			yield return new BatchBlock<double> (10, groupingDataflowBlockOptions);
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
			yield return new TransformManyBlock<double, dynamic>(
				x => new dynamic[0], executionDataflowBlockOptions);
			yield return new WriteOnceBlock<object> (x => x, dataflowBlockOptions);
		}

		public static IEnumerable<IDataflowBlock> CreateBlocks()
		{
			return CreateBlocksWithOptions (new DataflowBlockOptions (),
				new ExecutionDataflowBlockOptions (), new GroupingDataflowBlockOptions ());
		}

		public static IEnumerable<IDataflowBlock> CreateBlocksWithNameFormat(string nameFormat)
		{
			var dataflowBlockOptions = new DataflowBlockOptions { NameFormat = nameFormat };
			var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions { NameFormat = nameFormat };
			var groupingDataflowBlockOptions = new GroupingDataflowBlockOptions { NameFormat = nameFormat };

			return CreateBlocksWithOptions (dataflowBlockOptions,
				executionDataflowBlockOptions, groupingDataflowBlockOptions);
		}

		public static IEnumerable<IDataflowBlock> CreateBlocksWithCancellationToken(CancellationToken cancellationToken)
		{
			var dataflowBlockOptions = new DataflowBlockOptions { CancellationToken = cancellationToken};
			var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken};
			var groupingDataflowBlockOptions = new GroupingDataflowBlockOptions { CancellationToken = cancellationToken};

			return CreateBlocksWithOptions (dataflowBlockOptions,
				executionDataflowBlockOptions, groupingDataflowBlockOptions);
		}

		public static IEnumerable<IReceivableSourceBlock<T>> CreateSimpleSourceBlocks<T>()
		{
			yield return new BroadcastBlock<T> (x => x);
			yield return new BufferBlock<T> ();
			yield return new TransformBlock<T, T> (
				x => x);
			yield return new TransformManyBlock<T, T>(
				x => new T[0]);
			yield return new WriteOnceBlock<T> (x => x);
		}

		public static IEnumerable<ITargetBlock<T>> CreateTargetBlocks<T> ()
		{
			yield return new ActionBlock<T> (i => { });
			yield return new BatchBlock<T> (10);
			yield return new BatchedJoinBlock<T, T> (10).Target1;
			yield return new BatchedJoinBlock<T, T> (10).Target2;
			yield return new BatchedJoinBlock<T, T, T> (10).Target1;
			yield return new BatchedJoinBlock<T, T, T> (10).Target2;
			yield return new BatchedJoinBlock<T, T, T> (10).Target3;
			yield return new BroadcastBlock<T> (x => x);
			yield return new BufferBlock<T> ();
			yield return new JoinBlock<T, T> ().Target1;
			yield return new JoinBlock<T, T> ().Target2;
			yield return new JoinBlock<T, T, T> ().Target1;
			yield return new JoinBlock<T, T, T> ().Target2;
			yield return new JoinBlock<T, T, T> ().Target3;
			yield return new TransformBlock<T, T> (i => i);
			yield return new TransformManyBlock<T, T>(x => new T[0]);
			yield return new WriteOnceBlock<T> (x => x);
			yield return DataflowBlock.NullTarget<T> ();
		}
	}
}