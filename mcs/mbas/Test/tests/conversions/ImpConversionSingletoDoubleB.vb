'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoDoubleC
	Sub Main()
			Dim a as Single= 111
			Dim b as Double = 111.9 + a
			if b <> 222.9
				Throw new System.Exception("Addition of Single & Double not working. Expected 222.9 but got " &b) 
			End if		
	End Sub
End Module

