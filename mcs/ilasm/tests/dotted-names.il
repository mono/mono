//Tests dotted names (namespace, class names)
//It should emit namespace = A.B.C and type name = D
//For E.F, namespace = E, name = F

.assembly extern mscorlib
{
}

.assembly Output 
{
}

.module Output

.namespace A.B {
	
.class public auto autochar sealed beforefieldinit C.D
       extends [mscorlib]System.Object
{
}

}

.class public auto autochar sealed beforefieldinit E.F 
       extends [mscorlib]System.Object
{
}

