# powerpc cpu description file
# this file is read by genmdesc to pruduce a table with all the relevant information
# about the cpu instructions that may be used by the regsiter allocator, the scheduler
# and other parts of the arch-dependent part of mini.
#
# An opcode name is followed by a colon and optional specifiers.
# A specifier has a name, a colon and a value. Specifiers are separated by white space.
# Here is a description of the specifiers valid for this file and their possible values.
#
# dest:register       describes the destination register of an instruction
# src1:register       describes the first source register of an instruction
# src2:register       describes the second source register of an instruction
#
#	i  integer register
#	a  r0 register (output from calls)
#	b  base register (used in address references)
#	f  floating point register
#	g  floating point register returned in r0:r1 for soft-float mode
#
# len:number         describe the maximun length in bytes of the instruction
# number is a positive integer
#
# cost:number        describe how many cycles are needed to complete the instruction (unused)
#
# clob:spec          describe if the instruction clobbers registers or has special needs
#
#	c  clobbers caller-save registers
#	r  'reserves' the destination register until a later instruction unreserves it
#          used mostly to set output registers in function calls
#
# flags:spec        describe if the instruction uses or sets the flags (unused)
#
# 	s  sets the flags
#       u  uses the flags
#       m  uses and modifies the flags
#
# res:spec          describe what units are used in the processor (unused)
#
# delay:            describe delay slots (unused)
#
# the required specifiers are: len, clob (if registers are clobbered), the registers
# specifiers if the registers are actually used, flags (when scheduling is implemented).
#
# See the code in mini-x86.c for more details on how the specifiers are used.
#
label:
nop: len:0
dummy_use: len:0
dummy_store: len:0
not_reached: len:0
not_null: src1:i len:0
memory_barrier: len:4
break: len:32
jmp: len:92
call: dest:a clob:c len:20
br: len:4
br_reg: src1:i len:8

beq: len:8
bge: len:8
bgt: len:8
ble: len:8
blt: len:8
bne.un: len:8
bge.un: len:8
bgt.un: len:8
ble.un: len:8
blt.un: len:8

int_beq: len:8
int_bge: len:8
int_bgt: len:8
int_ble: len:8
int_blt: len:8
int_bne_un: len:8
int_bge_un: len:8
int_bgt_un: len:8
int_ble_un: len:8
int_blt_un: len:8

switch: src1:i len:8

ldind.i1: dest:i len:8
ldind.u1: dest:i len:8
ldind.i2: dest:i len:8
ldind.u2: dest:i len:8
ldind.i4: dest:i len:8
ldind.u4: dest:i len:8
ldind.i: dest:i len:8
ldind.ref: dest:i len:8
stind.ref: src1:b src2:i
stind.i1: src1:b src2:i
stind.i2: src1:b src2:i
stind.i4: src1:b src2:i
stind.r4: src1:b src2:f
stind.r8: src1:b src2:f

add: dest:i src1:i src2:i len:4
sub: dest:i src1:i src2:i len:4
mul: dest:i src1:i src2:i len:4
div: dest:i src1:i src2:i len:40
div.un: dest:i src1:i src2:i len:16
rem: dest:i src1:i src2:i len:48
rem.un: dest:i src1:i src2:i len:24
and: dest:i src1:i src2:i len:4
or: dest:i src1:i src2:i len:4
xor: dest:i src1:i src2:i len:4
shl: dest:i src1:i src2:i len:4
shr: dest:i src1:i src2:i len:4
shr.un: dest:i src1:i src2:i len:4

neg: dest:i src1:i len:4
not: dest:i src1:i len:4
conv.i1: dest:i src1:i len:8
conv.i2: dest:i src1:i len:8
conv.i4: dest:i src1:i len:4
conv.r4: dest:f src1:i len:36
conv.r8: dest:f src1:i len:36
conv.u4: dest:i src1:i
conv.r.un: dest:f src1:i len:56

conv.u2: dest:i src1:i len:8
conv.u1: dest:i src1:i len:4
conv.i: dest:i src1:i len:4

