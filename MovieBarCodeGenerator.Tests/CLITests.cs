using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using MovieBarCodeGenerator.CLI;
using MovieBarCodeGenerator.Core;
using NUnit.Framework;

namespace MovieBarCodeGenerator.Tests
{
    [TestFixture]
    public class CLITests
    {
        private IFileSystemService _fakeFileSystemService;

        // Fake FS
        const string _root = @"C:\";
        const string _file1 = @"C:\File1.mkv";

        const string _myTopDir = @"C:\MyTopDir\";
        const string _file2 = @"C:\MyTopDir\File2.mkv";
        const string _file3 = @"C:\MyTopDir\File3.avi";

        const string _mySubDirWithOneFile = @"C:\MyTopDir\MySubDirWithOneFile\";
        const string _file4 = @"C:\MyTopDir\MySubDirWithOneFile\File4.mp4";

        const string _mySubDirWithThreeFiles = @"C:\MyTopDir\MySubDirWithThreeFiles\";
        const string _file5 = @"C:\MyTopDir\MySubDirWithThreeFiles\File5.mkv";
        const string _file6 = @"C:\MyTopDir\MySubDirWithThreeFiles\File6.mkv";
        const string _file7 = @"C:\MyTopDir\MySubDirWithThreeFiles\File7.avi";

        const string _myEmptySubDir = @"C:\MyTopDir\MyEmptySubDir\";

