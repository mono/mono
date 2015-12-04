//------------------------------------------------------------------------------
// <copyright file="CopyCodeAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;

    internal class CopyCodeAction : Action {
        // Execution states:
        private const int Outputting = 2;
        
        private ArrayList copyEvents;   // Copy code action events

        internal CopyCodeAction() {
            this.copyEvents = new ArrayList();
        }

        internal void AddEvent(Event copyEvent) {
            this.copyEvents.Add(copyEvent);
        }

        internal void AddEvents(ArrayList copyEvents) {
            Debug.Assert(copyEvents != null);
            this.copyEvents.AddRange(copyEvents);
        }

        internal override void ReplaceNamespaceAlias(Compiler compiler) {
            int count = this.copyEvents.Count;
            for(int i = 0; i< count; i++) {
                ((Event) this.copyEvents[i]).ReplaceNamespaceAlias(compiler);
            }
        }
        
        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);
            Debug.Assert(this.copyEvents != null && this.copyEvents.Count > 0);

            switch (frame.State) {
            case Initialized:
                frame.Counter = 0;
                frame.State   = Outputting;
                goto case Outputting;

            case Outputting:
                Debug.Assert(frame.State == Outputting);

                while (processor.CanContinue) {
                    Debug.Assert(frame.Counter < this.copyEvents.Count);
                    Event copyEvent = (Event) this.copyEvents[frame.Counter];

                    if (copyEvent.Output(processor, frame) == false) {
                        // This event wasn't processed
                        break;
                    }

                    if (frame.IncrementCounter() >= this.copyEvents.Count) {
                        frame.Finished();
                        break;
                    }
                }
                break;
            default:
                Debug.Fail("Invalid CopyCodeAction execution state");
                break;
            }
        }

        internal override DbgData GetDbgData(ActionFrame frame) {
            Debug.Assert(frame.Counter < this.copyEvents.Count);
            return ((Event)this.copyEvents[frame.Counter]).DbgData;        
        }
    }
}
