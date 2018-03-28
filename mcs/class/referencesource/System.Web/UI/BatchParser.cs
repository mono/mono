//------------------------------------------------------------------------------
// <copyright file="BatchParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
using System;
using System.IO;
using System.Web.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Globalization;
using System.Web.Hosting;
using System.Web.Caching;
using System.Web.Util;
using System.Web.Compilation;
using HttpException = System.Web.HttpException;
using Debug=System.Web.Util.Debug;
using System.Text.RegularExpressions;


internal abstract class DependencyParser : BaseParser {

    private VirtualPath _virtualPath;
    private StringSet _virtualPathDependencies;

    // Used to detect circular references
    private StringSet _circularReferenceChecker = new CaseInsensitiveStringSet();

    // The <pages> config section
    private PagesSection _pagesConfig;
    protected PagesSection PagesConfig {
        get { return _pagesConfig; }
    }

    internal void Init(VirtualPath virtualPath) {
        CurrentVirtualPath = virtualPath;
        _virtualPath = virtualPath;
        _pagesConfig = MTConfigUtil.GetPagesConfig(virtualPath);
    }

    internal ICollection GetVirtualPathDependencies() {

        // Always set the culture to Invariant when parsing (ASURT 99071)
        Thread currentThread = Thread.CurrentThread;
        CultureInfo prevCulture = currentThread.CurrentCulture;
        HttpRuntime.SetCurrentThreadCultureWithAssert(CultureInfo.InvariantCulture);

        try {
            try {
                PrepareParse();
                ParseFile();
            }
            finally {
                // Restore the previous culture
                HttpRuntime.SetCurrentThreadCultureWithAssert(prevCulture);
            }
        }
        catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)

        return _virtualPathDependencies;
    }

    protected void AddDependency(VirtualPath virtualPath) {
        virtualPath = ResolveVirtualPath(virtualPath);
        Debug.Trace("Template", "Parsed dependency: " + _virtualPath + " depends on " + virtualPath);

        if (_virtualPathDependencies == null)
            _virtualPathDependencies = new CaseInsensitiveStringSet();

        _virtualPathDependencies.Add(virtualPath.VirtualPathString);
    }

    internal abstract string DefaultDirectiveName { get; }

    protected virtual void PrepareParse() {}

    private void ParseFile() {
        ParseFile(null /*physicalPath*/, _virtualPath);
    }

    private void ParseFile(string physicalPath, VirtualPath virtualPath) {

        // Determine the file used for the circular references checker.  Normally,
        // we use the virtualPath, but we use the physical path if it specified,
        // as is the case for <!-- #include file="foo.inc" -->
        string fileToReferenceCheck = physicalPath != null ? physicalPath : virtualPath.VirtualPathString;

        // Check for circular references of include files
        if (_circularReferenceChecker.Contains(fileToReferenceCheck)) {
            throw new HttpException(
                SR.GetString(SR.Circular_include));
        }

        // Add the current file to the circular references checker.
        _circularReferenceChecker.Add(fileToReferenceCheck);

        try {
            // Open a TextReader either from the physical or virtual path
            TextReader reader;
            if (physicalPath != null) {
                using (reader = Util.ReaderFromFile(physicalPath, virtualPath)) {
                    ParseReader(reader);
                }
            }
            else {
                using (Stream stream = virtualPath.OpenFile()) {
                    reader = Util.ReaderFromStream(stream, virtualPath);
                    ParseReader(reader);
                }
            }
        }
        finally {
            // Remove the current file from the circular references checker
            _circularReferenceChecker.Remove(fileToReferenceCheck);
        }
    }

    private void ParseReader(TextReader input) {
        ParseString(input.ReadToEnd());
    }

    private void ParseString(string text) {

        int textPos = 0;

        for (;;) {
            Match match;

            // 1: scan for text up to the next tag.

            if ((match = textRegex.Match(text, textPos)).Success) {
                textPos = match.Index + match.Length;
            }

            // we might be done now

            if (textPos == text.Length)
                break;

            // 2: handle constructs that start with <

            // Check to see if it's a directive (i.e. <%@ %> block)

            if ((match = directiveRegex.Match(text, textPos)).Success) {
                IDictionary directive = CollectionsUtil.CreateCaseInsensitiveSortedList();
                string directiveName = ProcessAttributes(match, directive);                
                ProcessDirective(directiveName, directive);
                textPos = match.Index + match.Length;
            }

            else if ((match = includeRegex.Match(text, textPos)).Success) {
                ProcessServerInclude(match);
                textPos = match.Index + match.Length;
            }

            else if ((match = commentRegex.Match(text, textPos)).Success) {
                // Just skip it
                textPos = match.Index + match.Length;
            }

            else {
                int newPos = text.IndexOf("<%@", textPos, StringComparison.Ordinal);
                // 2nd condition is used to catch invalid directives, e.g. <%@ attr="value_without_end_quote >
                if (newPos == -1 || newPos == textPos) {
                    return;
                }

                textPos = newPos;
            }

            // we might be done now
            if (textPos == text.Length)
                return;
        }
    }

    /*
     * Process a server side include.  e.g. <!-- #include file="foo.inc" -->
     */
    private void ProcessServerInclude(Match match) {

        string pathType = match.Groups["pathtype"].Value;
        string filename = match.Groups["filename"].Value;

        if (filename.Length == 0) return;

        VirtualPath newVirtualPath;
        string newPhysicalPath = null;

        if (StringUtil.EqualsIgnoreCase(pathType, "file")) {
            
            if (UrlPath.IsAbsolutePhysicalPath(filename)) {
                // If it's an absolute physical path, use it as is
                newPhysicalPath = filename;

                // Reuse the current virtual path
                newVirtualPath = CurrentVirtualPath;
            }
            else {

                // If it's relative, just treat it as virtual
                newVirtualPath = ResolveVirtualPath(VirtualPath.Create(filename));
            }
        }
        else if (StringUtil.EqualsIgnoreCase(pathType, "virtual")) {
            newVirtualPath = ResolveVirtualPath(VirtualPath.Create(filename));
        }
        else {
            // Unknown #include type: ignore it
            return;
        }

        VirtualPath prevVirtualPath = _virtualPath;
        try {
            _virtualPath = newVirtualPath;

            // Parse the included file recursively
            ParseFile(newPhysicalPath, newVirtualPath);
        }
        finally {
            // Restore the paths
            _virtualPath = prevVirtualPath;
        }
    }

    /*
     * Process a <%@ %> block
     */
    internal virtual void ProcessDirective(string directiveName, IDictionary directive) {
        // Get all the directives into a bag
        // Check for the main directive (e.g. "page" for an aspx)
        if (directiveName == null ||
            StringUtil.EqualsIgnoreCase(directiveName, DefaultDirectiveName) ) {
            ProcessMainDirective(directive);
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "register")) {

            VirtualPath src = Util.GetAndRemoveVirtualPathAttribute(directive, "src");

            if (src != null) {
                AddDependency(src);
            }
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "reference")) {

            VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualpath");
            if (virtualPath != null)
                AddDependency(virtualPath);

            VirtualPath page = Util.GetAndRemoveVirtualPathAttribute(directive, "page");
            if (page != null)
                AddDependency(page);

            VirtualPath control = Util.GetAndRemoveVirtualPathAttribute(directive, "control");
            if (control != null)
                AddDependency(control);
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "assembly")) {

            VirtualPath src = Util.GetAndRemoveVirtualPathAttribute(directive, "src");
            if (src != null)
                AddDependency(src);
        }
    }

    private void ProcessMainDirective(IDictionary mainDirective) {

        // Go through all the attributes on the directive
        foreach (DictionaryEntry entry in mainDirective) {

            string attribName = ((string)entry.Key).ToLower(CultureInfo.InvariantCulture);

            // Parse out the device name, if any
            string name;
            string deviceName = Util.ParsePropertyDeviceFilter(attribName, out name);

            // Process the attribute
            ProcessMainDirectiveAttribute(deviceName, name, (string) entry.Value);
        }
    }

    internal virtual void ProcessMainDirectiveAttribute(string deviceName, string name,
        string value) {

        // A "src" attribute is equivalent to an imported source file
        if (name == "src") {
            string src = Util.GetNonEmptyAttribute(name, value);
            AddDependency(VirtualPath.Create(src));
        }
    }

    /*
     * Adds attributes and their values to the attribs
     */
    private string ProcessAttributes(Match match, IDictionary attribs) {
        string ret = null;
        CaptureCollection attrnames = match.Groups["attrname"].Captures;
        CaptureCollection attrvalues = match.Groups["attrval"].Captures;
        CaptureCollection equalsign = match.Groups["equal"].Captures;

        for (int i = 0; i < attrnames.Count; i++) {
            string attribName = attrnames[i].ToString();
            string attribValue = attrvalues[i].ToString();
            bool fHasEqual = (equalsign[i].ToString().Length > 0);

            if (attribName != null && !fHasEqual && ret == null) {
                ret = attribName;
                continue;
            }

            try {
                if (attribs != null)
                    attribs.Add(attribName, attribValue);
            }
            catch (ArgumentException) {}
        }

        return ret;
    }
}


