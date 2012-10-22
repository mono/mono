using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace object_traversal
{
    class Program2 : Program
    {
        public static Node d;

    }
    class Program
    {
        [DllImport("__Internal")]
        static extern IntPtr mono_unity_liveness_calculation_from_statics_managed(IntPtr typeHandle);

        [DllImport("__Internal")]
        static extern IntPtr mono_unity_liveness_calculation_from_root_managed(IntPtr rootHandle, IntPtr typeHandle);

        static void Main(string[] args)
        {
            Action[] tests = new Action[] {
                Test1,
                Test2,
                Test3,
                Test4,
                Test5,
                Test6,
                Test7,
                Test8,
                Test9,
                Test10,
                Test11,
            };

            foreach (var test in tests)
            {
                test();
                Program.n1 = null;
                Program.n2 = null;
                Program.n3 = null;
                Program.nnot = null;
                Program.o = null;
                Program.o2 = null;
                Program2.d = null;
            }
        }

        static void VerifyObjects( string label, int expectedCount)
        {
            IntPtr typeHandle = (IntPtr)GCHandle.Alloc(typeof(Node));
            IntPtr gchandle = mono_unity_liveness_calculation_from_statics_managed(typeHandle);

            object[] vals = (object[])((GCHandle)gchandle).Target;

            ((GCHandle)typeHandle).Free();
            ((GCHandle)gchandle).Free();
            if (vals.Length != expectedCount)
            {
                Console.WriteLine("FAILED: {0} expected {1} actual {2}", label, expectedCount, vals.Length);
            }
            else
            {
                Console.WriteLine("PASSED: {0}", label);
            }
        }

        static void VerifyObjects(string label, int expectedCount, object root)
        {
            IntPtr typeHandle = (IntPtr)GCHandle.Alloc(typeof(Node));
            IntPtr rootHandle = (IntPtr)GCHandle.Alloc(root);
            IntPtr gchandle = mono_unity_liveness_calculation_from_root_managed(rootHandle, typeHandle);

            object[] vals = (object[])((GCHandle)gchandle).Target;

            if (typeHandle != IntPtr.Zero)
                ((GCHandle)typeHandle).Free();
            ((GCHandle)rootHandle).Free();
            ((GCHandle)gchandle).Free();
            if (vals.Length != expectedCount)
            {
                Console.WriteLine("FAILED: {0} expected {1} actual {2}", label, expectedCount, vals.Length);
            }
            else
            {
                Console.WriteLine("PASSED: {0}", label);
            }
        }

        static void Test1()
        {
            s = "a" + 1.ToString();
            o = new Node();
            o2 = new NodeNotDerived();
            n1 = new Node();
            n2 = new Node(new Node(), (Node)n1);

            VerifyObjects("Test1", 4);
        }

        static void Test2()
        {
            s = "a" + 1.ToString();
            n1 = new Node();
            n2 = new Node(new Node(), new Node());

            VerifyObjects("Test2", 4);
        }

        static void Test3()
        {
            s = "a" + 1.ToString();
            n1 = new NodeDerived();
            n2 = new Node(new NodeDerived(), null);
            nnot = new NodeNotDerived();

            VerifyObjects("Test3", 3);
        }

        static void Test4()
        {
            Node root = new Node();

            VerifyObjects("Test4", 1, root);
        }

        static void Test5()
        {
            Node root = new Node(new Node(), new NodeDerived());

            VerifyObjects("Test5", 3, root);
        }

        static void Test6()
        {
            Node useMeTwice = new Node();
            Node root = new Node(useMeTwice, useMeTwice);

            VerifyObjects("Test6", 2, root);
        }

        static void Test7()
        {
            Node useMeTwice = new Node();
            Node root = new NodeDerived(useMeTwice, useMeTwice, new Node());

            VerifyObjects("Test7", 3, root);
        }

        static void Test8()
        {
            Program.n1 = null;
            Program.n2 = null;
            Program.nnot = null;
            Program2.d = new Node();

            VerifyObjects("Test8", 1);
        }

        static void Test9()
        {
            Node useMeTwice = new Node();
            Node root = new NodeDerived(useMeTwice, useMeTwice, new Node());
            root.o1 = new Node();
            root.o2 = new NodeNotDerived();

            VerifyObjects("Test9", 4, root);
        }

        static void Test10()
        {
            Node useMeTwice = new Node();
            Node root = new NodeDerived(useMeTwice, useMeTwice, new Node());
            root.o1 = new NodeBase();
            root.o2 = new NodeDerived();

            VerifyObjects("Test10", 4, root);
        }

        static void Test11()
        {
            Node useMeTwice = new Node();
            Node root = new NodeDerived(useMeTwice, useMeTwice, new Node());
            root.o1 = new Node();
            root.o2 = root.o1;

            VerifyObjects("Test11", 4, root);
        }

        static int i = 0;
        static string s;
        public static object o;
        public static object o2;
        public static NodeBase n1;
        public static Node n2;
        public static NodeDerived n3;
        public static NodeNotDerived nnot;
    }

    class NodeBase
    {
    }

    class Node : NodeBase
    {
        Node left;
        Node right;

        public object o1;
        public object o2;

        public Node()
        {
        }

        public Node(Node l, Node r)
        {
            this.left = l;
            this.right = r;
        }
    }

    class NodeDerived : Node
    {
        Node d;

        public NodeDerived()
        {
        }

        public NodeDerived(Node l, Node r, Node d) : base (l, r)
        {
            this.d = d;
        }
    }

    class NodeNotDerived
    {
    }
}
