namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;


    internal partial class StateDesigner : FreeformActivityDesigner
    {
        #region ContainedDesignersParser class
        /// <summary>
        /// Helper class to sort the contained designers
        /// </summary>
        private class ContainedDesignersParser
        {
            List<StateInitializationDesigner> _stateInitializationDesigners = new List<StateInitializationDesigner>();
            List<StateFinalizationDesigner> _stateFinalizationDesigners = new List<StateFinalizationDesigner>();
            List<EventDrivenDesigner> _eventDrivenDesigners = new List<EventDrivenDesigner>();
            List<StateDesigner> _leafStateDesigners = new List<StateDesigner>();
            List<StateDesigner> _stateDesigners = new List<StateDesigner>();
            List<ActivityDesigner> _ordered;
            internal ContainedDesignersParser(ReadOnlyCollection<ActivityDesigner> containedDesigners)
            {
                foreach (ActivityDesigner designer in containedDesigners)
                {
                    StateInitializationDesigner stateInitializationDesigner = designer as StateInitializationDesigner;
                    if (stateInitializationDesigner != null)
                    {
                        _stateInitializationDesigners.Add(stateInitializationDesigner);
                        continue;
                    }
                    StateFinalizationDesigner stateFinalizationDesigner = designer as StateFinalizationDesigner;
                    if (stateFinalizationDesigner != null)
                    {
                        _stateFinalizationDesigners.Add(stateFinalizationDesigner);
                        continue;
                    }
                    EventDrivenDesigner eventDrivenDesigner = designer as EventDrivenDesigner;
                    if (eventDrivenDesigner != null)
                    {
                        _eventDrivenDesigners.Add(eventDrivenDesigner);
                        continue;
                    }

                    StateDesigner stateDesigner = designer as StateDesigner;
                    if (stateDesigner != null)
                    {
                        if (StateMachineHelpers.IsLeafState((StateActivity)designer.Activity))
                            _leafStateDesigners.Add(stateDesigner);
                        else
                            _stateDesigners.Add(stateDesigner);

                        continue;
                    }
                }
            }

            public List<ActivityDesigner> Ordered
            {
                get
                {
                    if (_ordered == null)
                    {
                        _ordered = new List<ActivityDesigner>();
                        _ordered.AddRange(_stateInitializationDesigners.ToArray());
                        _ordered.AddRange(_eventDrivenDesigners.ToArray());
                        _ordered.AddRange(_stateFinalizationDesigners.ToArray());
                        _ordered.AddRange(_leafStateDesigners.ToArray());
                        _ordered.AddRange(_stateDesigners.ToArray());
                    }
                    return _ordered;
                }
            }

            public List<StateInitializationDesigner> StateInitializationDesigners
            {
                get
                {
                    return _stateInitializationDesigners;
                }
            }

            public List<StateFinalizationDesigner> StateFinalizationDesigners
            {
                get
                {
                    return _stateFinalizationDesigners;
                }
            }

            public List<EventDrivenDesigner> EventDrivenDesigners
            {
                get
                {
                    return _eventDrivenDesigners;
                }
            }

            public List<StateDesigner> LeafStateDesigners
            {
                get
                {
                    return _leafStateDesigners;
                }
            }

            public List<StateDesigner> StateDesigners
            {
                get
                {
                    return _stateDesigners;
                }
            }
        }
        #endregion
    }
}
