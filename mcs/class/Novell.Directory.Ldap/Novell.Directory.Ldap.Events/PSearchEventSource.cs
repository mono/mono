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
// Novell.Directory.Ldap.Events.PSearchEventSource.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using Novell.Directory.Ldap.Controls;

namespace Novell.Directory.Ldap.Events
{
  /// <summary> 
  /// This is the source class for Ldap events.
  /// </summary>
  public class PSearchEventSource : LdapEventSource
  {
    protected SearchResultEventHandler search_result_event;

    /// <summary> 
    /// Caller has to register with this event in order to be notified of
    /// corresponding Ldap search result event.
    /// </summary>
    public event SearchResultEventHandler SearchResultEvent
    {
      add
      {
	search_result_event += value;
	ListenerAdded();
      }
      remove
      {
	search_result_event -= value;
	ListenerRemoved();
      }
    }

    protected SearchReferralEventHandler search_referral_event;

    /// <summary>
    /// Caller has to register with this event in order to be notified of
    /// corresponding Ldap search reference event.
    /// </summary>
    public event SearchReferralEventHandler SearchReferralEvent
    {
      add
      {
	search_referral_event += value;
	ListenerAdded();
      }
      remove
      {
	search_referral_event -= value;
	ListenerRemoved();
      }
    }

    /// <summary> 
    /// SearchResultEventHandler is the delegate definition for SearchResultEvent.
    /// The client (listener) has to register using this delegate in order to
    /// get corresponding Ldap events.
    /// </summary>
    public delegate 
    void SearchResultEventHandler(
				  object source,
				  SearchResultEventArgs objArgs
				  );

    /// <summary> 
    /// SearchReferralEventHandler is the delegate definition for SearchReferralEvent.
    /// The client (listener) has to register using this delegate in order to
    /// get corresponding Ldap events.
    /// </summary>
    public delegate 
    void SearchReferralEventHandler(
				    object source,
				    SearchReferralEventArgs objArgs
				    );

    protected override int GetListeners()
    {
      int nListeners = 0;
      if (null != search_result_event)
	nListeners = search_result_event.GetInvocationList().Length;

      if (null != search_referral_event)
	nListeners += search_referral_event.GetInvocationList().Length;

      return nListeners;
    }

    protected LdapConnection mConnection;
    protected string mSearchBase;
    protected int mScope;
    protected string[] mAttrs;
    protected string mFilter;
    protected bool mTypesOnly;
    protected LdapSearchConstraints mSearchConstraints;
    protected LdapEventType mEventChangeType;

    protected LdapSearchQueue mQueue;

    // Constructor
    public PSearchEventSource(
			      LdapConnection conn,
			      string searchBase,
			      int scope,
			      string filter,
			      string[] attrs,
			      bool typesOnly,
			      LdapSearchConstraints constraints,
			      LdapEventType eventchangetype,
			      bool changeonly
			      )
    {
      // validate the input arguments
      if ((conn == null)
	  || (searchBase == null)
	  || (filter == null)
	  || (attrs == null)) 
      {
	throw new ArgumentException("Null argument specified");
      }
      
      mConnection = conn;
      mSearchBase = searchBase;
      mScope = scope;
      mFilter = filter;
      mAttrs = attrs;
      mTypesOnly = typesOnly;
      mEventChangeType = eventchangetype;

      // make things ready for starting a search operation
      if (constraints == null) 
      {
	mSearchConstraints = new LdapSearchConstraints();
      } 
      else 
      {
	mSearchConstraints = constraints;
      }
      
      //Create the persistent search control
      LdapPersistSearchControl psCtrl =
	new LdapPersistSearchControl((int)eventchangetype,// any change
				     changeonly, //only get changes
				     true, //return entry change controls
				     true); //control is critcal

      // add the persistent search control to the search constraints
      mSearchConstraints.setControls(psCtrl);
    } // end of Constructor

    protected override void StartSearchAndPolling()
    {
      // perform the search with no attributes returned
      mQueue =
	mConnection.Search(mSearchBase, // container to search
		    mScope, // search container's subtree
		    mFilter, // search filter, all objects
		    mAttrs, // don't return attributes
		    mTypesOnly, // return attrs and values or attrs only.
		    null, // use default search queue
		    mSearchConstraints); // use default search constraints

      int[] ids = mQueue.MessageIDs;

      if (ids.Length != 1)
      {
	throw new LdapException(
				null,
				LdapException.LOCAL_ERROR,
				"Unable to Obtain Message Id"
				);
      }

      StartEventPolling(mQueue, mConnection, ids[0]);
    }

    protected override void StopSearchAndPolling()
    {
      mConnection.Abandon(mQueue);
      StopEventPolling();
    }

    protected override bool NotifyEventListeners(LdapMessage sourceMessage,
			       EventClassifiers aClassification,
			       int nType)
    {
      bool bListenersNotified = false;
      if (null == sourceMessage)
      {
	return bListenersNotified;
      }

      switch (sourceMessage.Type)
      {
      case LdapMessage.SEARCH_RESULT_REFERENCE :
	if (null != search_referral_event)
	{
	  search_referral_event(this,
				new SearchReferralEventArgs(
							sourceMessage,
							aClassification,
							(LdapEventType)nType)
				);
	  bListenersNotified = true;
	}
	break;

      case LdapMessage.SEARCH_RESPONSE:
	if (null != search_result_event)
	{
	  LdapEventType changeType = LdapEventType.TYPE_UNKNOWN;
	  LdapControl[] controls = sourceMessage.Controls;
	  foreach(LdapControl control in controls)
	  {
	    if (control is LdapEntryChangeControl)
	    {
	      changeType = (LdapEventType)(((LdapEntryChangeControl)control).ChangeType);
	      // TODO: Why is this continue here..? (from Java code..)
	      // TODO: Why are we interested only in the last changeType..?
	      continue;
	    }
	  }
	  // if no changeType then value is TYPE_UNKNOWN
	  search_result_event(this, 
			      new SearchResultEventArgs(
							sourceMessage, 
							aClassification, 
							changeType)
			      );
	  bListenersNotified = true;
	}
	break;

      case LdapMessage.SEARCH_RESULT:
	// This is a generic LDAP Event
	// TODO: Why the type is ANY...? (java code)
	NotifyDirectoryListeners(new LdapEventArgs(sourceMessage, 
						   EventClassifiers.CLASSIFICATION_LDAP_PSEARCH, 
						   LdapEventType.LDAP_PSEARCH_ANY));
	bListenersNotified = true;
	break;

      default:
	// This seems to be some unknown event.
	// Let this be notified to generic DirectoryListeners in the base class...
	break;
      }

      return bListenersNotified;
    }
  } // end of class PSearchEventSource
}
