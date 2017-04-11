//------------------------------------------------------------------------------
// <copyright file="XmlDomTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {

    using System;
    using System.IO;
    using System.Text;
    using System.Runtime.Versioning;

    // Represents a writer that will make it possible to work with prefixes even
    // if the namespace is not specified.
    // This is not possible with XmlTextWriter. But this class inherits XmlTextWriter.
    internal class XmlDOMTextWriter : XmlTextWriter {

        public XmlDOMTextWriter( Stream w, Encoding encoding ) : base( w,encoding ) {
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public XmlDOMTextWriter( String filename, Encoding encoding ) : base( filename,encoding ){
        }

        public XmlDOMTextWriter( TextWriter w ) : base( w ){
        }

        // Overrides the baseclass implementation so that emptystring prefixes do
        // do not fail if namespace is not specified.
        public override void WriteStartElement( string prefix, string localName, string ns ){
            if( ( ns.Length == 0 ) && ( prefix.Length != 0 ) )
                prefix = "" ;

            base.WriteStartElement( prefix, localName, ns );
        }

        // Overrides the baseclass implementation so that emptystring prefixes do
        // do not fail if namespace is not specified.
        public override  void WriteStartAttribute( string prefix, string localName, string ns ){
            if( ( ns.Length == 0 ) && ( prefix.Length != 0 )  )
                prefix = "" ;

            base.WriteStartAttribute( prefix, localName, ns );
        }
    }
}
    

  
