// Copyright (c) Microsoft Corp., 2004. All rights reserved.
#region Using directives

using System;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    [ComImport, Guid(Guids.CLSID_WDEProgramPublisher)]
    internal class WDEProgramPublisher
    {
    }

    internal sealed class ProgramPublisher
    {
        #region Data members

        private bool isPublished = false;
        IWDEProgramPublisher publisher;
        private DebugController controller;
        GCHandle gchWdeProgramNode; // This is used to pin the wdeProgramNodeSingleton (VS Debugger is using address to calculate cookies)
        private IWDEProgramNode wdeProgramNodeSingleton;

        #endregion

        #region Methods

        public ProgramPublisher()
        {
            this.publisher = null;
        }

        public bool Publish(DebugController controller)
        {
            Debug.WriteLine("WDE: ProgramPublisher.Publish()");

            // In order to guarantee that the Program Nodes are always in the MTA, publish on a separate thread.
            if (isPublished)
                return false;

            try
            {
                this.controller = controller;
                Thread publisherThread = new Thread(PublisherThreadFunc);
                publisherThread.SetApartmentState(ApartmentState.MTA);
                publisherThread.IsBackground = true;
                publisherThread.Start();
                publisherThread.Join();
            }
            catch (Exception e)
            {
                // Eat up exceptions if the debugger is not installed.
                Debug.WriteLine("WDE: ProgramPublisher.Publish() exception: " + e.ToString());
            }

            return this.isPublished;
        }

        private void PublisherThreadFunc()
        {
            try
            {
                this.publisher = new WDEProgramPublisher() as IWDEProgramPublisher;
                this.wdeProgramNodeSingleton = new ProgramNode(this.controller);
                this.gchWdeProgramNode = GCHandle.Alloc(this.wdeProgramNodeSingleton);

                this.publisher.Publish(this.wdeProgramNodeSingleton);
                this.isPublished = true;
            }
            catch (Exception e)
            {
                // Ignore any exceptions that are caused by WDE.dll not being present or registered.
                Debug.WriteLine("WDE: ProgramPublisher.PublisherThreadFunc() exception: " + e.ToString());
            }
        }

        public void Unpublish()
        {
            if (!isPublished)
                return;

            Debug.WriteLine("WDE: ProgramPublisher.Unpublish()");

            // In order to guarantee that the Program Nodes are always in the MTA, unpublish on a separate thread.
            try
            {
                Thread unpublisherThread = new Thread(UnpublishThreadFunc);
                unpublisherThread.SetApartmentState(ApartmentState.MTA);
                unpublisherThread.IsBackground = true;
                unpublisherThread.Start();
                unpublisherThread.Join();
            }
            catch (Exception e)
            {
                // Eat up exceptions if the debugger is not installed, etc.
                Debug.WriteLine("WDE: ProgramPublisher.Unpublish() exception: " + e.ToString());
            }

            Debug.WriteLine("WDE: ProgramPublisher.Unpublish() Done");
        }

        private void UnpublishThreadFunc()
        {
            try
            {
                this.publisher.Unpublish(this.wdeProgramNodeSingleton);
            }
            catch (Exception e)
            {
                Debug.WriteLine("WDE: ProgramPublisher.UnpublishThreadFunc(): catch exception " + e.ToString());
                // We eat up any exceptions that can occur because the host process is abnormally terminated.
            }
            finally
            {
                this.gchWdeProgramNode.Free(); // Rrelease pin on the this.wdeProgramNodeSingleton

                Marshal.ReleaseComObject(this.publisher);
                this.publisher = null;
            }

            this.isPublished = false;
        }

        #endregion
    }

    [ComImport(), Guid(Guids.IID_IWDEProgramPublisher), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IWDEProgramPublisher
    {
        void Publish([MarshalAs(UnmanagedType.IUnknown)] object ProgramNode);
        void Unpublish([MarshalAs(UnmanagedType.IUnknown)] object ProgramNode);
    }


}
