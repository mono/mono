//
// Mono.ILASM.InstrTable
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;
 
namespace Mono.ILASM {

	public class InstrTable {

		private static Hashtable op_table;
		private static Hashtable int_table;		
		private static Hashtable type_table;

		static InstrTable ()
		{
			CreateOpTable ();
			CreateIntTable ();
			CreateTypeTable ();
		}
		
		public static ILToken GetToken (string str)
		{
			if (IsOp (str)) {
				Op op = GetOp (str);
				return new ILToken (Token.INSTR_NONE, op);
			} else if (IsIntOp (str)) {
				IntOp op = GetIntOp (str);
				return new ILToken (Token.INSTR_I, op);
			} else if (IsTypeOp (str)) {
				TypeOp op = GetTypeOp (str);
				return new ILToken (Token.INSTR_TYPE, op);
			}

			return null;
		}

		public static bool IsInstr (string str)
		{
			return (IsOp (str) || IsIntOp (str) || IsTypeOp (str));
		}

		public static bool IsOp (string str)
		{
			return op_table.Contains (str);
		}
		
		public static bool IsIntOp (string str)
		{
			return int_table.Contains (str);
		}

		public static bool IsTypeOp (string str) 
		{
			return type_table.Contains (str);
		}

		public static Op GetOp (string str)
		{
			return (Op) op_table[str];
		}

		public static IntOp GetIntOp (string str)
		{
			return (IntOp) int_table[str];
		}

		public static TypeOp GetTypeOp (string str)
		{
			return (TypeOp) type_table[str];
		}

