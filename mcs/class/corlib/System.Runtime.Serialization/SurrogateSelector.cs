//
// System.Runtime.Serialization.SurrogateSelector.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.Serialization
{
	public class SurrogateSelector : ISurrogateSelector
	{
		// Constructor
		public SurrogateSelector()
			: base ()
		{
		}

		// Methods
		[MonoTODO]
		public virtual void AddSurrogate (Type type,
			  StreamingContext context, ISerializationSurrogate surrogate)
		{
			if (type == null || surrogate == null)
				throw new ArgumentNullException ("Null reference");
		}

		[MonoTODO]
		public virtual void ChainSelector (ISurrogateSelector selector)
		{
		}

		[MonoTODO]
		public virtual ISurrogateSelector GetNextSelector ()
		{
			return null;
		}

		[MonoTODO]
		public virtual ISerializationSurrogate GetSurrogate (Type type, StreamingContext context, out ISurrogateSelector selector)
		{
			selector = null;
			return null;
		}

		[MonoTODO]
		public virtual void RemoveSurrogate (Type type, StreamingContext context)
		{
		}
	}
	
}
