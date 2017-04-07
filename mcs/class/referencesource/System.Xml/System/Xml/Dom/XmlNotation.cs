//------------------------------------------------------------------------------
// <copyright file="XmlNotation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System;
    using System.Diagnostics;

    // Contains a notation declared in the DTD or schema.
    public class XmlNotation : XmlNode {
        String publicId;
        String systemId;
        String name;

        internal XmlNotation( String name, String publicId, String systemId, XmlDocument doc ): base( doc ) {
            this.name = doc.NameTable.Add(name);
            this.publicId = publicId;
            this.systemId = systemId;
        }

        // Gets the name of the node.
        public override string Name { 
            get { return name;}
        }

        // Gets the name of the current node without the namespace prefix.
        public override string LocalName { 
            get { return name;}
        }

        // Gets the type of the current node.
        public override XmlNodeType NodeType {
            get { return XmlNodeType.Notation;}
        }

        // Throws an InvalidOperationException since Notation can not be cloned.
        public override XmlNode CloneNode(bool deep) {

            throw new InvalidOperationException(Res.GetString(Res.Xdom_Node_Cloning));
        }

        //
        // Microsoft extensions
        //

        // Gets a value indicating whether the node is read-only.
        public override bool IsReadOnly {
            get { 
                return true;        // Make notations readonly
            }
        }

        // Gets the value of the public identifier on the notation declaration.
        public String PublicId { 
            get { return publicId;}
        }

        // Gets the value of
        // the system identifier on the notation declaration.
        public String SystemId { 
            get { return systemId;}
        }

        // Without override these two functions, we can't guarantee that WriteTo()/WriteContent() functions will never be called
        public override String OuterXml { 
            get { return String.Empty; }
        }        
                
        public override String InnerXml { 
            get { return String.Empty; }
            set { throw new InvalidOperationException( Res.GetString(Res.Xdom_Set_InnerXml ) ); }
        }        
        
        // Saves the node to the specified XmlWriter.
        public override void WriteTo(XmlWriter w) {
        }

        // Saves all the children of the node to the specified XmlWriter.
        public override void WriteContentTo(XmlWriter w) {
        }
    } 
}
