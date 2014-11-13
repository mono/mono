//------------------------------------------------------------------------------
// <copyright file="ProcessModelInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ProcessInfo class
 */
namespace System.Web {
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Security.Permissions;

    public class ProcessModelInfo {

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        static public ProcessInfo    GetCurrentProcessInfo() {
            HttpContext context = HttpContext.Current;
            if (context == null || context.WorkerRequest == null || 
                !(context.WorkerRequest is System.Web.Hosting.ISAPIWorkerRequestOutOfProc))
            {
                throw new HttpException(SR.GetString(SR.Process_information_not_available));                
            }

            int     dwReqExecuted = 0;
            int     dwReqExecuting = 0;
            long    tmCreateTime = 0;
            int     pid = 0;
            int     mem = 0;

            int iRet = UnsafeNativeMethods.PMGetCurrentProcessInfo (
                    ref dwReqExecuted, ref dwReqExecuting, ref mem, 
                    ref tmCreateTime, ref pid);

            if (iRet < 0)
                throw new HttpException(SR.GetString(SR.Process_information_not_available));

            DateTime startTime = DateTime.FromFileTime(tmCreateTime);
            TimeSpan age = DateTime.Now.Subtract(startTime);

            return new ProcessInfo(startTime, age, pid, dwReqExecuted, 
                                   ProcessStatus.Alive, ProcessShutdownReason.None, mem);
        }


        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        static public ProcessInfo[]  GetHistory(int numRecords) {
            HttpContext context = HttpContext.Current;
            if (context == null || context.WorkerRequest == null || 
                !(context.WorkerRequest is System.Web.Hosting.ISAPIWorkerRequestOutOfProc))
            {
                throw new HttpException(SR.GetString(SR.Process_information_not_available));                
            }

            if (numRecords < 1)
                return null;

            int [] dwPID   = new int [numRecords];
            int [] dwExed  = new int [numRecords];
            int [] dwExei  = new int [numRecords];
            int [] dwPend  = new int [numRecords];
            int [] dwReas  = new int [numRecords];
            long [] tmCrea = new long [numRecords];
            long [] tmDeat = new long [numRecords];
            int []  mem    = new int [numRecords];

            int iRows = UnsafeNativeMethods.PMGetHistoryTable (numRecords, dwPID, dwExed, dwPend, dwExei, dwReas, mem, tmCrea, tmDeat);
            if (iRows < 0)
                throw new HttpException(SR.GetString(SR.Process_information_not_available));

            ProcessInfo[] ret = new ProcessInfo[iRows];
            for (int iter=0; iter<iRows; iter++) {

                DateTime startTime = DateTime.FromFileTime(tmCrea[iter]);
                TimeSpan age       = DateTime.Now.Subtract(startTime);

                ProcessStatus          status = ProcessStatus.Alive;
                ProcessShutdownReason  rea    = ProcessShutdownReason.None;

                if (dwReas[iter] != 0) {
                    if (tmDeat[iter] > 0)
                        age = DateTime.FromFileTime(tmDeat[iter]).Subtract(startTime);

                    if ((dwReas[iter] & 0x0004) != 0)
                        status = ProcessStatus.Terminated;
                    else if ((dwReas[iter] & 0x0002) != 0)
                        status = ProcessStatus.ShutDown;
                    else
                        status = ProcessStatus.ShuttingDown;
                    
                    if ((0x0040 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.IdleTimeout;
                    else if ((0x0080 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.RequestsLimit;
                    else if ((0x0100 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.RequestQueueLimit;
                    else if ((0x0020 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.Timeout;
                    else if ((0x0200 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.MemoryLimitExceeded;
                    else if ((0x0400 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.PingFailed;
                    else if ((0x0800 & dwReas[iter]) != 0)
                        rea = ProcessShutdownReason.DeadlockSuspected;
                    else
                        rea = ProcessShutdownReason.Unexpected;
                }

                ret[iter] = new ProcessInfo(startTime, age, dwPID[iter], dwExed[iter], status, rea, mem[iter]);
            }

            return ret;
        }
    }


}
