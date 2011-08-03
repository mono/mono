/*
  Copyright (C) 2008 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;

namespace IKVM.Reflection.Emit
{
	public sealed class OpCodes
	{
		public static readonly OpCode Nop = new OpCode(4888);
		public static readonly OpCode Break = new OpCode(4199116);
		public static readonly OpCode Ldarg_0 = new OpCode(8492847);
		public static readonly OpCode Ldarg_1 = new OpCode(12687151);
		public static readonly OpCode Ldarg_2 = new OpCode(16881455);
		public static readonly OpCode Ldarg_3 = new OpCode(21075759);
		public static readonly OpCode Ldloc_0 = new OpCode(25270063);
		public static readonly OpCode Ldloc_1 = new OpCode(29464367);
		public static readonly OpCode Ldloc_2 = new OpCode(33658671);
		public static readonly OpCode Ldloc_3 = new OpCode(37852975);
		public static readonly OpCode Stloc_0 = new OpCode(41949467);
		public static readonly OpCode Stloc_1 = new OpCode(46143771);
		public static readonly OpCode Stloc_2 = new OpCode(50338075);
		public static readonly OpCode Stloc_3 = new OpCode(54532379);
		public static readonly OpCode Ldarg_S = new OpCode(58824508);
		public static readonly OpCode Ldarga_S = new OpCode(63224012);
		public static readonly OpCode Starg_S = new OpCode(67115304);
		public static readonly OpCode Ldloc_S = new OpCode(71407420);
		public static readonly OpCode Ldloca_S = new OpCode(75806924);
		public static readonly OpCode Stloc_S = new OpCode(79698216);
		public static readonly OpCode Ldnull = new OpCode(84609339);
		public static readonly OpCode Ldc_I4_M1 = new OpCode(88389823);
		public static readonly OpCode Ldc_I4_0 = new OpCode(92584127);
		public static readonly OpCode Ldc_I4_1 = new OpCode(96778431);
		public static readonly OpCode Ldc_I4_2 = new OpCode(100972735);
		public static readonly OpCode Ldc_I4_3 = new OpCode(105167039);
		public static readonly OpCode Ldc_I4_4 = new OpCode(109361343);
		public static readonly OpCode Ldc_I4_5 = new OpCode(113555647);
		public static readonly OpCode Ldc_I4_6 = new OpCode(117749951);
		public static readonly OpCode Ldc_I4_7 = new OpCode(121944255);
		public static readonly OpCode Ldc_I4_8 = new OpCode(126138559);
		public static readonly OpCode Ldc_I4_S = new OpCode(130332874);
		public static readonly OpCode Ldc_I4 = new OpCode(134530584);
		public static readonly OpCode Ldc_I8 = new OpCode(138827489);
		public static readonly OpCode Ldc_R4 = new OpCode(143124407);
		public static readonly OpCode Ldc_R8 = new OpCode(147421301);
		public static readonly OpCode Dup = new OpCode(155404637);
		public static readonly OpCode Pop = new OpCode(159393399);
		public static readonly OpCode Jmp = new OpCode(163582686);
		public static readonly OpCode Call = new OpCode(168690130);
		public static readonly OpCode Calli = new OpCode(172884439);
		public static readonly OpCode Ret = new OpCode(176258034);
		public static readonly OpCode Br_S = new OpCode(180356455);
		public static readonly OpCode Brfalse_S = new OpCode(184566035);
		public static readonly OpCode Brtrue_S = new OpCode(188760339);
		public static readonly OpCode Beq_S = new OpCode(192949342);
		public static readonly OpCode Bge_S = new OpCode(197143646);
		public static readonly OpCode Bgt_S = new OpCode(201337950);
		public static readonly OpCode Ble_S = new OpCode(205532254);
		public static readonly OpCode Blt_S = new OpCode(209726558);
		public static readonly OpCode Bne_Un_S = new OpCode(213920862);
		public static readonly OpCode Bge_Un_S = new OpCode(218115166);
		public static readonly OpCode Bgt_Un_S = new OpCode(222309470);
		public static readonly OpCode Ble_Un_S = new OpCode(226503774);
		public static readonly OpCode Blt_Un_S = new OpCode(230698078);
		public static readonly OpCode Br = new OpCode(234885812);
		public static readonly OpCode Brfalse = new OpCode(239095392);
		public static readonly OpCode Brtrue = new OpCode(243289696);
		public static readonly OpCode Beq = new OpCode(247475279);
		public static readonly OpCode Bge = new OpCode(251669583);
		public static readonly OpCode Bgt = new OpCode(255863887);
		public static readonly OpCode Ble = new OpCode(260058191);
		public static readonly OpCode Blt = new OpCode(264252495);
		public static readonly OpCode Bne_Un = new OpCode(268446799);
		public static readonly OpCode Bge_Un = new OpCode(272641103);
		public static readonly OpCode Bgt_Un = new OpCode(276835407);
		public static readonly OpCode Ble_Un = new OpCode(281029711);
		public static readonly OpCode Blt_Un = new OpCode(285224015);
		public static readonly OpCode Switch = new OpCode(289427051);
		public static readonly OpCode Ldind_I1 = new OpCode(293929358);
		public static readonly OpCode Ldind_U1 = new OpCode(298123662);
		public static readonly OpCode Ldind_I2 = new OpCode(302317966);
		public static readonly OpCode Ldind_U2 = new OpCode(306512270);
		public static readonly OpCode Ldind_I4 = new OpCode(310706574);
		public static readonly OpCode Ldind_U4 = new OpCode(314900878);
		public static readonly OpCode Ldind_I8 = new OpCode(319197782);
		public static readonly OpCode Ldind_I = new OpCode(323289486);
		public static readonly OpCode Ldind_R4 = new OpCode(327688990);
		public static readonly OpCode Ldind_R8 = new OpCode(331985894);
		public static readonly OpCode Ldind_Ref = new OpCode(336282798);
		public static readonly OpCode Stind_Ref = new OpCode(339768820);
		public static readonly OpCode Stind_I1 = new OpCode(343963124);
		public static readonly OpCode Stind_I2 = new OpCode(348157428);
		public static readonly OpCode Stind_I4 = new OpCode(352351732);
		public static readonly OpCode Stind_I8 = new OpCode(356551166);
		public static readonly OpCode Stind_R4 = new OpCode(360755730);
		public static readonly OpCode Stind_R8 = new OpCode(364955164);
		public static readonly OpCode Add = new OpCode(369216329);
		public static readonly OpCode Sub = new OpCode(373410633);
		public static readonly OpCode Mul = new OpCode(377604937);
		public static readonly OpCode Div = new OpCode(381799241);
		public static readonly OpCode Div_Un = new OpCode(385993545);
		public static readonly OpCode Rem = new OpCode(390187849);
		public static readonly OpCode Rem_Un = new OpCode(394382153);
		public static readonly OpCode And = new OpCode(398576457);
		public static readonly OpCode Or = new OpCode(402770761);
		public static readonly OpCode Xor = new OpCode(406965065);
		public static readonly OpCode Shl = new OpCode(411159369);
		public static readonly OpCode Shr = new OpCode(415353673);
		public static readonly OpCode Shr_Un = new OpCode(419547977);
		public static readonly OpCode Neg = new OpCode(423737322);
		public static readonly OpCode Not = new OpCode(427931626);
		public static readonly OpCode Conv_I1 = new OpCode(432331130);
		public static readonly OpCode Conv_I2 = new OpCode(436525434);
		public static readonly OpCode Conv_I4 = new OpCode(440719738);
		public static readonly OpCode Conv_I8 = new OpCode(445016642);
		public static readonly OpCode Conv_R4 = new OpCode(449313546);
		public static readonly OpCode Conv_R8 = new OpCode(453610450);
		public static readonly OpCode Conv_U4 = new OpCode(457496954);
		public static readonly OpCode Conv_U8 = new OpCode(461793858);
		public static readonly OpCode Callvirt = new OpCode(466484004);
		public static readonly OpCode Cpobj = new OpCode(469790542);
		public static readonly OpCode Ldobj = new OpCode(474077528);
		public static readonly OpCode Ldstr = new OpCode(478872210);
		public static readonly OpCode Newobj = new OpCode(483158791);
		public static readonly OpCode Castclass = new OpCode(487311950);
		public static readonly OpCode Isinst = new OpCode(491095854);
		public static readonly OpCode Conv_R_Un = new OpCode(495553490);
		public static readonly OpCode Unbox = new OpCode(507874780);
		public static readonly OpCode Throw = new OpCode(511759452);
		public static readonly OpCode Ldfld = new OpCode(516056466);
		public static readonly OpCode Ldflda = new OpCode(520455970);
		public static readonly OpCode Stfld = new OpCode(524347262);
		public static readonly OpCode Ldsfld = new OpCode(528588249);
		public static readonly OpCode Ldsflda = new OpCode(532987753);
		public static readonly OpCode Stsfld = new OpCode(536879045);
		public static readonly OpCode Stobj = new OpCode(541090290);
		public static readonly OpCode Conv_Ovf_I1_Un = new OpCode(545577338);
		public static readonly OpCode Conv_Ovf_I2_Un = new OpCode(549771642);
		public static readonly OpCode Conv_Ovf_I4_Un = new OpCode(553965946);
		public static readonly OpCode Conv_Ovf_I8_Un = new OpCode(558262850);
		public static readonly OpCode Conv_Ovf_U1_Un = new OpCode(562354554);
		public static readonly OpCode Conv_Ovf_U2_Un = new OpCode(566548858);
		public static readonly OpCode Conv_Ovf_U4_Un = new OpCode(570743162);
		public static readonly OpCode Conv_Ovf_U8_Un = new OpCode(575040066);
		public static readonly OpCode Conv_Ovf_I_Un = new OpCode(579131770);
		public static readonly OpCode Conv_Ovf_U_Un = new OpCode(583326074);
		public static readonly OpCode Box = new OpCode(587930786);
		public static readonly OpCode Newarr = new OpCode(592133640);
		public static readonly OpCode Ldlen = new OpCode(595953446);
		public static readonly OpCode Ldelema = new OpCode(600157847);
		public static readonly OpCode Ldelem_I1 = new OpCode(604352143);
		public static readonly OpCode Ldelem_U1 = new OpCode(608546447);
		public static readonly OpCode Ldelem_I2 = new OpCode(612740751);
		public static readonly OpCode Ldelem_U2 = new OpCode(616935055);
		public static readonly OpCode Ldelem_I4 = new OpCode(621129359);
		public static readonly OpCode Ldelem_U4 = new OpCode(625323663);
		public static readonly OpCode Ldelem_I8 = new OpCode(629620567);
		public static readonly OpCode Ldelem_I = new OpCode(633712271);
		public static readonly OpCode Ldelem_R4 = new OpCode(638111775);
		public static readonly OpCode Ldelem_R8 = new OpCode(642408679);
		public static readonly OpCode Ldelem_Ref = new OpCode(646705583);
		public static readonly OpCode Stelem_I = new OpCode(650186475);
		public static readonly OpCode Stelem_I1 = new OpCode(654380779);
		public static readonly OpCode Stelem_I2 = new OpCode(658575083);
		public static readonly OpCode Stelem_I4 = new OpCode(662769387);
		public static readonly OpCode Stelem_I8 = new OpCode(666968821);
		public static readonly OpCode Stelem_R4 = new OpCode(671168255);
		public static readonly OpCode Stelem_R8 = new OpCode(675367689);
		public static readonly OpCode Stelem_Ref = new OpCode(679567123);
		public static readonly OpCode Ldelem = new OpCode(683838727);
		public static readonly OpCode Stelem = new OpCode(687965999);
		public static readonly OpCode Unbox_Any = new OpCode(692217246);
		public static readonly OpCode Conv_Ovf_I1 = new OpCode(751098234);
		public static readonly OpCode Conv_Ovf_U1 = new OpCode(755292538);
		public static readonly OpCode Conv_Ovf_I2 = new OpCode(759486842);
		public static readonly OpCode Conv_Ovf_U2 = new OpCode(763681146);
		public static readonly OpCode Conv_Ovf_I4 = new OpCode(767875450);
		public static readonly OpCode Conv_Ovf_U4 = new OpCode(772069754);
		public static readonly OpCode Conv_Ovf_I8 = new OpCode(776366658);
		public static readonly OpCode Conv_Ovf_U8 = new OpCode(780560962);
		public static readonly OpCode Refanyval = new OpCode(814012802);
		public static readonly OpCode Ckfinite = new OpCode(818514898);
		public static readonly OpCode Mkrefany = new OpCode(830595078);
		public static readonly OpCode Ldtoken = new OpCode(872728098);
		public static readonly OpCode Conv_U2 = new OpCode(876927354);
		public static readonly OpCode Conv_U1 = new OpCode(881121658);
		public static readonly OpCode Conv_I = new OpCode(885315962);
		public static readonly OpCode Conv_Ovf_I = new OpCode(889510266);
		public static readonly OpCode Conv_Ovf_U = new OpCode(893704570);
		public static readonly OpCode Add_Ovf = new OpCode(897698633);
		public static readonly OpCode Add_Ovf_Un = new OpCode(901892937);
		public static readonly OpCode Mul_Ovf = new OpCode(906087241);
		public static readonly OpCode Mul_Ovf_Un = new OpCode(910281545);
		public static readonly OpCode Sub_Ovf = new OpCode(914475849);
		public static readonly OpCode Sub_Ovf_Un = new OpCode(918670153);
		public static readonly OpCode Endfinally = new OpCode(922751806);
		public static readonly OpCode Leave = new OpCode(926945972);
		public static readonly OpCode Leave_S = new OpCode(931140291);
		public static readonly OpCode Stind_I = new OpCode(935359988);
		public static readonly OpCode Conv_U = new OpCode(939841914);
		public static readonly OpCode Prefix7 = new OpCode(1040189696);
		public static readonly OpCode Prefix6 = new OpCode(1044384000);
		public static readonly OpCode Prefix5 = new OpCode(1048578304);
		public static readonly OpCode Prefix4 = new OpCode(1052772608);
		public static readonly OpCode Prefix3 = new OpCode(1056966912);
		public static readonly OpCode Prefix2 = new OpCode(1061161216);
		public static readonly OpCode Prefix1 = new OpCode(1065355520);
		public static readonly OpCode Prefixref = new OpCode(1069549824);
		public static readonly OpCode Arglist = new OpCode(-2147170789);
		public static readonly OpCode Ceq = new OpCode(-2142966567);
		public static readonly OpCode Cgt = new OpCode(-2138772263);
		public static readonly OpCode Cgt_Un = new OpCode(-2134577959);
		public static readonly OpCode Clt = new OpCode(-2130383655);
		public static readonly OpCode Clt_Un = new OpCode(-2126189351);
		public static readonly OpCode Ldftn = new OpCode(-2122004966);
		public static readonly OpCode Ldvirtftn = new OpCode(-2117759533);
		public static readonly OpCode Ldarg = new OpCode(-2109627244);
		public static readonly OpCode Ldarga = new OpCode(-2105227740);
		public static readonly OpCode Starg = new OpCode(-2101336448);
		public static readonly OpCode Ldloc = new OpCode(-2097044332);
		public static readonly OpCode Ldloca = new OpCode(-2092644828);
		public static readonly OpCode Stloc = new OpCode(-2088753536);
		public static readonly OpCode Localloc = new OpCode(-2084241010);
		public static readonly OpCode Endfilter = new OpCode(-2076160335);
		public static readonly OpCode Unaligned = new OpCode(-2071982151);
		public static readonly OpCode Volatile = new OpCode(-2067787858);
		public static readonly OpCode Tailcall = new OpCode(-2063593554);
		public static readonly OpCode Initobj = new OpCode(-2059384859);
		public static readonly OpCode Constrained = new OpCode(-2055204938);
		public static readonly OpCode Cpblk = new OpCode(-2050974371);
		public static readonly OpCode Initblk = new OpCode(-2046780067);
		public static readonly OpCode Rethrow = new OpCode(-2038428509);
		public static readonly OpCode Sizeof = new OpCode(-2029730269);
		public static readonly OpCode Refanytype = new OpCode(-2025531014);
		public static readonly OpCode Readonly = new OpCode(-2021650514);

		internal static string GetName(int value)
		{
			switch (value)
			{
				case 0:
					return "nop";
				case 1:
					return "break";
				case 2:
					return "ldarg.0";
				case 3:
					return "ldarg.1";
				case 4:
					return "ldarg.2";
				case 5:
					return "ldarg.3";
				case 6:
					return "ldloc.0";
				case 7:
					return "ldloc.1";
				case 8:
					return "ldloc.2";
				case 9:
					return "ldloc.3";
				case 10:
					return "stloc.0";
				case 11:
					return "stloc.1";
				case 12:
					return "stloc.2";
				case 13:
					return "stloc.3";
				case 14:
					return "ldarg.s";
				case 15:
					return "ldarga.s";
				case 16:
					return "starg.s";
				case 17:
					return "ldloc.s";
				case 18:
					return "ldloca.s";
				case 19:
					return "stloc.s";
				case 20:
					return "ldnull";
				case 21:
					return "ldc.i4.m1";
				case 22:
					return "ldc.i4.0";
				case 23:
					return "ldc.i4.1";
				case 24:
					return "ldc.i4.2";
				case 25:
					return "ldc.i4.3";
				case 26:
					return "ldc.i4.4";
				case 27:
					return "ldc.i4.5";
				case 28:
					return "ldc.i4.6";
				case 29:
					return "ldc.i4.7";
				case 30:
					return "ldc.i4.8";
				case 31:
					return "ldc.i4.s";
				case 32:
					return "ldc.i4";
				case 33:
					return "ldc.i8";
				case 34:
					return "ldc.r4";
				case 35:
					return "ldc.r8";
				case 37:
					return "dup";
				case 38:
					return "pop";
				case 39:
					return "jmp";
				case 40:
					return "call";
				case 41:
					return "calli";
				case 42:
					return "ret";
				case 43:
					return "br.s";
				case 44:
					return "brfalse.s";
				case 45:
					return "brtrue.s";
				case 46:
					return "beq.s";
				case 47:
					return "bge.s";
				case 48:
					return "bgt.s";
				case 49:
					return "ble.s";
				case 50:
					return "blt.s";
				case 51:
					return "bne.un.s";
				case 52:
					return "bge.un.s";
				case 53:
					return "bgt.un.s";
				case 54:
					return "ble.un.s";
				case 55:
					return "blt.un.s";
				case 56:
					return "br";
				case 57:
					return "brfalse";
				case 58:
					return "brtrue";
				case 59:
					return "beq";
				case 60:
					return "bge";
				case 61:
					return "bgt";
				case 62:
					return "ble";
				case 63:
					return "blt";
				case 64:
					return "bne.un";
				case 65:
					return "bge.un";
				case 66:
					return "bgt.un";
				case 67:
					return "ble.un";
				case 68:
					return "blt.un";
				case 69:
					return "switch";
				case 70:
					return "ldind.i1";
				case 71:
					return "ldind.u1";
				case 72:
					return "ldind.i2";
				case 73:
					return "ldind.u2";
				case 74:
					return "ldind.i4";
				case 75:
					return "ldind.u4";
				case 76:
					return "ldind.i8";
				case 77:
					return "ldind.i";
				case 78:
					return "ldind.r4";
				case 79:
					return "ldind.r8";
				case 80:
					return "ldind.ref";
				case 81:
					return "stind.ref";
				case 82:
					return "stind.i1";
				case 83:
					return "stind.i2";
				case 84:
					return "stind.i4";
				case 85:
					return "stind.i8";
				case 86:
					return "stind.r4";
				case 87:
					return "stind.r8";
				case 88:
					return "add";
				case 89:
					return "sub";
				case 90:
					return "mul";
				case 91:
					return "div";
				case 92:
					return "div.un";
				case 93:
					return "rem";
				case 94:
					return "rem.un";
				case 95:
					return "and";
				case 96:
					return "or";
				case 97:
					return "xor";
				case 98:
					return "shl";
				case 99:
					return "shr";
				case 100:
					return "shr.un";
				case 101:
					return "neg";
				case 102:
					return "not";
				case 103:
					return "conv.i1";
				case 104:
					return "conv.i2";
				case 105:
					return "conv.i4";
				case 106:
					return "conv.i8";
				case 107:
					return "conv.r4";
				case 108:
					return "conv.r8";
				case 109:
					return "conv.u4";
				case 110:
					return "conv.u8";
				case 111:
					return "callvirt";
				case 112:
					return "cpobj";
				case 113:
					return "ldobj";
				case 114:
					return "ldstr";
				case 115:
					return "newobj";
				case 116:
					return "castclass";
				case 117:
					return "isinst";
				case 118:
					return "conv.r.un";
				case 121:
					return "unbox";
				case 122:
					return "throw";
				case 123:
					return "ldfld";
				case 124:
					return "ldflda";
				case 125:
					return "stfld";
				case 126:
					return "ldsfld";
				case 127:
					return "ldsflda";
				case 128:
					return "stsfld";
				case 129:
					return "stobj";
				case 130:
					return "conv.ovf.i1.un";
				case 131:
					return "conv.ovf.i2.un";
				case 132:
					return "conv.ovf.i4.un";
				case 133:
					return "conv.ovf.i8.un";
				case 134:
					return "conv.ovf.u1.un";
				case 135:
					return "conv.ovf.u2.un";
				case 136:
					return "conv.ovf.u4.un";
				case 137:
					return "conv.ovf.u8.un";
				case 138:
					return "conv.ovf.i.un";
				case 139:
					return "conv.ovf.u.un";
				case 140:
					return "box";
				case 141:
					return "newarr";
				case 142:
					return "ldlen";
				case 143:
					return "ldelema";
				case 144:
					return "ldelem.i1";
				case 145:
					return "ldelem.u1";
				case 146:
					return "ldelem.i2";
				case 147:
					return "ldelem.u2";
				case 148:
					return "ldelem.i4";
				case 149:
					return "ldelem.u4";
				case 150:
					return "ldelem.i8";
				case 151:
					return "ldelem.i";
				case 152:
					return "ldelem.r4";
				case 153:
					return "ldelem.r8";
				case 154:
					return "ldelem.ref";
				case 155:
					return "stelem.i";
				case 156:
					return "stelem.i1";
				case 157:
					return "stelem.i2";
				case 158:
					return "stelem.i4";
				case 159:
					return "stelem.i8";
				case 160:
					return "stelem.r4";
				case 161:
					return "stelem.r8";
				case 162:
					return "stelem.ref";
				case 163:
					return "ldelem";
				case 164:
					return "stelem";
				case 165:
					return "unbox.any";
				case 179:
					return "conv.ovf.i1";
				case 180:
					return "conv.ovf.u1";
				case 181:
					return "conv.ovf.i2";
				case 182:
					return "conv.ovf.u2";
				case 183:
					return "conv.ovf.i4";
				case 184:
					return "conv.ovf.u4";
				case 185:
					return "conv.ovf.i8";
				case 186:
					return "conv.ovf.u8";
				case 194:
					return "refanyval";
				case 195:
					return "ckfinite";
				case 198:
					return "mkrefany";
				case 208:
					return "ldtoken";
				case 209:
					return "conv.u2";
				case 210:
					return "conv.u1";
				case 211:
					return "conv.i";
				case 212:
					return "conv.ovf.i";
				case 213:
					return "conv.ovf.u";
				case 214:
					return "add.ovf";
				case 215:
					return "add.ovf.un";
				case 216:
					return "mul.ovf";
				case 217:
					return "mul.ovf.un";
				case 218:
					return "sub.ovf";
				case 219:
					return "sub.ovf.un";
				case 220:
					return "endfinally";
				case 221:
					return "leave";
				case 222:
					return "leave.s";
				case 223:
					return "stind.i";
				case 224:
					return "conv.u";
				case 248:
					return "prefix7";
				case 249:
					return "prefix6";
				case 250:
					return "prefix5";
				case 251:
					return "prefix4";
				case 252:
					return "prefix3";
				case 253:
					return "prefix2";
				case 254:
					return "prefix1";
				case 255:
					return "prefixref";
				case -512:
					return "arglist";
				case -511:
					return "ceq";
				case -510:
					return "cgt";
				case -509:
					return "cgt.un";
				case -508:
					return "clt";
				case -507:
					return "clt.un";
				case -506:
					return "ldftn";
				case -505:
					return "ldvirtftn";
				case -503:
					return "ldarg";
				case -502:
					return "ldarga";
				case -501:
					return "starg";
				case -500:
					return "ldloc";
				case -499:
					return "ldloca";
				case -498:
					return "stloc";
				case -497:
					return "localloc";
				case -495:
					return "endfilter";
				case -494:
					return "unaligned.";
				case -493:
					return "volatile.";
				case -492:
					return "tail.";
				case -491:
					return "initobj";
				case -490:
					return "constrained.";
				case -489:
					return "cpblk";
				case -488:
					return "initblk";
				case -486:
					return "rethrow";
				case -484:
					return "sizeof";
				case -483:
					return "refanytype";
				case -482:
					return "readonly.";
			}
			throw new ArgumentOutOfRangeException();
		}

		public static bool TakesSingleByteArgument(OpCode inst)
		{
			switch (inst.Value)
			{
				case 14:
				case 15:
				case 16:
				case 17:
				case 18:
				case 19:
				case 31:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
				case 48:
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 222:
				case -494:
					return true;
				default:
					return false;
			}
		}
	}
}
