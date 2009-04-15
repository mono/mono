//
// The simple provider is an example provider
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Use like this:
//   mono assembler.exe --simple DIRECTORY --out name
//
// Then create a .source file in your sources directory, and copy
// name.tree and name.zip to the sources directory.
//
// To view the tree generated, use:
//   mono dump.exe name.tree
//
namespace Monodoc {
using System;
using System.IO;
using System.Text;

//
// The simple provider generates the information source
//
public class SimpleProvider : Provider {
	string basedir;
	
	public SimpleProvider (string base_directory)
	{
		basedir = base_directory;
		if (!Directory.Exists (basedir))
			throw new FileNotFoundException (String.Format ("The directory `{0}' does not exist", basedir));
	}

	public override void PopulateTree (Tree tree)
	{
		Node top = tree.LookupNode ("Directory at: " + basedir, "simple:");
		
		foreach (string dir in Directory.GetDirectories (basedir)){
			string url = Path.GetFileName (dir);
			Node n = top.LookupNode ("Dir: " + url, url);
			PopulateDir (n, dir);
		}
	}

#pragma warning disable 219
	void PopulateDir (Node me, string dir)
	{
		Console.WriteLine ("Adding: " + dir);
		foreach (string child_dir in Directory.GetDirectories (dir)){
			string url = Path.GetFileName (child_dir);
			Node n = me.LookupNode ("Dir: " + url, "simple-directory:" + url);
			PopulateDir (me, child_dir);
		}

		foreach (string file in Directory.GetFiles (dir)){
			Console.WriteLine ("   File: " + file);
			string file_code = me.tree.HelpSource.PackFile (file);

			//
			// The url element encoded for the file is:
			//  originalfilename#CODE
			//
			// The code is assigned to us after the file has been packaged
			// We use the original-filename later to render html or text files
			//
			Node n = me.LookupNode (Path.GetFileName (file), file + "#" + file_code);
			
		}
	}

	public override void CloseTree (HelpSource hs, Tree tree)
	{
	}
}

//
// The HelpSource is used during the rendering phase.
//

public class SimpleHelpSource : HelpSource {
	Encoding enc;
	
	public SimpleHelpSource (string base_file, bool create) : base (base_file, create)
	{
		enc = new UTF8Encoding (false, false);
	}

	public override string GetText (string url, out Node match_node)
	{
		match_node = null;

		string c = GetCachedText (url);
		if (c != null)
			return c;

		if (url.StartsWith ("simple:") || url.StartsWith ("simple-directory:"))
			return GetTextFromUrl (url);

		return null;
	}

	string GetTextFromUrl (string url)
	{
		// Remove "simple:" prefix
		url = url.Substring (7);

		if (url.StartsWith ("simple-directory:"))
			return String.Format ("<html>This is a directory entry point: {0} </html>",
					      url.Substring (17));

		// Otherwise the last element of the url is the file code we got.
		int pound = url.LastIndexOf ("#");
		string code;
		if (pound == -1)
			code = url;
		else
			code = url.Substring (pound+1);


		Stream s = GetHelpStream (code);
		if (s == null)
			return String.Format ("<html>No stream for this node: {0} </html>", url);

		//
		// Now, get the file type
		//
		int slash = url.LastIndexOf ("/");
		string fname = url.Substring (slash + 1, pound - slash - 1).ToLower ();

		if (fname.EndsWith (".html") || fname.EndsWith (".htm")){
			TextReader r = new StreamReader (s, enc);
			return r.ReadToEnd ();
		}

		if (fname.EndsWith (".png") || fname.EndsWith (".jpg") ||
		    fname.EndsWith (".jpeg") || fname.EndsWith (".gif")){
			return "<html>Image file, have not implemented rendering this yet</html>";
		}

		// Convert text to HTML
		StringBuilder result = new StringBuilder ("<html>");
		TextReader reader = new StreamReader (s, enc);
		string line;
		
		while ((line = reader.ReadLine ()) != null){
			result.Append (line);
			result.Append ("<br>");
		}
		result.Append ("<html>");
		return result.ToString ();
	}
}
}
