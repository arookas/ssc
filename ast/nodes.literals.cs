using System;
using System.Globalization;
using System.Text;

namespace arookas
{
	class sunIntLiteral : sunToken<int> // base-10 integer
	{
		public sunIntLiteral(sunSourceLocation location, string token)
			: base(location, token)
		{

		}

		protected override int ParseValue(string token) { return Int32.Parse(token); }

		public override void Compile(sunContext context)
		{
			context.Text.PushInt(Value);
		}
	}

	class sunHexLiteral : sunIntLiteral // base-16 integer
	{
		public sunHexLiteral(sunSourceLocation location, string token)
			: base(location, token)
		{

		}

		protected override int ParseValue(string token)
		{
			// because .NET's hex parsing is gay and doesn't support
			// leading signs, manually detect negative literals
			var neg = (token[0] == '-');
			var trim = neg ? 3 : 2;
			var digits = token.Substring(trim); // trim the '0x' prefix before parsing
			var value = Int32.Parse(token.Substring(2), NumberStyles.AllowHexSpecifier);
			if (neg)
			{
				value = -value;
			}
			return value;
		}
	}

	class sunFloatLiteral : sunToken<float>
	{
		public sunFloatLiteral(sunSourceLocation location, string token)
			: base(location, token)
		{

		}

		protected override float ParseValue(string image) { return Single.Parse(image); }

		public override void Compile(sunContext context)
		{
			context.Text.PushFloat(Value);
		}
	}

	class sunStringLiteral : sunToken<string>
	{
		public sunStringLiteral(sunSourceLocation location, string token)
			: base(location, token)
		{

		}

		protected override string ParseValue(string image) { return UnescapeString(image.Substring(1, image.Length - 2)); } // remove enclosing quotes

		public override void Compile(sunContext context)
		{
			context.Text.PushData(context.DataTable.Add(Value));
		}

		// string unescaping utility
		string UnescapeString(string value)
		{
			// based on Hans Passant's code
			StringBuilder sb = new StringBuilder(value.Length);
			for (int i = 0; i < value.Length;)
			{
				int j = value.IndexOf('\\', i);
				if (j < 0 || j >= value.Length - 1)
				{
					j = value.Length;
				}
				sb.Append(value, i, j - i);
				if (j >= value.Length)
				{
					break;
				}
				switch (value[j + 1])
				{
					case '\'': sb.Append('\''); break;
					case '"': sb.Append('"'); break;
					case '\\': sb.Append('\\'); break;
					case '0': sb.Append('\0'); break;
					case 'a': sb.Append('\a'); break;
					case 'b': sb.Append('\b'); break;
					case 'f': sb.Append('\f'); break;
					case 'n': sb.Append('n'); break;
					case 't': sb.Append('\t'); break;
					case 'v': sb.Append('\v'); break;
					case 'x': sb.Append(UnescapeHex(value, j + 2, out i)); continue;
					case 'u': sb.Append(UnescapeUnicodeCodeUnit(value, j + 2, out i)); continue;
					case 'U': sb.Append(UnescapeUnicodeSurrogatePair(value, j + 2, out i)); continue;
					default: throw new sunEscapeSequenceException(this);
				}
				i = j + 2;
			}
			return sb.ToString();
		}
		char UnescapeHex(string value, int start, out int end)
		{
			if (start > value.Length)
			{
				throw new sunEscapeSequenceException(this); // we need at least one digit
			}
			StringBuilder sb = new StringBuilder(4);
			int digits = 0;
			while (digits < 4 && start < value.Length && IsHexDigit(value[start]))
			{
				sb.Append(value[start]);
				++digits;
				++start;
			}
			end = start;
			return (char)Int32.Parse(sb.ToString(), NumberStyles.AllowHexSpecifier);
		}
		char UnescapeUnicodeCodeUnit(string value, int start, out int end)
		{
			if (start >= value.Length - 4)
			{
				throw new sunEscapeSequenceException(this); // we need four digits
			}
			end = start + 4;
			return (char)Int32.Parse(value.Substring(start, 4), NumberStyles.AllowHexSpecifier);
		}
		string UnescapeUnicodeSurrogatePair(string value, int start, out int end)
		{
			if (start >= value.Length - 8)
			{
				throw new sunEscapeSequenceException(this); // we need eight digits
			}
			char high = (char)Int32.Parse(value.Substring(start, 4), NumberStyles.AllowHexSpecifier);
			char low = (char)Int32.Parse(value.Substring(start + 4, 4), NumberStyles.AllowHexSpecifier);
			if (!Char.IsHighSurrogate(high) || !Char.IsLowSurrogate(low))
			{
				throw new sunEscapeSequenceException(this); // characters are not a surrogate pair
			}
			end = start + 8;
			return String.Concat(high, low);
		}
		static bool IsHexDigit(char c)
		{
			return (c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'F') ||
				(c >= 'a' && c <= 'f');
		}
	}

	class sunIdentifier : sunRawToken
	{
		public sunIdentifier(sunSourceLocation location, string token)
			: base(location, token)
		{
			// make sure it is a valid identifier name (i.e. not a keyword)
			if (sunParser.IsKeyword(Value))
			{
				throw new sunIdentifierException(this);
			}
		}

		// identifiers are compiled on a per-context basis (i.e. at a higher level)
	}

	class sunTrue : sunIntLiteral
	{
		public sunTrue(sunSourceLocation location, string token)
			: base(location, token)
		{

		}

		protected override int ParseValue(string token) { return 1; }
	}

	class sunFalse : sunIntLiteral
	{
		public sunFalse(sunSourceLocation location, string token)
			: base(location, token)
		{

		}

		protected override int ParseValue(string token) { return 0; }
	}
}
