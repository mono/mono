'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDoubletoStringC
	Sub Main()
			Dim a as Double = 123.90
			Dim b as String = a + "123"
			if b <> "246.9"
				Throw new System.Exception("Concat of Double & String not working. Expected 246.9 but got " &b) 
			End if		
	End Sub
End Module

