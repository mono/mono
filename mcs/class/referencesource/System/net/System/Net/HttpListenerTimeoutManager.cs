﻿using System.Diagnostics;
using System.Net.Configuration;

namespace System.Net {

    //
    // See the native HTTP_TIMEOUT_LIMIT_INFO structure documentation for additional information.
    // http://msdn.microsoft.com/en-us/library/aa364661.aspx
    //
    public class HttpListenerTimeoutManager
    {
        private HttpListener listener;
        private int[] timeouts;
        private uint minSendBytesPerSecond;

        internal HttpListenerTimeoutManager(HttpListener context)
        {
            listener = context;

            //
            // We have to maintain local state since we allow applications to set individual timeouts. Native Http
            // API for setting timeouts expects all timeout values in every call so we have remember timeout values 
            // to fill in the blanks. Except MinSendBytesPerSecond, local state for remaining five timeouts is 
            // maintained in timeouts array.
            //
            // No initialization is required because a value of zero indicates that system defaults should be used.
            timeouts = new int[5];

            LoadConfigurationSettings();
        }

        // Initial values come from the config.  The values can then be overridden using this public API.
        private void LoadConfigurationSettings()
        {
            long[] configTimeouts = SettingsSectionInternal.Section.HttpListenerTimeouts;
            Debug.Assert(configTimeouts != null);
            Debug.Assert(configTimeouts.Length == (timeouts.Length + 1));

            bool setNonDefaults = false;
            for (int i = 0; i < timeouts.Length; i++)
            {
                if (configTimeouts[i] != 0)
                {
                    Debug.Assert(configTimeouts[i] <= ushort.MaxValue, "Timeout out of range: " + configTimeouts[i]);
                    timeouts[i] = (int)configTimeouts[i];
                    setNonDefaults = true;
                }
            }

            if (configTimeouts[5] != 0)
            {
                Debug.Assert(configTimeouts[5] <= uint.MaxValue, "Timeout out of range: " + configTimeouts[5]);
                minSendBytesPerSecond = (uint)configTimeouts[5];
                setNonDefaults = true;
            }

            if (setNonDefaults)
            {
                listener.SetServerTimeout(timeouts, minSendBytesPerSecond);
            }
        }

        #region Helpers

        private TimeSpan GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE type)
        {
            //
            // Since we maintain local state, GET is local.
            //
            return new TimeSpan(0, 0, (int)timeouts[(int)type]);            
        }

        private void SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE type, TimeSpan value)
        {
            Int64 timeoutValue;

            //
            // All timeouts are defined as USHORT in native layer (except MinSendRate, which is ULONG). Make sure that
            // timeout value is within range.
            //
            timeoutValue = Convert.ToInt64(value.TotalSeconds);

            if (timeoutValue < 0 || timeoutValue > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            //
            // Use local state to get values for other timeouts. Call into the native layer and if that 
            // call succeeds, update local state.
            //

            int[] currentTimeouts = timeouts;
            currentTimeouts[(int)type] = (int)timeoutValue;
            listener.SetServerTimeout(currentTimeouts, minSendBytesPerSecond);
            timeouts[(int)type] = (int)timeoutValue;
        }

        #endregion Helpers

        #region Properties

        // The time, in seconds, allowed for the request entity body to arrive.  The default timer is 2 minutes.
        // 
        // The HTTP Server API turns on this timer when the request has an entity body. The timer expiration is 
        // initially set to the configured value. When the HTTP Server API receives additional data indications on the 
        // request, it resets the timer to give the connection another interval.
        //
        // Use TimeSpan.Zero to indiate that system defaults should be used.
        public TimeSpan EntityBody
        {
            get
            {
                return GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.EntityBody);
            }
            set
            {
                SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.EntityBody, value);
            }
        }

        // The time, in seconds, allowed for the HTTP Server API to drain the entity body on a Keep-Alive connection. 
        // The default timer is 2 minutes.
        // 
        // On a Keep-Alive connection, after the application has sent a response for a request and before the request 
        // entity body has completely arrived, the HTTP Server API starts draining the remainder of the entity body to 
        // reach another potentially pipelined request from the client. If the time to drain the remaining entity body 
        // exceeds the allowed period the connection is timed out.
        //
        // Use TimeSpan.Zero to indiate that system defaults should be used.
        public TimeSpan DrainEntityBody
        {
            get
            {
                return GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.DrainEntityBody);
            }
            set
            {
                SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.DrainEntityBody, value);
            }
        }

        // The time, in seconds, allowed for the request to remain in the request queue before the application picks 
        // it up.  The default timer is 2 minutes.
        //
        // Use TimeSpan.Zero to indiate that system defaults should be used.
        public TimeSpan RequestQueue
        {
            get
            {
                return GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.RequestQueue);
            }
            set
            {
                SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.RequestQueue, value);
            }
        }

        // The time, in seconds, allowed for an idle connection.  The default timer is 2 minutes.
        // 
        // This timeout is only enforced after the first request on the connection is routed to the application.
        //
        // Use TimeSpan.Zero to indiate that system defaults should be used.
        public TimeSpan IdleConnection
        {
            get
            {
                return GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.IdleConnection);
            }
            set
            {
                SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.IdleConnection, value);
            }
        }

        // The time, in seconds, allowed for the HTTP Server API to parse the request header.  The default timer is 
        // 2 minutes.
        //  
        // This timeout is only enforced after the first request on the connection is routed to the application.
        //
        // Use TimeSpan.Zero to indiate that system defaults should be used.
        public TimeSpan HeaderWait
        {
            get
            {
                return GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.HeaderWait);
            }
            set
            {
                SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.HeaderWait, value);
            }
        }

        // The minimum send rate, in bytes-per-second, for the response. The default response send rate is 150 
        // bytes-per-second.
        //
        // To disable this timer set it to UInt32.MaxValue
        public long MinSendBytesPerSecond
        {
            get
            {
                //
                // Since we maintain local state, GET is local.
                //
                return minSendBytesPerSecond;
            }
            set
            {
                //
                // MinSendRate value is ULONG in native layer.
                //
                if (value < 0 || value > uint.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                listener.SetServerTimeout(timeouts, (uint)value);
                minSendBytesPerSecond = (uint)value;
            }
        }

        #endregion Properties
    }
}