int_add: dest:i src1:i src2:i len:4
int_sub: dest:i src1:i src2:i len:4
int_mul: dest:i src1:i src2:i len:4
int_div: dest:i src1:i src2:i len:40
int_div_un: dest:i src1:i src2:i len:16
int_rem: dest:i src1:i src2:i len:48
int_rem_un: dest:i src1:i src2:i len:24
int_and: dest:i src1:i src2:i len:4
int_or: dest:i src1:i src2:i len:4
int_xor: dest:i src1:i src2:i len:4
int_shl: dest:i src1:i src2:i len:4
int_shr: dest:i src1:i src2:i len:4
int_shr_un: dest:i src1:i src2:i len:4

int_neg: dest:i src1:i len:4
int_not: dest:i src1:i len:4
int_conv_to_i1: dest:i src1:i len:8
int_conv_to_i2: dest:i src1:i len:8
int_conv_to_i4: dest:i src1:i len:4
int_conv_to_r4: dest:f src1:i len:36
int_conv_to_r8: dest:f src1:i len:36
int_conv_to_u4: dest:i src1:i
int_conv_to_r_un: dest:f src1:i len:56
int_conv_to_u2: dest:i src1:i len:8
int_conv_to_u1: dest:i src1:i len:4
int_conv_to_i: dest:i src1:i len:4

throw: src1:i len:24
rethrow: src1:i len:20

ckfinite: dest:f src1:f len:24

add.ovf: dest:i src1:i src2:i len:16
add.ovf.un: dest:i src1:i src2:i len:16
mul.ovf: dest:i src1:i src2:i len:16
# this opcode is handled specially in the code generator
mul.ovf.un: dest:i src1:i src2:i len:16
sub.ovf: dest:i src1:i src2:i len:16
sub.ovf.un: dest:i src1:i src2:i len:16
add_ovf_carry: dest:i src1:i src2:i len:16
sub_ovf_carry: dest:i src1:i src2:i len:16
add_ovf_un_carry: dest:i src1:i src2:i len:16
sub_ovf_un_carry: dest:i src1:i src2:i len:16

int_add_ovf: dest:i src1:i src2:i len:16
int_add_ovf_un: dest:i src1:i src2:i len:16
int_mul_ovf: dest:i src1:i src2:i len:16
# this opcode is handled specially in the code generator
int_mul_ovf_un: dest:i src1:i src2:i len:16
int_sub_ovf: dest:i src1:i src2:i len:16
int_sub_ovf_un: dest:i src1:i src2:i len:16

start_handler: len:20
endfinally: len:20
call_handler: len:12
endfilter: src1:i len:16

conv.u: dest:i src1:i len:4

ceq: dest:i len:12
cgt: dest:i len:12
cgt.un: dest:i len:12
clt: dest:i len:12
clt.un: dest:i len:12

int_ceq: dest:i len:12
int_cgt: dest:i len:12
int_cgt_un: dest:i len:12
int_clt: dest:i len:12
int_clt_un: dest:i len:12

localloc: dest:i src1:i len:60

compare: src1:i src2:i len:4
icompare: src1:i src2:i len:4
fcompare: src1:f src2:f len:12
compare_imm: src1:i len:12
icompare_imm: src1:i len:12
oparglist: src1:i len:12
outarg: src1:i len:1
outarg_imm: len:16
setret: dest:a src1:i len:4
setlret: src1:i src2:i len:12
setreg: dest:i src1:i len:4 clob:r
setregimm: dest:i len:16 clob:r
setfreg: dest:f src1:f len:4 clob:r
checkthis: src1:b len:4

call: dest:a clob:c len:20
voidcall: len:20 clob:c
voidcall_reg: src1:i len:8 clob:c
voidcall_membase: src1:b len:12 clob:c
fcall: dest:g len:24 clob:c
fcall_reg: dest:g src1:i len:12 clob:c
fcall_membase: dest:g src1:b len:16 clob:c
lcall: dest:l len:20 clob:c
lcall_reg: dest:l src1:i len:8 clob:c
lcall_membase: dest:l src1:b len:12 clob:c

vcall: len:20 clob:c
vcall_reg: src1:i len:8 clob:c
vcall_membase: src1:b len:12 clob:c

vcall2: len:20 clob:c
vcall2_reg: src1:i len:8 clob:c
vcall2_membase: src1:b len:12 clob:c

