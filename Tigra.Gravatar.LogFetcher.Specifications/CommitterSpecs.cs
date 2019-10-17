// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: CommitterSpecs.cs  Created: 2013-07-07@19:31
// Last modified: 2013-07-07@19:56 by Tim

using Machine.Specifications;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    [Subject(typeof(Committer), "Gravatar hash")]
    public class when_computing_the_gravatar_hash_for_a_committer
        {
        /**
         * Gravatar hash rules are at: https://en.gravatar.com/site/implement/hash/
         * In summary:
         * All URLs on Gravatar are based on the use of the hashed value of an email address.
         * Images and profiles are both accessed via the hash of an email, and it is considered
         * the primary way of identifying an identity within the system. To ensure a consistent
         * and accurate hash, the following steps should be taken to create a hash:
         * 1. Trim leading and trailing whitespace from an email address
         * 2. Force all characters to lower-case
         * 3. md5 hash the final string
         * 
         * Note that .NET does not return the required hexadecimal representation, so the hash code
         * needs to be trasformed into the correct string format.
         */
        It should_compute_the_hash_according_to_gravatar_rules =
            () =>
            Committer.GetGravatarMd5Hash("tim@tigranetworks.co.uk").ShouldEqual("df0478426c0e47cc5e557d5391e5255d");
        It should_compute_the_hash_based_on_all_lower_case_characters =
            () =>
            Committer.GetGravatarMd5Hash("TIM@tigranetworks.co.uk").ShouldEqual("df0478426c0e47cc5e557d5391e5255d");
        It should_ignore_leading_and_trailing_white_space =
            () =>
            Committer.GetGravatarMd5Hash(" tim@tigranetworks.co.uk ").ShouldEqual("df0478426c0e47cc5e557d5391e5255d");
        }
    }
