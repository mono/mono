//
// System.Data.ObjectSpaces.PersistenceOptions.cs 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003-2004
//

#if NET_1_2

namespace System.Data.ObjectSpaces {
        public class PersistenceOptions
        {
		#region Fields

		Depth depth;
		PersistenceErrorBehavior errorBehavior;
		static readonly PersistenceOptions DefaultPersistenceOptions = new PersistenceOptions ();

		#endregion // Fields

		#region Constructors

		public PersistenceOptions (Depth depth, PersistenceErrorBehavior errorBehavior)
		{
			this.depth = depth;
			this.errorBehavior = errorBehavior;
		}

		public PersistenceOptions (PersistenceErrorBehavior errorBehavior)
			: this (Depth.ObjectGraph, errorBehavior)
		{
		}

		public PersistenceOptions (Depth depth)
			: this (depth, PersistenceErrorBehavior.ThrowAtFirstError)
		{
		}

		public PersistenceOptions ()
			: this (Depth.ObjectGraph, PersistenceErrorBehavior.ThrowAtFirstError)
		{
		}

		#endregion // Constructors

		#region Properties

		public static PersistenceOptions Default {
			get { return DefaultPersistenceOptions; }
		}

		public Depth Depth {
			get { return depth; }
		}
		
		public PersistenceErrorBehavior ErrorBehavior {
			get { return errorBehavior; }
		}

		#endregion // Properties
        }
}

#endif