call_reg: dest:a src1:i len:8 clob:c
call_membase: dest:a src1:b len:12 clob:c
iconst: dest:i len:16
r4const: dest:f len:20
r8const: dest:f len:20
<<<<<<< .working
=======
label: len:0
>>>>>>> .merge-right.r94008
store_membase_imm: dest:b len:20
store_membase_reg: dest:b src1:i len:20
storei1_membase_imm: dest:b len:20
storei1_membase_reg: dest:b src1:i len:12
storei2_membase_imm: dest:b len:20
storei2_membase_reg: dest:b src1:i len:12
storei4_membase_imm: dest:b len:20
storei4_membase_reg: dest:b src1:i len:20
storei8_membase_imm: dest:b 
storei8_membase_reg: dest:b src1:i 
storer4_membase_reg: dest:b src1:f len:12
storer8_membase_reg: dest:b src1:f len:32
store_memindex: dest:b src1:i src2:i len:4
storei1_memindex: dest:b src1:i src2:i len:4
storei2_memindex: dest:b src1:i src2:i len:4
storei4_memindex: dest:b src1:i src2:i len:4
load_membase: dest:i src1:b len:20
loadi1_membase: dest:i src1:b len:4
loadu1_membase: dest:i src1:b len:4
loadi2_membase: dest:i src1:b len:4
loadu2_membase: dest:i src1:b len:4
loadi4_membase: dest:i src1:b len:4
loadu4_membase: dest:i src1:b len:4
loadi8_membase: dest:i src1:b
loadr4_membase: dest:f src1:b len:4
loadr8_membase: dest:f src1:b len:32
load_memindex: dest:i src1:b src2:i len:4
loadi1_memindex: dest:i src1:b src2:i len:4
loadu1_memindex: dest:i src1:b src2:i len:4
loadi2_memindex: dest:i src1:b src2:i len:4
loadu2_memindex: dest:i src1:b src2:i len:4
loadi4_memindex: dest:i src1:b src2:i len:4
loadu4_memindex: dest:i src1:b src2:i len:4
loadu4_mem: dest:i len:8
move: dest:i src1:i len:4
fmove: dest:f src1:f len:4

add_imm: dest:i src1:i len:12
sub_imm: dest:i src1:i len:12
mul_imm: dest:i src1:i len:12
# there is no actual support for division or reminder by immediate
# we simulate them, though (but we need to change the burg rules 
# to allocate a symbolic reg for src2)
div_imm: dest:i src1:i src2:i len:20
div_un_imm: dest:i src1:i src2:i len:12
rem_imm: dest:i src1:i src2:i len:28
rem_un_imm: dest:i src1:i src2:i len:16
and_imm: dest:i src1:i len:12
or_imm: dest:i src1:i len:12
xor_imm: dest:i src1:i len:12
shl_imm: dest:i src1:i len:8
shr_imm: dest:i src1:i len:8
shr_un_imm: dest:i src1:i len:8

int_add_imm: dest:i src1:i len:12
int_sub_imm: dest:i src1:i len:12
int_mul_imm: dest:i src1:i len:12
# there is no actual support for division or reminder by immediate
# we simulate them, though (but we need to change the burg rules 
# to allocate a symbolic reg for src2)
int_div_imm: dest:i src1:i src2:i len:20
int_div_un_imm: dest:i src1:i src2:i len:12
int_rem_imm: dest:i src1:i src2:i len:28
int_rem_un_imm: dest:i src1:i src2:i len:16
int_and_imm: dest:i src1:i len:12
int_or_imm: dest:i src1:i len:12
int_xor_imm: dest:i src1:i len:12
int_shl_imm: dest:i src1:i len:8
int_shr_imm: dest:i src1:i len:8
int_shr_un_imm: dest:i src1:i len:8

cond_exc_eq: len:8
cond_exc_ne_un: len:8
cond_exc_lt: len:8
cond_exc_lt_un: len:8
cond_exc_gt: len:8
cond_exc_gt_un: len:8
cond_exc_ge: len:8
cond_exc_ge_un: len:8
cond_exc_le: len:8
cond_exc_le_un: len:8
cond_exc_ov: len:12
cond_exc_no: len:8
cond_exc_c: len:12
cond_exc_nc: len:8
<<<<<<< .working

