using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace arookas
{
	public abstract class sunImportResolver
	{
		static sunImportResolver defaultResolver = new sunDefaultImportResolver();
		public static sunImportResolver Default { get { return defaultResolver; } }

		public abstract void EnterFile(sunScriptFile file);
		public abstract void ExitFile(sunScriptFile file);
		public abstract sunImportResult ResolveImport(string name, out sunScriptFile file);

		// default implementation
		sealed class sunDefaultImportResolver : sunImportResolver
		{
			List<sunScriptFile> imports = new List<sunScriptFile>(10);
			Stack<sunScriptFile> current = new Stack<sunScriptFile>(5);
			string rootDirectory;

			public sunDefaultImportResolver()
			{
				rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
			}

			public override void EnterFile(sunScriptFile file) { current.Push(file); }
			public override void ExitFile(sunScriptFile file) { current.Pop(); }
			public override sunImportResult ResolveImport(string name, out sunScriptFile file)
			{
				file = null;
				string fullPath;
				if (Path.IsPathRooted(name))
				{
					// if the path is absolute, just use it directly
					fullPath = name;
					if (!File.Exists(fullPath))
					{
						return sunImportResult.Missing;
					}
				}
				else
				{
					// check if the file exists relative to the current one;
					// if it's not there, check the root directory
					fullPath = Path.Combine(Path.GetDirectoryName(current.Peek().Name), name);
					if (!File.Exists(fullPath))
					{
						fullPath = Path.Combine(rootDirectory, name);
						if (!File.Exists(fullPath))
						{
							return sunImportResult.Missing;
						}
					}
				}
				// open the file
				try
				{
					file = new sunScriptFile(name, File.OpenRead(fullPath));
				}
				catch
				{
					return sunImportResult.FailedToLoad;
				}
				// make sure the file has not been imported yet
				if (imports.Any(i => i.Name == fullPath))
				{
					return sunImportResult.Skipped;
				}
				imports.Add(file);
				return sunImportResult.Loaded;
			}
		}
	}

	public enum sunImportResult
	{
		Loaded,
		Skipped,
		Missing,
		FailedToLoad,
	}

	public class sunScriptFile
	{
		public string Name { get; private set; }
		public Stream Stream { get; private set; }

		public sunScriptFile(string name, Stream stream)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Stream does not support reading.", "stream");
			}
			Name = name;
			Stream = stream;
		}

		public TextReader GetReader()
		{
			return new StreamReader(Stream);
		}
	}
}
