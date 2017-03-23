using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class ObjectMirror : Value, IInvocableMethodOwnerMirror {
		TypeMirror type;
		AppDomainMirror domain;
	
		internal ObjectMirror (VirtualMachine vm, long id) : base (vm, id) {
		}
	
		internal ObjectMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id) {
			this.type = type;
			this.domain = domain;
		}

		void GetInfo () {
			var info = vm.conn.Object_GetInfo (id);
			type = vm.GetType (info.type_id);
			domain = vm.GetDomain (info.domain_id);
		}

		public TypeMirror Type {
			get {
				if (type == null) {
					if (vm.conn.Version.AtLeast (2, 5))
						GetInfo ();
					else
				 		type = vm.GetType (vm.conn.Object_GetType (id));
				}
				return type;
			}
		}

		public AppDomainMirror Domain {
			get {
				if (domain == null) {
					if (vm.conn.Version.AtLeast (2, 5))
						GetInfo ();
					else
						domain = vm.GetDomain (vm.conn.Object_GetDomain (id));
				}
				return domain;
			}
		}

		public bool IsCollected {
			get {
				return vm.conn.Object_IsCollected (id);
			}
		}

		public Value GetValue (FieldInfoMirror field) {
			return GetValues (new FieldInfoMirror [] { field }) [0];
		}

		public Value[] GetValues (IList<FieldInfoMirror> fields) {
			if (fields == null)
				throw new ArgumentNullException ("fields");
			foreach (FieldInfoMirror f in fields) {
				if (f == null)
					throw new ArgumentNullException ("field");
				CheckMirror (f);
			}
			long[] ids = new long [fields.Count];
			for (int i = 0; i < fields.Count; ++i)
				ids [i] = fields [i].Id;
			try {
				return vm.DecodeValues (vm.conn.Object_GetValues (id, ids));
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID) {
					if (fields.Count == 1)
						throw new ArgumentException (string.Format ("The field '{0}' is not valid for this type.", fields[0].Name));
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				} else
					throw;
			}
		}

		public void SetValues (IList<FieldInfoMirror> fields, Value[] values) {
			if (fields == null)
				throw new ArgumentNullException ("fields");
			if (values == null)
				throw new ArgumentNullException ("values");
			foreach (FieldInfoMirror f in fields) {
				if (f == null)
					throw new ArgumentNullException ("field");
				CheckMirror (f);
			}
			foreach (Value v in values) {
				if (v == null)
					throw new ArgumentNullException ("values");
				CheckMirror (v);
			}
			long[] ids = new long [fields.Count];
			for (int i = 0; i < fields.Count; ++i)
				ids [i] = fields [i].Id;
			try {
				vm.conn.Object_SetValues (id, ids, vm.EncodeValues (values));
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID)
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				else if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("One of the values is not valid for its field.", "values");
				else
					throw;
			}
		}

		public void SetValue (FieldInfoMirror field, Value value) {
			SetValues (new FieldInfoMirror [] { field }, new Value [] { value });
		}

		/*
		 * The current address of the object. It can change during garbage 
		 * collections. Use a long since the debuggee might have a different 
		 * pointer size. 
		 */
		public long Address {
			get {
				return vm.conn.Object_GetAddress (id);
			}
		}

		Value IInvocableMethodOwnerMirror.GetThisObject () {
			return this;
		}

		void IInvocableMethodOwnerMirror.ProcessResult (IInvokeResult result)
		{
		}
	}
}
