using System.Collections.Generic;
using System.IO;

namespace arookas
{
	class sunImportTable
	{
		List<string> imports = new List<string>(10);
		Stack<string> curDir = new Stack<string>(5);
		string RootDir { get; set; }

		public sunImportTable(string rootDir)
		{
			RootDir = rootDir;
		}

		public void PushDir(string dir) { curDir.Push(Path.GetDirectoryName(dir)); }
		public void PopDir() { curDir.Pop(); }

		public string ResolveImport(sunImport import)
		{
			string fullPath;
			string file = import.ImportFile.Value;
			if (Path.IsPathRooted(file))
			{
				// if the path is absolute, just use it directly
				fullPath = file;
				if (!File.Exists(fullPath))
				{
					// could not find file
					throw new sunMissingImportException(import);
				}
			}
			else
			{
				// check if the file exists relative to the current one;
				// if it's not there, check the root directory
				fullPath = Path.Combine(curDir.Peek(), file);
				if (!File.Exists(fullPath))
				{
					fullPath = Path.Combine(RootDir, file);
					if (!File.Exists(fullPath))
					{
						// could not find file
						throw new sunMissingImportException(import);
					}
				}
			}
			// make sure the file has not been imported yet
			if (imports.Contains(fullPath))
			{
				return null;
			}
			imports.Add(fullPath);
			return fullPath;
		}
	}
}
