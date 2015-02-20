//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Threading;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Runtime.InteropServices;
    using System.Runtime;
    using System.Diagnostics;

    // The clipboard may be accessed by other processes. 
    // RetriableClipboard retries several times before giving up.
    static class RetriableClipboard
    {
        const int retryCount = 10;
        const int sleepTime = 50;

        internal static IDataObject GetDataObject()
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return Clipboard.GetDataObject();
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err.ToString());
                    if (Fx.IsFatal(err))
                    {
                        throw;
                    }
                    Thread.Sleep(sleepTime);
                }
            }
            return null;
        }

        internal static void SetDataObject(object data, bool copy)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    Clipboard.SetDataObject(data, copy);
                    return;
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err.ToString());
                    if (Fx.IsFatal(err))
                    {
                        throw;
                    }
                    Thread.Sleep(sleepTime);
                }
            }
        }

        internal static void SetImage(BitmapSource image)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    Clipboard.SetImage(image);
                    return;
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err.ToString());
                    if (Fx.IsFatal(err))
                    {
                        throw;
                    }
                    Thread.Sleep(sleepTime);
                }
            }
        }
    }
}
