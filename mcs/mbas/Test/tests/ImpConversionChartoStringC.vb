'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionChartoStringB
	Sub Main()
			Dim a as Char = "a"
			'Dim b as String = "hello" + a
			Dim b as String = a + "hello" 
			if b <> "ahello"
				Throw new System.Exception("Concat of Char & String not working. Expected helloa but got " &b) 
			End if		
	End Sub
End Module


