#
# Author: Zoltan Varga (vargaz@gmail.com)
# License: MIT/X11
#

#
# This is a mono support mode for a python-enabled gdb:
# http://sourceware.org/gdb/wiki/PythonGdb
# Usage:
# - copy/symlink this file, plus mono-gdbinit to the directory where the mono 
#   executable lives.
# - run mono under gdb, or attach to a mono process using gdb
# - Type 'xdb' in gdb to load/reload the debugging info emitted by the runtime.
# - The debug info is emitted to a file called xdb.s in the current working directory.
#   When attaching to a mono process, make sure you are in the same directory.
#

from __future__ import print_function
import os

class StringPrinter:
    "Print a C# string"

    def __init__(self, val):
        self.val = val

    def to_string(self):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "null"

        obj = self.val.cast (gdb.lookup_type ("MonoString").pointer ()).dereference ()
        len = obj ['length']
        chars = obj ['chars']
        i = 0
        res = ['"']
        while i < len:
            val = (chars.cast(gdb.lookup_type ("gint64")) + (i * 2)).cast(gdb.lookup_type ("gunichar2").pointer ()).dereference ()
            if val >= 256:
                c = "\u%X".format (val)
            else:
                c = chr (val)
            res.append (c)
            i = i + 1
        res.append ('"')
        return ''.join (res)

def stringify_class_name(ns, name):
    if ns == "System":
        if name == "Byte":
            return "byte"
        if name == "String":
            return "string"
    if ns == "":
        return name
    else:
        return "{0}.{1}".format (ns, name)

class ArrayPrinter:
    "Print a C# array"

    def __init__(self, val, class_ns, class_name):
        self.val = val
        self.class_ns = class_ns
        self.class_name = class_name

    def to_string(self):
        obj = self.val.cast (gdb.lookup_type ("MonoArray").pointer ()).dereference ()
        length = obj ['max_length']
        return "{0} [{1}]".format (stringify_class_name (self.class_ns, self.class_name [0:len (self.class_name) - 2]), int (length))
        
class ObjectPrinter:
    "Print a C# object"

    def __init__(self, val):
        if str(val.type)[-1] == "&":
            self.val = val.address.cast (gdb.lookup_type ("MonoObject").pointer ())
        else:
            self.val = val.cast (gdb.lookup_type ("MonoObject").pointer ())

    class _iterator:
        def __init__(self,obj):
            self.obj = obj
            self.iter = self.obj.type.fields ().__iter__ ()
            pass

        def __iter__(self):
            return self

        def next(self):
            field = self.iter.next ()
            try:
                if str(self.obj [field.name].type) == "object":
                    # Avoid recursion
                    return (field.name, self.obj [field.name].cast (gdb.lookup_type ("void").pointer ()))
                else:
                    return (field.name, self.obj [field.name])
            except:
                # Superclass
                return (field.name, self.obj.cast (gdb.lookup_type ("{0}".format (field.name))))

    def children(self):
        # FIXME: It would be easier if gdb.Value would support iteration itself
        # It would also be better if we could return None
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return {}.__iter__ ()
        try:
            obj = self.val.dereference ()
            class_ns = obj ['vtable'].dereference ()['klass'].dereference ()['name_space'].string ()
            class_name = obj ['vtable'].dereference ()['klass'].dereference ()['name'].string ()
            if class_name [-2:len(class_name)] == "[]":
                return {}.__iter__ ()
            gdb_type = gdb.lookup_type ("struct {0}_{1}".format (class_ns.replace (".", "_"), class_name))
            return self._iterator(obj.cast (gdb_type))
        except:
            print (sys.exc_info ()[0])
            print (sys.exc_info ()[1])
            return {}.__iter__ ()

    def to_string(self):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "null"
        try:
            obj = self.val.dereference ()
            class_ns = obj ['vtable'].dereference ()['klass'].dereference ()['name_space'].string ()
            class_name = obj ['vtable'].dereference ()['klass'].dereference ()['name'].string ()
            if class_ns == "System" and class_name == "String":
                return StringPrinter (self.val).to_string ()
            if class_name [-2:len(class_name)] == "[]":
                return ArrayPrinter (self.val,class_ns,class_name).to_string ()
            if class_ns != "":
                try:
                    gdb_type = gdb.lookup_type ("struct {0}.{1}".format (class_ns, class_name))
                except:
                    # Maybe there is no debug info for that type
                    return "{0}.{1}".format (class_ns, class_name)
                #return obj.cast (gdb_type)
                return "{0}.{1}".format (class_ns, class_name)
            return class_name
        except:
            print (sys.exc_info ()[0])
            print (sys.exc_info ()[1])
            # FIXME: This can happen because we don't have liveness information
            return self.val.cast (gdb.lookup_type ("guint64"))
        
