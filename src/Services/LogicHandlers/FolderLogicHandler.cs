﻿using System.Collections.Generic;
using System.Linq;
using System.IO;
using Services.Interfaces;
using Services.Constants;
using System.IO.Abstractions;

namespace Services
{
    public class FolderLogicHandler : IFolderLogicHandler
    {
        public IList<string> GetAllTreeNames(string baseDirectory)
        {
            bool baseDirectoryExists = Directory.Exists(baseDirectory);
            if (baseDirectoryExists)
            {
                try
                {
                    return Directory.GetDirectories(baseDirectory).Select(Path.GetFileName).ToList();
                }
                catch (IOException ex)
                {
                    throw new IOException(ex.Message);
                }
            }
            else
            {
                Directory.CreateDirectory(baseDirectory);
                return new List<string>();
            }
        }

        public IList<string> GetAllLeafNamesWithNoExtension(string treePath)
        {
            if (Directory.Exists(treePath))
            {
                try
                {
                    var allLeaves = Directory.GetFiles(treePath).Select(Path.GetFileName).ToArray();

                    // Word documents create meta files when they are running, 
                    // this loop excludes these files from our leaves list.
                    var output = new List<string>();
                    for (int i = 0; i < allLeaves.Length; i++)
                    {
                        if (allLeaves[i].Contains("~"))
                            continue;

                        output.Add(allLeaves[i].Substring(0, allLeaves[i].Length - 5)); // 5 is length of .docx extension.
                    }

                    return output.ToArray();
                }
                catch (IOException ex)
                {
                    throw new IOException(ex.Message);
                }
            }

            return new List<string>();
        }

        public void CreateNewTreeFolder(string fullTreePath)
        {
            if (Directory.Exists(fullTreePath) == false)
                Directory.CreateDirectory(fullTreePath);
        }

        public void DeleteTree(string treePath)
        {
            if (Directory.Exists(treePath))
                Directory.Delete(treePath, true);
        }

        public void DeleteLeaf(string leafPath)
        {
            if (File.Exists(leafPath))
                File.Delete(leafPath);
        }

        public void BackupTrees(string baseDirectory, string destinationDirectory, bool copySubDirectories)
        {
            try
            {
                // Check if directory with data exists.
                var sourceDirectory = new DirectoryInfo(baseDirectory);
                if (sourceDirectory.Exists == false)
                    throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {baseDirectory}");

                // If the destination directory doesn't exist, create it.
                if (Directory.Exists(destinationDirectory) == false)
                    Directory.CreateDirectory(destinationDirectory);

                // Get the files in the base directory and copy them to the new location.
                var files = sourceDirectory.GetFiles();
                foreach (FileInfo file in files)
                {
                    var temporaryPath = Path.Combine(destinationDirectory, file.Name);
                    file.CopyTo(temporaryPath, false);
                }

                // If copying subdirectories, copy them and their contents to new location.
                DirectoryInfo[] sourceDirectories = sourceDirectory.GetDirectories();
                if (copySubDirectories)
                    foreach (DirectoryInfo subDirectory in sourceDirectories)
                    {
                        string temporaryPath = Path.Combine(destinationDirectory, subDirectory.Name);
                        BackupTrees(subDirectory.FullName, temporaryPath, copySubDirectories);
                    }
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        public string GetFullLeafPath(string treeName, string leafName)
        {
            return Path.GetFullPath($@"{DirectoryConstants.CurrentWorkingPath}\{treeName}\{leafName}.docx");
        }

        public string GetFullTreePath(string treeName)
        {
            return Path.GetFullPath($@"{DirectoryConstants.CurrentWorkingPath}\{treeName}");
        }
    }
}
