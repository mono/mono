//
// System.Data.ObjectSpaces.CommonObjectContext.cs : A basic ObjectContext for handling persistent object identity and state.
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.ObjectSpaces.Schema;
using System.Globalization;

namespace System.Data.ObjectSpaces
{
        public class CommonObjectContext : ObjectContext
        {
		#region Fields

		ObjectSchema objectSchema;

		#endregion // Fields

		#region Constructors

		public CommonObjectContext (ObjectSchema objectSchema)
		{
                        if (objectSchema == null)
				throw new ArgumentNullException ("objectSchema", Locale.GetText ("'objectSchema' argument cannot be null."));
			this.objectSchema = objectSchema;
		}

		#endregion // Constructors

		#region Methods
                
                [MonoTODO]
                public override void Add (object obj, ObjectState state)
                {
                        if (obj == null)
				throw new ContextException (Locale.GetText ("Cannot add null object into any object context."));
			if (!Enum.IsDefined (typeof (ObjectState), state))
				throw new NullReferenceException ();
			if (state == ObjectState.Unknown)
				throw new ContextException (Locale.GetText ("Cannot add any object into an object context as an Unknown object."));
                }

                [MonoTODO]
                public override void Delete (object obj)
                {
                        if (obj == null)
				throw new ArgumentNullException ("obj", Locale.GetText ("'obj' argument cannot be null."));
                }
                
                [MonoTODO]
                public override ValueRecord GetCurrentValueRecord (object obj)
                {
                        if (obj == null)
				throw new ArgumentNullException ("obj", Locale.GetText ("'obj' argument cannot be null."));
                        
                        return null;        
                }
                               
                [MonoTODO]
                public override ObjectState GetObjectState (object obj)
                {
                        if (obj == null)
				throw new ArgumentNullException ("obj", Locale.GetText ("'obj' argument cannot be null."));
                      
			throw new NotImplementedException ();
                }
                
                [MonoTODO]
                public override ValueRecord GetOriginalValueRecord (object obj)
                {
                        if (obj == null)
				throw new ArgumentNullException ("obj", Locale.GetText ("'obj' argument cannot be null."));
                        
                        return null;        
                }
                
                [MonoTODO]
                public override void Import (ObjectContext context)
                {
                        if (context == null)
				throw new ArgumentNullException ("context", Locale.GetText ("'context' argument cannot be null."));
                }
                                
                [MonoTODO]
                public override void Import (ObjectContext context, object obj)
                {
                        if (obj == null)
				throw new ArgumentNullException ("obj", Locale.GetText ("'obj' argument cannot be null."));
                        if (context == null)
				throw new ArgumentNullException ("context", Locale.GetText ("'context' argument cannot be null."));
                }

                [MonoTODO]
                public override void Remove (object obj)
                {
                        if (obj == null)
				throw new ArgumentNullException ("obj", Locale.GetText ("'obj' argument cannot be null."));
                }

		#endregion // Methods
        }
}

#endif
