#
# Author: Zoltan Varga (vargaz@gmail.com)
# License: MIT/X11
#

#
# This is a mono support mode for gdb 7.0 and later
# Usage:
# - copy/symlink this file to the directory where the mono executable lives.
# - run mono under gdb, or attach to a mono process started with --debug=gdb using gdb.
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
                c = unichr (val)
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
        return "{0} [{1}]".format (stringify_class_name (self.class_ns, self.class_name [0:len(self.class_name) - 2]), int(length))
        
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
            try:
                gdb_type = gdb.lookup_type ("struct {0}_{1}".format (class_ns.replace (".", "_"), class_name))
                return self._iterator(obj.cast (gdb_type))
            except:
                return {}.__iter__ ()
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

    def to_string_inner(self, add_quotes):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "0x0"
        klass = self.val.dereference ()
        class_name = stringify_class_name (klass ["name_space"].string (), klass ["name"].string ())
        if add_quotes:
            return "\"{0}\"".format (class_name)
        else:
            return class_name
        # This returns more info but requires calling into the inferior
        #return "\"{0}\"".format (gdb.parse_and_eval ("mono_type_full_name (&((MonoClass*){0})->byval_arg)".format (str (int ((self.val).cast (gdb.lookup_type ("guint64")))))))

    def to_string(self):
        try:
            return self.to_string_inner (True)
        except:
            #print (sys.exc_info ()[0])
            #print (sys.exc_info ()[1])
            return str(self.val.cast (gdb.lookup_type ("gpointer")))

class MonoGenericInstPrinter:
    "Print a MonoGenericInst structure"

    def __init__(self, val):
        self.val = val

    def to_string(self):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "0x0"
        inst = self.val.dereference ()
        inst_len = inst ["type_argc"]
        inst_args = inst ["type_argv"]
        inst_str = ""
        for i in range(0, inst_len):
            # print (inst_args)
            type_printer = MonoTypePrinter (inst_args [i])
            if i > 0:
                inst_str = inst_str + ", "
            inst_str = inst_str + type_printer.to_string ()
        return inst_str

class MonoGenericClassPrinter:
    "Print a MonoGenericClass structure"

    def __init__(self, val):
        self.val = val

    def to_string_inner(self):
        gclass = self.val.dereference ()
        container_str = str(gclass ["container_class"])
        class_inst = gclass ["context"]["class_inst"]
        class_inst_str = ""
        if int(class_inst.cast (gdb.lookup_type ("guint64"))) != 0:
            class_inst_str  = str(class_inst)
        method_inst = gclass ["context"]["method_inst"]
        method_inst_str = ""
        if int(method_inst.cast (gdb.lookup_type ("guint64"))) != 0:
            method_inst_str  = str(method_inst)
        return "{0}, [{1}], [{2}]>".format (container_str, class_inst_str, method_inst_str)

    def to_string(self):
        try:
            return self.to_string_inner ()
        except:
            #print (sys.exc_info ()[0])
            #print (sys.exc_info ()[1])
            return str(self.val.cast (gdb.lookup_type ("gpointer")))

class MonoTypePrinter:
    "Print a MonoType structure"

    def __init__(self, val):
        self.val = val

    def to_string_inner(self, csharp):
        try:
            t = self.val.referenced_value ()

            kind = str (t ["type"]).replace ("MONO_TYPE_", "").lower ()
            info = ""

            if kind == "class":
                p = MonoClassPrinter(t ["data"]["klass"])
                info = p.to_string ()
            elif kind == "genericinst":
                info = str(t ["data"]["generic_class"])

            if info != "":
                return "{{{0}, {1}}}".format (kind, info)
            else:
                return "{{{0}}}".format (kind)
        except:
            #print (sys.exc_info ()[0])
            #print (sys.exc_info ()[1])
            return str(self.val.cast (gdb.lookup_type ("gpointer")))

    def to_string(self):
        return self.to_string_inner (False)

class MonoMethodRgctxPrinter:
    "Print a MonoMethodRgctx structure"

    def __init__(self, val):
        self.val = val

    def to_string(self):
        rgctx = self.val.dereference ()
        klass = rgctx ["class_vtable"].dereference () ["klass"]
        klass_printer = MonoClassPrinter (klass)
        inst = rgctx ["method_inst"].dereference ()
        inst_len = inst ["type_argc"]
        inst_args = inst ["type_argv"]
        inst_str = ""
        for i in range(0, inst_len):
            # print (inst_args)
            type_printer = MonoTypePrinter (inst_args [i])
            if i > 0:
                inst_str = inst_str + ", "
            inst_str = inst_str + type_printer.to_string ()
        return "MRGCTX[{0}, [{1}]]".format (klass_printer.to_string(), inst_str)

class MonoVTablePrinter:
    "Print a MonoVTable structure"

    def __init__(self, val):
        self.val = val

    def to_string(self):
        if int(self.val.cast (gdb.lookup_type ("guint64"))) == 0:
            return "0x0"
        vtable = self.val.dereference ()
        klass = vtable ["klass"]
        klass_printer = MonoClassPrinter (klass)

        return "vtable({0})".format (klass_printer.to_string ())

def lookup_pretty_printer(val):
    t = str (val.type)
    if t == "object":
        return ObjectPrinter (val)
    if t[0:5] == "class" and t[-1] == "&":
        return ObjectPrinter (val)    
    if t == "string":
        return StringPrinter (val)
    if t == "MonoString *":
        return StringPrinter (val)
    if t == "MonoMethod *":
        return MonoMethodPrinter (val)
    if t == "MonoClass *":
        return MonoClassPrinter (val)
    if t == "MonoType *":
        return MonoTypePrinter (val)
    if t == "MonoGenericInst *":
        return MonoGenericInstPrinter (val)
    if t == "MonoGenericClass *":
        return MonoGenericClassPrinter (val)
    if t == "MonoMethodRuntimeGenericContext *":
        return MonoMethodRgctxPrinter (val)
    if t == "MonoVTable *":
        return MonoVTablePrinter (val)
    return None

def register_csharp_printers(obj):
    "Register C# pretty-printers with objfile Obj."

    if obj == None:
        obj = gdb

    obj.pretty_printers.append (lookup_pretty_printer)

# This command will flush the debugging info collected by the runtime
class XdbCommand (gdb.Command):
    def __init__ (self):
        super (XdbCommand, self).__init__ ("xdb", gdb.COMMAND_NONE,
                                           gdb.COMPLETE_COMMAND)

    def invoke(self, arg, from_tty):
        gdb.execute ("call mono_xdebug_flush ()")

register_csharp_printers (gdb.current_objfile())

XdbCommand ()

gdb.execute ("set environment MONO_XDEBUG gdb")

print ("Mono support loaded.")


