﻿// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GravatarFetcher.cs  Created: 2013-06-29@12:13
// Last modified: 2013-06-30@13:54 by Tim

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /// <summary>
    ///   Class GravatarFetcher - fetches Gravatar images from the Gravatar web service
    /// </summary>
    public class GravatarFetcher
        {
        const string GravatarBaseUrl = "http://www.gravatar.com/avatar/";
        const string QueryString = "{0}?d=404&size={1}";
        readonly StreamReader reader;
        readonly HttpClient httpClient;

        /// <summary>
        ///   Initializes a new instance of the <see cref="GravatarFetcher" /> class.
        /// </summary>
        /// <param name="reader">The stream reader over the Git log data (dependency injection).</param>
        /// <param name="client">
        ///   An HttpClient to be used for making web requests (dependency injection).
        ///   If null or omitted, then a new instance is created internally.
        /// </param>
        public GravatarFetcher(StreamReader reader, HttpClient client = null)
            {
            this.reader = reader;
            httpClient = client ?? new HttpClient();
            httpClient.BaseAddress = new Uri(GravatarBaseUrl);
            UniqueCommitters = new List<Committer>();
            ParseCommitLog();
            }

        void ParseCommitLog()
            {
            UniqueCommitters = new List<Committer>();

            while (!reader.EndOfStream)
                {
                string logLine = reader.ReadLine();
                if (string.IsNullOrEmpty(logLine))
                    continue;
                string[] splitLine = logLine.Split('|');
                if (splitLine.Length != 2)
                    Debug.WriteLine("Skipping invalid line: " + logLine);
                var committer = new Committer(name: splitLine[0], emailAddress: splitLine[1]);
                if (UniqueCommitters.Contains(committer))
                    continue;

                UniqueCommitters.Add(committer);
                }
            }

        /// <summary>
        ///   Fetches the gravatar image for each person in the UniqueCommitters collection and saves to the specified path.
        ///   Images will be in PNG format and will be named as the person's full name.
        /// </summary>
        /// <param name="saveTo">The path to save the images to.</param>
        public void FetchGravatars(string saveTo)
            {
            string imagePath = Path.GetFullPath(saveTo); // Throws if the path is invalid.
            foreach (Committer committer in UniqueCommitters)
                FetchSingleGravatar(committer, imagePath);
            }

        /// <summary>
        ///   Fetches the gravatar image for a single committer and saves it to the specified path.
        /// </summary>
        /// <param name="committer">The committer.</param>
        /// <param name="imagePath">The image path.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        async void FetchSingleGravatar(Committer committer, string imagePath)
            {
            var gravatarId = Committer.GetGravatarMd5Hash(committer.EmailAddress);
            var result = await httpClient.GetAsync(string.Format(QueryString, gravatarId, 90));
            //ToDo: extract the image bytes and save to a file.
            }

        /// <summary>
        ///   Gets the unique committer count.
        /// </summary>
        /// <value>The unique committer count.</value>
        public int UniqueCommitterCount { get { return UniqueCommitters.Count; } }

        /// <summary>
        ///   Gets the collection of unique committers.
        /// </summary>
        /// <value>The unique committers.</value>
        public IList<Committer> UniqueCommitters { get; internal set; }
        }
    }
