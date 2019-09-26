#include <config.h>
#include <glib.h>
#include "mono/utils/mono-threads-api.h"
#include "mono/utils/atomic.h"
#include "mono/metadata/icall-internals.h"
#include "pal-socket-icalls.h"


void
mono_pal_init_sockets (void)
{
	mono_add_internal_call_with_flags ("Interop/Sys::Accept_internal", ves_icall_Interop_Sys_Accept_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Bind_internal", ves_icall_Interop_Sys_Bind_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Close", ves_icall_Interop_Sys_Close, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Connect_internal", ves_icall_Interop_Sys_Connect_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetSockOpt_internal", ves_icall_Interop_Sys_GetSockOpt_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Listen_internal", ves_icall_Interop_Sys_Listen_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Read_internal", ves_icall_Interop_Sys_Read_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::ReceiveMessage", ves_icall_Interop_Sys_ReceiveMessage, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetAtOutOfBandMark_internal", ves_icall_Interop_Sys_GetAtOutOfBandMark_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetBytesAvailable_internal", ves_icall_Interop_Sys_GetBytesAvailable_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetDomainSocketSizes", ves_icall_Interop_Sys_GetDomainSocketSizes, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetIPv4MulticastOption_internal", ves_icall_Interop_Sys_GetIPv4MulticastOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetIPv6MulticastOption_internal", ves_icall_Interop_Sys_GetIPv6MulticastOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetLingerOption_internal", ves_icall_Interop_Sys_GetLingerOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetPeerName_internal", ves_icall_Interop_Sys_GetPeerName_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetSockName_internal", ves_icall_Interop_Sys_GetSockName_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::GetSocketErrorOption_internal", ves_icall_Interop_Sys_GetSocketErrorOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::IsRuntimeShuttingDown_internal", ves_icall_Interop_Sys_IsRuntimeShuttingDown_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Pipe", ves_icall_Interop_Sys_Pipe, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Poll", ves_icall_Interop_Sys_Poll, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SendFile_internal", ves_icall_Interop_Sys_SendFile_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SendMessage", ves_icall_Interop_Sys_SendMessage, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetIPv4MulticastOption_internal", ves_icall_Interop_Sys_SetIPv4MulticastOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetIPv6MulticastOption_internal", ves_icall_Interop_Sys_SetIPv6MulticastOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetIsNonBlocking_internal", ves_icall_Interop_Sys_SetIsNonBlocking_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetLingerOption_internal", ves_icall_Interop_Sys_SetLingerOption_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetReceiveTimeout_internal", ves_icall_Interop_Sys_SetReceiveTimeout_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetSendTimeout_internal", ves_icall_Interop_Sys_SetSendTimeout_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::SetSockOpt_internal", ves_icall_Interop_Sys_SetSockOpt_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Shutdown_internal", ves_icall_Interop_Sys_Shutdown_internal, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::WaitForSocketEvents", ves_icall_Interop_Sys_WaitForSocketEvents, TRUE);
	mono_add_internal_call_with_flags ("Interop/Sys::Write_internal", ves_icall_Interop_Sys_Write_internal, TRUE);
}

int32_t
SystemNative_IsRuntimeShuttingDown (void)
{
	return mono_runtime_is_shutting_down ();
}

int32_t ves_icall_Interop_Sys_IsRuntimeShuttingDown_internal (void)
{
	return SystemNative_IsRuntimeShuttingDown ();
}

int32_t
ves_icall_Interop_Sys_Accept_internal (intptr_t socket, uint8_t* socketAddress, int32_t* socketAddressLen, intptr_t* acceptedSocket)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Accept (socket, socketAddress, socketAddressLen, acceptedSocket);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Bind_internal (intptr_t socket, int32_t protocolType, uint8_t* socketAddress, int32_t socketAddressLen)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Bind (socket, protocolType, socketAddress, socketAddressLen);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Close (intptr_t fd)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Close (fd);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Connect_internal (intptr_t socket, uint8_t* socketAddress, int32_t socketAddressLen)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Connect (socket, socketAddress, socketAddressLen);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetAtOutOfBandMark_internal (intptr_t socket, int32_t* atMark)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetAtOutOfBandMark (socket, atMark);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetBytesAvailable_internal (intptr_t socket, int32_t* available)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetBytesAvailable (socket, available);
	MONO_EXIT_GC_SAFE;
	return result;
}

