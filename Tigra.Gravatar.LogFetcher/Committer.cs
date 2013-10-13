// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: Committer.cs  Created: 2013-06-29@12:12
// Last modified: 2013-10-13@20:38 by Tim

using System;
using System.Security.Cryptography;
using System.Text;

namespace Tigra.Gravatar.LogFetcher
    {
    /// <summary>
    ///   Class Committer - represents a person who appears in a Git repository commit log.
    /// </summary>
    public class Committer : IComparable<Committer>
        {
        public string Name { get; private set; }
        public string EmailAddress { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Committer" /> class.
        /// </summary>
        /// <param name="name">The committer's display name.</param>
        /// <param name="emailAddress">The committer's email address.</param>
        public Committer(string name, string emailAddress)
            {
            Name = name;
            EmailAddress = emailAddress;
            }

        /// <summary>
        ///   Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   A value that indicates the relative order of the objects being compared.
        ///   The return value has the following meanings:
        ///   Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.
        ///   Zero This object is equal to <paramref name="other" />.
        ///   Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        /// <remarks>
        ///   Only the <see cref="EmailAddress" /> property is considered. The comparison is performed in
        ///   the current culture and ignores case.
        /// </remarks>
        public int CompareTo(Committer other)
            {
            return string.Compare(EmailAddress, other.EmailAddress, StringComparison.CurrentCultureIgnoreCase);
            }

        public override string ToString()
            {
            return string.Format("{0} <{1}>", Name, EmailAddress);
            }

        /// <summary>
        ///   Determines whether the specified <see cref="System.Object" /> is equal to this instance,
        ///   using some fairly liberal comparison rules. Items are assumed to be equal if any of
        ///   the following are true:
        ///   <list type="number">
        ///     <item>The other object is a reference to the same object in memory (ReferenceEquals)</item>
        ///     <item>
        ///       The other object is a committer and the <see cref="EmailAddress" /> properties are equal.
        ///       Comparisons are performed using the current culture and are not case sensitive.
        ///     </item>
        ///     <item>
        ///       The other object is a string and it matches the <see cref="Name" /> property
        ///     </item>
        ///     <item>
        ///       The other object is a string and it matches the <see cref="EmailAddress" /> property
        ///     </item>
        ///     <item>
        ///       The other object is a string and it matches the <see cref="ToString" /> representation of this committer
        ///     </item>
        ///   </list>
        ///   If none of the above are true, then the objects are considered unequal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
            {
            if (ReferenceEquals(obj, this))
                return true; // The same object therefore always equal.

            if (obj is Committer)
                {
                var other = obj as Committer;
                if (Name == other.Name)
                    return true;
                if (string.Equals(EmailAddress, other.EmailAddress, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                }

            if (obj is string)
                {
                var other = obj as string;
                if (string.Equals(Name, other, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                if (string.Equals(EmailAddress, other, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                if (ToString() == other)
                    return true;
                }
            return false; // Default to not equal
            }

        public override int GetHashCode()
            {
            return ToString().GetHashCode();
            }

        /// <summary>
        ///   Gets the gravatar MD5 hash as specified at https://en.gravatar.com/site/implement/hash/.
        /// </summary>
        /// <param name="email">The email address to hash.</param>
        /// <returns>A string containing the hexadecimal MD5 hash code required by Gravatar.</returns>
        /// <exception cref="ArgumentException">Thrown if the email address is null or empty.</exception>
        public static string GetGravatarMd5Hash(string email)
            {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("You must supply a valid email address");
            var md5 = new MD5CryptoServiceProvider();
            var encoder = new UTF8Encoding();
            var md5Hasher = new MD5CryptoServiceProvider();
            string stringToHash = email.Trim().ToLower();
            byte[] hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(stringToHash));

            var sb = new StringBuilder(hashedBytes.Length*2);
            for (int i = 0; i < hashedBytes.Length; i++)
                sb.Append(hashedBytes[i].ToString("x2"));
            return sb.ToString();
            }
        }
    }
