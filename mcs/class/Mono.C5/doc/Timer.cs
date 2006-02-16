/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;

namespace DocNet
{
    class Timer
    {
        static System.Diagnostics.Process p;

        static long sws;

        static long svm;

        static double stt;

        static DateTime swc;

        public long ws;

        public long vm;

        public double tt;

        public DateTime wc;

        public double deltat;

        public double deltac;


        public Timer()
        {
            if (p == null)
            {
                p = System.Diagnostics.Process.GetCurrentProcess();
                stt = p.TotalProcessorTime.TotalMilliseconds;
                sws = p.WorkingSet64;
                svm = p.VirtualMemorySize64;
                swc = DateTime.Now;
            }
        }


        public double snap()
        {
            double oldt = tt;
            DateTime oldc = wc; p.Refresh();
            tt = p.TotalProcessorTime.TotalMilliseconds - stt;
            deltat = tt - oldt;
            ws = p.WorkingSet64 - sws;
            vm = p.VirtualMemorySize64 - svm;
            wc = DateTime.Now;

            TimeSpan x = oldc.Subtract(wc);

            deltac = -x.TotalMilliseconds;
            return deltac;
        }
    }
}