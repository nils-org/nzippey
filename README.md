# NZippey

> Provides a simple git filter tool to deal with zipped files.

## Rationale

Dealing with zipped files in git is not fun at all. To overcome this,
Sippey Fun Lab created a tool called [zippey](https://sites.google.com/site/sippeyfunlabs/random-projects/zippey-enable-git-to-efficiently-handle-zip-based-file) in 2014. Its purpose is:

> Zippey is a Git filter that un-zip zip-based file into a simple text format during git add/commit ("clean" process) and recover the original zip-based file after git checkout ("smudge" process). Since diff is taken on the "cleaned" file after file is added, it is likely real changes to file can be reflected by original git diff command. This also solves the problem that the diff results for these files being useless and not readable for humanbeings.

Since `zipppey` needs python to be installed on a machine (an I generally don't have that), I created `NZippey` to achieve the same goal using a .NET tool.
The output will be fully compatible to `zippey` so switching/mixing the two will not be an issue.

## Usage

TBD

## Installation

TBD
