// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: FakeFileSystemWrapper.cs  Created: 2013-06-29@12:09
// Last modified: 2013-07-07@17:56 by Tim

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global   (used in fakes)

namespace Tigra.Gravatar.LogFetcher
    {
    /// <summary>
    ///   Class FakeFileSystemWrapper - an abstraction over file system services.
    ///   This class consists mainly of virtual methods and exists primarily to aid testability.
    /// </summary>
    public class FakeFileSystemWrapper
        {
        /// <summary>
        ///   Determines whether the specified path exists as a directory in the file system.
        ///   A path that is invalid, null or empty is deemed not to exist.
        /// </summary>
        /// <param name="path">The directory path to be tested.</param>
        /// <returns>
        ///   <c>true</c> if the path is valid under file system rules and resolves to a directory which exists,
        ///   <c>false</c> otherwise
        /// </returns>
        /// <exception cref=""></exception>
        public virtual bool DirectoryExists(string path)
            {
            return Directory.Exists(path);
            }

        /// <summary>
        /// Combines two path segments into a single fully qualified path.
        /// <see cref="Path.Combine(string, string)"/> for full details.
        /// </summary>
        /// <param name="path1">The first path segment.</param>
        /// <param name="path2">The second path segment.</param>
        /// <returns>A string containing the fully qualified combined path.</returns>
        public virtual string PathCombine(string path1, string path2)
            {
            return Path.Combine(path1, path2);
            }

        public virtual string GetFullPath(string path)
            {
            return Path.GetFullPath(path);
            }

        public virtual void SaveImage(string path, Bitmap image, ImageFormat format)
            {
            image.Save(path, ImageFormat.Png);
            }
        }
    }
