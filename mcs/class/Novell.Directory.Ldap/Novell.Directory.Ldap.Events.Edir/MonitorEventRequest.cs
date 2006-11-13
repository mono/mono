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
// Novell.Directory.Ldap.Events.Edir.MonitorEventRequest.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;

using Novell.Directory.Ldap.Utilclass;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir
{
  /// <summary>
  /// This class denotes the mechanism to specify the event of interest.
  /// </summary>
  public class MonitorEventRequest : LdapExtendedOperation
  {
    static MonitorEventRequest()
    {
      /*
       * Register the extendedresponse class which is returned by the
       * server in response to a MonitorEventRequest
       */
      try
      {
	LdapExtendedResponse.register(EventOids.NLDAP_MONITOR_EVENTS_RESPONSE,
				      Type.GetType("Novell.Directory.Ldap.Events.Edir.MonitorEventResponse", true));
      }
      catch(TypeLoadException e)
      {
	// TODO: put something in the Debug...
      }
      catch(Exception e)
      {
	// TODO: put something in the Debug...
      }

      //Also try to register EdirEventIntermediateResponse
      try
      {
	LdapIntermediateResponse.register(EventOids.NLDAP_EVENT_NOTIFICATION,
					  Type.GetType("Novell.Directory.Ldap.Events.Edir.EdirEventIntermediateResponse", true));
      }
      catch(TypeLoadException e)
      {
	// TODO: put something in the Debug...
      }
      catch(Exception e)
      {
	// TODO: put something in the Debug...
      }
    } // end of static constructor

    public MonitorEventRequest(EdirEventSpecifier[] specifiers) :
      base(EventOids.NLDAP_MONITOR_EVENTS_REQUEST, null)
    {
      if ((specifiers == null)) 
      {
	throw new ArgumentException(ExceptionMessages.PARAM_ERROR);
      }

      MemoryStream encodedData = new MemoryStream();
      LBEREncoder encoder = new LBEREncoder();

      Asn1Sequence asnSequence = new Asn1Sequence();
      try
      {
	asnSequence.add(new Asn1Integer(specifiers.Length));

	Asn1Set asnSet = new Asn1Set();
	bool bFiltered = false;
	for (int nIndex = 0; nIndex < specifiers.Length; nIndex++)
	{
	  Asn1Sequence specifierSequence = new Asn1Sequence();
	  specifierSequence.add(new Asn1Integer((int)(specifiers[nIndex].EventType)));
	  specifierSequence.add(new Asn1Enumerated((int)(specifiers[nIndex].EventResultType)));
	  if (0 == nIndex)
	  {
	    bFiltered = (null != specifiers[nIndex].EventFilter);
	    if (bFiltered)
	      setID(EventOids.NLDAP_FILTERED_MONITOR_EVENTS_REQUEST);
	  }
	  
	  if (bFiltered)
	  {
	    // A filter is expected
	    if (null == specifiers[nIndex].EventFilter)
	      throw new ArgumentException("Filter cannot be null,for Filter events");

	    specifierSequence.add(new Asn1OctetString(specifiers[nIndex].EventFilter));
	  }
	  else
	  {
	    // No filter is expected
	    if (null != specifiers[nIndex].EventFilter)
	      throw new ArgumentException("Filter cannot be specified for non Filter events");	 
	  }

	  asnSet.add(specifierSequence);
	}

	asnSequence.add(asnSet);
	asnSequence.encode(encoder, encodedData);
      }
      catch(Exception e)
      {
	throw new LdapException(ExceptionMessages.ENCODING_ERROR,
				LdapException.ENCODING_ERROR, 
				null);
      }

      setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
    } // end of the constructor MonitorEventRequest
  }
}