		private static void CreateOpTable ()
		{
			op_table = new Hashtable ();

			op_table["nop"] = Op.nop;
			op_table["break"] = Op.breakOp;
			op_table["ldarg.0"] = Op.ldarg_0;
			op_table["ldarg.1"] = Op.ldarg_1;
			op_table["ldarg.2"] = Op.ldarg_2;
			op_table["ldarg.3"] = Op.ldarg_3;
			op_table["ldloc.0"] = Op.ldloc_0;
			op_table["ldloc.1"] = Op.ldloc_1;
			op_table["ldloc.2"] = Op.ldloc_2;
			op_table["ldloc.3"] = Op.ldloc_3;
			op_table["stloc.0"] = Op.stloc_0;
			op_table["stloc.1"] = Op.stloc_1;
			op_table["stloc.2"] = Op.stloc_2;
			op_table["stloc.3"] = Op.stloc_3;
			op_table["ldnull"] = Op.ldnull;
			op_table["ldc.i4.m1"] = Op.ldc_i4_m1;
			op_table["ldc.i4.0"] = Op.ldc_i4_0;
			op_table["ldc.i4.1"] = Op.ldc_i4_1;
			op_table["ldc.i4.2"] = Op.ldc_i4_2;
			op_table["ldc.i4.3"] = Op.ldc_i4_3;
			op_table["ldc.i4.4"] = Op.ldc_i4_4;
			op_table["ldc.i4.5"] = Op.ldc_i4_5;
			op_table["ldc.i4.6"] = Op.ldc_i4_6;
			op_table["ldc.i4.7"] = Op.ldc_i4_7;
			op_table["ldc.i4.8"] = Op.ldc_i4_8;
			op_table["dup"] = Op.dup;
			op_table["pop"] = Op.pop;
			op_table["ret"] = Op.ret;
			op_table["ldind.i1"] = Op.ldind_i1;
			op_table["ldind.u1"] = Op.ldind_u1;
			op_table["ldind.i2"] = Op.ldind_i2;
			op_table["ldind.u2"] = Op.ldind_u2;
			op_table["ldind.i4"] = Op.ldind_i4;
			op_table["ldind.u4"] = Op.ldind_u4;
			op_table["ldind.i8"] = Op.ldind_i8;
			op_table["ldind.i"] = Op.ldind_i;
			op_table["ldind.r4"] = Op.ldind_r4;
			op_table["ldind.r8"] = Op.ldind_r8;
			op_table["ldind.ref"] = Op.ldind_ref;
			op_table["stind.ref"] = Op.stind_ref;
			op_table["stind.i1"] = Op.stind_i1;
			op_table["stind.i2"] = Op.stind_i2;
			op_table["stind.i4"] = Op.stind_i4;
			op_table["stind.i8"] = Op.stind_i8;
			op_table["stind.r4"] = Op.stind_r4;
			op_table["stind.r8"] = Op.stind_r8;
			op_table["add"] = Op.add;
			op_table["sub"] = Op.sub;
			op_table["mul"] = Op.mul;
			op_table["div"] = Op.div;
			op_table["div.un"] = Op.div_un;
			op_table["rem"] = Op.rem;
			op_table["rem.un"] = Op.rem_un;
			op_table["and"] = Op.and;
			op_table["or"] = Op.or;
			op_table["xor"] = Op.xor;
			op_table["shl"] = Op.shl;
			op_table["shr"] = Op.shr;
			op_table["shr.un"] = Op.shr_un;
			op_table["neg"] = Op.neg;
			op_table["not"] = Op.not;
			op_table["conv.i1"] = Op.conv_i1;
			op_table["conv.i2"] = Op.conv_i2;
			op_table["conv.i4"] = Op.conv_i4;
			op_table["conv.i8"] = Op.conv_i8;
			op_table["conv.r4"] = Op.conv_r4;
			op_table["conv.r8"] = Op.conv_r8;
			op_table["conv.u4"] = Op.conv_u4;
			op_table["conv.u8"] = Op.conv_u8;
			op_table["conv.r.un"] = Op.conv_r_un;
			op_table["throwOp"] = Op.throwOp;
			op_table["conv.ovf.i1.un"] = Op.conv_ovf_i1_un;
			op_table["conv.ovf.i2.un"] = Op.conv_ovf_i2_un;
			op_table["conv.ovf.i4.un"] = Op.conv_ovf_i4_un;
			op_table["conv.ovf.i8.un"] = Op.conv_ovf_i8_un;
			op_table["conf.ovf.u1.un"] = Op.conf_ovf_u1_un;
			op_table["conv.ovf.u2.un"] = Op.conv_ovf_u2_un;
			op_table["conv.ovf.u4.un"] = Op.conv_ovf_u4_un;
			op_table["conv.ovf.u8.un"] = Op.conv_ovf_u8_un;
			op_table["conv.ovf.i.un"] = Op.conv_ovf_i_un;
			op_table["conv.ovf.u.un"] = Op.conv_ovf_u_un;
			op_table["ldlen"] = Op.ldlen;
			op_table["ldelem.i1"] = Op.ldelem_i1;
			op_table["ldelem.u1"] = Op.ldelem_u1;
			op_table["ldelem.i2"] = Op.ldelem_i2;
			op_table["ldelem.u2"] = Op.ldelem_u2;
			op_table["ldelem.i4"] = Op.ldelem_i4;
			op_table["ldelem.u4"] = Op.ldelem_u4;
			op_table["ldelem.i8"] = Op.ldelem_i8;
			op_table["ldelem.i"] = Op.ldelem_i;
			op_table["ldelem.r4"] = Op.ldelem_r4;
			op_table["ldelem.r8"] = Op.ldelem_r8;
			op_table["ldelem.ref"] = Op.ldelem_ref;
			op_table["stelem.i"] = Op.stelem_i;
			op_table["stelem.i1"] = Op.stelem_i1;
			op_table["stelem.i2"] = Op.stelem_i2;
			op_table["stelem.i4"] = Op.stelem_i4;
			op_table["stelem.i8"] = Op.stelem_i8;
			op_table["stelem.ref"] = Op.stelem_ref;
			op_table["conv.ovf.i1"] = Op.conv_ovf_i1;
			op_table["conv.ovf.u1"] = Op.conv_ovf_u1;
			op_table["conv.ovf.i2"] = Op.conv_ovf_i2;
			op_table["conv.ovf.u2"] = Op.conv_ovf_u2;
			op_table["conv.ovf.i4"] = Op.conv_ovf_i4;
			op_table["conv.ovf.u4"] = Op.conv_ovf_u4;
			op_table["conv.ovf.i8"] = Op.conv_ovf_i8;
			op_table["conv.ovf.u8"] = Op.conv_ovf_u8;
			op_table["ckfinite"] = Op.ckfinite;
			op_table["conv.u2"] = Op.conv_u2;
			op_table["conv.u1"] = Op.conv_u1;
			op_table["conv.i"] = Op.conv_i;
			op_table["conv.ovf.i"] = Op.conv_ovf_i;
			op_table["conv.ovf.u"] = Op.conv_ovf_u;
			op_table["add.ovf"] = Op.add_ovf;
			op_table["add.ovf.un"] = Op.add_ovf_un;
			op_table["mul.ovf"] = Op.mul_ovf;
			op_table["mul.ovf.un"] = Op.mul_ovf_un;
			op_table["sub.ovf"] = Op.sub_ovf;
			op_table["sub.ovf.un"] = Op.sub_ovf_un;
			op_table["endfinally"] = Op.endfinally;
			op_table["stind.i"] = Op.stind_i;
			op_table["conv.u"] = Op.conv_u;
			op_table["arglist"] = Op.arglist;
			op_table["ceq"] = Op.ceq;
			op_table["cgt"] = Op.cgt;
			op_table["cgt.un"] = Op.cgt_un;
			op_table["clt"] = Op.clt;
			op_table["clt.un"] = Op.clt_un;
			op_table["localloc"] = Op.localloc;
			op_table["endfilter"] = Op.endfilter;
			op_table["volatile."] = Op.volatile_;
			op_table["tail."] = Op.tail_;
			op_table["cpblk"] = Op.cpblk;
			op_table["initblk"] = Op.initblk;
			op_table["rethrow"] = Op.rethrow;
			op_table["refanytype"] = Op.refanytype;

		}