void
ves_icall_Interop_Sys_GetDomainSocketSizes (int32_t* pathOffset, int32_t* pathSize, int32_t* addressSize)
{
	return SystemNative_GetDomainSocketSizes (pathOffset, pathSize, addressSize);
}

int32_t
ves_icall_Interop_Sys_GetIPv4MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv4MulticastOption* option)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetIPv4MulticastOption (socket, multicastOption, option);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetIPv6MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv6MulticastOption* option)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetIPv6MulticastOption (socket, multicastOption, option);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetLingerOption_internal (intptr_t socket, struct LingerOption* option)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetLingerOption (socket, option);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetPeerName_internal (intptr_t socket, uint8_t* socketAddress, int32_t* socketAddressLen)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetPeerName (socket, socketAddress, socketAddressLen);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetSockName_internal (intptr_t socket, uint8_t* socketAddress, int32_t* socketAddressLen)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetSockName (socket, socketAddress, socketAddressLen);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SetIPv4MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv4MulticastOption* option)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SetIPv4MulticastOption (socket, multicastOption, option);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t ves_icall_Interop_Sys_SetIPv6MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv6MulticastOption* option)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SetIPv6MulticastOption (socket, multicastOption, option);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SetLingerOption_internal (intptr_t socket, struct LingerOption* option)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SetLingerOption (socket, option);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SetReceiveTimeout_internal (intptr_t socket, int32_t millisecondsTimeout)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SetReceiveTimeout (socket, millisecondsTimeout);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;

}

int32_t
ves_icall_Interop_Sys_SetSendTimeout_internal (intptr_t socket, int32_t millisecondsTimeout)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SetSendTimeout (socket, millisecondsTimeout);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;

}

int32_t
ves_icall_Interop_Sys_GetSockOpt_internal (intptr_t socket, int32_t socketOptionLevel, int32_t socketOptionName, uint8_t* optionValue, int32_t* optionLen)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetSockOpt (socket, socketOptionLevel, socketOptionName, optionValue, optionLen);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_GetSocketErrorOption_internal (intptr_t socket, int32_t* error)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_GetSocketErrorOption (socket, error);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Listen_internal (intptr_t socket, int32_t backlog)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Listen (socket, backlog);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

gint32
ves_icall_Interop_Sys_Read_internal (intptr_t fd, void* buffer, int32_t count)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Read (fd, buffer, count);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_ReceiveMessage (intptr_t socket, void* messageHeader, int32_t flags, int64_t* received)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_ReceiveMessage (socket, messageHeader, flags, received);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Pipe (int32_t pipeFds[2], int32_t flags)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Pipe (pipeFds, flags);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Poll (void* pollEvents, uint32_t eventCount, int32_t milliseconds, uint32_t* triggered)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Poll (pollEvents, eventCount, milliseconds, triggered);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SendFile_internal (intptr_t out_fd, intptr_t in_fd, int64_t offset, int64_t count, int64_t* sent)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SendFile (out_fd, in_fd, offset, count, sent);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SendMessage (intptr_t socket, void* messageHeader, int32_t flags, int64_t* sent)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SendMessage (socket, messageHeader, flags, sent);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SetIsNonBlocking_internal (intptr_t fd, int32_t isNonBlocking)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_FcntlSetIsNonBlocking (fd, isNonBlocking);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_SetSockOpt_internal (intptr_t socket, int32_t socketOptionLevel, int32_t socketOptionName, uint8_t* optionValue, int32_t optionLen)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_SetSockOpt (socket, socketOptionLevel, socketOptionName, optionValue, optionLen);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Shutdown_internal (intptr_t socket, int32_t socketShutdown)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Shutdown (socket, socketShutdown);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_WaitForSocketEvents (intptr_t port, void *buffer, int32_t* count)
{
	int32_t result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_WaitForSocketEvents (port, buffer, count);
	MONO_EXIT_GC_SAFE;
	return result;
}

int32_t
ves_icall_Interop_Sys_Write_internal (intptr_t fd, const void* buffer, int32_t bufferSize)
{
	gint32 result;
	MONO_ENTER_GC_SAFE;
	result = SystemNative_Write (fd, buffer, bufferSize);
	mono_marshal_set_last_error ();
	MONO_EXIT_GC_SAFE;
	return result;
}
