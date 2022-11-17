    AREA    |.text|,CODE

    EXPORT  |mono_context_get_current|
    
|mono_context_get_current|  PROC
		mov x16, x0
		stp     x0, x1, [x16], #16
        stp     x2, x3, [x16], #16
        stp     x4, x5, [x16], #16
        stp     x6, x7, [x16], #16
        stp     x8, x9, [x16], #16
        stp     x10, x11, [x16], #16
        stp     x12, x13, [x16], #16
        stp     x14, x15, [x16], #16
        stp     xzr, x17, [x16], #16
        stp     x18, x19, [x16], #16
        stp     x20, x21, [x16], #16
        stp     x22, x23, [x16], #16
        stp     x24, x25, [x16], #16
        stp     x26, x27, [x16], #16
        stp     x28, x29, [x16], #16
        stp     x30, xzr, [x16], #8
        mov     x30, sp
        str     x30, [x16], #8
        stp     q0, q1, [x16], #32
        stp     q2, q3, [x16], #32
        stp     q4, q5, [x16], #32
        stp     q6, q7, [x16], #32
        stp     q8, q9, [x16], #32
        stp     q10, q11, [x16], #32
        stp     q12, q13, [x16], #32
        stp     q14, q15, [x16], #32
        stp     q16, q17, [x16], #32
        stp     q18, q19, [x16], #32
        stp     q20, q21, [x16], #32
        stp     q22, q23, [x16], #32
        stp     q24, q25, [x16], #32
        stp     q26, q27, [x16], #32
        stp     q28, q29, [x16], #32
        stp     q30, q31, [x16], #32
		
		ret

|mono_context_get_current._end|

    ENDP

    END
