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
// Novell.Directory.Ldap.Events.LdapEventSource.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;

using Novell.Directory.Ldap;

namespace Novell.Directory.Ldap.Events
{
  /// <summary> 
  /// This is the base class for any EventSource.
  /// </summary>
  /// <seealso cref='Novell.Directory.Ldap.Events.PSearchEventSource'/>
  /// <seealso cref='Novell.Directory.Ldap.Events.Edir.EdirEventSource'/>
  public abstract class LdapEventSource
  {
    protected enum LISTENERS_COUNT
    {
      ZERO,
      ONE,
      MORE_THAN_ONE
    }

    internal protected const int EVENT_TYPE_UNKNOWN = -1;
    protected const int DEFAULT_SLEEP_TIME = 1000;

    protected int sleep_interval = DEFAULT_SLEEP_TIME;

    /// <summary> 
    /// SleepInterval controls the duration after which event polling is repeated.
    /// </summary>
    public int SleepInterval
    {
      get
      {
	return sleep_interval;
      }
      set
      {
		  if(value <= 0)
			  throw new ArgumentOutOfRangeException("SleepInterval","cannot take the negative or zero values ");
		  else
			  sleep_interval = value;
      }
    }

    protected abstract int GetListeners();

    protected LISTENERS_COUNT GetCurrentListenersState()
    {
      int nListeners = 0;

      // Get Listeners registered with Actual EventSource
      nListeners += GetListeners();

      // Get Listeners registered for generic events
      if (null != directory_event)
	nListeners += directory_event.GetInvocationList().Length;

      // Get Listeners registered for exception events
      if (null != directory_exception_event)
	nListeners += directory_exception_event.GetInvocationList().Length;

      if (0 == nListeners)
	return LISTENERS_COUNT.ZERO;
      
      if (1 == nListeners)
	return LISTENERS_COUNT.ONE;

      return LISTENERS_COUNT.MORE_THAN_ONE;
    }

    protected void ListenerAdded()
    {
      // Get current state
      LISTENERS_COUNT lc = GetCurrentListenersState();

      switch (lc)
      {
      case LISTENERS_COUNT.ONE:
	// start search and polling if not already started
	StartSearchAndPolling();
	break;

      case LISTENERS_COUNT.ZERO:
      case LISTENERS_COUNT.MORE_THAN_ONE:
      default:
	break;
      }
    }

    protected void ListenerRemoved()
    {
      // Get current state
      LISTENERS_COUNT lc = GetCurrentListenersState();

      switch (lc)
      {
      case LISTENERS_COUNT.ZERO:
	// stop search and polling if not already stopped
	StopSearchAndPolling();
	break;

      case LISTENERS_COUNT.ONE:
      case LISTENERS_COUNT.MORE_THAN_ONE:
      default:
	break;
      }
    }

    protected abstract void StartSearchAndPolling();
    protected abstract void StopSearchAndPolling();

    protected DirectoryEventHandler directory_event;

    /// <summary> 
    /// DirectoryEvent represents a generic Directory event.
    /// If any event is not recognized by the actual
    /// event sources, an object of corresponding DirectoryEventArgs 
    /// class is passed as part of the notification.
    /// </summary>
    public event DirectoryEventHandler DirectoryEvent
    {
      add
      {
	directory_event += value;
	ListenerAdded();
      }
      remove
      {
	directory_event -= value;
	ListenerRemoved();
      }
    }

    /// <summary> 
    /// DirectoryEventHandler is the delegate definition for DirectoryEvent.
    /// The client (listener) has to register using this delegate in order to
    /// get events that may not be recognized by the actual event source.
    /// </summary>
    public delegate void DirectoryEventHandler(object source, DirectoryEventArgs objDirectoryEventArgs);

    protected DirectoryExceptionEventHandler directory_exception_event;
    /// <summary> 
    /// DirectoryEvent represents a generic Directory exception event.
    /// </summary>
    public event DirectoryExceptionEventHandler DirectoryExceptionEvent
    {
      add
      {
	directory_exception_event += value;
	ListenerAdded();
      }
      remove
      {
	directory_exception_event -= value;
	ListenerRemoved();
      }
    }

    /// <summary> 
    /// DirectoryEventHandler is the delegate definition for DirectoryExceptionEvent.
    /// </summary>
    public delegate void DirectoryExceptionEventHandler(object source, 
						   DirectoryExceptionEventArgs objDirectoryExceptionEventArgs);

    protected EventsGenerator m_objEventsGenerator = null;

    protected void StartEventPolling(
				 LdapMessageQueue queue,
				 LdapConnection conn,
				 int msgid)
    {
      // validate the argument values
      if ( (queue == null)
	   || (conn == null))
      {
	throw new ArgumentException("No parameter can be Null.");
      }

      if (null == m_objEventsGenerator)
      {
	m_objEventsGenerator =  new EventsGenerator(this, queue, conn, msgid);
	m_objEventsGenerator.SleepTime = sleep_interval;

	m_objEventsGenerator.StartEventPolling();
      }
    } // end of method StartEventPolling

