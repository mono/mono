//------------------------------------------------------------------------------
// <copyright file="_AutoWebProxyScriptHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#pragma warning disable 618

#define AUTOPROXY_MANAGED_JSCRIPT

namespace System.Net
{
#if AUTOPROXY_MANAGED_JSCRIPT
    using System.Security.Permissions;
	using System.Collections.Generic;
#endif
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;

    /// <summary>
    /// Provides a set of functions that can be called by the JS script.  There are based on
    /// an earlier API set that is used for these networking scripts.
    /// for a description of the API see:
    /// http://home.netscape.com/eng/mozilla/2.0/relnotes/demo/proxy-live.html 
    /// </summary>
#if !AUTOPROXY_MANAGED_JSCRIPT
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public sealed class WebProxyScriptHelper
    {
        internal WebProxyScriptHelper() { }
#else
    internal class WebProxyScriptHelper : IReflect
    {
        private class MyMethodInfo : MethodInfo {
            string name;
            // used by JScript
            public MyMethodInfo(string name) : base() {
                GlobalLog.Print("MyMethodInfo::.ctor() name:" + name);
                this.name = name;
            }
            // used by JScript
            public override Type ReturnType {
                get { 
                    GlobalLog.Print("MyMethodInfo::ReturnType()");
                    Type type = null;
                    if (string.Compare(name, "isPlainHostName", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }
                    else if (string.Compare(name, "dnsDomainIs", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }
                    else if (string.Compare(name, "localHostOrDomainIs", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }
                    else if (string.Compare(name, "isResolvable", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }
                    else if (string.Compare(name, "dnsResolve", StringComparison.Ordinal)==0) {
                        type = typeof(string);
                    }
                    else if (string.Compare(name, "myIpAddress", StringComparison.Ordinal)==0) {
                        type = typeof(string);
                    }
                    else if (string.Compare(name, "dnsDomainLevels", StringComparison.Ordinal)==0) {
                        type = typeof(int);
                    }
                    else if (string.Compare(name, "isInNet", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }
                    else if (string.Compare(name, "shExpMatch", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }
					else if (string.Compare(name, "weekdayRange", StringComparison.Ordinal)==0) {
                        type = typeof(bool);
                    }

					//-------------------------------------
					//Don't even inject these methods 
					//if the OS does not support IPv6
					//-------------------------------------
					else if(Socket.OSSupportsIPv6)
					{
	                    //---------------------------------------------------------------------
	                    //The following changes are made to support IPv6
	                    //IE7 ships with this support and WinInet ships with this support
	                    //we are adding support for Ipv6
	                    //---------------------------------------------------------------------
	                    if (string.Compare(name, "dnsResolveEx", StringComparison.Ordinal)==0) {
	                        type = typeof(string);
	                    }
	                    else if (string.Compare(name, "isResolvableEx", StringComparison.Ordinal)==0) {
	                        type = typeof(bool);
	                    }
	                    else if (string.Compare(name, "myIpAddressEx", StringComparison.Ordinal)==0) {
	                        type = typeof(string);
	                    }
	                    else if (string.Compare(name, "isInNetEx", StringComparison.Ordinal)==0) {
	                        type = typeof(bool);
	                    }                    
	                    else if (string.Compare(name, "sortIpAddressList", StringComparison.Ordinal)==0) {
	                        type = typeof(string);
	                    }                    
	                    else if (string.Compare(name, "getClientVersion", StringComparison.Ordinal)==0) {
	                        type = typeof(string);
	                    }                    
					}                                        
                    GlobalLog.Print("MyMethodInfo::ReturnType() name:" + name + " type:" + type.FullName);
                    return type;
                }
            }
            // used by JScript
            public override ICustomAttributeProvider ReturnTypeCustomAttributes {
                get { 
                    GlobalLog.Print("MyMethodInfo::ReturnTypeCustomAttributes()");
                    return null;
                }
            }
            public override RuntimeMethodHandle MethodHandle {
                get { 
                    GlobalLog.Print("MyMethodInfo::MethodHandle()");
                    return new RuntimeMethodHandle();
                }
            }
            public override MethodAttributes Attributes {
                get { 
                    GlobalLog.Print("MyMethodInfo::Attributes()");
                    return MethodAttributes.Public;
                }
            }
            public override string Name {
                get { 
                    GlobalLog.Print("MyMethodInfo::Name()");
                    return name;
                }
            }
            // used by JScript
            public override Type DeclaringType {
                get { 
                    GlobalLog.Print("MyMethodInfo::DeclaringType()");
                    return typeof(MyMethodInfo);
                }
            }
            public override Type ReflectedType {
                get { 
                    GlobalLog.Print("MyMethodInfo::ReflectedType()");
                    return null;
                }
            }
            public override object[] GetCustomAttributes(bool inherit) {
                GlobalLog.Print("MyMethodInfo::GetCustomAttributes() inherit:" + inherit);
                return null;
            }
            public override object[] GetCustomAttributes(Type type, bool inherit) {
                GlobalLog.Print("MyMethodInfo::GetCustomAttributes() inherit:" + inherit);
                return null;
            }
            public override bool IsDefined(Type type, bool inherit) {
                GlobalLog.Print("MyMethodInfo::IsDefined() type:" + type.FullName + " inherit:" + inherit);
                return type.Equals(typeof(WebProxyScriptHelper));
            }
            // used by JScript
            public override object Invoke(object target, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture) {
                GlobalLog.Print("MyMethodInfo::Invoke() target:" + target);
                return typeof(WebProxyScriptHelper).GetMethod(name, (BindingFlags)unchecked(-1)).Invoke(target, (BindingFlags)unchecked(-1), binder, args, culture);
            }
            public override ParameterInfo[] GetParameters() {
                GlobalLog.Print("MyMethodInfo::GetParameters() name:" + name);
                ParameterInfo[] pars = typeof(WebProxyScriptHelper).GetMethod(name, (BindingFlags)unchecked(-1)).GetParameters();
                GlobalLog.Print("MyMethodInfo::GetParameters() returning pars.Length:" + pars.Length);
                return pars;
            }
            public override MethodImplAttributes GetMethodImplementationFlags() {
                GlobalLog.Print("MyMethodInfo::GetMethodImplementationFlags()");
                return MethodImplAttributes.IL;
            }
            public override MethodInfo GetBaseDefinition() {
                GlobalLog.Print("MyMethodInfo::GetBaseDefinition()");
                return null;
            }


            public override Module Module
            {
                get
                {
                    return GetType().Module;
                }
            }
        }
        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetMethod(1) name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return null;
        }
        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetMethod(2) name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return null;
        }
        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetMethods() bindingAttr:" + bindingAttr);
            return new MethodInfo[0];
        }
        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetField() name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return null;
        }
        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetFields() bindingAttr:" + bindingAttr);
            return new FieldInfo[0];
        }
        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetProperty(1) name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return null;
        }
        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetProperty(2) name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return null;
        }
        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetProperties() bindingAttr:" + bindingAttr);
            return new PropertyInfo[0];
        }
        // used by JScript
        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetMember() name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return new MemberInfo[]{new MyMethodInfo(name)};
        }
        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() GetMembers() bindingAttr:" + bindingAttr);
            return new MemberInfo[0];
        }
        object IReflect.InvokeMember(string name, BindingFlags bindingAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
            GlobalLog.Print("WebProxyScriptHelper::IReflect() InvokeMember() name:" + ValidationHelper.ToString(name) + " bindingAttr:" + bindingAttr);
            return null;
        }
        Type IReflect.UnderlyingSystemType {
            get {
                GlobalLog.Print("WebProxyScriptHelper::IReflect() UnderlyingSystemType_get()");
                return null;
            }
        }   