class MonoMethodPrinter:
    "Print a MonoMethod structure"

    def __init__(self, val):
        self.val = val

    def to_string(self):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "0x0"
        val = self.val.dereference ()
        klass = val ["klass"].dereference ()
        class_name = stringify_class_name (klass ["name_space"].string (), klass ["name"].string ())
        return "\"{0}:{1} ()\"".format (class_name, val ["name"].string ())
        # This returns more info but requires calling into the inferior
        #return "\"{0}\"".format (gdb.parse_and_eval ("mono_method_full_name ({0}, 1)".format (str (int (self.val.cast (gdb.lookup_type ("guint64")))))).string ())

class MonoClassPrinter:
    "Print a MonoClass structure"

    def __init__(self, val):
        self.val = val

    def to_string(self):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "0x0"
        klass = self.val.dereference ()
        class_name = stringify_class_name (klass ["name_space"].string (), klass ["name"].string ())
        return "\"{0}\"".format (class_name)
        # This returns more info but requires calling into the inferior
        #return "\"{0}\"".format (gdb.parse_and_eval ("mono_type_full_name (&((MonoClass*){0})->byval_arg)".format (str (int ((self.val).cast (gdb.lookup_type ("guint64")))))))

def lookup_pretty_printer(val):
    t = str (val.type)
    if t == "object":
        return ObjectPrinter (val)
    if t[0:5] == "class" and t[-1] == "&":
        return ObjectPrinter (val)    
    if t == "string":
        return StringPrinter (val)
    if t == "MonoMethod *":
        return MonoMethodPrinter (val)
    if t == "MonoClass *":
        return MonoClassPrinter (val)
    return None

def register_csharp_printers(obj):
    "Register C# pretty-printers with objfile Obj."

    if obj == None:
        obj = gdb

    obj.pretty_printers.append (lookup_pretty_printer)

register_csharp_printers (gdb.current_objfile())

class MonoSupport(object):

    def __init__(self):
        self.s_size = 0

    def run_hook(self):
        if os.access ("xdb.s", os.F_OK):
            os.remove ("xdb.s")
        gdb.execute ("set environment MONO_XDEBUG gdb")
        
    def stop_hook(self):
        # Called when the program is stopped
        # Need to recompile+reload the xdb.s file if needed
        # FIXME: Need to detect half-written files created when the child is
        # interrupted while executing the xdb.s writing code
        # FIXME: Handle appdomain unload
        if os.access ("xdb.s", os.F_OK):
            new_size = os.stat ("xdb.s").st_size
            if new_size > self.s_size:
                sofile = "xdb.so"
                gdb.execute ("shell as -o xdb.o xdb.s && ld -shared -o {} xdb.o".format (sofile))
                # FIXME: This prints messages which couldn't be turned off
                gdb.execute ("add-symbol-file {} 0".format (sofile))
                self.s_size = new_size

class RunHook (gdb.Command):
    def __init__ (self):
        super (RunHook, self).__init__ ("hook-run", gdb.COMMAND_NONE,
                                        gdb.COMPLETE_COMMAND, pre_hook_of="run")

    def invoke(self, arg, from_tty):
        mono_support.run_hook ()

print ("Mono support loaded.")

mono_support = MonoSupport ()

# This depends on the changes in gdb-python.diff to work
#RunHook ()

# Register our hooks
# This currently cannot be done from python code

exec_file = gdb.current_objfile ().filename
# FIXME: Is there a way to detect symbolic links ?
if os.stat (exec_file).st_size != os.lstat (exec_file).st_size:
    exec_file = os.readlink (exec_file)
exec_dir = os.path.dirname (exec_file)
gdb.execute ("source {0}/{1}-gdbinit".format (exec_dir, os.path.basename (exec_file)))
