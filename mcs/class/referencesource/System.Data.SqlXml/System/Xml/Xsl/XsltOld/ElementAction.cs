//------------------------------------------------------------------------------
// <copyright file="ElementAction.cs" company="Microsoft">
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

    internal class ElementAction : ContainerAction {
        private const int NameDone           = 2;

        private Avt                nameAvt;
        private Avt                nsAvt;
        private bool               empty;
        private InputScopeManager  manager;
        // Compile time precalculated AVTs
        private string              name;
        private string              nsUri;
        private PrefixQName         qname; // When we not have AVTs at all we can do this. null otherwise.

        internal ElementAction() {}

        private static PrefixQName CreateElementQName(string name, string nsUri, InputScopeManager manager) {
            if (nsUri == XmlReservedNs.NsXmlNs) {
                throw XsltException.Create(Res.Xslt_ReservedNS, nsUri);
            }

            PrefixQName qname = new PrefixQName();
            qname.SetQName(name);

            if (nsUri == null) {
                qname.Namespace = manager.ResolveXmlNamespace(qname.Prefix);
            }
            else {
                qname.Namespace = nsUri;
            }
            return qname;
        }

        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);
            CheckRequiredAttribute(compiler, this.nameAvt, "name");

            this.name  = PrecalculateAvt(ref this.nameAvt);
            this.nsUri = PrecalculateAvt(ref this.nsAvt  );

            // if both name and ns are not AVT we can calculate qname at compile time and will not need namespace manager anymore
            if (this.nameAvt == null && this.nsAvt == null) {
                if(this.name != "xmlns") {
                    this.qname = CreateElementQName(this.name, this.nsUri, compiler.CloneScopeManager());                    
                }
            }
            else {
                this.manager = compiler.CloneScopeManager();
            }

            if (compiler.Recurse()) {
                Debug.Assert(this.empty == false);
                CompileTemplate(compiler);
                compiler.ToParent();
            }
            this.empty = (this.containedActions == null) ;
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;
            if (Ref.Equal(name, compiler.Atoms.Name)) {
                this.nameAvt      = Avt.CompileAvt(compiler, value);
            }
            else if (Ref.Equal(name, compiler.Atoms.Namespace)) {
                this.nsAvt = Avt.CompileAvt(compiler, value);
            }
            else if (Ref.Equal(name, compiler.Atoms.UseAttributeSets)) {
                AddAction(compiler.CreateUseAttributeSetsAction());
            }
            else {
                return false;
            }

            return true;
        }

        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);

            switch (frame.State) {
            case Initialized:
                if(this.qname != null) {
                    frame.CalulatedName = this.qname;
                }
                else {
                    frame.CalulatedName = CreateElementQName(
                        this.nameAvt == null ? this.name  : this.nameAvt.Evaluate(processor, frame),
                        this.nsAvt   == null ? this.nsUri : this.nsAvt  .Evaluate(processor, frame),
                        this.manager
                    );
                }
                goto case NameDone;

            case NameDone:
                {
                    PrefixQName qname = frame.CalulatedName;
                    if (processor.BeginEvent(XPathNodeType.Element, qname.Prefix, qname.Name, qname.Namespace, this.empty) == false) {
                        // Come back later
                        frame.State = NameDone;
                        break;
                    }

                    if (! this.empty) {
                        processor.PushActionFrame(frame);
                        frame.State = ProcessingChildren;
                        break;                              // Allow children to run
                    }
                    else {
                        goto case ProcessingChildren;
                    }
                }
            case ProcessingChildren:
                if (processor.EndEvent(XPathNodeType.Element) == false) {
                    frame.State = ProcessingChildren;
                    break;
                }
                frame.Finished();
                break;
            default:
                Debug.Fail("Invalid ElementAction execution state");
    		    break;
            }
        }
    }
}