#endif

        public bool isPlainHostName(string hostName) {
            GlobalLog.Print("WebProxyScriptHelper::isPlainHostName() hostName:" + ValidationHelper.ToString(hostName));
            if (hostName==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isPlainHostName()", "hostName"));
                throw new ArgumentNullException("hostName");
            }
            return hostName.IndexOf('.') == -1;
        }

        public bool dnsDomainIs(string host, string domain) {
            GlobalLog.Print("WebProxyScriptHelper::dnsDomainIs() host:" + ValidationHelper.ToString(host) + " domain:" + ValidationHelper.ToString(domain));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.dnsDomainIs()", "host"));
                throw new ArgumentNullException("host");
            }
            if (domain==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.dnsDomainIs()", "domain"));
                throw new ArgumentNullException("domain");
            }
            int index = host.LastIndexOf(domain);
            return index != -1 && (index+domain.Length) == host.Length;
        }

        /// <devdoc>
        /// <para>         
        /// This is a strange function, if its not a local hostname
        /// we do a straight compare against the passed in domain
        /// string.  If its not a direct match, then its false,
        /// even if the root of the domain/hostname are the same.
        /// </para>
        /// </devdoc>
        public bool localHostOrDomainIs(string host, string hostDom) {
            GlobalLog.Print("WebProxyScriptHelper::localHostOrDomainIs() host:" + ValidationHelper.ToString(host) + " hostDom:" + ValidationHelper.ToString(hostDom));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.localHostOrDomainIs()", "host"));
                throw new ArgumentNullException("host");
            }
            if (hostDom==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.localHostOrDomainIs()", "hostDom"));
                throw new ArgumentNullException("hostDom");
            }
            if (isPlainHostName(host)) {
                 int index = hostDom.IndexOf('.');
                 if (index > 0) {
                     hostDom = hostDom.Substring(0,index);
                 }
            }
            return string.Compare(host, hostDom, StringComparison.OrdinalIgnoreCase)==0;
        }

        public bool isResolvable(string host) {
            GlobalLog.Print("WebProxyScriptHelper::isResolvable() host:" + ValidationHelper.ToString(host));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isResolvable()", "host"));
                throw new ArgumentNullException("host");
            }
            IPHostEntry ipHostEntry = null;
            try
            {
                ipHostEntry = Dns.InternalGetHostByName(host);
            }
            catch { }
            if (ipHostEntry == null)
            {
                return false;
            }
            for (int i = 0; i < ipHostEntry.AddressList.Length; i++)
            {
                if (ipHostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return true;
                }
            }
            return false;
        }

        public string dnsResolve(string host) {
            GlobalLog.Print("WebProxyScriptHelper::dnsResolve() host:" + ValidationHelper.ToString(host));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.dnsResolve()", "host"));
                throw new ArgumentNullException("host");
            }
            IPHostEntry ipHostEntry = null;
            try
            {
                ipHostEntry = Dns.InternalGetHostByName(host);
            }
            catch { }
            if (ipHostEntry == null)
            {
                return string.Empty;
            }
            for (int i = 0; i < ipHostEntry.AddressList.Length; i++)
            {
                if (ipHostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipHostEntry.AddressList[i].ToString();
                }
            }
            return string.Empty;
        }

        public string myIpAddress() {
            GlobalLog.Print("WebProxyScriptHelper::myIpAddress()");
            IPAddress[] ipAddresses = NclUtilities.LocalAddresses;
            for (int i = 0; i < ipAddresses.Length; i++)
            {
                if (!IPAddress.IsLoopback(ipAddresses[i]) && ipAddresses[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddresses[i].ToString();
                }
            }
            return string.Empty;
        }

        public int dnsDomainLevels(string host) {
            GlobalLog.Print("WebProxyScriptHelper::dnsDomainLevels() host:" + ValidationHelper.ToString(host));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.dnsDomainLevels()", "host"));
                throw new ArgumentNullException("host");
            }
            int index = 0;
            int domainCount = 0;
            while((index = host.IndexOf('.', index)) != -1) {
                domainCount++;
                index++;
            }
            return domainCount;
        }

        public bool isInNet(string host, string pattern, string mask) {
            GlobalLog.Print("WebProxyScriptHelper::isInNet() host:" + ValidationHelper.ToString(host) + " pattern:" + ValidationHelper.ToString(pattern) + " mask:" + ValidationHelper.ToString(mask));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isInNet()", "host"));
                throw new ArgumentNullException("host");
            }
            if (pattern==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isInNet()", "pattern"));
                throw new ArgumentNullException("pattern");
            }
            if (mask==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isInNet()", "mask"));
                throw new ArgumentNullException("mask");
            }
            try {
                IPAddress hostAddress = IPAddress.Parse(host);
                IPAddress patternAddress = IPAddress.Parse(pattern);
                IPAddress maskAddress = IPAddress.Parse(mask);

                byte[] maskAddressBytes = maskAddress.GetAddressBytes();
                byte[] hostAddressBytes = hostAddress.GetAddressBytes();
                byte[] patternAddressBytes = patternAddress.GetAddressBytes();
                if (maskAddressBytes.Length!=hostAddressBytes.Length || maskAddressBytes.Length!=patternAddressBytes.Length) {
                    return false;
                }
                for (int i=0; i<maskAddressBytes.Length; i++) {
                    if ( (patternAddressBytes[i] & maskAddressBytes[i]) != (hostAddressBytes[i] & maskAddressBytes[i]) ) {
                        return false;
                    }
                }
            }
            catch {
                return false;
            }
            return true;
        }

        // See bug 87334 for details on the implementation.
        public bool shExpMatch(string host, string pattern) {
            GlobalLog.Print("WebProxyScriptHelper::shExpMatch() host:" + ValidationHelper.ToString(host) + " pattern:" + ValidationHelper.ToString(pattern));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.shExpMatch()", "host"));
                throw new ArgumentNullException("host");
            }
            if (pattern==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.shExpMatch()", "pattern"));
                throw new ArgumentNullException("pattern");
            }

            try
            {
                // This can throw - treat as no match.
                ShellExpression exp = new ShellExpression(pattern);
                return exp.IsMatch(host);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public bool weekdayRange(string wd1, [Optional] object wd2, [Optional] object gmt)
        {
            GlobalLog.Print("WebProxyScriptHelper::weekdayRange() wd1:" + ValidationHelper.ToString(wd1) + " wd2:" + ValidationHelper.ToString(wd2) + " gmt:" + ValidationHelper.ToString(gmt));
            if (wd1 == null)
            {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.weekdayRange()", "wd1"));
                throw new ArgumentNullException("wd1");
            }
            string _gmt = null;
            string _wd2 = null;
            if (gmt != null && gmt != DBNull.Value && gmt != Missing.Value)
            {
                _gmt = gmt as string;
                if (_gmt == null)
                {
                    throw new ArgumentException(SR.GetString(SR.net_param_not_string, gmt.GetType().FullName), "gmt");
                }
            }
            if (wd2 != null && wd2 != DBNull.Value && gmt != Missing.Value)
            {
                _wd2 = wd2 as string;
                if (_wd2 == null)
                {
                    throw new ArgumentException(SR.GetString(SR.net_param_not_string, wd2.GetType().FullName), "wd2");
                }
            }
            if (_gmt != null)
            {
                if (!isGMT(_gmt))
                {
                    if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.weekdayRange()", "gmt"));
                    throw new ArgumentException(SR.GetString(SR.net_proxy_not_gmt), "gmt");
                }
                return weekdayRangeInternal(DateTime.UtcNow, dayOfWeek(wd1), dayOfWeek(_wd2));
            }
            if (_wd2 != null)
            {
                if (isGMT(_wd2))
                {
                    return weekdayRangeInternal(DateTime.UtcNow, dayOfWeek(wd1), dayOfWeek(wd1));
                }
                return weekdayRangeInternal(DateTime.Now, dayOfWeek(wd1), dayOfWeek(_wd2));
            }
            return weekdayRangeInternal(DateTime.Now, dayOfWeek(wd1), dayOfWeek(wd1));
        }

        private static bool isGMT(string gmt)
        {
            return string.Compare(gmt, "GMT", StringComparison.OrdinalIgnoreCase)==0;
        }

        private static DayOfWeek dayOfWeek(string weekDay)
        {
            if (weekDay!=null && weekDay.Length==3) {
                if (weekDay[0]=='T' || weekDay[0]=='t') {
                    if ((weekDay[1]=='U' || weekDay[1]=='u') && (weekDay[2]=='E' || weekDay[2]=='e')) {
                        return DayOfWeek.Tuesday;
                    }
                    if ((weekDay[1]=='H' || weekDay[1]=='h') && (weekDay[2]=='U' || weekDay[2]=='u')) {
                        return DayOfWeek.Thursday;
                    }
                }
                if (weekDay[0]=='S' || weekDay[0]=='s') {
                    if ((weekDay[1]=='U' || weekDay[1]=='u') && (weekDay[2]=='N' || weekDay[2]=='n')) {
                        return DayOfWeek.Sunday;
                    }
                    if ((weekDay[1]=='A' || weekDay[1]=='a') && (weekDay[2]=='T' || weekDay[2]=='t')) {
                        return DayOfWeek.Saturday;
                    }
                }
                if ((weekDay[0]=='M' || weekDay[0]=='m') && (weekDay[1]=='O' || weekDay[1]=='o') && (weekDay[2]=='N' || weekDay[2]=='n')) {
                    return DayOfWeek.Monday;
                }
                if ((weekDay[0]=='W' || weekDay[0]=='w') && (weekDay[1]=='E' || weekDay[1]=='e') && (weekDay[2]=='D' || weekDay[2]=='d')) {
                    return DayOfWeek.Wednesday;
                }
                if ((weekDay[0]=='F' || weekDay[0]=='f') && (weekDay[1]=='R' || weekDay[1]=='r') && (weekDay[2]=='I' || weekDay[2]=='i')) {
                    return DayOfWeek.Friday;
                }
            }
            return (DayOfWeek)unchecked(-1);
        }

        private static bool weekdayRangeInternal(DateTime now, DayOfWeek wd1, DayOfWeek wd2)
        {
            if (wd1<0 || wd2<0) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_invalid_parameter, "WebProxyScriptHelper.weekdayRange()"));
                throw new ArgumentException(SR.GetString(SR.net_proxy_invalid_dayofweek), wd1 < 0 ? "wd1" : "wd2");
            }
            if (wd1<=wd2) {
                return wd1<=now.DayOfWeek && now.DayOfWeek<=wd2;
            }
            return wd2>=now.DayOfWeek || now.DayOfWeek>=wd1;
        }

        //-----------------------------------------------
        //Additional methods for IPv6 support
        //-----------------------------------------------
        public string getClientVersion()
        {
            return "1.0";
        }
		private static int MAX_IPADDRESS_LIST_LENGTH = 1024;
        public string sortIpAddressList(string IPAddressList)
        {

            //---------------------------------------------------------------            
            //If the input is nothing, return nothing
            //---------------------------------------------------------------
            if(IPAddressList == null || IPAddressList.Length == 0)
            {
                return string.Empty;
            }

            //---------------------------------------------------------------            
            //The input string is supposed to be a list of IPAddress strings 
            //separated by a semicolon
            //---------------------------------------------------------------
            string[] IPAddressStrings = IPAddressList.Split(new char[] {';'});
			if(IPAddressStrings.Length > MAX_IPADDRESS_LIST_LENGTH)
			{
				throw new ArgumentException(string.Format(
									SR.GetString(SR.net_max_ip_address_list_length_exceeded),
									MAX_IPADDRESS_LIST_LENGTH), "IPAddressList");
			}
			

            //----------------------------------------------------------------
            //If there are no separators, just return the original string
            //----------------------------------------------------------------
            if(IPAddressStrings.Length == 1)
            {
                return IPAddressList;
            }

            //----------------------------------------------------------------
            //Parse the strings into Socket Address buffers
            //----------------------------------------------------------------
            SocketAddress[] SockAddrIn6List = new SocketAddress[IPAddressStrings.Length];
            for(int i = 0; i < IPAddressStrings.Length; i++)
            {
            	//Trim leading and trailing spaces
                IPAddressStrings[i] = IPAddressStrings[i].Trim();
				if(IPAddressStrings[i].Length == 0)
	                throw new ArgumentException(SR.GetString(SR.dns_bad_ip_address), "IPAddressList");
                SocketAddress saddrv6 = new SocketAddress(AddressFamily.InterNetworkV6, 
															SocketAddress.IPv6AddressSize);
            	//Parse the string to a v6 address structure
                SocketError errorCode =
                    UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(
                        IPAddressStrings[i],
                        AddressFamily.InterNetworkV6,
                        IntPtr.Zero,
                        saddrv6.m_Buffer,
                        ref saddrv6.m_Size );
                if(errorCode != SocketError.Success)
                {
                    //Could not parse this into a SOCKADDR_IN6
                    //See if we can parse this into s SOCKEADDR_IN
                    SocketAddress saddrv4 = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
                    errorCode =
                        UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(
                            IPAddressStrings[i],
                            AddressFamily.InterNetwork,
                            IntPtr.Zero,
                            saddrv4.m_Buffer,
                            ref saddrv4.m_Size );
                    if(errorCode != SocketError.Success)
                    {
                        //This address is neither IPv4 nor IPv6 string throw
		                throw new ArgumentException(SR.GetString(SR.dns_bad_ip_address), "IPAddressList");
                    }
					else
					{
						//This is a valid IPv4 address. We need to map this to a mapped v6 address
						IPEndPoint dummy = new IPEndPoint(IPAddress.Any, 0);
						IPEndPoint IPv4EndPoint = (IPEndPoint)dummy.Create(saddrv4);
						byte[] IPv4AddressBytes = IPv4EndPoint.Address.GetAddressBytes();
						byte[] IPv6MappedAddressBytes = new byte[16]; //IPv6 is 16 bytes address
						for(int j = 0; j < 10; j++) IPv6MappedAddressBytes[j] = 0x00;
						IPv6MappedAddressBytes[10] = 0xFF;
						IPv6MappedAddressBytes[11] = 0xFF;						
						IPv6MappedAddressBytes[12] = IPv4AddressBytes[0];
						IPv6MappedAddressBytes[13] = IPv4AddressBytes[1];						
						IPv6MappedAddressBytes[14] = IPv4AddressBytes[2];
						IPv6MappedAddressBytes[15] = IPv4AddressBytes[3];
						IPAddress v6Address = new IPAddress(IPv6MappedAddressBytes);
						IPEndPoint IPv6EndPoint = new IPEndPoint(v6Address, IPv4EndPoint.Port);
						saddrv6 = IPv6EndPoint.Serialize();
					}
                }

				//At this point,we have SOCKADDR_IN6 buffer
				//add them to the list 
				SockAddrIn6List[i] = saddrv6;
            }

           	//----------------------------------------------------------------
            //All the IPAddress strings are parsed into 
            //either a native v6 address or mapped v6 address
            //The Next step is to prepare for calling the WSAIOctl
            //By creating a SOCKET_ADDRESS_LIST
            //----------------------------------------------------------------
            int cbRequiredBytes = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS_LIST)) + 
            						(SockAddrIn6List.Length -1)*Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS));
			Dictionary<IntPtr, KeyValuePair<SocketAddress, string> > UnmanagedToManagedMapping = new Dictionary<IntPtr, KeyValuePair<SocketAddress, string>>();
			GCHandle[] GCHandles = new GCHandle[SockAddrIn6List.Length];
			for(int i = 0; i < SockAddrIn6List.Length; i++)
			{
				GCHandles[i] = GCHandle.Alloc(SockAddrIn6List[i].m_Buffer, GCHandleType.Pinned);
			}
			IntPtr pSocketAddressList = Marshal.AllocHGlobal(cbRequiredBytes);
			try
			{
				//---------------------------------------------------
				//Create a socket address list structure
				//and set the pointers to the pinned sock addr buffers
				//---------------------------------------------------
				unsafe 
				{
					UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS_LIST* pList 
								= (UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS_LIST*)pSocketAddressList;
					pList->iAddressCount = SockAddrIn6List.Length; //Set the number of addresses
					UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS* pSocketAddresses =
							&pList->Addresses;
					for(int i = 0; i < pList->iAddressCount; i++)
					{
						pSocketAddresses[i].iSockaddrLength = SocketAddress.IPv6AddressSize;
						pSocketAddresses[i].lpSockAddr = GCHandles[i].AddrOfPinnedObject();
						UnmanagedToManagedMapping[pSocketAddresses[i].lpSockAddr]
							= new KeyValuePair<SocketAddress, string>(SockAddrIn6List[i], IPAddressStrings[i]);
					}
					//---------------------------------------------------
					//Create a socket and ask it to sort the list 
					//---------------------------------------------------								
					Socket s = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
 					int cbProcessed = s.IOControl(
							IOControlCode.AddressListSort,
							pSocketAddressList, //Input buffer
							cbRequiredBytes, //Buffer size
							pSocketAddressList, //Outbuffer - same as in buffer
							cbRequiredBytes //out buffer size - same as in buffer size
							);

					//---------------------------------------------------
					//At this point The sorting is complete
					//---------------------------------------------------
					StringBuilder sb = new StringBuilder();
					for(int i = 0; i < pList->iAddressCount; i++)
					{
						IntPtr lpSockAddr = pSocketAddresses[i].lpSockAddr;
						KeyValuePair<SocketAddress, string> kv = UnmanagedToManagedMapping[lpSockAddr];
						sb.Append(kv.Value);
						if(i != pList->iAddressCount - 1) sb.Append(";");
					}					
					return sb.ToString();
				}
			}
			finally
			{
				if(pSocketAddressList != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(pSocketAddressList);
				}
				for(int i = 0; i < GCHandles.Length; i++)
				{
					if(GCHandles[i].IsAllocated)
						GCHandles[i].Free();
				}				
			}

        }
        public bool isInNetEx(string ipAddress, string ipPrefix)
        {
            //---------------------------------------------------------------            
            //Check for Null arguments
            //---------------------------------------------------------------
            if (ipAddress==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isResolvable()", "ipAddress"));
                throw new ArgumentNullException("ipAddress");
            }
            if (ipPrefix==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.isResolvable()", "ipPrefix"));
                throw new ArgumentNullException("ipPrefix");
            }            
            //---------------------------------------------------------------            
            //Try parsing the ipAddress given
            //If we can't parse throw a bad ip address exception
            //---------------------------------------------------------------            
            IPAddress address; 
            if(!IPAddress.TryParse(ipAddress, out address))
            {
                throw new FormatException(SR.GetString(SR.dns_bad_ip_address));
            }

            //---------------------------------------------------------------            
            //First check if there is a separator for prefix
            //---------------------------------------------------------------
            int prefixSeparatorIndex = ipPrefix.IndexOf("/");
            if(prefixSeparatorIndex < 0)
            {
                //There is no separator - throw an exception - we require a prefix
                throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                
            }

            //---------------------------------------------------------------            
            //Now separate the prefix into address and prefix length
            //---------------------------------------------------------------            
            string[] parts = ipPrefix.Split(new char[] {'/'});
            if(parts.Length != 2 || parts[0] == null || parts[0].Length == 0 ||
                parts[1] == null || parts[1].Length == 0 || parts[1].Length > 2)
            {
                //Invalid address or prefix lengths
                throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                                
            }
            
            //---------------------------------------------------------------            
            //Now that we have an address and a prefix length, validate 
            //the address and prefix lengths
            //---------------------------------------------------------------            
            IPAddress prefixAddress; 
            if(!IPAddress.TryParse(parts[0], out prefixAddress))
            {
                throw new FormatException(SR.GetString(SR.dns_bad_ip_address));
            }
            int prefixLength = 0;
            if(!Int32.TryParse(parts[1], out prefixLength))
            {
                throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                                                
            }
        
            //---------------------------------------------------------------            
            //Now check that the AddressFamilies match
            //---------------------------------------------------------------            
            if(address.AddressFamily !=  prefixAddress.AddressFamily)
            {
                throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                                                                                
            }

            //---------------------------------------------------------------            
            //We have validated the input.
            //Now check the prefix match
            //---------------------------------------------------------------            

            //----------------------------------------------
            //IPv6 prefix length can not be greater than 64 for v6
            //and can't be greater than 32 for v4
            //----------------------------------------------
            if ( 
                ( (address.AddressFamily == AddressFamily.InterNetworkV6) && 
                  ( prefixLength < 1 || prefixLength > 64) ) ||
                  
                ( (address.AddressFamily == AddressFamily.InterNetwork) && 
                  ( prefixLength < 1 || prefixLength > 32) )

            )
            {
                throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                                                                
            }                

            //----------------------------------------------
            //Validate that the Prefix address supplied
            //has zeros after the prefix length
            //for example: feco:2002::/16 is invalid 
            //because the bits other than the first 16 bits [top most]
            //are non-zero
            //----------------------------------------------
            byte[] prefixAddressBytes = prefixAddress.GetAddressBytes();

            //----------------------------------------------
            //Get the number of complete bytes and then the 
            //remaning bits
            //----------------------------------------------                
            byte prefixBytes = (byte)(prefixLength/8);
            byte prefixExtraBits = (byte)(prefixLength%8); 
            byte i = prefixBytes;

            //----------------------------------------------
            //If the extra bits are non zero the bits
            //must come from the byte after the "prefixBytes"
            //For example in the feco:2000/19 prefix
            //19 = 2 * 8 + 3 
            //So the extra bits must come from the 3rd byte 
            //which is at the index 2 in the addressbytes array
            //
            //Now in the 3rd byte at index 2, anything after the 
            //3rd bit must be zero
            //Thats what we are testing below
            //----------------------------------------------                                
            if(prefixExtraBits != 0)
            {
                if( (0xFF & (prefixAddressBytes[prefixBytes] << prefixExtraBits)) != 0)
                {
                    throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                                                                                                        
                }
                i++;
            }

            //-----------------------------------------------
            //Now that we checked the last byte any 
            //byte after that must be zero 
            //Note that i here is at 3 in the example above
            //after incrementing from the above if statement
            //That means anything from the 4th byte [inclusive]
            //must be zero
            //-----------------------------------------------
            int MaxBytes = (prefixAddress.AddressFamily == AddressFamily.InterNetworkV6)?IPAddress.IPv6AddressBytes:IPAddress.IPv4AddressBytes;
            while( i < MaxBytes)
            {
                if(prefixAddressBytes[i++] != 0)
                {
                    throw new FormatException(SR.GetString(SR.net_bad_ip_address_prefix));                         
                }
            }

            //------------------------------------------------
            //Now that we verifiex the prefix
            //we must make sure that that the 
            //bits match until the prefix 
            //------------------------------------------------
            byte[] addressBytes = address.GetAddressBytes();
            for( i = 0; i < prefixBytes; i++)
            {
                if(addressBytes[i] != prefixAddressBytes[i])
                {
                    return false;
                }
            }

            //-------------------------------------------------
            //Compare the remaining bits
            //-------------------------------------------------
            if(prefixExtraBits > 0)
            {
                byte addrbyte = addressBytes[prefixBytes];
                byte prefixbyte = prefixAddressBytes[prefixBytes];

                //Clear 8 - Remaining bits from the addr byte
                addrbyte = (byte)(addrbyte >> (8 - prefixExtraBits));
                addrbyte = (byte)(addrbyte << (8 - prefixExtraBits));
                if(addrbyte != prefixbyte)
                {
                    return false;
                }
            }
            return true;
        }        
        public string myIpAddressEx()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                GlobalLog.Print("WebProxyScriptHelper::myIPAddressesEx()");
                IPAddress[] ipAddresses = NclUtilities.LocalAddresses;
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    if (!IPAddress.IsLoopback(ipAddresses[i]))
                    {
                        sb.Append(ipAddresses[i].ToString());
                        if(i != ipAddresses.Length -1)
                            sb.Append(";");                        
                    }
                }
             
            }
            catch {}
            return sb.Length > 0 ? sb.ToString(): string.Empty;
        }

        public string dnsResolveEx(string host) {
            GlobalLog.Print("WebProxyScriptHelper::dnsResolveEx() host:" + ValidationHelper.ToString(host));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.dnsResolve()", "host"));
                throw new ArgumentNullException("host");
            }
            IPHostEntry ipHostEntry = null;
            try
            {
                ipHostEntry = Dns.InternalGetHostByName(host);
            }
            catch { }
            if (ipHostEntry == null)
            {
                return string.Empty;
            }
            IPAddress[] addresses = ipHostEntry.AddressList;            
            if(addresses.Length == 0)
            {
               return string.Empty;
            }
            
            StringBuilder sb = new StringBuilder();                            
            for (int i = 0; i < addresses.Length; i++)
            {
                sb.Append(addresses[i].ToString());
                if(i != addresses.Length -1)
                    sb.Append(";");  
             }
            return sb.Length > 0 ? sb.ToString(): string.Empty;
        }
        

        public bool isResolvableEx(string host) {
            GlobalLog.Print("WebProxyScriptHelper::dnsResolveEx() host:" + ValidationHelper.ToString(host));
            if (host==null) {
                if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_proxy_called_with_null_parameter, "WebProxyScriptHelper.dnsResolve()", "host"));
                throw new ArgumentNullException("host");
            }
            IPHostEntry ipHostEntry = null;
            try
            {
                ipHostEntry = Dns.InternalGetHostByName(host);
            }
            catch { }
            if (ipHostEntry == null)
            {
                return false;
            }
            IPAddress[] addresses = ipHostEntry.AddressList;            
            if(addresses.Length == 0)
            {
               return false;
            }
            return true;
            
        }

    }
}
