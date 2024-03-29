﻿// This file is part of the Tigra.Gravatar.LogFetcher project
//
// Copyright © 2013 TiGra Networks, all rights reserved.
//
// File: Options.cs  Created: 2013-10-17@07:18
// Last modified: 2013-10-17@07:52 by Tim

using CommandLine;

namespace Tigra.Gravatar.LogFetcher
    {
    internal class Options
        {
        [Option('r', "repository", Required = false, Default = ".",
            HelpText = "Path to Git repository (working copy)")]
        public string Repository { get; set; }

        [Option('o', "output", Required = false, Default = @".\Gravatars",
            HelpText = "Output directory for Gravatar images")]
        public string OutputDirectory { get; set; }

        [Option('f', "format", Required = false, Default = "png", HelpText = "Gravatar image format (png, gif, jpg, etc.)")]
        public string ImageFormat { get; set; }

        [Option('s', "size", Required = false, Default = 90, HelpText = "Size of Gravatar images (in pixels).")]
        public int GravatarSize { get; set; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Show diagnostic information")]
        public bool Verbose { get; set; }
        }
    }
