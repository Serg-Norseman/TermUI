using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Core
{
	public static class Extents
	{
		public static string GetRangeEx (this string str, int start, int end)
		{
			int size = str.Length;

			if (end < 0)
				end = size + end;

			if (start < 0)
				start = size + start;

			if (start < 0 || start >= size || start >= end)
				return string.Empty;

			if (end < 0 || end > size)
				return string.Empty;

			// exclusive end index
			return str.Substring (start, end - start);
		}

		public static string GetRangeEx (this string str, int start, object end)
		{
			int size = str.Length;
			int iend = size;
			if (start < 0)
				start = size + start;

			if (start < 0 || start >= size || start >= iend)
				return string.Empty;
			if (iend < 0 || iend > size)
				return string.Empty;

			return str.Substring (start, iend - start);
		}

		/// <summary>
		/// Count the number of non-overlapping instances of substr in the string.
		/// </summary>
		/// <returns>If substr is an empty string, Count returns 1 + the number of Unicode code points in the string, otherwise the count of non-overlapping instances in string.</returns>
		/// <param name="substr">Substr.</param>
		public static int Count (this string str, string substr)
		{
			if ((object)substr == null)
				throw new ArgumentNullException (nameof (substr));
			int n = 0;
			if (substr.Length == 0)
				return str.Length + 1;
			int offset = 0;
			int len = str.Length;
			int slen = substr.Length;
			while (offset < len) {
				var i = str.IndexOf (substr, offset);
				if (i == -1)
					break;
				n++;
				offset = i + slen;
			}
			return n;
		}

		public static List<char> ToRuneList (this string str)
		{
			return str.ToCharArray ().ToList ();
		}
	}

	public static class Rn
	{
		/// <summary>
		/// Number of column positions of a wide-character code.   This is used to measure runes as displayed by text-based terminals.
		/// </summary>
		/// <returns>The width in columns, 0 if the argument is the null character, -1 if the value is not printable, otherwise the number of columns that the rune occupies.</returns>
		/// <param name="rune">The rune.</param>
		public static int ColumnWidth (Rune rune)
		{
			return Rune.ColumnWidth (rune);
		}

		public static int ColumnWidth (int rune)
		{
			return Rune.ColumnWidth (rune);
		}

		/// <summary>
		/// Gets the total width of the passed text.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>The text width.</returns>
		public static int StrWidth (IEnumerable<Rune> str)
		{
			return str.Sum (Rune.ColumnWidth);
		}

		/// <summary>
		/// Gets the total width of the passed text.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>The text width.</returns>
		public static int StrWidth (string str)
		{
			return str.Sum (r => Math.Max (Rn.ColumnWidth (r), 1));
		}

		/// <summary>
		/// Gets the total width of the passed text.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>The text width.</returns>
		public static int StrWidth (char[] str)
		{
			return str.Sum (r => Math.Max (Rn.ColumnWidth (r), 1));
		}

		/// <summary>
		/// number of bytes required to encode the rune.
		/// </summary>
		/// <returns>The length, or -1 if the rune is not a valid value to encode in UTF-8.</returns>
		/// <param name="rune">Rune to probe.</param>
		public static int RuneLen (Rune rune)
		{
			return Rune.RuneLen (rune);
		}
	}
}
