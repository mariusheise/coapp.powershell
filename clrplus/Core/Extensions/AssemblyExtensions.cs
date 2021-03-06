﻿//-----------------------------------------------------------------------
// <copyright company="CoApp Project" >
//     Original Copyright (c) 2009 Microsoft Corporation. All rights reserved.
//     Changes Copyright (c) 2010-2013 Garrett Serack and CoApp Contributors. 
//     Contributors can be discovered using the 'git log' command.
//     All rights reserved.
// </copyright>
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------

// -----------------------------------------------------------------------
// Original Code: 
// (c) 2009 Microsoft Corporation -- All rights reserved
// This code is licensed under the MS-PL
// http://www.opensource.org/licenses/ms-pl.html
// Courtesy of the Open Source Techology Center: http://port25.technet.com
// -----------------------------------------------------------------------

namespace ClrPlus.Core.Extensions {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Exceptions;
    using Utility;

    /// <summary>
    ///     Extension methods to work with Assemblies.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class AssemblyExtensions {
        /// <summary>
        ///     a "logo" of an assembly
        /// </summary>
        private static string _logo;

        static AssemblyExtensions() {
            AssemblyResolver.Initialize();
        }

        public static string Logo(this Assembly assembly) {
            if (_logo == null) {
                var assemblycomments = assembly.Comments();
                assemblycomments = string.IsNullOrEmpty(assemblycomments) ? string.Empty : "\r\n" + assemblycomments;

                _logo =
                    @"{0} {1} Version {2} for {3}
{4}. All rights reserved{5}
-------------------------------------------------------------------------------".format(assembly.Company(), assembly.Title(), assembly.Version(), IntPtr.Size == 8 ? "x64" : "x86",
                        assembly.Copyright().Replace("©", "(c)"), assemblycomments);
            }
            return _logo;
        }

        public static void SetLogo(this Assembly assembly, string logoText) {
            _logo = logoText;
        }

        /// <summary>
        ///     Geths the assembly for a given object.
        /// </summary>
        /// <param name="obj"> The obj. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static Assembly Assembly(this object obj) {
            return obj.GetType().Assembly;
        }

        /// <summary>
        ///     Extracts the title of an assembly (TitleAttribute)
        /// </summary>
        /// <param name="assembly"> The assembly. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string Title(this Assembly assembly) {
            try {
                return ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof (AssemblyTitleAttribute))).Title;
            } catch {
            }
            return string.Empty;
        }

        /// <summary>
        ///     Extracts the Description of the assembly (DescriptionAttribute)
        /// </summary>
        /// <param name="assembly"> The assembly. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string Description(this Assembly assembly) {
            try {
                return ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof (AssemblyDescriptionAttribute))).Description;
            } catch {
            }
            return string.Empty;
        }

        public static string Version(this Assembly assembly) {
            try {
                var vi = FileVersionInfo.GetVersionInfo(assembly.Location);

                return "{0}.{1}.{2}.{3}".format(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
            } catch {
            }
            return string.Empty;
        }

        public static string ExtractFileResourceToPath(this Assembly assembly, string name, string filePath) {
            name = assembly.ResolveManifestResource(name, out assembly);
            if (assembly != null) {
                var s = assembly.GetManifestResourceStream(name);
                if (s != null) {
                    var buf = new byte[s.Length];

                    var targetFile = new FileStream(filePath, FileMode.Create);
                    var sz = s.Read(buf, 0, buf.Length);
                    targetFile.Write(buf, 0, sz);
                    s.Close();
                    targetFile.Close();
                    return filePath;
                }
            }
            throw new ClrPlusException("Resource '{0}' not found in assembly.".format(name));
        }

        public static string ExtractFileResource(this Assembly assembly, string name) {
            name = assembly.ResolveManifestResource(name, out assembly);
            if (assembly != null) {
                using (var s = new StreamReader(assembly.GetManifestResourceStream(name))) {
                    return s.ReadToEnd();
                }
            }
            throw new ClrPlusException("Resource '{0}' not found in assembly.".format(name));
        }

        public static string ResolveManifestResource(this Assembly assembly, string name, out Assembly actualAssembly) {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    var n = a.GetManifestResourceNames().FirstOrDefault(each => each.EndsWith(name, StringComparison.CurrentCultureIgnoreCase));
                    if (n != null) {
                        actualAssembly = a;
                        return n;
                    }
                } catch {
                    
                }
            }

            actualAssembly = null;
            return null;
        }

#if TODO 
    // if we dont' use this, maybe we should just remove it and have people use the above method.
    // warning: case sensitive. 
        public static string ExtractFileResourceToTemp(this Assembly assembly, string name) {
            var tempPath = name.GenerateTemporaryFilename();
            var s = assembly.GetManifestResourceStream(name);
            if (s == null) {
                // not specified exactly
                var n = assembly.GetManifestResourceNames();
                foreach (var each in n) {
                    if (each.EndsWith("." + name)) {
                        name = each;
                        break;
                    }
                }
            }
            return ExtractFileResourceToPath(assembly, name, tempPath);
        }
#endif

        public static string Copyright(this Assembly assembly) {
            try {
                return FileVersionInfo.GetVersionInfo(assembly.Location).LegalCopyright;
            } catch {
            }
            return string.Empty;
        }

        public static string Company(this Assembly assembly) {
            try {
                return FileVersionInfo.GetVersionInfo(assembly.Location).CompanyName;
            } catch {
            }
            return string.Empty;
        }

        public static string Comments(this Assembly assembly) {
            try {
                return FileVersionInfo.GetVersionInfo(assembly.Location).Comments;
            } catch {
            }
            return string.Empty;
        }
    }
}