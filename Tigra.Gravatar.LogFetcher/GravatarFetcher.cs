// This file is part of the Tigra.Gravatar.LogFetcher project
//
// Copyright © 2016-2019 Tigra Astronomy, all rights reserved.
//
// File: GravatarFetcher.cs  Last modified: 2019-10-16@21:47 by Tim Long

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NLog.Fluent;

namespace Tigra.Gravatar.LogFetcher
{
    /// <summary>
    ///     Class GravatarFetcher - fetches Gravatar images from the Gravatar web service
    /// </summary>
    public class GravatarFetcher
    {
        private const string GravatarBaseUrl = "http://www.gravatar.com/avatar/";
        private const string QueryString = "{0}.png?default=404&size={1}&rating=g";
        private readonly HttpClient httpClient;
        private readonly FakeFileSystemWrapper fileSystem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GravatarFetcher" /> class.
        /// </summary>
        /// <param name="committers">
        ///     The list of committers. Ideally, this should already have been de-duplicated.
        ///     Duplicate entries are not necessarily fatal but could result in redundant
        ///     web requests and possibly some failed operations due to file locking semantics.
        /// </param>
        /// <param name="client">
        ///     An HttpClient to be used for making web requests (dependency injection).
        ///     If null or omitted, then a new instance is created internally.
        /// </param>
        /// <param name="filesystem">
        ///     A <see cref="FakeFileSystemWrapper" /> to be used for accessing the file system (dependency injection).
        ///     If null or omitted, then a new instance is created internally.
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
        ///     Gets the unique committer count.
        /// </summary>
        /// <value>The unique committer count.</value>
        public int UniqueCommitterCount => UniqueCommitters.Count();

        /// <summary>
        ///     Gets the collection of unique committers.
        /// </summary>
        /// <value>The unique committers.</value>
        public IEnumerable<Committer> UniqueCommitters { get; internal set; }

        public Task FetchGravatars(string saveTo, IList<string> errorsOut)
            {
            var imagePath = Path.GetFullPath(saveTo); // Throws if the path is invalid.
            var pendingTasks = new List<Task>();
            foreach (var committer in UniqueCommitters)
                {
                var task = FetchSingleGravatar(committer, imagePath);
                pendingTasks.Add(task);
                }
            try
                {
                return Task.WhenAll(pendingTasks);
                }
            catch (AggregateException ae)
                {
                errorsOut.Add("Fetching Gravatar images resulted in multiple errors as follows:");
                foreach (var exception in ae.InnerExceptions)
                    {
                    errorsOut.Add(exception.Message);
                    }
                }
            catch (Exception ex)
                {
                errorsOut.Add($"Unhandled exception: {ex.Message}");
                }
            return Task.Delay(0);
            }

        /// <summary>
        ///     Fetches the gravatar image for each person in the UniqueCommitters collection and saves to the specified path.
        ///     Images will be in PNG format and will be named as the person's full name.
        /// </summary>
        /// <param name="saveTo">The path to save the images to.</param>
        public void FetchGravatarsSynchronously(string saveTo)
        {
            var imagePath = Path.GetFullPath(saveTo); // Throws if the path is invalid.
            foreach (var committer in UniqueCommitters)
            {
                FetchSingleGravatarSynchronously(committer, imagePath);
            }
        }

        /// <summary>
        ///     Fetches the gravatar image for a single committer and saves it to the specified path.
        /// </summary>
        /// <param name="committer">The committer.</param>
        /// <param name="imagePath">The image path.</param>
        /// <param name="fileFormat"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal async Task FetchSingleGravatar(Committer committer, string imagePath)
            {
            var gravatarId = Committer.GetGravatarMd5Hash(committer.EmailAddress);
            var query = string.Format(QueryString, gravatarId, 90);
            var result = await httpClient.GetAsync(query);
            Log.Debug()
                .Message("HTTP Status {status} retrieving image for committer {committer}",
                    result.StatusCode,
                    committer)
                .Write();

            if (result.IsSuccessStatusCode)
                {
                var imageStream = await result.Content.ReadAsStreamAsync();
                var bitmap = new Bitmap(imageStream);
                var filename = committer.Name + ".png";
                var fileToSave = Path.Combine(imagePath, filename);
                fileSystem.SaveImage(fileToSave, bitmap, ImageFormat.Png);
                Log.Info().Message("Saved {committer} => {filename}", committer, filename).Write();
                }
            else
                {
                Log.Info().Message("Unable to retrieve Gravatar for {committer}", committer).Write();
                }
            }
        internal void FetchSingleGravatarSynchronously(Committer committer, string imagePath)
            {
            var gravatarId = Committer.GetGravatarMd5Hash(committer.EmailAddress);
            var query = string.Format(QueryString, gravatarId, 90);
            var result = httpClient.GetAsync(query).Result;
            Log.Debug()
                .Message("HTTP Status {status} retrieving image for committer {committer}",
                    result.StatusCode,
                    committer)
                .Write();

            if (result.IsSuccessStatusCode)
                {
                var imageStream = result.Content.ReadAsStreamAsync().Result;
                var bitmap = new Bitmap(imageStream);
                var filename = committer.Name + ".png";
                var fileToSave = Path.Combine(imagePath, filename);
                fileSystem.SaveImage(fileToSave, bitmap, ImageFormat.Png);
                Log.Info().Message("Saved {committer} => {filename}", committer, filename).Write();
                }
            else
                {
                Log.Info().Message("Unable to retrieve Gravatar for {committer}", committer).Write();
                }
            }
        }
    }