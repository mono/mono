//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;

    public class ExceptionPersistenceExtension
    {
        bool persistExceptions;

        public ExceptionPersistenceExtension()
        {
            this.persistExceptions = true;
        }

        public bool PersistExceptions
        {
            get
            {
                return this.persistExceptions;
            }
            set
            {
                this.persistExceptions = value;
            }
        }
    }
}
