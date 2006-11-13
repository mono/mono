/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Events.Edir.EdirEventSpecifier.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


namespace Novell.Directory.Ldap.Events.Edir
{
  /// <summary>
  /// This class denotes the mechanism to specify the event of interest.
  /// </summary>
  public class EdirEventSpecifier
  {
    private EdirEventType event_type;
    public EdirEventType EventType 
    {
      get
      {
	return event_type;
      }
    }

    private EdirEventResultType event_result_type;
    public EdirEventResultType EventResultType
    {
      get
      {
	return event_result_type;
      }
    }

    private string event_filter;
    public string EventFilter
    {
      get
      {
	return event_filter;
      } 
    }

    public EdirEventSpecifier(EdirEventType eventType, EdirEventResultType eventResultType) :
      this(eventType, eventResultType, null)
    {
    }

    public EdirEventSpecifier(EdirEventType eventType, EdirEventResultType eventResultType, string filter)
    {
      event_type = eventType;
      event_result_type = eventResultType;
      event_filter = filter;
    }
  }
}