    protected void StopEventPolling()
    {
      if (null != m_objEventsGenerator)
      {
	m_objEventsGenerator.StopEventPolling();
	m_objEventsGenerator = null;
      }
    } // end of method StopEventPolling

    protected abstract bool 
    NotifyEventListeners(LdapMessage sourceMessage,
			 EventClassifiers aClassification,
			 int nType);

    protected void NotifyListeners(LdapMessage sourceMessage,
				   EventClassifiers aClassification,
				   int nType)
    {
      // first let the actual source Notify the listeners with
      // appropriate EventArgs
      
      bool bListenersNotified = NotifyEventListeners(sourceMessage,
						     aClassification, 
						     nType);

      if (!bListenersNotified)
      {
	// Actual EventSource could not recognize the event
	// Just notify the listeners for generic directory events
	NotifyDirectoryListeners(sourceMessage, aClassification);
      }
    }

    protected void NotifyDirectoryListeners(LdapMessage sourceMessage,
					  EventClassifiers aClassification)
    {
      NotifyDirectoryListeners(new DirectoryEventArgs(sourceMessage, 
						      aClassification));
    }

    protected void NotifyDirectoryListeners(DirectoryEventArgs objDirectoryEventArgs)
    {
      if (null != directory_event)
      {
	directory_event(this, objDirectoryEventArgs);
      }
    }

    protected void NotifyExceptionListeners(LdapMessage sourceMessage, LdapException ldapException)
    {
      if (null != directory_exception_event)
      {
	directory_exception_event(this, new DirectoryExceptionEventArgs(sourceMessage, ldapException));
      }
    }

  
    ///  <summary> This is a nested class that is supposed to monitor 
    ///  LdapMessageQueue for events generated by the LDAP Server.
    /// 
    ///  </summary>
    protected class EventsGenerator
    {
      private LdapEventSource m_objLdapEventSource;
      private LdapMessageQueue searchqueue;
      private int messageid;
      private LdapConnection ldapconnection;
      private volatile bool isrunning = true;
      
      private int sleep_time;
      /// <summary> 
      /// SleepTime controls the duration after which event polling is repeated.
      /// </summary>
      public int SleepTime
      {
	get
	{
	  return sleep_time;
	}
	set
	{
	  sleep_time = value;
	}
      }
      
      
      public EventsGenerator(LdapEventSource objEventSource,
			     LdapMessageQueue queue,
			     LdapConnection conn,
			     int msgid)
      {
	m_objLdapEventSource = objEventSource;
	searchqueue = queue;
	ldapconnection = conn;
	messageid = msgid;
	sleep_time = DEFAULT_SLEEP_TIME;
      } // end of Constructor
      
      protected void Run() 
      {
	while (isrunning) 
	{
	  LdapMessage response = null;
	  try 
	  {
	    while ((isrunning)
		   && (!searchqueue.isResponseReceived(messageid))) 
	    {
	      try 
	      {
		Thread.Sleep(sleep_time);
	      } 
	      catch (ThreadInterruptedException e) 
	      {
		Console.WriteLine("EventsGenerator::Run Got ThreadInterruptedException e = {0}", e);
	      }
	    }
	    
	    if (isrunning) 
	    {
	      response = searchqueue.getResponse(messageid);
	    }
	    
	    if (response != null) 
	    {
	      processmessage(response);
	    }
	  } 
	  catch (LdapException e) 
	  {
	    m_objLdapEventSource.NotifyExceptionListeners(response, e);
	  }
	}
      } // end of method run
      
      protected void processmessage(LdapMessage response) 
      {
	if (response is LdapResponse) 
	{
	  try 
	  {
	    ((LdapResponse) response).chkResultCode();
	    
	    m_objLdapEventSource.NotifyEventListeners(response, 
						      EventClassifiers.CLASSIFICATION_UNKNOWN, 
						      EVENT_TYPE_UNKNOWN);
	  } 
	  catch (LdapException e) 
	  {
	    m_objLdapEventSource.NotifyExceptionListeners(response, e);
	  }
	} 
	else 
	{
	  m_objLdapEventSource.NotifyEventListeners(response, 
						    EventClassifiers.CLASSIFICATION_UNKNOWN, 
						    EVENT_TYPE_UNKNOWN);
	}
      } // end of method processmessage
      
      public void StartEventPolling()
      {
	isrunning = true;
	new Thread( new ThreadStart( Run ) ).Start();
      }
      
      public void StopEventPolling() 
      {
	isrunning = false;
      } // end of method stopEventGeneration
    } // end of class EventsGenerator
    
  } // end of class LdapEventSource
  
} // end of namespace
