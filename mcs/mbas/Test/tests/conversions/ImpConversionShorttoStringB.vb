'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionShorttoStringC
	Sub Main()
			Dim a as Short = 123
			Dim b as String = a + "123"
			if b <> "246"
				Throw new System.Exception("Concat of Short & String not working. Expected 246 but got " &b) 
			End if		
	End Sub
End Module



