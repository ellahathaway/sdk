// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class TempFile
    {
        private readonly string _path;
 
        internal TempFile(string path)
        {
            Debug.Assert(PathUtilities.IsAbsolute(path));
            _path = path;
        }
 
        internal TempFile(string prefix, string extension, string directory, string callerSourcePath, int callerLineNumber)
        {
            while (true)
            {
                if (prefix == null)
                {
                    prefix = System.IO.Path.GetFileName(callerSourcePath) + "_" + callerLineNumber.ToString() + "_";
                }
 
                _path = System.IO.Path.Combine(directory ?? TempRoot.Root, prefix + Guid.NewGuid() + (extension ?? ".tmp"));
 
                try
                {
                    TempRoot.CreateStream(_path);
                    break;
                }
                catch (PathTooLongException)
                {
                    throw;
                }
                catch (DirectoryNotFoundException)
                {
                    throw;
                }
                catch (IOException)
                {
                    // retry
                }
            }
        }

        public FileStream Open(FileAccess access = FileAccess.ReadWrite) => new FileStream(_path, FileMode.Open, access);

        public string Path => _path;

        public TempFile WriteAllText(string content, Encoding encoding)
        {
            File.WriteAllText(_path, content, encoding);
            return this;
        }
 
        public TempFile WriteAllText(string content)
        {
            File.WriteAllText(_path, content);
            return this;
        }
 
        public async Task<TempFile> WriteAllTextAsync(string content, Encoding encoding)
        {
            using (var sw = new StreamWriter(File.Create(_path), encoding))
            {
                await sw.WriteAsync(content).ConfigureAwait(false);
            }
 
            return this;
        }

        public Task<TempFile> WriteAllTextAsync(string content) => WriteAllTextAsync(content, Encoding.UTF8);

        public TempFile WriteAllBytes(byte[] content)
        {
            File.WriteAllBytes(_path, content);
            return this;
        }
 
        public TempFile WriteAllBytes(ImmutableArray<byte> content)
        {
            content.WriteToFile(_path);
            return this;
        }

        public string ReadAllText() => File.ReadAllText(_path);

        public TempFile CopyContentFrom(string path) => WriteAllBytes(File.ReadAllBytes(path));

        public override string ToString() => _path;
    }
}
