// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: FileSystemHelper.cs  Created: 2013-06-24@20:38
// Last modified: 2013-06-24@20:43 by Tim

using System.IO;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global   (used in fakes)

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /// <summary>
    ///   Class FileSystemHelper - a mockable wrapper class that aids testability
    /// </summary>
    public class FileSystemHelper
        {
        public virtual bool DirectoryExists(string path)
            {
            return Directory.Exists(path);
            }

        public virtual string PathCombine(string path1, string path2)
            {
            return Path.Combine(path1, path2);
            }

        public virtual string GetFullPath(string path)
            {
            return Path.GetFullPath(path);
            }
        }
    }