internal abstract class TemplateControlDependencyParser : DependencyParser {

    internal override void ProcessMainDirectiveAttribute(string deviceName, string name,
        string value) {

        switch (name) {

        case "masterpagefile":
            value = value.Trim();
            if (value.Length > 0) {
                // Add a dependency on the master, whether it has a device filter or not
                AddDependency(VirtualPath.Create(value));
            }
            break;

        default:
            // We didn't handle the attribute.  Try the base class
            base.ProcessMainDirectiveAttribute(deviceName, name, value);
            break;
        }
    }
}

internal class PageDependencyParser : TemplateControlDependencyParser {
    internal override string DefaultDirectiveName {
        get { return PageParser.defaultDirectiveName; }
    }

    protected override void PrepareParse() {
        if (PagesConfig != null) {
            if (PagesConfig.MasterPageFileInternal != null && PagesConfig.MasterPageFileInternal.Length != 0)
                AddDependency(VirtualPath.Create(PagesConfig.MasterPageFileInternal));
        }
    }

    internal override void ProcessDirective(string directiveName, IDictionary directive) {
        base.ProcessDirective(directiveName, directive);

        if (StringUtil.EqualsIgnoreCase(directiveName, "previousPageType") || 
            StringUtil.EqualsIgnoreCase(directiveName, "masterType")) {

            VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualPath");
            if (virtualPath != null)
                AddDependency(virtualPath);
        }
    }
}

internal class UserControlDependencyParser : TemplateControlDependencyParser {
    internal override string DefaultDirectiveName {
        get { return UserControlParser.defaultDirectiveName; }
    }
}

internal class MasterPageDependencyParser : UserControlDependencyParser {
    internal override string DefaultDirectiveName {
        get { return MasterPageParser.defaultDirectiveName; }
    }

    internal override void ProcessDirective(string directiveName, IDictionary directive) {
        base.ProcessDirective(directiveName, directive);

        if (StringUtil.EqualsIgnoreCase(directiveName, "masterType")) {
            VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualPath");
            if (virtualPath != null)
                AddDependency(virtualPath);
        }
    }
}
}
