
using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Extensions;
using Novell.Directory.Ldap.Rfc2251;


namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> 
	/// Takes an LdapExtendedResponse and returns an object
	/// (that implements the base class ParsedExtendedResponse)
	/// based on the OID.
	/// 
	/// <p>You can then call methods defined in the child
	/// class to parse the contents of the response.  The methods available
	/// depend on the child class. All child classes inherit from the
	/// ParsedExtendedResponse.</p>
	/// 
	/// </summary>
	public class ExtResponseFactory
	{
		
		/// <summary> Used to Convert an RfcLdapMessage object to the appropriate
		/// LdapExtendedResponse object depending on the operation being performed.
		/// 
		/// </summary>
		/// <param name="inResponse">  The LdapExtendedReponse object as returned by the
		/// extendedOperation method in the LdapConnection object.
		/// </param>
		/// <returns> An object of base class LdapExtendedResponse.  The actual child
		/// class of this returned object depends on the operation being
		/// performed.
		/// </returns>
		
		static public LdapExtendedResponse convertToExtendedResponse(RfcLdapMessage inResponse)
		{
			
			LdapExtendedResponse tempResponse = new LdapExtendedResponse(inResponse);
			// Get the oid stored in the Extended response
			System.String inOID = tempResponse.ID;
			
			RespExtensionSet regExtResponses = LdapExtendedResponse.RegisteredResponses;
			try
			{
				System.Type extRespClass = regExtResponses.findResponseExtension(inOID);
				if (extRespClass == null)
				{
					return tempResponse;
				}
				System.Type[] argsClass = new System.Type[]{typeof(RfcLdapMessage)};
				System.Object[] args = new System.Object[]{inResponse};
				System.Exception ex;
				try
				{
					System.Reflection.ConstructorInfo extConstructor = extRespClass.GetConstructor(argsClass);
					try
					{
						System.Object resp = null;
						resp = extConstructor.Invoke(args);
						return (LdapExtendedResponse) resp;
					}
					catch (System.UnauthorizedAccessException e)
					{
						ex = e;
					}
					catch (System.Reflection.TargetInvocationException e)
					{
						ex = e;
					}
					catch (System.Exception e)
					{
						// Could not create the ResponseControl object
						// All possible exceptions are ignored. We fall through
						// and create a default LdapControl object
						ex = e;
					}
				}
				catch (System.MethodAccessException e)
				{
					// bad class was specified, fall through and return a
					// default  LdapExtendedResponse object
					ex = e;
				}
			}
			catch (System.FieldAccessException e)
			{
			}
			// If we get here we did not have a registered extendedresponse
			// for this oid.  Return a default LdapExtendedResponse object.
			return tempResponse;
		}
	}
}