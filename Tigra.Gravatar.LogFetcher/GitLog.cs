using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TiGra;

namespace Tigra.Gravatar.LogFetcher
    {
    public class GitLog
        {
        const string GitLogCommitterRegex = @"^(?<email>[^|]+)\|(?<name>.*)$";
        FileSystemHelper fileSystem;

        /// <summary>
        ///   Initializes a new instance of the <see cref="GitLog" /> class.
        /// </summary>
        /// <param name="pathToWorkingCopy">The path to a Git working copy.</param>
        /// <param name="fileSystem">A helper class that provides file system services (optional).</param>
        /// <exception cref="ArgumentException">Thrown if the path is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no Git repository at the specified path.</exception>
        public GitLog(string pathToWorkingCopy, FileSystemHelper fileSystem = null)
            {
            this.fileSystem = fileSystem ?? new FileSystemHelper();
            string fullPath = fileSystem.GetFullPath(pathToWorkingCopy); // ArgumentException if path invalid.
            if (!fileSystem.DirectoryExists(fullPath))
                throw new ArgumentException("The specified working copy directory does not exist.");
            GitWorkingCopyPath = pathToWorkingCopy;
            string git = fileSystem.PathCombine(fullPath, ".git");
            if (!fileSystem.DirectoryExists(git))
                {
                throw new InvalidOperationException(
                    "There does not appear to be a Git repository at the specified location.");
                }
            }

        public string GitWorkingCopyPath { get; set; }

        /// <summary>
        /// Launches a command line process with the command
        /// <c>git --git-dir="{path-to-working-copy}\.git" log --pretty=format:"%ae|%an"</c>
        /// and captures stdout into a StreamReader object.
        /// </summary>
        /// <param name="psi">
        /// The process start info containing the executable to be run.
        /// Dependency Injection: a script can be supplied to mock out Git.
        /// If null is passed, then "git.exe" is used by default.
        /// </param>
        /// <returns>A StreamReader hooked up to the stdout of the git command process.</returns>
        public StreamReader GetLogStream(ProcessStartInfo psi)
            {
            /*
             * ToDo: By experimentation, use this command: git log --pretty=format:"%ae|%an"
             * This will produce an output in this format:
             * tim@tigranetworks.co.uk|Tim Long     <-- note lower case
             * Tim@tigranetworks.co.uk|Tim Long     <-- note mixed case - these are equivalent
             * Tim@tigranetworks.co.uk|Tim Long     <-- note duplicate entries, these must be filtered
             * Tim@tigranetworks.co.uk|Tim Long
             * Tim@tigranetworks.co.uk|Tim Long
             * fernjampel@hotmail.co.uk|Fern Hughes
             * Tim@tigranetworks.co.uk|Tim Long
             * Tim@tigranetworks.co.uk|Tim Long
             *
             * TO get to a repository directly, use a command like this (note the trailing '.git'
             * git --git-dir="C:\Users\Tim\Documents\Visual Studio 2012\Projects\PowerThreading\.git" log --pretty=format:"%ae|%an"
             * There is no direct access to the log of a remote repository. It must first be fetched, then log operations can be performed locally.
             */
            const string gitArgumentFormat = "--git-dir=\"{0}\\.git\" log --pretty=format:\"%ae|%an\"";
            if (psi == null)
                {
                psi = new ProcessStartInfo("git.exe");
                psi.Arguments = string.Format(gitArgumentFormat, GitWorkingCopyPath);
                }
            psi.CreateNoWindow = true;
            psi.RedirectStandardError = false;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            var process = Process.Start(psi);
            return process.StandardOutput;
            }

        public IEnumerable<Committer> GetListOfUniqueCommitters(ProcessStartInfo gitPsi)
            {
            var logStream = GetLogStream(gitPsi);
            var uniqueCommitters = new SortedSet<Committer>();
            var committersAdded = 0;
            while (!logStream.EndOfStream)
                {
                try
                    {
                    var logLine = logStream.ReadLine();
                    Diagnostics.TraceVerbose("Read from log stream => {0}", logLine);
                    var parts = logLine.Split('|');
                    if (parts.Length != 2)
                        continue;
                    if (string.IsNullOrEmpty(parts[0])) // Must have a non-blank email address
                        continue;
                    var committer = new Committer(parts[1], parts[0]);
                    var added = uniqueCommitters.Add(committer);
                    Diagnostics.TraceVerbose("Committer={0}, Added={1}", committer,added);
                    if (added)
                        ++committersAdded;
                    }
                catch (Exception ex)
                    {
                    Diagnostics.TraceError(ex);
                    }
                }
            Diagnostics.TraceVerbose("Added a total of {0} committers", committersAdded);
            return uniqueCommitters;
            }
        }
    }