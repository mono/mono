//
// SqlEditor.cs
//
// Need to port the gtk+ widget in gnome-db-sql-editor.c to this file SqlEditor.cs
// to be completely in GTK# and C#.
//
// Modified from the GTK# generated SqlEditor class found in GnomeDb# which
// is C# bindings to gnome-db-sql-editor in libgnomedb
// 
// Authors:
//     Daniel Morgan <danmorg@sc.rr.com>
//     Mike Kestner
//     Rodrigo Moya <rodrigo@gnome-db.org>
//
// (c)copyright 2002 Mike Kestner
// (c)copyright 2002 Daniel Morgan
// (c)copyright 2002 Rodrigo Moya
//
// This file falls under the X11 license.
//

namespace SqlEditorSharp {

	using System;
	using Gtk;
	using Gdk;
	using Glib;
	using System.Collections;
	using System.Runtime.InteropServices;

		/// <summary> SqlEditor Class</summary>
		/// <remarks>
		/// </remarks>
	public class SqlEditor : TextView {

		~SqlEditor()
		{
			//Dispose();
		}

		public SqlEditor(IntPtr raw) : base(raw) {}
		
		[DllImport("sqleditor")]
		static extern IntPtr gnome_db_sql_editor_new();

		/// <summary> SqlEditor Constructor </summary>
		/// <remarks> To be completed </remarks>
		public SqlEditor()
		{
			Raw = gnome_db_sql_editor_new();
		}

		private Hashtable Signals = new Hashtable();
		[DllImport("sqleditor")]
		static extern void gnome_db_sql_editor_use_syntax_hi_lighting(IntPtr raw, bool hi_lighting);

		/// <summary> UseSyntaxHiLighting Method </summary>
		/// <remarks> To be completed </remarks>
		public void UseSyntaxHiLighting(bool hi_lighting) {
			gnome_db_sql_editor_use_syntax_hi_lighting(Handle, hi_lighting);
		}

		[DllImport("sqleditor")]
		static extern void gnome_db_sql_editor_set_editable(IntPtr raw, bool setting);

		/// <summary> Editable Property </summary>
		/// <remarks> To be completed </remarks>
		public new bool Editable { 
			set {
				gnome_db_sql_editor_set_editable(Handle, value);
			}
		}

		[DllImport("sqleditor")]
		static extern bool gnome_db_sql_editor_save_to_file(IntPtr raw, string filename);

		/// <summary> SaveToFile Method </summary>
		/// <remarks> To be completed </remarks>
		public bool SaveToFile(string filename) {
			bool raw_ret = gnome_db_sql_editor_save_to_file(Handle, filename);
			bool ret = raw_ret;
			return ret;
		}

		//[DllImport("sqleditor")]
		//static extern bool gnome_db_sql_editor_foreach_command(IntPtr raw, bool run_at_pref, GtkSharp.GnomeDbSqlEditorRunFuncNative run_command, System.IntPtr user_data);

		/// <summary> ForeachCommand Method </summary>
		/// <remarks> To be completed </remarks>
		//public bool ForeachCommand(bool run_at_pref, GnomeDb.SqlEditorRunFunc run_command) {
		//	GtkSharp.GnomeDbSqlEditorRunFuncWrapper run_command_wrapper = null;
			//run_command_wrapper = new GtkSharp.GnomeDbSqlEditorRunFuncWrapper (run_command);
		//	bool raw_ret = gnome_db_sql_editor_foreach_command(Handle, run_at_pref, run_command_wrapper.NativeDelegate, IntPtr.Zero);
		//	bool ret = raw_ret;
		//	return ret;
		//}

		[DllImport("sqleditor")]
		static extern int gnome_db_sql_editor_get_type();

		/// <summary> GType Property </summary>
		/// <remarks> To be completed </remarks>
		public static new int GType { 
			get {
				int raw_ret = gnome_db_sql_editor_get_type();
				int ret = raw_ret;
				return ret;
			}
		}

		public TextBuffer SqlBuffer {
			get {
				TextBuffer sqlBuff;
				IntPtr gtkTextBuffer;
				gtkTextBuffer = gnome_db_sql_editor_get_text_buffer(Raw);
				sqlBuff = new TextBuffer(gtkTextBuffer);
				return sqlBuff;
			}
		}

		[DllImport("sqleditor")]
		static extern IntPtr gnome_db_sql_editor_get_text_buffer(IntPtr raw);

		//[DllImport("sqleditor")]
		//static extern string gnome_db_sql_editor_get_all_text(IntPtr raw);

		/// <summary> AllText Property </summary>
		/// <remarks> To be completed </remarks>
		//public string AllText { 
		//	get {
		//		string raw_ret = gnome_db_sql_editor_get_all_text(Raw);
		//		string ret = raw_ret;
		//		return ret;
		//	}
		//}

		[DllImport("sqleditor")]
		static extern IntPtr gnome_db_sql_editor_get_all_commands(IntPtr raw, bool run_at_pref);

		/// <summary> GetAllCommands Method </summary>
		/// <remarks> To be completed </remarks>
		public GLib.List GetAllCommands(bool run_at_pref) {
			IntPtr raw_ret = gnome_db_sql_editor_get_all_commands(Handle, run_at_pref);
			GLib.List ret = new GLib.List (raw_ret);
			return ret;
		}

		[DllImport("sqleditor")]
		static extern IntPtr gnome_db_sql_editor_get_command_at_cursor(IntPtr raw);

		/// <summary> CommandAtCursor Property </summary>
		/// <remarks> To be completed </remarks>
		/*
		public GnomeDb.SqlEditorCommand CommandAtCursor { 
			get {
				IntPtr raw_ret = gnome_db_sql_editor_get_command_at_cursor(Handle);
				//GnomeDb.SqlEditorCommand ret = GnomeDb.SqlEditorCommand.New (raw_ret);
				return ret;
			}
		}
		*/

		[DllImport("sqleditor")]
		static extern bool gnome_db_sql_editor_load_from_file(IntPtr raw, string filename);

		/// <summary> LoadFromFile Method </summary>
		/// <remarks> To be completed </remarks>
		public bool LoadFromFile(string filename) {
			bool raw_ret = gnome_db_sql_editor_load_from_file(Handle, filename);
			bool ret = raw_ret;
			return ret;
		}

		[DllImport("sqleditor")]
		static extern void gnome_db_sql_editor_set_text(IntPtr raw, string text, int len);

		/// <summary> SetText Method </summary>
		/// <remarks> To be completed </remarks>
		public void SetText(string text, int len) {
			gnome_db_sql_editor_set_text(Handle, text, len);
		}

		[DllImport("sqleditor")]
		static extern void gnome_db_sql_editor_debug(IntPtr raw, string text);

		/// <summary> Debug Method </summary>
		/// <remarks> To be completed </remarks>
		public void Debug(string text) {
			gnome_db_sql_editor_debug(Handle, text);
		}

	}

}
