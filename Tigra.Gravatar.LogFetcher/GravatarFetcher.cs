// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GravatarFetcher.cs  Created: 2013-06-29@12:13
// Last modified: 2013-10-13@21:25 by Tim

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TiGra;

namespace Tigra.Gravatar.LogFetcher
    {
    /// <summary>
    ///   Class GravatarFetcher - fetches Gravatar images from the Gravatar web service
    /// </summary>
    public class GravatarFetcher
        {
        const string GravatarBaseUrl = "http://www.gravatar.com/avatar/";
        const string QueryString = "{0}.png?default=404&size={1}&rating=g";
        readonly StreamReader reader;
        readonly HttpClient httpClient;
        FakeFileSystemWrapper fileSystem;

        /// <summary>
        ///   Initializes a new instance of the <see cref="GravatarFetcher" /> class.
        /// </summary>
        /// <param name="committers">
        ///   The list of committers. Ideally, this should already have been de-duplicated.
        ///   Duplicate entries are not necessarily fatal but could result in redundant
        ///   web requests and possibly some failed operations due to file locking semantics.
        /// </param>
        /// <param name="client">
        ///   An HttpClient to be used for making web requests (dependency injection).
        ///   If null or omitted, then a new instance is created internally.
        /// </param>
        /// <param name="filesystem">
        ///   A <see cref="FakeFileSystemWrapper" /> to be used for accessing the file system (dependency injection).
        ///   If null or omitted, then a new instance is created internally.
        /// </param>
        public GravatarFetcher(IEnumerable<Committer> committers,
            HttpClient client = null,
            FakeFileSystemWrapper filesystem = null)
            {
            fileSystem = filesystem ?? new FakeFileSystemWrapper();
            httpClient = client ?? new HttpClient();
            httpClient.BaseAddress = new Uri(GravatarBaseUrl);
            UniqueCommitters = committers;
            }

        /// <summary>
        ///   Fetches the gravatar image for each person in the UniqueCommitters collection and saves to the specified path.
        ///   Images will be in PNG format and will be named as the person's full name.
        /// </summary>
        /// <param name="saveTo">The path to save the images to.</param>
        public void FetchGravatarsSynchronously(string saveTo)
            {
            string imagePath = Path.GetFullPath(saveTo); // Throws if the path is invalid.
            foreach (Committer committer in UniqueCommitters)
                {
                FetchSingleGravatar(committer, imagePath);
                }
            }

        public async Task<Task> FetchGravatars(string saveTo)
            {
                string imagePath = Path.GetFullPath(saveTo); // Throws if the path is invalid.
                var pendingTasks = new List<Task>();
                foreach (Committer committer in UniqueCommitters)
                {
                    var task = FetchSingleGravatar(committer, imagePath);
                    pendingTasks.Add(task);
                }
            var result =  Task.WhenAll(pendingTasks);
            return result;
            }

        /// <summary>
        ///   Fetches the gravatar image for a single committer and saves it to the specified path.
        /// </summary>
        /// <param name="committer">The committer.</param>
        /// <param name="imagePath">The image path.</param>
        /// <param name="fileFormat"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal async Task FetchSingleGravatar(Committer committer, string imagePath)
            {
            string gravatarId = Committer.GetGravatarMd5Hash(committer.EmailAddress);
            string query = string.Format(QueryString, gravatarId, 90);
            HttpResponseMessage result = await httpClient.GetAsync(query);
            //ToDo: extract the image bytes and save to a file.
            Stream imageStream = await result.Content.ReadAsStreamAsync();
            var bitmap = new Bitmap(imageStream);
            var filename = gravatarId + ".png";
            var fileToSave = Path.Combine(imagePath, filename);
            fileSystem.SaveImage(fileToSave, bitmap, ImageFormat.Png);
            Diagnostics.TraceInfo("Saved {0} => {1}", committer, filename);
            Console.WriteLine("Saved {0} => {1}", committer, filename);
            }

        /// <summary>
        ///   Gets the unique committer count.
        /// </summary>
        /// <value>The unique committer count.</value>
        public int UniqueCommitterCount { get { return UniqueCommitters.Count(); } }

        /// <summary>
        ///   Gets the collection of unique committers.
        /// </summary>
        /// <value>The unique committers.</value>
        public IEnumerable<Committer> UniqueCommitters { get; internal set; }
        }
    }
