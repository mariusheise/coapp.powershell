﻿//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     ResourceLib Original Code from http://resourcelib.codeplex.com
//     Original Copyright (c) 2008-2009 Vestris Inc.
//     Changes Copyright (c) 2011 Garrett Serack . All rights reserved.
// </copyright>
// <license>
// MIT License
// You may freely use and distribute this software under the terms of the following license agreement.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE
// </license>
//-----------------------------------------------------------------------

namespace ClrPlus.Windows.PeBinary.ResourceLib {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Api.Enumerations;
    using Api.Structures;

    /// <summary>
    ///     A collection of menu items.
    /// </summary>
    public class MenuExTemplateItemCollection : List<MenuExTemplateItem> {
        /// <summary>
        ///     Read the menu item collection.
        /// </summary>
        /// <param name="lpRes">Address in memory.</param>
        /// <returns>End of the menu item structure.</returns>
        internal IntPtr Read(IntPtr lpRes) {
            while (true) {
                lpRes = ResourceUtil.Align(lpRes.ToInt32());

                var childItem = (MenuExItemTemplate)Marshal.PtrToStructure(lpRes, typeof (MenuExItemTemplate));

                MenuExTemplateItem childMenu = null;
                if ((childItem.bResInfo & (uint)MenuResourceType.Sub) > 0) {
                    childMenu = new MenuExTemplateItemPopup();
                } else {
                    childMenu = new MenuExTemplateItemCommand();
                }

                lpRes = childMenu.Read(lpRes);
                Add(childMenu);

                if ((childItem.bResInfo & (uint)MenuResourceType.Last) > 0) {
                    break;
                }
            }

            return lpRes;
        }

        /// <summary>
        ///     Write the menu collection to a binary stream.
        /// </summary>
        /// <param name="w">Binary stream.</param>
        internal void Write(BinaryWriter w) {
            foreach (var menuItem in this) {
                ResourceUtil.PadToDWORD(w);
                menuItem.Write(w);
            }
        }

        /// <summary>
        ///     String representation in the MENU format.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString() {
            return ToString(0);
        }

        /// <summary>
        ///     String representation in the MENU format.
        /// </summary>
        /// <param name="indent">Indent.</param>
        /// <returns>String representation.</returns>
        public string ToString(int indent) {
            var sb = new StringBuilder();
            if (Count > 0) {
                sb.AppendLine(string.Format("{0}BEGIN", new String(' ', indent)));
                foreach (var child in this) {
                    sb.Append(child.ToString(indent + 1));
                }
                sb.AppendLine(string.Format("{0}END", new String(' ', indent)));
            }
            return sb.ToString();
        }
    }
}