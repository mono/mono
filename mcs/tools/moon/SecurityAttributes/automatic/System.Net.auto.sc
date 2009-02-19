# [SecurityCritical] needed to execute code inside 'System.Net, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e'.
# 34 methods needs to be decorated.

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Net.Sockets.Socket::Poll_internal(System.IntPtr,System.Net.Sockets.SelectMode,System.Int32,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Net.WebClient/<OnDownloadStringCompleted>c__AnonStorey2::<>m__4(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Net.WebClient/<OnOpenReadCompleted>c__AnonStorey1::<>m__3(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Net.WebClient/GSourceFunc::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Net.WebClient/GSourceFunc::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.LinuxNetworkInterface::getifaddrs(System.IntPtr&)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.LinuxNetworkInterface::if_nametoindex(System.String)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.Win32_FIXED_INFO::GetNetworkParams(System.Byte[],System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.Win32IPv4InterfaceProperties::GetPerAdapterInfo(System.Int32,System.Net.NetworkInformation.Win32_IP_PER_ADAPTER_INFO,System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.Win32NetworkInterface2::GetAdaptersAddresses(System.UInt32,System.UInt32,System.IntPtr,System.Byte[],System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.Win32NetworkInterface2::GetAdaptersInfo(System.Byte[],System.Int32&)

# p/invoke declaration
+SC-M: System.Int32 System.Net.NetworkInformation.Win32NetworkInterface2::GetIfEntry(System.Net.NetworkInformation.Win32_MIB_IFROW&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Net.Sockets.Socket::Receive_internal(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Net.Sockets.Socket::RecvFrom_internal(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.SocketAddress&,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Net.Sockets.Socket::Send_internal(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 System.Net.Sockets.Socket::SendTo_internal(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.SocketAddress,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr System.Net.Sockets.Socket::Accept_internal(System.IntPtr,System.Int32&,System.Boolean)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Net.Sockets.Socket::Socket_internal(System.Net.Sockets.AddressFamily,System.Net.Sockets.SocketType,System.Net.Sockets.ProtocolType,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Net.NetworkInformation.IPAddressInformationCollection System.Net.NetworkInformation.IPAddressInformationImplCollection::Win32FromAnycast(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Net.NetworkInformation.MulticastIPAddressInformationCollection System.Net.NetworkInformation.MulticastIPAddressInformationImplCollection::Win32FromMulticast(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Net.NetworkInformation.UnicastIPAddressInformationCollection System.Net.NetworkInformation.UnicastIPAddressInformationImplCollection::Win32FromUnicast(System.Int32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Net.NetworkInformation.Win32IPAddressCollection System.Net.NetworkInformation.Win32IPAddressCollection::FromDnsServer(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Net.SocketAddress System.Net.Sockets.Socket::RemoteEndPoint_internal(System.IntPtr,System.Int32&)

# p/invoke declaration
+SC-M: System.UInt32 System.Net.WebClient::g_timeout_add(System.UInt32,System.Net.WebClient/GSourceFunc,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void System.Net.NetworkInformation.LinuxNetworkInterface::freeifaddrs(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.NetworkInformation.Win32GatewayIPAddressInformationCollection::AddSubsequently(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.NetworkInformation.Win32IPAddressCollection::AddSubsequentlyString(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::Blocking_internal(System.IntPtr,System.Boolean,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::Close_internal(System.IntPtr,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::Connect_internal(System.IntPtr,System.Net.SocketAddress,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::Disconnect_internal(System.IntPtr,System.Boolean,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::GetSocketOption_obj_internal(System.IntPtr,System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Object&,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::SetSocketOption_internal(System.IntPtr,System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Object,System.Byte[],System.Int32,System.Int32&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Net.Sockets.Socket::Shutdown_internal(System.IntPtr,System.Net.Sockets.SocketShutdown,System.Int32&)

