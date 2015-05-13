/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.

using System;
using System.Collections.Generic;
using System.Text;


namespace System.Management.Internal
{
    internal class CimTreeNode
    {
        //private CimTreeNode _parent;

        //public CimTreeNode Parent
        //{
        //    get { return _parent; }
        //    set { _parent = value; }
        //}
        private CimName _name;
        private CimTreeNodeList _children = null;

        #region Constructors
        public CimTreeNode()
        {
        }

        public CimTreeNode(CimName name)
        {
            Name = name;
        }
        #endregion

        #region Properties and Indexers
        public CimName Name
        {
            get 
            {
                if (_name == null)
                    _name = new CimName(null);

                return _name; 
            }
            set { _name = value; }
        }

        public int TreeSize
        {
            get
            {
                int curSize = 0;

                if (Children.Count == 0)
                {
                    curSize = 1;
                }
                else
                {
                    //foreach (CimTreeNode curChild in Children)
                    //{
                    for (int i = 0; i < Children.Count; i++)
        			{
		                curSize += Children[i].TreeSize;
                    }
                }

                return curSize;
            }
        }

        public CimTreeNodeList Children
        {
            get 
            {
                if (_children == null)
                    _children = new CimTreeNodeList();

                return _children; 
            }
            set { _children = value; }
        }
        #endregion
    }
}
