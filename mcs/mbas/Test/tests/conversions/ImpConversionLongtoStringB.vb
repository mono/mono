'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionLongtoStringC
	Sub Main()
			Dim a as Long = 123
			Dim b as String = a + "123"
			if b <> "246"
				Throw new System.Exception("Concat of Long & String not working. Expected 246 but got " &b) 
			End if		
	End Sub
End Module


