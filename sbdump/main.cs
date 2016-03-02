using arookas.IO.Binary;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace arookas {
	class sbdump {
		static CommandLineSettings sSettings;
		static aBinaryReader sReader;
		static TextWriter sWriter;
		static uint sTextOffset, sDataOffset, sSymOffset;
		static int sDataCount, sSymCount, sVarCount;

		const string cTitle = "sbdump arookas";
		static readonly string[] sSymbolTypes = { "builtin", "function", "var", };
		static readonly string[] sCommandNames = {
			"int", "flt", "str", "adr", "var", "nop", "inc", "dec",
			"add", "sub", "mul", "div", "mod", "ass", "eq", "ne",
			"gt", "lt", "ge", "le", "neg", "not", "and", "or",
			"band", "bor", "shl", "shr", "call", "func", "mkfr", "mkds",
			"ret", "ret0", "jne", "jmp", "pop", "int0", "int1", "end",
		};

		static int Main(string[] args) {
#if !DEBUG
			try {
#endif
				Console.WriteLine(cTitle);
				ReadCommandLine(args);
				Console.WriteLine("Opening input file...");
				using (var sb = File.OpenRead(sSettings.Input)) {
					CreateReader(sb);
					Console.WriteLine("Creating output file...");
					using (sWriter = File.CreateText(sSettings.Output)) {
						ReadHeader();
						WritePreamble();
						if (sSettings.OutputHeader) {
							WriteHeader();
						}
						if (sSettings.OutputText) {
							WriteText();
						}
						if (sSettings.OutputData) {
							WriteData();
						}
						if (sSettings.OutputSym) {
							WriteSym();
						}
						if (sSettings.OutputVars) {
							WriteVars();
						}
						Console.WriteLine("Closing output file...");
					}
					Console.WriteLine("Closing input file...");
				}
				Console.WriteLine("Done.");
#if !DEBUG
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
				return 1;
			}
#endif
				return 0;
		}

		static void ReadCommandLine(string[] args) {
			Console.WriteLine("Reading command line...");
			sSettings = new CommandLineSettings(new aCommandLine(args));
		}
		static void CreateReader(Stream stream) {
			Console.WriteLine("Creating binary reader...");
			sReader = new aBinaryReader(stream, Endianness.Big, Encoding.GetEncoding(932));
		}
		static void WritePreamble() {
			Console.WriteLine("Writing preamble...");
			sWriter.WriteLine("# {0}", cTitle);
			sWriter.WriteLine("# {0}", DateTime.Now);
			sWriter.WriteLine("# {0}", Path.GetFileName(sSettings.Input));
			sWriter.WriteLine();
		}
		static void ReadHeader() {
			Console.WriteLine("Reading header...");
			if (sReader.Read32() != 0x53504342u) { // 'SPCB'
				throw new Exception("Invalid magic.");
			}
			sTextOffset = sReader.Read32();
			sDataOffset = sReader.Read32();
			sDataCount = sReader.ReadS32();
			sSymOffset = sReader.Read32();
			sSymCount = sReader.ReadS32();
			sVarCount = sReader.ReadS32();
		}
		static void WriteHeader() {
			Console.WriteLine("Outputting header...");
			sWriter.WriteLine("# Header information");
			sWriter.WriteLine("#   .text offset   : {0:X8}", sTextOffset);
			sWriter.WriteLine("#   .data offset   : {0:X8}", sDataOffset);
			sWriter.WriteLine("#   .data count    : {0}", sDataCount);
			sWriter.WriteLine("#   .sym offset    : {0:X8}", sSymOffset);
			sWriter.WriteLine("#   .sym count     : {0}", sSymCount);
			sWriter.WriteLine("#   var count      : {0}", sVarCount);
			sWriter.WriteLine();
		}
		static void WriteText() {
			Console.WriteLine("Outputting .text...");
			sWriter.WriteLine(".text");
			WriteText(0u);
			var symbols = new Symbol[sSymCount];
			for (var i = 0; i < sSymCount; ++i) {
				symbols[i] = FetchSymbol(i);
			}
			foreach (var symbol in symbols.Where(i => i.Type == SymbolType.Function).OrderBy(i => i.Data)) {
				sWriter.WriteLine("{0}: ", FetchSymbolName(symbol));
				WriteText(symbol.Data);
			}
		}
		static void WriteText(uint ofs) {
			byte command;
			sReader.Keep();
			sReader.Goto(sTextOffset + ofs);
			var maxofs = 0u;
			do {
				var pos = sReader.Position - sTextOffset;
				command = sReader.Read8();
				sWriter.Write("  {0:X8} {1}", pos, sCommandNames[command]);
				var nextofs = 0u;
				switch (command) {
					case 0x00: sWriter.Write(" {0}", sReader.ReadS32()); break;
					case 0x01: sWriter.Write(" {0}", sReader.ReadF32()); break;
					case 0x02: {
						var data = sReader.ReadS32();
						var value = FetchDataValue(data);
						sWriter.Write(" {0} # \"{1}\"", data, value);
						break;
					}
					case 0x03: sWriter.Write(" ${0:X8}", sReader.Read32()); break;
					case 0x04: WriteVar(); break;
					case 0x06: WriteVar(); break;
					case 0x07: WriteVar(); break;
					case 0x0D: {
						sReader.Read8(); // TSpcInterp skips this byte
						WriteVar();
						break;
					}
					case 0x1C: {
						var dest = sReader.Read32();
						var args = sReader.ReadS32();
						var symbol = FetchSymbol(i => i.Data == dest);
						if (symbol != null) {
							sWriter.Write(" {0}, {1}", FetchSymbolName(symbol), args);
						}
						else {
							sWriter.Write(" ${0:X8}, {1}", dest, args);
						}
						break;
					}
					case 0x1D: sWriter.Write(" {0}, {1}", FetchSymbolName(FetchSymbol(sReader.ReadS32())), sReader.ReadS32()); break;
					case 0x1E: sWriter.Write(" {0}", sReader.ReadS32()); break;
					case 0x1F: sWriter.Write(" {0}", sReader.ReadS32()); break;
					case 0x22: nextofs = WriteJmp(ofs); break;
					case 0x23: nextofs = WriteJmp(ofs); break;
				}
				sWriter.WriteLine();
				if (nextofs > maxofs) {
					maxofs = nextofs;
				}
			} while (!IsReturnCommand(command) || sReader.Position <= sTextOffset + maxofs);
			sWriter.WriteLine();
			sReader.Back();
		}
		static void WriteVar() {
			var display = sReader.ReadS32();
			var data = sReader.ReadS32();
			sWriter.Write(" {0} {1}", display, data);
			switch (display) {
				case 0: sWriter.Write(" # {0}", FetchSymbolName(FetchSymbol(i => i.Type == SymbolType.Variable && i.Data == data))); break;
				case 1: sWriter.Write(" # local{0}", data); break;
			}
		}
		static uint WriteJmp(uint ofs) {
			var dest = sReader.Read32();
			var symbol = FetchSymbol(i => i.Data == ofs);
			if (ofs > 0 && symbol != null) {
				var name = FetchSymbolName(symbol);
				sWriter.Write(" {0} + ${1:X4} # ${2:X8}", name, dest - ofs, dest);
			}
			else {
				sWriter.Write(" ${0:X8}", dest);
			}
			return dest;
		}
		static void WriteData() {
			Console.WriteLine("Outputting .data...");
			sWriter.WriteLine(".data");
			sReader.Goto(sDataOffset);
			for (var i = 0; i < sDataCount; ++i) {
				var ofs = sReader.Read32();
				var data = FetchDataValue(ofs);
				sWriter.WriteLine("  .string \"{0}\"", data);
			}
			sWriter.WriteLine();
		}
		static void WriteSym() {
			Console.WriteLine("Outputting .sym...");
			sWriter.WriteLine(".sym");
			sReader.Goto(sSymOffset);
			for (var i = 0; i < sSymCount; ++i) {
				var symbol = new Symbol(sReader);
				var name = FetchSymbolName(symbol);
				sWriter.WriteLine("  .{0} {1}", sSymbolTypes[(int)symbol.Type], name);
			}
			sWriter.WriteLine();
		}
		static void WriteVars() {
			Console.WriteLine("Outputting variables...");
			sWriter.WriteLine("# variables:");
			for (var i = 0; i < sVarCount; ++i) {
				var symbol = FetchSymbol(j => j.Type == SymbolType.Variable && j.Data == i);
				if (symbol != null) {
					sWriter.WriteLine("# {0}", FetchSymbolName(symbol));
				}
				else {
					sWriter.WriteLine("# (NULL)");
				}
			}
			sWriter.WriteLine();
		}

		static uint FetchData(int i) {
			sReader.Keep();
			sReader.Goto(sDataOffset + (4 * i));
			var data = sReader.Read32();
			sReader.Back();
			return data;
		}
		static string FetchDataValue(int i) {
			if (i < 0 || i >= sDataCount) {
				return "(NULL)";
			}
			return FetchDataValue(FetchData(i));
		}
		static string FetchDataValue(uint ofs) {
			sReader.Keep();
			sReader.Goto(sDataOffset + (4 * sDataCount) + ofs);
			var data = sReader.ReadString<aZSTR>();
			sReader.Back();
			return data;
		}
		static Symbol FetchSymbol(int i) {
			sReader.Keep();
			sReader.Goto(sSymOffset + (20 * i));
			var symbol = new Symbol(sReader);
			sReader.Back();
			return symbol;
		}
		static Symbol FetchSymbol(Predicate<Symbol> predicate) {
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			Symbol found = null;
			sReader.Keep();
			sReader.Goto(sSymOffset);
			for (var i = 0; i < sSymCount; ++i) {
				var symbol = new Symbol(sReader);
				if (predicate(symbol)) {
					found = symbol;
					break;
				}
			}
			sReader.Back();
			return found;
		}
		static string FetchSymbolName(Symbol symbol) {
			sReader.Keep();
			sReader.Goto(sSymOffset + (20 * sSymCount) + symbol.StringOffset);
			var name = sReader.ReadString<aZSTR>();
			sReader.Back();
			return name;
		}

		static bool IsReturnCommand(byte cmd) {
			return (
				cmd == 0x20 || // ret
				cmd == 0x21 || // ret0
				cmd == 0x27 // end
			);
		}
	}
}
