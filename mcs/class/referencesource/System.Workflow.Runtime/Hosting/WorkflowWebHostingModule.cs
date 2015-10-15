/*******************************************************************************
// Copyright (C) 2000-2001 Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Workflow Web Hosting Module.
 
// DESCRIPTION
//      Implementation of Workflow Web Host Module.
 
// REVISIONS
// Date          Ver     By           Remarks
// ~~~~~~~~~~    ~~~     ~~~~~~~~     ~~~~~~~~~~~~~~
// 02/22/05      1.0     Microsoft       Implementation.
 * ****************************************************************************/

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.Collections.Specialized;
using System.Threading;

#endregion

namespace System.Workflow.Runtime.Hosting
{
    /// <summary>
    /// Cookie based rotuing module implementation
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowWebHostingModule : IHttpModule
    {
        HttpApplication currentApplication;

        public WorkflowWebHostingModule()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Workflow Web Hosting Module Created");            
        }

        /// <summary>
        /// IHttpModule.Init()
        /// </summary>
        /// <param name="application"></param>
        void IHttpModule.Init(HttpApplication application)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Workflow Web Hosting Module Initialized");

            this.currentApplication = application;

            //Listen for Acquire and ReleaseRequestState event
            application.ReleaseRequestState += this.OnReleaseRequestState;
            application.AcquireRequestState += this.OnAcquireRequestState;
        }

        void IHttpModule.Dispose()
        {

        }

        void OnAcquireRequestState(Object sender, EventArgs e)
        {
            //Performs Cookie based routing.
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WebHost Module Routing Begin");

            HttpCookie routingCookie = HttpContext.Current.Request.Cookies.Get("WF_WorkflowInstanceId");

            if (routingCookie != null)
            {
                HttpContext.Current.Items.Add("__WorkflowInstanceId__", new Guid(routingCookie.Value));
            }
            //else no routing information found, it could be activation request or non workflow based request.
        }

        void OnReleaseRequestState(Object sender, EventArgs e)
        {
            //Saves cookie back to client.
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("WF_WorkflowInstanceId");

            if (cookie == null)
            {
                cookie = new HttpCookie("WF_WorkflowInstanceId");
                Object workflowInstanceId = HttpContext.Current.Items["__WorkflowInstanceId__"];

                if (workflowInstanceId != null)
                {
                    cookie.Value = workflowInstanceId.ToString();
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }            
        }        
    }   
}
