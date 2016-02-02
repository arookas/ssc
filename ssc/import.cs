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
			ulong mFileId;

			string CurrentDirectory {
				get {
					if (mFiles.Count > 0) {
						return Path.Combine(mCurrentDirectory, Path.GetDirectoryName(mFiles.Peek().Name));
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
				name = name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
				var path = "";
				if (Path.IsPathRooted(name)) {
					path = name;
					if (!File.Exists(path)) {
						return sunImportResult.Missing;
					}
				}
				else {
					path = Path.Combine(CurrentDirectory, name);
					if (!File.Exists(path)) {
						path = Path.Combine(mRootDirectory, name);
						if (!File.Exists(path)) {
							return sunImportResult.Missing;
						}
					}
				}
				if (mImports.Any(i => i.Name == path)) {
					return sunImportResult.Skipped;
				}
				try {
					file = new sunScriptFile(path, File.OpenRead(path), mFileId++);
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
		ulong mId;

		public string Name {
			get { return mName; }
		}
		public Stream Stream {
			get { return mStream; }
		}
		public ulong Id {
			get { return mId; }
		}

		public sunScriptFile(string name, Stream stream, ulong id) {
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
			mId = id;
		}

		public void Dispose() {
			mStream.Dispose();
		}

		public TextReader CreateReader() {
			return new StreamReader(mStream);
		}
	}
}
