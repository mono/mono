// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Xml;

namespace System.Workflow.Runtime
{
	public sealed class WorkflowRuntime : IDisposable

	{
		private bool is_started;
		private string name;
		private List <object> services;
		internal Dictionary <Guid, WorkflowInstance> instances; // for this runtime
		private static List <WorkflowRuntime> runtimes; // all runtimes

		public WorkflowRuntime ()
		{
			services = new List <object> ();
			runtimes = new List <WorkflowRuntime> ();
			instances = new Dictionary <Guid, WorkflowInstance> ();
			is_started = false;
		}

		public WorkflowRuntime (string configSectionName) : this ()
		{

		}

		/*public WorkflowRuntime (WorkflowRuntimeSection settings) {} */

		 // Events
		//public event EventHandler<WorkflowRuntimeEventArgs> Started;
		//public event EventHandler<WorkflowRuntimeEventArgs> Stopped;
		public event EventHandler <WorkflowCompletedEventArgs> WorkflowCompleted;
		public event EventHandler <WorkflowEventArgs> WorkflowCreated;
		public event EventHandler <WorkflowEventArgs> WorkflowResumed;
		public event EventHandler <WorkflowEventArgs> WorkflowStarted;
		public event EventHandler <WorkflowTerminatedEventArgs> WorkflowTerminated;
		public event EventHandler <WorkflowEventArgs> WorkflowIdled;

		// Properties
		public bool IsStarted {
			get { return is_started; }
		}

      		public string Name {
      			get { return name; }
      			set { name = value; }
      		}

		// Methods
		public void AddService (object service_toadd)
		{
			foreach (object service in services) {
				if (service == service_toadd) {
					throw new InvalidOperationException ("Cannot add a service that already exists.");
				}
			}

			services.Add (service_toadd);
		}

		public WorkflowInstance CreateWorkflow (Type workflowType)
		{
			return CreateWorkflow (workflowType, null, Guid.NewGuid ());
		}

		public WorkflowInstance CreateWorkflow (XmlReader workflowDefinitionReader)
		{
			throw new NotImplementedException ();
		}

		public WorkflowInstance CreateWorkflow (Type workflowType, Dictionary <string, object> namedArgumentValues)
		{
			return CreateWorkflow (workflowType, namedArgumentValues, Guid.NewGuid ());
		}

		public WorkflowInstance CreateWorkflow (Type workflowType, Dictionary <string, object> namedArgumentValues, Guid instanceId)
		{
			WorkflowLoaderService loader;
			Activity root_activity;
			WorkflowInstance instance;

			if (is_started == false) {
				StartRuntime ();
			}

			loader = (WorkflowLoaderService) GetService (typeof (WorkflowLoaderService));
			root_activity = loader.CreateInstance (workflowType);
			instance = new WorkflowInstance (instanceId, this, root_activity);
			instances.Add (instanceId, instance);

			if (WorkflowStarted != null) {
				WorkflowStarted (this, new WorkflowEventArgs (instance));
			}

			return instance;
		}

		public WorkflowInstance CreateWorkflow (XmlReader workflowDefinitionReader, XmlReader rulesReader, Dictionary <string, object> namedArgumentValues)
		{
			throw new NotImplementedException ();
		}

		public WorkflowInstance CreateWorkflow (XmlReader workflowDefinitionReader, XmlReader rulesReader, Dictionary <string, object> namedArgumentValues, Guid instanceId)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			StopRuntime ();
		}

		public ReadOnlyCollection <T> GetAllServices <T> ()
		{
			List <T> services_req = new List <T> ();

			foreach (T service in services) {
				// To determine if we have a service in the list we have to
				// check the class and its base types
				for (Type type = service.GetType (); type != null; type = type.BaseType) {
					if (type == typeof (T)) {
						services_req.Add (service);
					}
				}
			}

			return new ReadOnlyCollection <T> (services_req);
		}

		public ReadOnlyCollection <object> GetAllServices (Type serviceType)
		{
			List <object> services_req = new List <object> ();

			foreach (object service in services) {

				// To determine if we have a service in the list we have to
				// check the class and its base types
				for (Type type = service.GetType (); type != null; type = type.BaseType) {
					if (type == serviceType) {
						services_req.Add (service);
					}
				}
			}

			return new ReadOnlyCollection <object> (services_req);
		}


		public ReadOnlyCollection <WorkflowInstance> GetLoadedWorkflows ()
		{
			throw new NotImplementedException ();
		}

		public T GetService <T> ()
		{
			return (T) GetService (typeof (T));
		}

		public WorkflowInstance GetWorkflow (Guid instanceId)
		{
			// TODO: This is related to persistence services
			if (IsStarted == false) {
				throw new InvalidOperationException ("This operation can only be performed with a started WorkflowRuntime");
			}

			throw new NotImplementedException ();
		}

		public object GetService (Type serviceType)
		{
			ReadOnlyCollection <object> objs = GetAllServices (serviceType);

			if (objs.Count > 1) {
				throw new InvalidOperationException ("More than one runtime service exists");
			}

			return objs[0];
		}

		public void RemoveService (object service)
		{
			if (services.Contains (service) == false) {
				throw new InvalidOperationException ("Cannot remove a service that has not been added");
			}

			services.Remove (service);
		}

		public void StartRuntime ()
		{
			// Provide default services if the user has not provided them
			if ((GetAllServices (typeof (WorkflowLoaderService))).Count == 0) {
				AddService (new DefaultWorkflowLoaderService ());
			}

			if ((GetAllServices (typeof (WorkflowSchedulerService))).Count == 0) {
				AddService (new DefaultWorkflowSchedulerService ());
			}

			is_started = true;
			runtimes.Add (this);
		}

		public void StopRuntime ()
		{
			is_started = false;

			foreach (WorkflowInstance wi in instances.Values) {
				wi.Abort ();
			}

			runtimes.Remove (this);
		}

		// Private
		internal void OnWorkflowCompleted (WorkflowInstance wi)
		{
			if (WorkflowCompleted != null) {
				WorkflowCompleted (this, new WorkflowCompletedEventArgs (wi,
					wi.GetWorkflowDefinition ()));
			}
		}

		internal void OnWorkflowIdled (WorkflowInstance wi)
		{
			if (WorkflowIdled != null) {
				WorkflowIdled (this, new WorkflowEventArgs (wi));
			}
		}

		static internal WorkflowInstance GetInstanceFromGuid (Guid instanceId)
		{
			WorkflowInstance wi;
			foreach (WorkflowRuntime rt in runtimes) {
				wi = rt.instances [instanceId];

				if (wi != null) {
					return wi;
				}
			}

			return null;
		}

	}
}

