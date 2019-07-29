using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Mono.Profiler.Aot {
	//
	// Write the contents of a .aotprofile
	// See mono/profiler/aot.h for a description of the file format
	//
	public sealed class ProfileWriter : ProfileBase {
		Stream stream;
		ProfileData data;
		int id;

		readonly Dictionary<TypeRecord, int> typeIds = new Dictionary<TypeRecord, int> ();
		readonly Dictionary<ModuleRecord, int> moduleIds = new Dictionary<ModuleRecord, int> ();

		unsafe void WriteInt32 (int intValue)
		{
			byte* b = (byte*)&intValue;

			for (int i = 0; i < 4; i++)
				stream.WriteByte (b [i]);
		}

		void WriteString (string str)
		{
			WriteInt32 (str.Length);
			var buf = Encoding.UTF8.GetBytes (str);
			stream.Write (buf, 0, buf.Length);
		}

		int AddModule (ModuleRecord m)
		{
			int mId;
			if (moduleIds.TryGetValue (m, out mId))
				return mId;

			mId = id++;
			moduleIds [m] = mId;

			WriteRecord (RecordType.IMAGE, mId);
			WriteString (m.Name);
			WriteString (m.Mvid);

			return mId;
		}

		int AddType (TypeRecord t)
		{
			int tId;
			if (typeIds.TryGetValue (t, out tId))
				return tId;

			var moduleId = AddModule (t.Module);

			int instId = -1;
			if (t.GenericInst != null)
				instId = AddGenericInstance (t.GenericInst);

			tId = id++;
			typeIds [t] = tId;

			WriteRecord (RecordType.TYPE, tId);
			stream.WriteByte ((byte)MonoTypeEnum.MONO_TYPE_CLASS);
			WriteInt32 (moduleId);
			WriteInt32 (instId);
			WriteString (t.Name);

			return tId;
		}

		int AddGenericInstance (GenericInstRecord gi)
		{
			// add the types first, before we start writing the GINST record
			for (int i = 0; i < gi.Types.Length; i++)
				AddType (gi.Types [i]);

			var gId = id++;

			WriteRecord (RecordType.GINST, gId);
			WriteInt32 (gi.Types.Length);

			for (int i = 0; i < gi.Types.Length; i++)
				WriteInt32 (AddType (gi.Types [i]));

			return gId;
		}

		void WriteMethod (MethodRecord m)
		{
			var typeId = AddType (m.Type);

			int instId = -1;
			if (m.GenericInst != null)
				instId = AddGenericInstance (m.GenericInst);

			WriteRecord (RecordType.METHOD, id++);
			WriteInt32 (typeId);
			WriteInt32 (instId);
			WriteInt32 (m.ParamCount);
			WriteString (m.Name);
			WriteString (m.Signature);
		}

		void WriteRecord (RecordType rt, int value)
		{
			stream.WriteByte ((byte)rt);
			WriteInt32 (value);
		}

		public void WriteAllData (Stream s, ProfileData pd)
		{
			stream = s;
			data = pd;

			var buf = Encoding.UTF8.GetBytes (MAGIC);
			this.stream.Write (buf, 0, buf.Length);

			WriteInt32 ((MAJOR_VERSION << 16) | MINOR_VERSION);

			foreach (var m in data.Methods)
				WriteMethod (m);

			// make sure ew have all the types
			// sometime the profile contain type, which is not referenced from the methods
			foreach (var t in data.Types)
				AddType (t);

			// just to be complete, do not miss any module too
			foreach (var module in data.Modules)
				AddModule (module);

			WriteRecord (RecordType.NONE, 0);
		}
	}
}
