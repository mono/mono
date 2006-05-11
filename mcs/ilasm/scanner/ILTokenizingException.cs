//
// Mono.ILASM.ILTokenizingException
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright 2004 Novell, Inc (http://www.novell.com)
//


using System;

namespace Mono.ILASM {

        public class ILTokenizingException : ILAsmException {

                public readonly string Token;

                public ILTokenizingException (Location location, string token)
                        : base (location, token)
                {
                        Token = token;
                }
        }

}


