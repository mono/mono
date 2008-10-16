/* 
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
namespace Lucene.Net.Demo.Html
{
	
    class ParserThread:SupportClass.ThreadClass
    {
        internal HTMLParser parser;
		
        internal ParserThread(HTMLParser p)
        {
            parser = p;
        }
		
        override public void  Run()
        {
            // convert pipeOut to pipeIn
            try
            {
                try
                {
                    // parse document to pipeOut
                    parser.HTMLDocument();
                }
                catch (ParseException e)
                {
                    System.Console.Out.WriteLine("Parse Aborted: " + e.Message);
                }
                catch (TokenMgrError e)
                {
                    System.Console.Out.WriteLine("Parse Aborted: " + e.Message);
                }
                finally
                {
                    //parser.pipeOut.Close();
                    lock (parser)
                    {
                        parser.summary.Length = Lucene.Net.Demo.Html.HTMLParser.SUMMARY_LENGTH;
                        parser.summaryComplete = true;
                        parser.titleComplete = true;
                        System.Threading.Monitor.PulseAll(parser);
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                System.Console.Error.WriteLine(e.StackTrace);
            }
        }
    }
}