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
		
		static InstrTable ()
		{
			CreateOpTable ();
		}
		
		public static ILToken GetToken (string str)
		{
			if (IsOp (str)) {
				Op op = GetOp (str);
				return new ILToken (Token.INSTR_NONE, op);
			}

			return null;
		}

		public static bool IsOp (string str)
		{
			return op_table.Contains (str);
		}
		
		public static Op GetOp (string str)
		{
			return (Op) op_table[str];
		}

		private static void CreateOpTable ()
		{
			op_table = new Hashtable ();

			op_table["nop"] = Op.nop;
			op_table["breakOp"] = Op.breakOp;
			op_table["ldarg_0"] = Op.ldarg_0;
			op_table["ldarg_1"] = Op.ldarg_1;
			op_table["ldarg_2"] = Op.ldarg_2;
			op_table["ldarg_3"] = Op.ldarg_3;
			op_table["ldloc_0"] = Op.ldloc_0;
			op_table["ldloc_1"] = Op.ldloc_1;
			op_table["ldloc_2"] = Op.ldloc_2;
			op_table["ldloc_3"] = Op.ldloc_3;
			op_table["stloc_0"] = Op.stloc_0;
			op_table["stloc_1"] = Op.stloc_1;
			op_table["stloc_2"] = Op.stloc_2;
			op_table["stloc_3"] = Op.stloc_3;
			op_table["ldnull"] = Op.ldnull;
			op_table["ldc_i4_m1"] = Op.ldc_i4_m1;
			op_table["ldc_i4_0"] = Op.ldc_i4_0;
			op_table["ldc_i4_1"] = Op.ldc_i4_1;
			op_table["ldc_i4_2"] = Op.ldc_i4_2;
			op_table["ldc_i4_3"] = Op.ldc_i4_3;
			op_table["ldc_i4_4"] = Op.ldc_i4_4;
			op_table["ldc_i4_5"] = Op.ldc_i4_5;
			op_table["ldc_i4_6"] = Op.ldc_i4_6;
			op_table["ldc_i4_7"] = Op.ldc_i4_7;
			op_table["ldc_i4_8"] = Op.ldc_i4_8;
			op_table["dup"] = Op.dup;
			op_table["pop"] = Op.pop;
			op_table["ret"] = Op.ret;
			op_table["ldind_i1"] = Op.ldind_i1;
			op_table["ldind_u1"] = Op.ldind_u1;
			op_table["ldind_i2"] = Op.ldind_i2;
			op_table["ldind_u2"] = Op.ldind_u2;
			op_table["ldind_i4"] = Op.ldind_i4;
			op_table["ldind_u4"] = Op.ldind_u4;
			op_table["ldind_i8"] = Op.ldind_i8;
			op_table["ldind_i"] = Op.ldind_i;
			op_table["ldind_r4"] = Op.ldind_r4;
			op_table["ldind_r8"] = Op.ldind_r8;
			op_table["ldind_ref"] = Op.ldind_ref;
			op_table["stind_ref"] = Op.stind_ref;
			op_table["stind_i1"] = Op.stind_i1;
			op_table["stind_i2"] = Op.stind_i2;
			op_table["stind_i4"] = Op.stind_i4;
			op_table["stind_i8"] = Op.stind_i8;
			op_table["stind_r4"] = Op.stind_r4;
			op_table["stind_r8"] = Op.stind_r8;
			op_table["add"] = Op.add;
			op_table["sub"] = Op.sub;
			op_table["mul"] = Op.mul;
			op_table["div"] = Op.div;
			op_table["div_un"] = Op.div_un;
			op_table["rem"] = Op.rem;
			op_table["rem_un"] = Op.rem_un;
			op_table["and"] = Op.and;
			op_table["or"] = Op.or;
			op_table["xor"] = Op.xor;
			op_table["shl"] = Op.shl;
			op_table["shr"] = Op.shr;
			op_table["shr_un"] = Op.shr_un;
			op_table["neg"] = Op.neg;
			op_table["not"] = Op.not;
			op_table["conv_i1"] = Op.conv_i1;
			op_table["conv_i2"] = Op.conv_i2;
			op_table["conv_i4"] = Op.conv_i4;
			op_table["conv_i8"] = Op.conv_i8;
			op_table["conv_r4"] = Op.conv_r4;
			op_table["conv_r8"] = Op.conv_r8;
			op_table["conv_u4"] = Op.conv_u4;
			op_table["conv_u8"] = Op.conv_u8;
			op_table["conv_r_un"] = Op.conv_r_un;
			op_table["throwOp"] = Op.throwOp;
			op_table["conv_ovf_i1_un"] = Op.conv_ovf_i1_un;
			op_table["conv_ovf_i2_un"] = Op.conv_ovf_i2_un;
			op_table["conv_ovf_i4_un"] = Op.conv_ovf_i4_un;
			op_table["conv_ovf_i8_un"] = Op.conv_ovf_i8_un;
			op_table["conf_ovf_u1_un"] = Op.conf_ovf_u1_un;
			op_table["conv_ovf_u2_un"] = Op.conv_ovf_u2_un;
			op_table["conv_ovf_u4_un"] = Op.conv_ovf_u4_un;
			op_table["conv_ovf_u8_un"] = Op.conv_ovf_u8_un;
			op_table["conv_ovf_i_un"] = Op.conv_ovf_i_un;
			op_table["conv_ovf_u_un"] = Op.conv_ovf_u_un;
			op_table["ldlen"] = Op.ldlen;
			op_table["ldelem_i1"] = Op.ldelem_i1;
			op_table["ldelem_u1"] = Op.ldelem_u1;
			op_table["ldelem_i2"] = Op.ldelem_i2;
			op_table["ldelem_u2"] = Op.ldelem_u2;
			op_table["ldelem_i4"] = Op.ldelem_i4;
			op_table["ldelem_u4"] = Op.ldelem_u4;
			op_table["ldelem_i8"] = Op.ldelem_i8;
			op_table["ldelem_i"] = Op.ldelem_i;
			op_table["ldelem_r4"] = Op.ldelem_r4;
			op_table["ldelem_r8"] = Op.ldelem_r8;
			op_table["ldelem_ref"] = Op.ldelem_ref;
			op_table["stelem_i"] = Op.stelem_i;
			op_table["stelem_i1"] = Op.stelem_i1;
			op_table["stelem_i2"] = Op.stelem_i2;
			op_table["stelem_i4"] = Op.stelem_i4;
			op_table["stelem_i8"] = Op.stelem_i8;
			op_table["stelem_ref"] = Op.stelem_ref;
			op_table["conv_ovf_i1"] = Op.conv_ovf_i1;
			op_table["conv_ovf_u1"] = Op.conv_ovf_u1;
			op_table["conv_ovf_i2"] = Op.conv_ovf_i2;
			op_table["conv_ovf_u2"] = Op.conv_ovf_u2;
			op_table["conv_ovf_i4"] = Op.conv_ovf_i4;
			op_table["conv_ovf_u4"] = Op.conv_ovf_u4;
			op_table["conv_ovf_i8"] = Op.conv_ovf_i8;
			op_table["conv_ovf_u8"] = Op.conv_ovf_u8;
			op_table["ckfinite"] = Op.ckfinite;
			op_table["conv_u2"] = Op.conv_u2;
			op_table["conv_u1"] = Op.conv_u1;
			op_table["conv_i"] = Op.conv_i;
			op_table["conv_ovf_i"] = Op.conv_ovf_i;
			op_table["conv_ovf_u"] = Op.conv_ovf_u;
			op_table["add_ovf"] = Op.add_ovf;
			op_table["add_ovf_un"] = Op.add_ovf_un;
			op_table["mul_ovf"] = Op.mul_ovf;
			op_table["mul_ovf_un"] = Op.mul_ovf_un;
			op_table["sub_ovf"] = Op.sub_ovf;
			op_table["sub_ovf_un"] = Op.sub_ovf_un;
			op_table["endfinally"] = Op.endfinally;
			op_table["stind_i"] = Op.stind_i;
			op_table["conv_u"] = Op.conv_u;
			op_table["arglist"] = Op.arglist;
			op_table["ceq"] = Op.ceq;
			op_table["cgt"] = Op.cgt;
			op_table["cgt_un"] = Op.cgt_un;
			op_table["clt"] = Op.clt;
			op_table["clt_un"] = Op.clt_un;
			op_table["localloc"] = Op.localloc;
			op_table["endfilter"] = Op.endfilter;
			op_table["volatile_"] = Op.volatile_;
			op_table["tail_"] = Op.tail_;
			op_table["cpblk"] = Op.cpblk;
			op_table["initblk"] = Op.initblk;
			op_table["rethrow"] = Op.rethrow;
			op_table["refanytype"] = Op.refanytype;

		}

	}

}

