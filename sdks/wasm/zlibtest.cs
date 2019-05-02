using System;
using System.IO.Compression;
using System.IO;

public class ZlibTest
{
	public static void Main (String[] args) {
		
		    using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = archive.CreateEntry("foo.txt");

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("Bar!");
                    }
                }
            }            

	}
}
