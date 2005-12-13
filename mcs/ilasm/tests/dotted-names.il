//Tests dotted names (namespace, class names)
//It should emit namespace = A.B.C and type name = D

//.assembly extern mscorlib
//{
//}

.module Output

.namespace A.B {
	
.class public auto autochar sealed beforefieldinit C.D
       extends [mscorlib]System.Object
{
}

}
