#addin nuget:?package=Cake.FileHelpers&version=6.1.3
#addin nuget:?package=Cake.7zip&version=3.0.0

var target = Argument("target", "Default");
var gitDir = Directory("./test");
var oneFile = gitDir + File("One.txt");
var twoFile = gitDir + File("Two.txt");
var zipFile = gitDir + File("zipped.zip");
var nzippey = MakeAbsolute(File("../src/NZippey/bin/Debug/net7.0/NZippey.exe"));
FilePath gitTool;

private void RunGit(string arguments)
{
    var exitCode = StartProcess(
        gitTool,
        new ProcessSettings
        {
            Arguments = arguments,
            WorkingDirectory = gitDir
        });
    if (exitCode != 0)
    {
        throw new Exception("git exited abnormally.");
    }
}

Setup((context) => {
    gitTool = context.Tools.Resolve("git");
    if (gitTool == null)
    {
        gitTool = context.Tools.Resolve("git.exe");
    }
    if (gitTool == null)
    {
        throw new Exception("Could not resolve git.");
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory(gitDir);
});

Task("Init")
    .Does(() =>
{
    RunGit("init");
    RunGit($"config filter.nzippey.smudge \"{nzippey.FullPath} smudge\"");
    RunGit($"config filter.nzippey.clean \"{nzippey.FullPath} clean\"");
    RunGit("config commit.gpgsign false"); // do not sign during this test...
    FileWriteLines(gitDir + File(".gitattributes"), new[]{"*.zip filter=nzippey"});
    RunGit("add .");
    RunGit("commit -m \"init\"");
});

Task("FirstMod")
    .Does(() =>
{
    FileWriteLines(oneFile, new[]{"Hello, World"});
    FileWriteLines(twoFile, new[]{"Hello, NZippey"});
    SevenZip(m => m
      .InAddMode()
      .WithArchive(zipFile)
      .WithFiles(oneFile)
      .WithFiles(twoFile));
    DeleteFile(oneFile);
    DeleteFile(twoFile);
    RunGit("add .");
    RunGit("commit -m \"FirstMod\"");
});

Task("SecondMod")
    .Does(() =>
{
    DeleteFile(zipFile);
    FileWriteLines(oneFile, new[]{"Hello, World"}); // unchanged
    FileWriteLines(twoFile, new[]{"Hello, again"}); // changed
    SevenZip(m => m
      .InAddMode()
      .WithArchive(zipFile)
      .WithFiles(oneFile)
      .WithFiles(twoFile)
      .WithWorkingDirectory(gitDir));
    DeleteFile(oneFile);
    DeleteFile(twoFile);
    RunGit("add .");
    RunGit("commit -m \"SecondMod\"");
});

// git diff HEAD^1
Task("ShowMod")
    .Does(() =>
{
    RunGit("diff HEAD^1");
});

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("Init")
  .IsDependentOn("FirstMod")
  .IsDependentOn("SecondMod")
  .IsDependentOn("ShowMod");

RunTarget(target);