	
.text
    
    .set   linkageArea,24
    .set   params,4
    .set   alignment,4

    .set   spaceToSave,linkageArea+params+alignment
    .set   spaceToSave8,spaceToSave+8

; Mark from machine registers that are saved by C compiler
    .globl  _GC_push_regs
_GC_push_regs:
    ; PROLOG
    mflr    r0          ; get return address
    stw     r0,8(r1)    ; save return address
    stwu    r1,-spaceToSave(r1)   ; skip over caller save area
    ;
    mr      r3,r2         ; mark from r2. Well Im not really sure
                          ; that this is necessary or even the right
                          ; thing to do - at least it doesnt harm...
                          ; According to Apples docs it points to
                          ; the direct data area, whatever that is...
    bl 	    L_GC_push_one$stub
    mr      r3,r13        ; mark from r13-r31
    bl 	    L_GC_push_one$stub
    mr      r3,r14
    bl 	    L_GC_push_one$stub
    mr      r3,r15
    bl 	    L_GC_push_one$stub
    mr      r3,r16
    bl 	    L_GC_push_one$stub
    mr      r3,r17
    bl 	    L_GC_push_one$stub
    mr      r3,r18
    bl 	    L_GC_push_one$stub
    mr      r3,r19
    bl 	    L_GC_push_one$stub
    mr      r3,r20
    bl 	    L_GC_push_one$stub
    mr      r3,r21
    bl 	    L_GC_push_one$stub
    mr      r3,r22
    bl 	    L_GC_push_one$stub
    mr      r3,r23
    bl 	    L_GC_push_one$stub
    mr      r3,r24
    bl 	    L_GC_push_one$stub
    mr      r3,r25
    bl 	    L_GC_push_one$stub
    mr      r3,r26
    bl 	    L_GC_push_one$stub
    mr      r3,r27
    bl 	    L_GC_push_one$stub
    mr      r3,r28
    bl 	    L_GC_push_one$stub
    mr      r3,r29
    bl 	    L_GC_push_one$stub
    mr      r3,r30
    bl 	    L_GC_push_one$stub
    mr      r3,r31
    bl 	    L_GC_push_one$stub
    ; EPILOG
    lwz     r0,spaceToSave8(r1)   ; get return address back
    mtlr    r0    ; reset link register
    addic   r1,r1,spaceToSave   ; restore stack pointer
    blr

.data
.picsymbol_stub
L_GC_push_one$stub:
	.indirect_symbol _GC_push_one
	mflr r0
	bcl 20,31,L0$_GC_push_one
L0$_GC_push_one:
	mflr r11
	addis r11,r11,ha16(L_GC_push_one$lazy_ptr-L0$_GC_push_one)
	mtlr r0
	lwz r12,lo16(L_GC_push_one$lazy_ptr-L0$_GC_push_one)(r11)
	mtctr r12
	addi r11,r11,lo16(L_GC_push_one$lazy_ptr-L0$_GC_push_one)
	bctr
.data
.lazy_symbol_pointer
L_GC_push_one$lazy_ptr:
	.indirect_symbol _GC_push_one
	.long dyld_stub_binding_helper
.non_lazy_symbol_pointer
L_GC_push_one$non_lazy_ptr:
	.indirect_symbol _GC_push_one
	.long 0
	



