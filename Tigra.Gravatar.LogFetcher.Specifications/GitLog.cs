using System;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    public class GitLog
        {
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
        }
    }