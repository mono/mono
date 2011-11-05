/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using AttributeImpl = Mono.Lucene.Net.Util.AttributeImpl;
using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> This TokenFilter provides the ability to set aside attribute states
	/// that have already been analyzed.  This is useful in situations where multiple fields share
	/// many common analysis steps and then go their separate ways.
	/// <p/>
	/// It is also useful for doing things like entity extraction or proper noun analysis as
	/// part of the analysis workflow and saving off those tokens for use in another field.
	/// 
	/// <pre>
	/// TeeSinkTokenFilter source1 = new TeeSinkTokenFilter(new WhitespaceTokenizer(reader1));
	/// TeeSinkTokenFilter.SinkTokenStream sink1 = source1.newSinkTokenStream();
	/// TeeSinkTokenFilter.SinkTokenStream sink2 = source1.newSinkTokenStream();
	/// TeeSinkTokenFilter source2 = new TeeSinkTokenFilter(new WhitespaceTokenizer(reader2));
	/// source2.addSinkTokenStream(sink1);
	/// source2.addSinkTokenStream(sink2);
	/// TokenStream final1 = new LowerCaseFilter(source1);
	/// TokenStream final2 = source2;
	/// TokenStream final3 = new EntityDetect(sink1);
	/// TokenStream final4 = new URLDetect(sink2);
	/// d.add(new Field("f1", final1));
	/// d.add(new Field("f2", final2));
	/// d.add(new Field("f3", final3));
	/// d.add(new Field("f4", final4));
	/// </pre>
	/// In this example, <code>sink1</code> and <code>sink2</code> will both get tokens from both
	/// <code>reader1</code> and <code>reader2</code> after whitespace tokenizer
	/// and now we can further wrap any of these in extra analysis, and more "sources" can be inserted if desired.
	/// It is important, that tees are consumed before sinks (in the above example, the field names must be
	/// less the sink's field names). If you are not sure, which stream is consumed first, you can simply
	/// add another sink and then pass all tokens to the sinks at once using {@link #consumeAllTokens}.
	/// This TokenFilter is exhausted after this. In the above example, change
	/// the example above to:
	/// <pre>
	/// ...
	/// TokenStream final1 = new LowerCaseFilter(source1.newSinkTokenStream());
	/// TokenStream final2 = source2.newSinkTokenStream();
	/// sink1.consumeAllTokens();
	/// sink2.consumeAllTokens();
	/// ...
	/// </pre>
	/// In this case, the fields can be added in any order, because the sources are not used anymore and all sinks are ready.
	/// <p/>Note, the EntityDetect and URLDetect TokenStreams are for the example and do not currently exist in Lucene.
	/// </summary>
	public sealed class TeeSinkTokenFilter:TokenFilter
	{
		public class AnonymousClassSinkFilter:SinkFilter
		{
			public override bool Accept(AttributeSource source)
			{
				return true;
			}
		}
		private System.Collections.IList sinks = new System.Collections.ArrayList();
		
		/// <summary> Instantiates a new TeeSinkTokenFilter.</summary>
		public TeeSinkTokenFilter(TokenStream input):base(input)
		{
		}
		
		/// <summary> Returns a new {@link SinkTokenStream} that receives all tokens consumed by this stream.</summary>
		public SinkTokenStream NewSinkTokenStream()
		{
			return NewSinkTokenStream(ACCEPT_ALL_FILTER);
		}
		
		/// <summary> Returns a new {@link SinkTokenStream} that receives all tokens consumed by this stream
		/// that pass the supplied filter.
		/// </summary>
		/// <seealso cref="SinkFilter">
		/// </seealso>
		public SinkTokenStream NewSinkTokenStream(SinkFilter filter)
		{
			SinkTokenStream sink = new SinkTokenStream(this.CloneAttributes(), filter);
			this.sinks.Add(new System.WeakReference(sink));
			return sink;
		}
		
		/// <summary> Adds a {@link SinkTokenStream} created by another <code>TeeSinkTokenFilter</code>
		/// to this one. The supplied stream will also receive all consumed tokens.
		/// This method can be used to pass tokens from two different tees to one sink.
		/// </summary>
		public void  AddSinkTokenStream(SinkTokenStream sink)
		{
			// check that sink has correct factory
			if (!this.GetAttributeFactory().Equals(sink.GetAttributeFactory()))
			{
				throw new System.ArgumentException("The supplied sink is not compatible to this tee");
			}
			// add eventually missing attribute impls to the existing sink
            foreach (AttributeImpl impl in this.CloneAttributes().GetAttributeImplsIterator())
            {
                sink.AddAttributeImpl(impl);
            }
			this.sinks.Add(new System.WeakReference(sink));
		}
		
		/// <summary> <code>TeeSinkTokenFilter</code> passes all tokens to the added sinks
		/// when itself is consumed. To be sure, that all tokens from the input
		/// stream are passed to the sinks, you can call this methods.
		/// This instance is exhausted after this, but all sinks are instant available.
		/// </summary>
		public void  ConsumeAllTokens()
		{
			while (IncrementToken())
				;
		}
		
		public override bool IncrementToken()
		{
			if (input.IncrementToken())
			{
				// capture state lazily - maybe no SinkFilter accepts this state
				AttributeSource.State state = null;
				for (System.Collections.IEnumerator it = sinks.GetEnumerator(); it.MoveNext(); )
				{
					SinkTokenStream sink = (SinkTokenStream) ((System.WeakReference) it.Current).Target;
					if (sink != null)
					{
						if (sink.Accept(this))
						{
							if (state == null)
							{
								state = this.CaptureState();
							}
							sink.AddState(state);
						}
					}
				}
				return true;
			}
			
			return false;
		}
		
		public override void  End()
		{
			base.End();
			AttributeSource.State finalState = CaptureState();
			for (System.Collections.IEnumerator it = sinks.GetEnumerator(); it.MoveNext(); )
			{
				SinkTokenStream sink = (SinkTokenStream) ((System.WeakReference) it.Current).Target;
				if (sink != null)
				{
					sink.SetFinalState(finalState);
				}
			}
		}
		
		/// <summary> A filter that decides which {@link AttributeSource} states to store in the sink.</summary>
		public abstract class SinkFilter
		{
			/// <summary> Returns true, iff the current state of the passed-in {@link AttributeSource} shall be stored
			/// in the sink. 
			/// </summary>
			public abstract bool Accept(AttributeSource source);
			
			/// <summary> Called by {@link SinkTokenStream#Reset()}. This method does nothing by default
			/// and can optionally be overridden.
			/// </summary>
			public void  Reset()
			{
				// nothing to do; can be overridden
			}
		}
		
		public sealed class SinkTokenStream:TokenStream
		{
			private System.Collections.IList cachedStates = new System.Collections.ArrayList();
			private AttributeSource.State finalState;
			private System.Collections.IEnumerator it = null;
			private SinkFilter filter;
			
			internal SinkTokenStream(AttributeSource source, SinkFilter filter):base(source)
			{
				this.filter = filter;
			}
			
			internal /*private*/ bool Accept(AttributeSource source)
			{
				return filter.Accept(source);
			}
			
			internal /*private*/ void  AddState(AttributeSource.State state)
			{
				if (it != null)
				{
					throw new System.SystemException("The tee must be consumed before sinks are consumed.");
				}
				cachedStates.Add(state);
			}
			
			internal /*private*/ void  SetFinalState(AttributeSource.State finalState)
			{
				this.finalState = finalState;
			}
			
			public override bool IncrementToken()
			{
				// lazy init the iterator
				if (it == null)
				{
					it = cachedStates.GetEnumerator();
				}
				
				if (!it.MoveNext())
				{
					return false;
				}
				
				AttributeSource.State state = (State) it.Current;
				RestoreState(state);
				return true;
			}
			
			public override void  End()
			{
				if (finalState != null)
				{
					RestoreState(finalState);
				}
			}
			
			public override void  Reset()
			{
				it = cachedStates.GetEnumerator();
			}
		}
		
		private static readonly SinkFilter ACCEPT_ALL_FILTER;
		static TeeSinkTokenFilter()
		{
			ACCEPT_ALL_FILTER = new AnonymousClassSinkFilter();
		}
	}
}
