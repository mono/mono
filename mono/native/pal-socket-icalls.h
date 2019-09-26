#ifndef __MONO_METADATA_PAL_SOCKET_ICALLS_H__
#define __MONO_METADATA_PAL_SOCKET_ICALLS_H__

#include "mono/metadata/metadata.h"
#include "mono/metadata/class-internals.h"
#include "pal_runtimeshutdown.h"
#include "pal_networking.h"
#include "pal_io.h"

#include "pal-icalls.h"

struct IPv4MulticastOption;
struct IPv6MulticastOption;
struct LingerOption;
struct MessageHeader;
struct PollEvent;
struct SocketEvent;

extern void mono_marshal_set_last_error (void);

int32_t ves_icall_Interop_Sys_Accept_internal (intptr_t socket, uint8_t* socketAddress, int32_t* socketAddressLen, intptr_t* acceptedSocket);
int32_t ves_icall_Interop_Sys_Bind_internal (intptr_t socket, int32_t protocolType, uint8_t* socketAddress, int32_t socketAddressLen);
int32_t ves_icall_Interop_Sys_Close (intptr_t fd);
int32_t ves_icall_Interop_Sys_Connect_internal (intptr_t socket, uint8_t* socketAddress, int32_t socketAddressLen);
int32_t ves_icall_Interop_Sys_GetAtOutOfBandMark_internal (intptr_t socket, int32_t* atMark);
int32_t ves_icall_Interop_Sys_GetBytesAvailable_internal (intptr_t socket, int32_t* available);
void ves_icall_Interop_Sys_GetDomainSocketSizes (int32_t* pathOffset, int32_t* pathSize, int32_t* addressSize);
int32_t ves_icall_Interop_Sys_GetIPv4MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv4MulticastOption* option);
int32_t ves_icall_Interop_Sys_GetIPv6MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv6MulticastOption* option);
int32_t ves_icall_Interop_Sys_GetLingerOption_internal (intptr_t socket, struct LingerOption* option);
int32_t ves_icall_Interop_Sys_GetPeerName_internal (intptr_t socket, uint8_t* socketAddress, int32_t* socketAddressLen);
int32_t ves_icall_Interop_Sys_GetSockName_internal (intptr_t socket, uint8_t* socketAddress, int32_t* socketAddressLen);
int32_t ves_icall_Interop_Sys_GetSockOpt_internal (intptr_t socket, int32_t socketOptionLevel, int32_t socketOptionName, uint8_t* optionValue, int32_t* optionLen);
int32_t ves_icall_Interop_Sys_GetSocketErrorOption_internal (intptr_t socket, int32_t* error);
int32_t ves_icall_Interop_Sys_IsRuntimeShuttingDown_internal (void);
int32_t ves_icall_Interop_Sys_Listen_internal (intptr_t socket, int32_t backlog);
int32_t ves_icall_Interop_Sys_Pipe (int32_t pipeFds[2], int32_t flags);
int32_t ves_icall_Interop_Sys_Poll (void* pollEvents, uint32_t eventCount, int32_t milliseconds, uint32_t* triggered);
int32_t ves_icall_Interop_Sys_Read_internal (intptr_t fd, void* buffer, int32_t count);
int32_t ves_icall_Interop_Sys_ReceiveMessage (intptr_t socket, void* messageHeader, int32_t flags, int64_t* received);
int32_t ves_icall_Interop_Sys_SendFile_internal (intptr_t out_fd, intptr_t in_fd, int64_t offset, int64_t count, int64_t* sent);
int32_t ves_icall_Interop_Sys_SendMessage (intptr_t socket, void* messageHeader, int32_t flags, int64_t* sent);
int32_t ves_icall_Interop_Sys_SetLingerOption_internal (intptr_t socket, struct LingerOption* option);
int32_t ves_icall_Interop_Sys_SetIPv4MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv4MulticastOption* option);
int32_t ves_icall_Interop_Sys_SetIPv6MulticastOption_internal (intptr_t socket, int32_t multicastOption, struct IPv6MulticastOption* option);
int32_t ves_icall_Interop_Sys_SetIsNonBlocking_internal (intptr_t fd, int32_t isNonBlocking);
int32_t ves_icall_Interop_Sys_SetReceiveTimeout_internal (intptr_t socket, int32_t millisecondsTimeout);
int32_t ves_icall_Interop_Sys_SetSendTimeout_internal (intptr_t socket, int32_t millisecondsTimeout);
int32_t ves_icall_Interop_Sys_SetSockOpt_internal (intptr_t socket, int32_t socketOptionLevel, int32_t socketOptionName, uint8_t* optionValue, int32_t optionLen);
int32_t ves_icall_Interop_Sys_Shutdown_internal (intptr_t socket, int32_t socketShutdown);
int32_t ves_icall_Interop_Sys_WaitForSocketEvents (intptr_t port, void* buffer, int32_t* count);
int32_t ves_icall_Interop_Sys_Write_internal (intptr_t fd, const void* buffer, int32_t bufferSize);

#endif
