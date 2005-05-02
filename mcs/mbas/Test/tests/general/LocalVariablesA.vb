Imports System

Module LocalVariablesA

    Function swap(ByVal a As Integer, ByVal b As Integer) As Integer
        Dim c As Integer
        c = a
        a = b
        b = c
        Return 0
    End Function

    ' Local variable having same name as Sub containing it
    Sub f2()
        Dim f2 As Integer = 1
        f2 = f2 + 1
	  if f2<>2	
	        throw new System.Exception("#A1 Local Variables not working")
	  end if
    End Sub

    Sub main()
        Dim a, b As Integer
        a = 10 : b = 32
  	  if a<>10 and b<>32	 
	        throw new System.Exception("#A2 Local Variables not working")
	  end if
        swap(a, b)
        if a<>10 and b<>32	
	        throw new System.Exception("#A3 Local Variables not working")
	  end if
        f2()
    End Sub

End Module