		private static void CreateIntTable ()
		{
			int_table = new Hashtable ();
			
			int_table["ldarg.s"] = IntOp.ldarg_s;
			int_table["ldarga.s"] = IntOp.ldarga_s;
			int_table["starg.s"] = IntOp.starg_s;
			int_table["ldloc.s"] = IntOp.ldloc_s;
			int_table["ldloca.s"] = IntOp.ldloca_s;
			int_table["stloc.s"] = IntOp.stloc_s;
			int_table["ldc_i4.s"] = IntOp.ldc_i4_s;
			int_table["ldc_i4"] = IntOp.ldc_i4;
			int_table["ldarg"] = IntOp.ldarg;
			int_table["ldarga"] = IntOp.ldarga;
			int_table["starf"] = IntOp.starg;
			int_table["ldloc"] = IntOp.ldloc;
			int_table["ldloca"] = IntOp.ldloca;
			int_table["stloc"] = IntOp.stloc;
			int_table["unaligned"] =  IntOp.unaligned;
		}

		public static void CreateTypeTable ()
		{
			type_table = new Hashtable ();
			
			type_table["cpobj"] = TypeOp.cpobj;
			type_table["ldobj"] = TypeOp.ldobj;
			type_table["castclass"] = TypeOp.castclass;
			type_table["isinst"] = TypeOp.isinst;
			type_table["unbox"] = TypeOp.unbox;
			type_table["stobj"] = TypeOp.stobj;
			type_table["box"] = TypeOp.box;
			type_table["newarr"] = TypeOp.newarr;
			type_table["ldelema"] = TypeOp.ldelema;
			type_table["refanyval"] = TypeOp.refanyval;
			type_table["mkrefany"] = TypeOp.mkrefany;
			type_table["ldtoken"] = TypeOp.ldtoken;
			type_table["initobj"] = TypeOp.initobj;
			type_table["sizeof"] = TypeOp.sizeOf;
		}
	}

}

