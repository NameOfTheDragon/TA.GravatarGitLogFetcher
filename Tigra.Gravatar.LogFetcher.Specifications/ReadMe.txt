This project is an exercise in TDD. THe objective is to implement the following Perl script in C#.
The script takes a Git log file and extracts the user names (email addresses). It then
looks up the Gravatar image for each user and downloads it in PNG format, saving it to a directory
with the file name set to the owner's name (as found in the Git log).

Steps:
1. Open the log file.
2. Read each line in the log and extract the committer name and email address
3. Build a set of unique committers.
4. Download the Gravatar icon for each committer. [Parallel/Async]
5. Save the image with the correct file name


#!/usr/bin/perl
#fetch Gravatars

use strict;
use warnings;

use LWP::Simple;
use Digest::MD5 qw(md5_hex);

my $size       = 90;
my $output_dir = '.git/avatar';

die("no .git/ directory found in current path\n") unless -d '.git';

mkdir($output_dir) unless -d $output_dir;

open(GITLOG, q/git log --pretty=format:"%ae|%an" |/) or die("failed to read git-log: $!\n");

my %processed_authors;

while(<GITLOG>) {
    chomp;
    my($email, $author) = split(/\|/, $_);

    next if $processed_authors{$author}++;

    my $author_image_file = $output_dir . '/' . $author . '.png';

    #skip images we have
    next if -e $author_image_file;

    #try and fetch image

    my $grav_url = "http://www.gravatar.com/avatar/".md5_hex(lc $email)."?d=404&size=".$size; 

    warn "fetching image for '$author' $email ($grav_url)...\n";

    my $rc = getstore($grav_url, $author_image_file);

    sleep(1);

    if($rc != 200) {
        unlink($author_image_file);
        next;
    }
}

close GITLOG;