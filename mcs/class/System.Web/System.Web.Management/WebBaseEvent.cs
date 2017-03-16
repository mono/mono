//
// System.Web.Management.WebBaseEvent.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;

namespace System.Web.Management
{
        public class WebBaseEvent
        {
                string message;
                object event_source;
                int event_code, event_detail_code;

                protected WebBaseEvent (string message, object eventSource, int eventCode)
                {
                        this.message = message;
                        this.event_source = eventSource;
                        this.event_code = eventCode;
                }

                protected WebBaseEvent (string message, object eventSource, int eventCode, int eventDetailCode)
                {
                        this.message = message;
                        this.event_source = eventSource;
                        this.event_code = eventCode;
                        this.event_detail_code = eventDetailCode;
                }
                
                public static WebApplicationInformation ApplicationInformation {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public int EventCode {
                        get {
                                return event_code;
                        }
                }

                public int EventDetailCode {
                        get {
                                return event_detail_code;
                        }
                }

                public Guid EventID {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public long EventSequence {
                        get {

                                throw new NotImplementedException ();
                        }
                }

                public object EventSource {
                        get {
                                return event_source;
                        }
                }

                public DateTime EventTime {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public DateTime EventTimeUtc {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public string Message {
                        get {
                                return message;
                        }
                }

                public virtual void FormatCustomEventDetails (WebEventFormatter formatter)
                {
                        throw new NotImplementedException ();
                }

                public virtual void Raise ()
                {
                        throw new NotImplementedException ();
                }

                public static void Raise (WebBaseEvent eventRaised)
                {
                        throw new NotImplementedException ();
                }

                public override string ToString ()
                {
                        throw new NotImplementedException ();
                }

                public virtual string ToString (bool includeAppInfo, bool includeCustomEventDetails)
                {
                        throw new NotImplementedException ();
                }
        }
}
