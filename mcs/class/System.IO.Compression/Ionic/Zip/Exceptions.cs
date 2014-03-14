// Exceptions.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008, 2009 Dino Chiesa and Microsoft Corporation.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-12 12:19:10>
//
// ------------------------------------------------------------------
//
// This module defines exceptions used in the class library.
//



using System;
using System.Collections.Generic;
using System.Text;
#if !NETCF
using System.Runtime.Serialization;
#endif

namespace Ionic.Zip
{
    ///// <summary>
    ///// Base exception type for all custom exceptions in the Zip library. It acts as a marker class.
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class)]
    //internal class ZipExceptionAttribute : Attribute { }



    /// <summary>
    /// Issued when an <c>ZipEntry.ExtractWithPassword()</c> method is invoked
    /// with an incorrect password.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Runtime.InteropServices.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d0000B")]
    internal class BadPasswordException : ZipException
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadPasswordException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadPasswordException(String message)
            : base(message)
        { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        /// <param name="innerException">The innerException for this exception.</param>
        public BadPasswordException(String message, Exception innerException)
            : base(message, innerException)
        {
        }


#if ! (NETCF || SILVERLIGHT)
        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="info">The serialization info for the exception.</param>
        /// <param name="context">The streaming context from which to deserialize.</param>
        protected BadPasswordException(SerializationInfo info, StreamingContext context)
            : base(info, context)
          {  }
#endif

    }

    /// <summary>
    /// Indicates that a read was attempted on a stream, and bad or incomplete data was
    /// received.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Runtime.InteropServices.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d0000A")]
    internal class BadReadException : ZipException
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadReadException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadReadException(String message)
            : base(message)
        { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        /// <param name="innerException">The innerException for this exception.</param>
        public BadReadException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

#if ! (NETCF || SILVERLIGHT)
        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="info">The serialization info for the exception.</param>
        /// <param name="context">The streaming context from which to deserialize.</param>
        protected BadReadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
          {  }
#endif

    }



    /// <summary>
    /// Issued when an CRC check fails upon extracting an entry from a zip archive.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Runtime.InteropServices.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d00009")]
    internal class BadCrcException : ZipException
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadCrcException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadCrcException(String message)
            : base(message)
        { }


#if ! (NETCF || SILVERLIGHT)
        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="info">The serialization info for the exception.</param>
        /// <param name="context">The streaming context from which to deserialize.</param>
        protected BadCrcException(SerializationInfo info, StreamingContext context)
            : base(info, context)
          {  }
#endif

    }


    /// <summary>
    /// Issued when errors occur saving a self-extracting archive.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Runtime.InteropServices.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d00008")]
    internal class SfxGenerationException : ZipException
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public SfxGenerationException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public SfxGenerationException(String message)
            : base(message)
        { }

#if ! (NETCF || SILVERLIGHT)
        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="info">The serialization info for the exception.</param>
        /// <param name="context">The streaming context from which to deserialize.</param>
        protected SfxGenerationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
          {  }
#endif

    }


    /// <summary>
    /// Indicates that an operation was attempted on a ZipFile which was not possible
    /// given the state of the instance. For example, if you call <c>Save()</c> on a ZipFile
    /// which has no filename set, you can get this exception.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Runtime.InteropServices.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d00007")]
    internal class BadStateException : ZipException
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadStateException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadStateException(String message)
            : base(message)
        { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        /// <param name="innerException">The innerException for this exception.</param>
        public BadStateException(String message, Exception innerException)
            : base(message, innerException)
        {}

#if ! (NETCF || SILVERLIGHT)
        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="info">The serialization info for the exception.</param>
        /// <param name="context">The streaming context from which to deserialize.</param>
        protected BadStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
          {  }
#endif

    }

    /// <summary>
    /// Base class for all exceptions defined by and throw by the Zip library.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [System.Runtime.InteropServices.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d00006")]
    internal class ZipException : Exception
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public ZipException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public ZipException(String message) : base(message) { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        /// <param name="innerException">The innerException for this exception.</param>
        public ZipException(String message, Exception innerException)
            : base(message, innerException)
        { }

#if ! (NETCF || SILVERLIGHT)
        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="info">The serialization info for the exception.</param>
        /// <param name="context">The streaming context from which to deserialize.</param>
        protected ZipException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
#endif

    }

}
