Public Class c
	public Function s (i As Integer, _
				 Optional j As Integer = 10, _
				 Optional k As String = "aaa") _
				 as string

		Return ("s : " + i.ToString() + " - " + j.ToString() + " - " + k)
	End Function
End Class