cond_exc_ieq: len:8
cond_exc_ine_un: len:8
cond_exc_ilt: len:8
cond_exc_ilt_un: len:8
cond_exc_igt: len:8
cond_exc_igt_un: len:8
cond_exc_ige: len:8
cond_exc_ige_un: len:8
cond_exc_ile: len:8
cond_exc_ile_un: len:8
cond_exc_iov: len:12
cond_exc_ino: len:8
cond_exc_ic: len:12
cond_exc_inc: len:8

=======
>>>>>>> .merge-right.r94008
long_conv_to_ovf_i: dest:i src1:i src2:i len:30
<<<<<<< .working
long_conv_to_ovf_i4_2: dest:i src1:i src2:i len:30
=======
>>>>>>> .merge-right.r94008
long_mul_ovf: 
long_conv_to_r_un: dest:f src1:i src2:i len:37 
<<<<<<< .working
float_beq: len:20
float_bne_un: len:20
float_blt: len:20
float_blt_un: len:20
float_bgt: len:20
float_bgt_un: len:20
float_bge: len:20
float_bge_un: len:20
float_ble: len:20
float_ble_un: len:20
=======
float_beq: src1:f src2:f len:20
float_bne_un: src1:f src2:f len:20
float_blt: src1:f src2:f len:20
float_blt_un: src1:f src2:f len:20
float_bgt: src1:f src2:f len:20
float_btg_un: src1:f src2:f len:20
float_bge: src1:f src2:f len:20
float_bge_un: src1:f src2:f len:20
float_ble: src1:f src2:f len:20
float_ble_un: src1:f src2:f len:20
>>>>>>> .merge-right.r94008
float_add: dest:f src1:f src2:f len:4
float_sub: dest:f src1:f src2:f len:4
float_mul: dest:f src1:f src2:f len:4
float_div: dest:f src1:f src2:f len:4
float_div_un: dest:f src1:f src2:f len:4
float_rem: dest:f src1:f src2:f len:16
float_rem_un: dest:f src1:f src2:f len:16
float_neg: dest:f src1:f len:4
float_not: dest:f src1:f len:4
float_conv_to_i1: dest:i src1:f len:40
float_conv_to_i2: dest:i src1:f len:40
float_conv_to_i4: dest:i src1:f len:40
float_conv_to_i8: dest:l src1:f len:40
float_conv_to_r4: dest:f src1:f len:4
float_conv_to_u4: dest:i src1:f len:40
float_conv_to_u8: dest:l src1:f len:40
float_conv_to_u2: dest:i src1:f len:40
float_conv_to_u1: dest:i src1:f len:40
float_conv_to_i: dest:i src1:f len:40
float_ceq: dest:i src1:f src2:f len:16
float_cgt: dest:i src1:f src2:f len:16
float_cgt_un: dest:i src1:f src2:f len:20
float_clt: dest:i src1:f src2:f len:16
float_clt_un: dest:i src1:f src2:f len:20
float_conv_to_u: dest:i src1:f len:36

aot_const: dest:i len:16
sqrt: dest:f src1:f len:4

adc: dest:i src1:i src2:i len:4
addcc: dest:i src1:i src2:i len:4
subcc: dest:i src1:i src2:i len:4
adc_imm: dest:i src1:i len:12
addcc_imm: dest:i src1:i len:12
subcc_imm: dest:i src1:i len:12
sbb: dest:i src1:i src2:i len:4
sbb_imm: dest:i src1:i len:12

int_adc: dest:i src1:i src2:i len:4
int_addcc: dest:i src1:i src2:i len:4
int_subcc: dest:i src1:i src2:i len:4
int_adc_imm: dest:i src1:i len:12
int_sbb: dest:i src1:i src2:i len:4
int_sbb_imm: dest:i src1:i len:12

arm_rsbs_imm: dest:i src1:i len:4
arm_rsc_imm: dest:i src1:i len:4
bigmul: len:8 dest:l src1:i src2:i
bigmul_un: len:8 dest:l src1:i src2:i
tls_get: len:8 dest:i
