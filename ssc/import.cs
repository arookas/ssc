using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace arookas {
	public abstract class sunImportResolver {
		public static sunImportResolver Default {
			get { return new sunDefaultImportResolver(); }
		}

		public abstract void EnterFile(sunScriptFile file);
		public abstract void ExitFile(sunScriptFile file);
		public abstract sunImportResult ResolveImport(string name, out sunScriptFile file);

		// default implementation
		sealed class sunDefaultImportResolver : sunImportResolver {
			List<sunScriptFile> mImports;
			Stack<sunScriptFile> mFiles;
			string mRootDirectory, mCurrentDirectory;

			string CurrentDirectory {
				get {
					if (mFiles.Count > 0) {
						return Path.GetDirectoryName(mFiles.Peek().Name);
					}
					return mCurrentDirectory;
				}
			}

			public sunDefaultImportResolver() {
				mImports = new List<sunScriptFile>(10);
				mFiles = new Stack<sunScriptFile>(5);
				mRootDirectory = AppDomain.CurrentDomain.BaseDirectory;
				mCurrentDirectory = Directory.GetCurrentDirectory();
			}

			public override void EnterFile(sunScriptFile file) {
				mFiles.Push(file);
			}
			public override void ExitFile(sunScriptFile file) {
				mFiles.Pop();
			}
			public override sunImportResult ResolveImport(string name, out sunScriptFile file) {
				file = null;
				var fullPath = "";
				if (Path.IsPathRooted(name)) {
					// if the path is absolute, just use it directly
					fullPath = name;
					if (!File.Exists(fullPath)) {
						return sunImportResult.Missing;
					}
				}
				else {
					// check if the file exists relative to the current one;
					// if it's not there, check the root directory
					fullPath = Path.Combine(CurrentDirectory, name);
					if (!File.Exists(fullPath)) {
						fullPath = Path.Combine(mRootDirectory, name);
						if (!File.Exists(fullPath)) {
							return sunImportResult.Missing;
						}
					}
				}
				// make sure the file has not been imported yet
				if (mImports.Any(i => i.Name == fullPath)) {
					return sunImportResult.Skipped;
				}
				// open the file
				try {
					file = new sunScriptFile(name, File.OpenRead(fullPath));
				}
				catch {
					return sunImportResult.FailedToLoad;
				}
				mImports.Add(file);
				return sunImportResult.Loaded;
			}
		}
	}

	public enum sunImportResult {
		Loaded,
		Skipped,
		Missing,
		FailedToLoad,
	}

	public class sunScriptFile : IDisposable {
		string mName;
		Stream mStream;

		public string Name {
			get { return mName; }
		}
		public Stream Stream {
			get { return mStream; }
		}

		public sunScriptFile(string name, Stream stream) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (stream == null) {
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead) {
				throw new ArgumentException("Stream does not support reading.", "stream");
			}
			mName = name;
			mStream = stream;
		}

		public void Dispose() {
			mStream.Dispose();
		}

		public TextReader CreateReader() {
			return new StreamReader(mStream);
		}
	}
}