        [SetUp]
        public void Setup()
        {
            _fakeFileSystemService = A.Fake<IFileSystemService>(x => x.Strict());

            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_root)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_myTopDir)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_mySubDirWithOneFile)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_mySubDirWithThreeFiles)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_myEmptySubDir)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_root.TrimEnd('\\'))).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_myTopDir.TrimEnd('\\'))).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_mySubDirWithOneFile.TrimEnd('\\'))).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_mySubDirWithThreeFiles.TrimEnd('\\'))).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_myEmptySubDir.TrimEnd('\\'))).Returns(true);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file1)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file2)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file3)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file4)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file5)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file6)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(_file7)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(".")).Returns(true); // special case for paths relatives to current dir
            A.CallTo(() => _fakeFileSystemService.DirectoryExists(A<string>.That.Contains("://"))).Returns(false); // urls

            A.CallTo(() => _fakeFileSystemService.FileExists(_root)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_myTopDir)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_mySubDirWithOneFile)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_mySubDirWithThreeFiles)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_myEmptySubDir)).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_root.TrimEnd('\\'))).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_myTopDir.TrimEnd('\\'))).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_mySubDirWithOneFile.TrimEnd('\\'))).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_mySubDirWithThreeFiles.TrimEnd('\\'))).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_myEmptySubDir.TrimEnd('\\'))).Returns(false);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file1)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file2)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file3)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file4)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file5)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file6)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(_file7)).Returns(true);
            A.CallTo(() => _fakeFileSystemService.FileExists(A<string>.That.Contains("://"))).Returns(false); // urls

            A.CallTo(() => _fakeFileSystemService.GetDirectoryName(A<string>._)).ReturnsLazily(x => Path.GetDirectoryName(x.GetArgument<string>(0)));

            A.CallTo(() => _fakeFileSystemService.GetFileName(A<string>._)).ReturnsLazily(x => Path.GetFileName(x.GetArgument<string>(0)));

            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_root, "*", SearchOption.TopDirectoryOnly)).Returns(new string[] { _file1 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_myTopDir, "*", SearchOption.TopDirectoryOnly)).Returns(new string[] { _file2, _file3 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_mySubDirWithOneFile, "*", SearchOption.TopDirectoryOnly)).Returns(new string[] { _file4 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_mySubDirWithThreeFiles, "*", SearchOption.TopDirectoryOnly)).Returns(new string[] { _file5, _file6, _file7 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_myEmptySubDir, "*", SearchOption.TopDirectoryOnly)).Returns(new string[] { });

            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_root, "*", SearchOption.AllDirectories)).Returns(new string[] { _file1, _file2, _file3, _file4, _file5, _file6, _file7 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_myTopDir, "*", SearchOption.AllDirectories)).Returns(new string[] { _file2, _file3, _file4, _file5, _file6, _file7 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_mySubDirWithOneFile, "*", SearchOption.AllDirectories)).Returns(new string[] { _file4 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_mySubDirWithThreeFiles, "*", SearchOption.AllDirectories)).Returns(new string[] { _file5, _file6, _file7 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(_myEmptySubDir, "*", SearchOption.AllDirectories)).Returns(new string[] { });

            // Patterns:

            // C:\File*
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\", "File*", SearchOption.TopDirectoryOnly)).Returns(new string[] { _file1 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\", "File*", SearchOption.AllDirectories)).Returns(new string[] { _file1, _file2, _file3, _file4, _file5, _file6, _file7 });

            // C:\Bla*
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\", "Bla*", SearchOption.TopDirectoryOnly)).Returns(new string[] { });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\", "Bla*", SearchOption.AllDirectories)).Returns(new string[] { });

            // C:\*.mp4
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\", "*.mp4", SearchOption.TopDirectoryOnly)).Returns(new string[] { });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\", "*.mp4", SearchOption.AllDirectories)).Returns(new string[] { _file4 });

            // C:\MyTopDir\File*
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\MyTopDir", "File*", SearchOption.TopDirectoryOnly)).Returns(new string[] { _file2, _file3 });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\MyTopDir", "File*", SearchOption.AllDirectories)).Returns(new string[] { _file2, _file3, _file4, _file5, _file6, _file7 });

            // C:\MyTopDir\*.mp4
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\MyTopDir", "*.mp4", SearchOption.TopDirectoryOnly)).Returns(new string[] { });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@"C:\MyTopDir", "*.mp4", SearchOption.AllDirectories)).Returns(new string[] { _file4 });

            // *.mp4 (relative to current dir)
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@".", "*.mp4", SearchOption.TopDirectoryOnly)).Returns(new string[] { });
            A.CallTo(() => _fakeFileSystemService.EnumerateDirectoryFiles(@".", "*.mp4", SearchOption.AllDirectories)).Returns(new string[] { _file4 });
        }

        [TestCase(null, false, new string[] { })]
        [TestCase(new string[] { }, false, new string[] { })]
        [TestCase(new string[] { _root }, false, new string[] { _file1 })]
        [TestCase(new string[] { _root, _root }, false, new string[] { _file1, _file1 })]
        [TestCase(new string[] { _root }, true, new string[] { _file1, _file2, _file3, _file4, _file5, _file6, _file7 })]
        [TestCase(new string[] { _myTopDir }, false, new string[] { _file2, _file3 })]
        [TestCase(new string[] { _myEmptySubDir }, false, new string[] { })]
        [TestCase(new string[] { _myEmptySubDir }, true, new string[] { })]
        [TestCase(new string[] { _file1 }, false, new string[] { _file1 })]
        [TestCase(new string[] { _file1 }, true, new string[] { _file1 })]
        [TestCase(new string[] { _file1, _file7 }, false, new string[] { _file1, _file7 })]
        [TestCase(new string[] { _file1, _file7 }, true, new string[] { _file1, _file7 })]
        [TestCase(new string[] { _file2, _mySubDirWithOneFile }, false, new string[] { _file2, _file4 })]
        [TestCase(new string[] { _file2, _mySubDirWithOneFile }, true, new string[] { _file2, _file4 })]
        [TestCase(new string[] { _file1, _myTopDir }, false, new string[] { _file1, _file2, _file3 })]
        [TestCase(new string[] { _file1, _myTopDir }, true, new string[] { _file1, _file2, _file3, _file4, _file5, _file6, _file7 })]
        [TestCase(new string[] { @"C:\*" }, false, new string[] { _file1 })]
        [TestCase(new string[] { @"C:\*" }, true, new string[] { _file1, _file2, _file3, _file4, _file5, _file6, _file7 })]
        [TestCase(new string[] { @"C:\File*" }, false, new string[] { _file1 })]
        [TestCase(new string[] { @"C:\File*" }, true, new string[] { _file1, _file2, _file3, _file4, _file5, _file6, _file7 })]
        [TestCase(new string[] { @"C:\Bla*" }, false, new string[] { })]
        [TestCase(new string[] { @"C:\Bla*" }, true, new string[] { })]
        [TestCase(new string[] { @"C:\*.mp4" }, false, new string[] { })]
        [TestCase(new string[] { @"C:\*.mp4" }, true, new string[] { _file4 })]
        [TestCase(new string[] { @"C:\MyTopDir\File*" }, false, new string[] { _file2, _file3 })]
        [TestCase(new string[] { @"C:\MyTopDir\File*" }, true, new string[] { _file2, _file3, _file4, _file5, _file6, _file7 })]
        [TestCase(new string[] { @"C:\MyTopDir\*.mp4" }, false, new string[] { })]
        [TestCase(new string[] { @"C:\MyTopDir\*.mp4" }, true, new string[] { _file4 })]
        [TestCase(new string[] { @"*.mp4" }, false, new string[] { })]
        [TestCase(new string[] { @"*.mp4" }, true, new string[] { _file4 })]
        [TestCase(new string[] { "http://test.com/bla/file.mkv" }, false, new string[] { "http://test.com/bla/file.mkv" })]
        [TestCase(new string[] { "http://test.com/bla/file.mkv" }, true, new string[] { "http://test.com/bla/file.mkv" })]
        [Test]
        public void GetExpandedAndValidatedFilePaths_Returns_Expected_Result(string[] input, bool recursive, string[] expectedOutput)
        {
            var output = CLIUtils.GetExpandedAndValidatedFilePaths(_fakeFileSystemService, input, recursive).ToList();
            CollectionAssert.AreEquivalent(expectedOutput, output);
        }
    }
}
