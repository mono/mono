//------------------------------------------------------------------------------
// <copyright file="ClientConfigPerf.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Globalization;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Net;
    using Assembly = System.Reflection.Assembly;
    using StringBuilder = System.Text.StringBuilder;

#if NOPERF
    internal class ClientConfigPerf {
        const int SIZE=100;

        long[]      _counters;
        long[]      _totals;
        string[]    _names;
        int         _current;
        bool        _enabled;

        static internal ClientConfigPerf ConfigSystem = new ClientConfigPerf(false);
        static internal ClientConfigPerf ScanSections = new ClientConfigPerf(false);
        static internal ClientConfigPerf CopySection = new ClientConfigPerf(false);
        static internal ClientConfigPerf CopyXmlNode = new ClientConfigPerf(false);
        static internal ClientConfigPerf GetConfig = new ClientConfigPerf(true);

        ClientConfigPerf(bool enabled) {
#if PERF
            _enabled = enabled;
            if (_enabled) {
                _counters = new long[SIZE];
                _totals = new long[SIZE];
                _names = new string[SIZE];
            }
#endif
        }

        internal void Reset() {
#if PERF
            _current = 0;
#endif
        }

        internal void Record(string name) {
#if PERF
            if (_enabled && _current < _counters.Length) {
                _names[_current] = name;
                Microsoft.Win32.SafeNativeMethods.QueryPerformanceCounter(out _counters[_current]);
                if (_current > 0) {
                    _totals[_current] += _counters[_current] - _counters[_current - 1];
                }

                _current++;
            }
#endif
        }

        void DoPrint() {
#if PERF
            if (_enabled) {
                long lfreq = 0;
                Microsoft.Win32.SafeNativeMethods.QueryPerformanceFrequency(out lfreq);
                double freq = (double) lfreq;
                double grandtotal = 0;

                for (int i = 0; i < _current; i++) {
                    double time = ((double)_totals[i]) / freq;
                    grandtotal += time;
                    Console.WriteLine("{0,-20} : {1:F6}", _names[i], time);
                }

                Console.WriteLine("{0,-20} : {1:F6}\n", "TOTAL", grandtotal);
            }
#endif
        }

        public static void Print() {
#if PERF
            ConfigSystem.DoPrint();
            ScanSections.DoPrint();
            CopySection.DoPrint();
            CopyXmlNode.DoPrint();
            GetConfig.DoPrint();
#endif
        }
    }
#endif
}
