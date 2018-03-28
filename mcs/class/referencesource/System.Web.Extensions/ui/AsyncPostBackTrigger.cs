//------------------------------------------------------------------------------
// <copyright file="AsyncPostBackTrigger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Web.Util;

    public class AsyncPostBackTrigger : UpdatePanelControlTrigger {

        private IScriptManagerInternal _scriptManager;

        private Control _associatedControl;
        private static MethodInfo _eventHandler;
        private bool _eventHandled;
        private string _eventName;

        public AsyncPostBackTrigger() {
        }

        internal AsyncPostBackTrigger(IScriptManagerInternal scriptManager) {
            _scriptManager = scriptManager;
        }

        private static MethodInfo EventHandler {
            get {
                if (_eventHandler == null) {
                    _eventHandler = typeof(AsyncPostBackTrigger).GetMethod("OnEvent");
                }
                return _eventHandler;
            }
        }

        [
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"),
        TypeConverter("System.Web.UI.Design.AsyncPostBackTriggerControlIDConverter, " +
            AssemblyRef.SystemWebExtensionsDesign)
        ]
        public new string ControlID {
            get {
                return base.ControlID;
            }
            set {
                base.ControlID = value;
            }
        }

        [
        DefaultValue(""),
        Category("Behavior"),
        ResourceDescription("AsyncPostBackTrigger_EventName"),
        TypeConverter("System.Web.UI.Design.AsyncPostBackTriggerEventNameConverter, " +
            AssemblyRef.SystemWebExtensionsDesign),
        ]
        public string EventName {
            get {
                if (_eventName == null) {
                    return String.Empty;
                }
                return _eventName;
            }
            set {
                _eventName = value;
            }
        }

        internal IScriptManagerInternal ScriptManager {
            get {
                if (_scriptManager == null) {
                    Page page = Owner.Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _scriptManager = UI.ScriptManager.GetCurrent(page);
                    if (_scriptManager == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.Common_ScriptManagerRequired, Owner.ID));
                    }
                }
                return _scriptManager;
            }
        }

        protected internal override void Initialize() {
            base.Initialize();

            _associatedControl = FindTargetControl(true);

            ScriptManager.RegisterAsyncPostBackControl(_associatedControl);

            string eventName = EventName;
            if (eventName.Length != 0) {
                // If EventName is specified, attach our event handler to it
                EventInfo eventInfo = _associatedControl.GetType().GetEvent(eventName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (eventInfo == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.AsyncPostBackTrigger_CannotFindEvent, eventName, ControlID, Owner.ID));
                }

                MethodInfo handlerMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
                ParameterInfo[] parameters = handlerMethod.GetParameters();
                if (!handlerMethod.ReturnType.Equals(typeof(void)) ||
                    (parameters.Length != 2) ||
                    (typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType) == false)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.AsyncPostBackTrigger_InvalidEvent, eventName, ControlID, Owner.ID));
                }

                Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, EventHandler);
                eventInfo.AddEventHandler(_associatedControl, handler);
            }
        }

        protected internal override bool HasTriggered() {
            if (!String.IsNullOrEmpty(EventName)) {
                // If EventName is specified we are triggered if our event was raised
                return _eventHandled;
            }
            else {
                // If EventName is not specified, check if the control that caused the
                // postback either has the exact UniqueID we're looking for, or at least
                // begins with it.
                string sourceElement = ScriptManager.AsyncPostBackSourceElementID;
                return
                    (sourceElement == _associatedControl.UniqueID) ||
                    (sourceElement.StartsWith(_associatedControl.UniqueID + "$", StringComparison.Ordinal));
            }
        }

        // DevDiv Bugs 127369: This method should be private and the reflection lookup should assert reflection permission
        // so the private reflection works in medium trust.
        // However, ASP.NET AJAX 1.0 was released with this method public. Since it would be a breaking change to make it private
        // now, it was decided to leave it as is.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            Justification="TODO: This will be fixed in fc_serverfx")]
        public void OnEvent(object sender, EventArgs e) {
            _eventHandled = true;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            if (String.IsNullOrEmpty(ControlID)) {
                return "AsyncPostBack";
            }
            else {
                return "AsyncPostBack: " + ControlID +
                    (String.IsNullOrEmpty(EventName) ? String.Empty : ("." + EventName));
            }
        }
    }
}
