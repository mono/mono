using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if FEATURE_CORRUPTING_EXCEPTIONS
using System.Runtime.ExceptionServices;
#endif // FEATURE_CORRUPTING_EXCEPTIONS
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Diagnostics.Contracts;

namespace System.Security.AccessControl
{
    internal static class Win32
    {
        internal const System.Int32 TRUE = 1;

        //
        // Wrapper around advapi32.ConvertSecurityDescriptorToStringSecurityDescriptorW
        //

        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermission( SecurityAction.Assert, UnmanagedCode=true )]
        internal static int ConvertSdToSddl(
            byte[] binaryForm,
            int requestedRevision,
            SecurityInfos si,
            out string resultSddl )
        {
            int errorCode;
            IntPtr ByteArray;
            uint ByteArraySize = 0;

            if ( TRUE != Win32Native.ConvertSdToStringSd( binaryForm, ( uint )requestedRevision, ( uint )si, out ByteArray, ref ByteArraySize ))
            {
                errorCode = Marshal.GetLastWin32Error();
                goto Error;
            }

            //
            // Extract data from the returned pointer
            //

            resultSddl = Marshal.PtrToStringUni( ByteArray );

            //
            // Now is a good time to get rid of the returned pointer
            //

            Win32Native.LocalFree( ByteArray );

            return 0;

        Error:

            resultSddl = null;

            if ( errorCode == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
            {
                throw new OutOfMemoryException();
            }

            return errorCode;
        }

        //
        // Wrapper around advapi32.GetSecurityInfo
        //

        [System.Security.SecurityCritical]  // auto-generated
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS

        internal static int GetSecurityInfo(
            ResourceType resourceType,
            string name,
            SafeHandle handle,
            AccessControlSections accessControlSections,
            out RawSecurityDescriptor resultSd
            )
        {
            resultSd = null;

            //
            // Demand unmanaged code permission
            // The integrator layer is free to assert this permission
            // and, in turn, demand another permission of its caller
            //

            new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();

            int errorCode;
            IntPtr SidOwner, SidGroup, Dacl, Sacl, ByteArray;
            SecurityInfos SecurityInfos = 0;
            Privilege privilege = null;

            if (( accessControlSections & AccessControlSections.Owner ) != 0 )
            {
                SecurityInfos |= SecurityInfos.Owner;
            }
            
            if (( accessControlSections & AccessControlSections.Group ) != 0 )
            {
                SecurityInfos |= SecurityInfos.Group;
            }

            if (( accessControlSections & AccessControlSections.Access ) != 0 )
            {
                SecurityInfos |= SecurityInfos.DiscretionaryAcl;
            }
            
            if (( accessControlSections & AccessControlSections.Audit ) != 0 )
            {
                SecurityInfos |= SecurityInfos.SystemAcl;
                privilege = new Privilege( Privilege.Security );
            }

            // Ensure that the finally block will execute
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                if ( privilege != null )
                {
                    try
                    {
                        privilege.Enable();
                    }
                    catch (PrivilegeNotHeldException)
                    {
                        // we will ignore this exception and press on just in case this is a remote resource
                    }
                }
                
                if ( name != null )
                {
                    errorCode = ( int )Win32Native.GetSecurityInfoByName( name, ( uint )resourceType, ( uint )SecurityInfos, out SidOwner, out SidGroup, out Dacl, out Sacl, out ByteArray );
                }
                else if (handle != null)
                {
                    if (handle.IsInvalid)
                    {
                        throw new ArgumentException(
                            Environment.GetResourceString( "Argument_InvalidSafeHandle" ),
                            "handle" );
                    }
                    else
                    {
                        errorCode = ( int )Win32Native.GetSecurityInfoByHandle( handle, ( uint )resourceType, ( uint )SecurityInfos, out SidOwner, out SidGroup, out Dacl, out Sacl, out ByteArray );
                    }
                }
                else
                {
                    // both are null, shouldn't happen
                    throw new SystemException();
                }

                if ( errorCode == Win32Native.ERROR_SUCCESS && IntPtr.Zero.Equals(ByteArray) )
                {
                    //
                    // This means that the object doesn't have a security descriptor. And thus we throw
                    // a specific exception for the caller to catch and handle properly.
                    //
                    throw new InvalidOperationException(Environment.GetResourceString( "InvalidOperation_NoSecurityDescriptor" ));
                }
                else if (errorCode == Win32Native.ERROR_NOT_ALL_ASSIGNED ||
                         errorCode == Win32Native.ERROR_PRIVILEGE_NOT_HELD)
                {
                    throw new PrivilegeNotHeldException( Privilege.Security );
                }
                else if ( errorCode == Win32Native.ERROR_ACCESS_DENIED ||
                    errorCode == Win32Native.ERROR_CANT_OPEN_ANONYMOUS )
                {
                    throw new UnauthorizedAccessException();
                }

                if ( errorCode != Win32Native.ERROR_SUCCESS )
                {
                    goto Error;
                }
            }
            catch
            {
                // protection against exception filter-based luring attacks
                if ( privilege != null )
                {
                    privilege.Revert();
                }
                throw;
            }
            finally
            {
                if ( privilege != null )
                {
                    privilege.Revert();
                }
            }

            //
            // Extract data from the returned pointer
            //

            uint Length = Win32Native.GetSecurityDescriptorLength( ByteArray );

            byte[] BinaryForm = new byte[Length];

            Marshal.Copy( ByteArray, BinaryForm, 0, ( int )Length );

            Win32Native.LocalFree( ByteArray );

            resultSd = new RawSecurityDescriptor( BinaryForm, 0 );

            return Win32Native.ERROR_SUCCESS;

        Error:

            if ( errorCode == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
            {
                throw new OutOfMemoryException();
            }

            return errorCode;
        }

        //
        // Wrapper around advapi32.SetNamedSecurityInfoW and advapi32.SetSecurityInfo
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        internal static int SetSecurityInfo(
            ResourceType type,
            string name,
            SafeHandle handle,
            SecurityInfos securityInformation,
            SecurityIdentifier owner,
            SecurityIdentifier group,
            GenericAcl sacl,
            GenericAcl dacl )
        {
            int errorCode;
            int Length;
            byte[] OwnerBinary = null, GroupBinary = null, SaclBinary = null, DaclBinary = null;
            Privilege securityPrivilege = null;

            //
            // Demand unmanaged code permission
            // The integrator layer is free to assert this permission
            // and, in turn, demand another permission of its caller
            //

            new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();

            if ( owner != null )
            {
                Length = owner.BinaryLength;
                OwnerBinary = new byte[Length];
                owner.GetBinaryForm( OwnerBinary, 0 );
            }

            if ( group != null )
            {
                Length = group.BinaryLength;
                GroupBinary = new byte[Length];
                group.GetBinaryForm( GroupBinary, 0 );
            }

            if ( dacl != null )
            {
                Length = dacl.BinaryLength;
                DaclBinary = new byte[Length];
                dacl.GetBinaryForm( DaclBinary, 0 );
            }

            if ( sacl != null )
            {
                Length = sacl.BinaryLength;
                SaclBinary = new byte[Length];
                sacl.GetBinaryForm( SaclBinary, 0 );
            }

            if ( ( securityInformation & SecurityInfos.SystemAcl ) != 0 )
            {
                //
                // Enable security privilege if trying to set a SACL. 
                // Note: even setting it by handle needs this privilege enabled!
                //
                
                securityPrivilege = new Privilege( Privilege.Security );
            }

            // Ensure that the finally block will execute
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                if ( securityPrivilege != null )
                {
                    try
                    {
                        securityPrivilege.Enable();
                    }
                    catch (PrivilegeNotHeldException)
                    {
                        // we will ignore this exception and press on just in case this is a remote resource
                    }
                }

                if ( name != null )
                {
                    errorCode = ( int )Win32Native.SetSecurityInfoByName( name, ( uint )type, ( uint )securityInformation, OwnerBinary, GroupBinary, DaclBinary, SaclBinary );
                }
                else if (handle != null)
                {
                    if (handle.IsInvalid)
                    {
                        throw new ArgumentException(
                            Environment.GetResourceString( "Argument_InvalidSafeHandle" ),
                            "handle" );
                    }
                    else
                    {
                        errorCode = ( int )Win32Native.SetSecurityInfoByHandle( handle, ( uint )type, ( uint )securityInformation, OwnerBinary, GroupBinary, DaclBinary, SaclBinary );
                    }
                }
                else
                {
                    // both are null, shouldn't happen
                    Contract.Assert( false, "Internal error: both name and handle are null" );
                    throw new InvalidProgramException();
                }

                if (errorCode == Win32Native.ERROR_NOT_ALL_ASSIGNED ||
                    errorCode == Win32Native.ERROR_PRIVILEGE_NOT_HELD)
                {
                    throw new PrivilegeNotHeldException( Privilege.Security );
                }
                else if ( errorCode == Win32Native.ERROR_ACCESS_DENIED ||
                    errorCode == Win32Native.ERROR_CANT_OPEN_ANONYMOUS )
                {
                    throw new UnauthorizedAccessException();
                }
                else if ( errorCode != Win32Native.ERROR_SUCCESS )
                {
                    goto Error;
                }
            }
            catch
            {
                // protection against exception filter-based luring attacks
                if ( securityPrivilege != null )
                {
                    securityPrivilege.Revert();
                }
                throw;
            }
            finally
            {
                if ( securityPrivilege != null )
                {
                    securityPrivilege.Revert();
                }
            }

            return 0;

        Error:

            if ( errorCode == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
            {
                throw new OutOfMemoryException();
            }

            return errorCode;
        }
    }
}
