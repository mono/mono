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

using TokenStream = Mono.Lucene.Net.Analysis.TokenStream;
using OffsetAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.OffsetAttribute;
using PositionIncrementAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PositionIncrementAttribute;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Holds state for inverting all occurrences of a single
	/// field in the document.  This class doesn't do anything
	/// itself; instead, it forwards the tokens produced by
	/// analysis to its own consumer
	/// (InvertedDocConsumerPerField).  It also interacts with an
	/// endConsumer (InvertedDocEndConsumerPerField).
	/// </summary>
	
	sealed class DocInverterPerField:DocFieldConsumerPerField
	{
		
		private DocInverterPerThread perThread;
		private FieldInfo fieldInfo;
		internal InvertedDocConsumerPerField consumer;
		internal InvertedDocEndConsumerPerField endConsumer;
		internal DocumentsWriter.DocState docState;
		internal FieldInvertState fieldState;
		
		public DocInverterPerField(DocInverterPerThread perThread, FieldInfo fieldInfo)
		{
			this.perThread = perThread;
			this.fieldInfo = fieldInfo;
			docState = perThread.docState;
			fieldState = perThread.fieldState;
			this.consumer = perThread.consumer.AddField(this, fieldInfo);
			this.endConsumer = perThread.endConsumer.AddField(this, fieldInfo);
		}
		
		public override void  Abort()
		{
			consumer.Abort();
			endConsumer.Abort();
		}
		
		public override void  ProcessFields(Fieldable[] fields, int count)
		{
			
			fieldState.Reset(docState.doc.GetBoost());
			
			int maxFieldLength = docState.maxFieldLength;
			
			bool doInvert = consumer.Start(fields, count);
			
			for (int i = 0; i < count; i++)
			{
				
				Fieldable field = fields[i];
				
				// TODO FI: this should be "genericized" to querying
				// consumer if it wants to see this particular field
				// tokenized.
				if (field.IsIndexed() && doInvert)
				{
					
					bool anyToken;
					
					if (fieldState.length > 0)
						fieldState.position += docState.analyzer.GetPositionIncrementGap(fieldInfo.name);
					
					if (!field.IsTokenized())
					{
						// un-tokenized field
						System.String stringValue = field.StringValue();
						int valueLength = stringValue.Length;
						perThread.singleTokenTokenStream.Reinit(stringValue, 0, valueLength);
						fieldState.attributeSource = perThread.singleTokenTokenStream;
						consumer.Start(field);
						
						bool success = false;
						try
						{
							consumer.Add();
							success = true;
						}
						finally
						{
							if (!success)
								docState.docWriter.SetAborting();
						}
						fieldState.offset += valueLength;
						fieldState.length++;
						fieldState.position++;
						anyToken = valueLength > 0;
					}
					else
					{
						// tokenized field
						TokenStream stream;
						TokenStream streamValue = field.TokenStreamValue();
						
						if (streamValue != null)
							stream = streamValue;
						else
						{
							// the field does not have a TokenStream,
							// so we have to obtain one from the analyzer
							System.IO.TextReader reader; // find or make Reader
							System.IO.TextReader readerValue = field.ReaderValue();
							
							if (readerValue != null)
								reader = readerValue;
							else
							{
								System.String stringValue = field.StringValue();
								if (stringValue == null)
									throw new System.ArgumentException("field must have either TokenStream, String or Reader value");
								perThread.stringReader.Init(stringValue);
								reader = perThread.stringReader;
							}
							
							// Tokenize field and add to postingTable
							stream = docState.analyzer.ReusableTokenStream(fieldInfo.name, reader);
						}
						
						// reset the TokenStream to the first token
						stream.Reset();
						
						int startLength = fieldState.length;
						
						// deprecated
						bool allowMinus1Position = docState.allowMinus1Position;
						
						try
						{
							int offsetEnd = fieldState.offset - 1;
							
							bool hasMoreTokens = stream.IncrementToken();
							
							fieldState.attributeSource = stream;
							
							OffsetAttribute offsetAttribute = (OffsetAttribute) fieldState.attributeSource.AddAttribute(typeof(OffsetAttribute));
							PositionIncrementAttribute posIncrAttribute = (PositionIncrementAttribute) fieldState.attributeSource.AddAttribute(typeof(PositionIncrementAttribute));
							
							consumer.Start(field);
							
							for (; ; )
							{
								
								// If we hit an exception in stream.next below
								// (which is fairly common, eg if analyzer
								// chokes on a given document), then it's
								// non-aborting and (above) this one document
								// will be marked as deleted, but still
								// consume a docID
								
								if (!hasMoreTokens)
									break;
								
								int posIncr = posIncrAttribute.GetPositionIncrement();
								fieldState.position += posIncr;
								if (allowMinus1Position || fieldState.position > 0)
								{
									fieldState.position--;
								}
								
								if (posIncr == 0)
									fieldState.numOverlap++;
								
								bool success = false;
								try
								{
									// If we hit an exception in here, we abort
									// all buffered documents since the last
									// flush, on the likelihood that the
									// internal state of the consumer is now
									// corrupt and should not be flushed to a
									// new segment:
									consumer.Add();
									success = true;
								}
								finally
								{
									if (!success)
										docState.docWriter.SetAborting();
								}
								fieldState.position++;
								offsetEnd = fieldState.offset + offsetAttribute.EndOffset();
								if (++fieldState.length >= maxFieldLength)
								{
									if (docState.infoStream != null)
										docState.infoStream.WriteLine("maxFieldLength " + maxFieldLength + " reached for field " + fieldInfo.name + ", ignoring following tokens");
									break;
								}
								
								hasMoreTokens = stream.IncrementToken();
							}
							// trigger streams to perform end-of-stream operations
							stream.End();
							
							fieldState.offset += offsetAttribute.EndOffset();
							anyToken = fieldState.length > startLength;
						}
						finally
						{
							stream.Close();
						}
					}
					
					if (anyToken)
						fieldState.offset += docState.analyzer.GetOffsetGap(field);
					fieldState.boost *= field.GetBoost();
				}
                
                // LUCENE-2387: don't hang onto the field, so GC can
                // reclaim
                fields[i] = null;
			}
			
			consumer.Finish();
			endConsumer.Finish();
		}
	}
}
