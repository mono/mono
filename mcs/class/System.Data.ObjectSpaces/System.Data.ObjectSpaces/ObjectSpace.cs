//
// System.Data.ObjectSpaces.ObjectSpace.cs : Handles high-level object persistence interactions with a data source.
//
// Authors:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System.Collections;
using System.Data;
using System.Data.Mapping;
using System.Data.ObjectSpaces.Query;
using System.Data.ObjectSpaces.Schema;

namespace System.Data.ObjectSpaces
{
        public class ObjectSpace
        {        
		#region Fields

		MappingSchema map;
		ObjectSources sources;
		ObjectSchema os;
		CommonObjectContext context;

		#endregion // Fields

		#region Constructors

		private ObjectSpace ()
			: base ()
		{
			os = new ObjectSchema ();
			context = new CommonObjectContext (os);
		}

                [MonoTODO]                
                public ObjectSpace (MappingSchema map, ObjectSources sources) 
			: this ()
		{
			this.map = map;
			this.sources = sources;
		}
                
                [MonoTODO ("Figure out correct name")]
                public ObjectSpace (string mapFile, IDbConnection con) 
			: this ()
		{
			map = new MappingSchema (mapFile);
			sources = new ObjectSources ();
			sources.Add (map.DataSources [0].Name, con);
		}
                
                public ObjectSpace (string mapFile, ObjectSources sources) 
			: this ()
		{
			map = new MappingSchema (mapFile);
			this.sources = sources;
		}
                
                [MonoTODO ("Figure out correct name")]
                public ObjectSpace (MappingSchema map, IDbConnection con) 
			: this ()
		{
			this.map = map;
			sources = new ObjectSources ();
			sources.Add (map.DataSources [0].Name, con);
		}

		#endregion // Constructors

		#region Properties

		internal ObjectContext ObjectContext {
			get { return context; }
		}

		#endregion // Properties

		#region Methods

                public object GetObject (ObjectQuery query, object[] parameters)
                {
			return GetObject (GetObjectReader (query, parameters));
                }

                public object GetObject (Type type, string queryString)
                {
			return GetObject (GetObjectReader (type, queryString));
                }

                public object GetObject (Type type, string queryString, string relatedSpan)
                {
			return GetObject (GetObjectReader (type, queryString));
                }

		private object GetObject (ObjectReader reader)
		{
			reader.Read ();
			object result = reader.Current;
			reader.Close ();
			return result;
		}

                public ObjectReader GetObjectReader (ObjectQuery query, object[] parameters)
                {
			ObjectExpression oe = OPath.Parse (query, os);
			CompiledQuery cq = oe.Compile (map);
			return ObjectEngine.GetObjectReader (sources, context, cq, parameters);
                }

                public ObjectReader GetObjectReader (Type type, string queryString)
                {
			return GetObjectReader (new ObjectQuery (type, queryString), null);
                }

                public ObjectReader GetObjectReader (Type type, string queryString, string relatedSpan)
                {
			return GetObjectReader (new ObjectQuery (type, queryString, relatedSpan), null);
                }

                public ObjectSet GetObjectSet (ObjectQuery query, object[] parameters)
                {
			return GetObjectSet (GetObjectReader (query, parameters));
                }

                public ObjectSet GetObjectSet (Type type, string queryString)
                {
			return GetObjectSet (GetObjectReader (type, queryString));
                }

                public ObjectSet GetObjectSet (Type type, string queryString, string relatedSpan)
                {
			return GetObjectSet (GetObjectReader (type, queryString, relatedSpan));
                }

		private ObjectSet GetObjectSet (ObjectReader reader)
		{
			ObjectSet result = new ObjectSet ();
			foreach (object o in reader)
				result.Add (o);
			reader.Close ();
			return result;
		}

                public void MarkForDeletion (object obj) 
		{
			MarkForDeletion (new object[] {obj});
		}

                [MonoTODO]
                public void MarkForDeletion (ICollection objs) {}

                public void PersistChanges (object obj) 
		{
			PersistChanges (new object[] {obj}, new PersistenceOptions ());
		}

                public void PersistChanges (object obj, PersistenceOptions options) 
		{
			PersistChanges (new object[] {obj}, options);
		}

                public void PersistChanges (ICollection objs) 
		{
			PersistChanges (objs, new PersistenceOptions ());
		}

                public void PersistChanges (ICollection objs, PersistenceOptions options) 
		{
			ObjectEngine.PersistChanges (map, sources, context, objs, options);
		}

                public void Resync (object obj, Depth depth) 
		{
			Resync (new object[] {obj}, depth);
		}

                public void Resync (ICollection objs, Depth depth) 
		{
			ObjectEngine.Resync (map, sources, context, objs, depth);
		}

                public void StartTracking (object obj, InitialState state) 
		{
			StartTracking (new object[] {obj}, state);
		}
        
                [MonoTODO]
                public void StartTracking (ICollection objs, InitialState state) 
		{
		}

		#endregion // Methods
        }
}

#endif
