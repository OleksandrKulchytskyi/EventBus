using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EventBus.Extensions
{
	public static class StringExtensions
	{
		public static string Or(this string s, string fallback)
		{
			return string.IsNullOrEmpty(s) ? fallback : s;
		}

		public static string WithoutPrefix(this string s, string prefix, StringComparison stringComparison)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (prefix == null)
			{
				throw new ArgumentNullException("prefix");
			}
			int i = prefix.Length;
			return s.StartsWith(prefix, stringComparison) ? s.Substring(i) : s;
		}

		public static string WithoutPrefix(this string s, string prefix)
		{
			return s.WithoutPrefix(prefix, StringComparison.CurrentCulture);
		}

		public static string[] Split(this string value, string separator)
		{
			return value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
		}

		public static int IndexOfAny(this string value, int startIndex, params char[] anyOf)
		{
			return value.IndexOfAny(anyOf, startIndex);
		}

		public static int IndexOfAny(this string value, string anyOf, int startIndex)
		{
			return value.IndexOfAny(startIndex, anyOf.ToCharArray());
		}

		public static string ToLowerCamelCase(this string s)
		{
			return s.Substring(0, 1).ToLowerInvariant() + s.Substring(1);
		}

		public static string SubstringBefore(this string s, string substring)
		{
			int substringOffset = s.IndexOf(substring);
			return (substringOffset == -1) ? s : s.Substring(0, substringOffset);
		}

		/// <summary>
		/// Returns a substring of <paramref name="s" /> until <paramref name="substring" />
		/// or <paramref name="default" />, if there's no substring in <paramref name="s" />.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="substring"></param>
		/// <param name="default"></param>
		/// <returns></returns>
		/// <example>
		/// <code>
		/// "ab" == "abc".SubstringBefore("c", null);
		/// null == "abc".SubstringBefore("d", null);
		/// "xx" == "abc".SubstringBefore("e", "xx");
		/// </code>
		/// </example>
		public static string SubstringBefore(this string s, string substring, string @default)
		{
			int substringOffset = s.IndexOf(substring);
			return (substringOffset == -1) ? @default : s.Substring(0, substringOffset);
		}

		public static string SubstringBeforeSuffix(this string s, string substring)
		{
			string result;
			if (!s.EndsWithSuffix(substring))
			{
				result = s;
			}
			else
			{
				int substringOffset = s.LastIndexOf(substring);
				result = ((substringOffset == -1) ? s : s.Substring(0, substringOffset));
			}
			return result;
		}

		public static string SubstringAfter(this string s, string substring)
		{
			int start = s.IndexOf(substring);
			return (start == -1) ? s : s.Substring(start + substring.Length);
		}

		public static string SubstringBetween(this string s, string start, string end)
		{
			return s.SubstringAfter(start).SubstringBefore(end);
		}

		public static bool EndsWithSuffix(this string s, string suffix)
		{
			return s.Length > suffix.Length && s.EndsWith(suffix);
		}

		/// <summary>
		/// Joins non-null and non-empty values from <paramref name="strings" />.
		/// </summary>
		/// <param name="strings"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string JoinNonEmpty(this string[] strings, string separator)
		{
			string[] nonEmpty = (
				from s in strings
				where s != null && !string.IsNullOrEmpty(s)
				select s).ToArray<string>();
			string result;
			if (nonEmpty.Length == 0)
			{
				result = "";
			}
			else
			{
				result = string.Join(separator, nonEmpty);
			}
			return result;
		}

		public static string FormatWith(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		public static string ToSlug(this string value)
		{
			return StringExtensions.BuildSlugCore(StringExtensions.RemoveDiacritics(value));
		}

		private static string BuildSlugCore(string value)
		{
			int maxLength = 200;
			Match match = Regex.Match(value.ToLower(), "[\\w]+");
			StringBuilder result = new StringBuilder("");
			bool maxLengthHit = false;
			while (match.Success && !maxLengthHit)
			{
				if (result.Length + match.Value.Length <= maxLength)
				{
					result.Append(match.Value + "-");
				}
				else
				{
					maxLengthHit = true;
					if (result.Length == 0)
					{
						result.Append(match.Value.Substring(0, maxLength));
					}
				}
				match = match.NextMatch();
			}
			if (result.Length > 0 && result[result.Length - 1] == '-')
			{
				result.Remove(result.Length - 1, 1);
			}
			string r = result.ToString();
			while (r.Contains("--"))
			{
				r = r.Replace("--", "-");
			}
			return r;
		}

		public static string RemoveDiacritics(string stIn)
		{
			string stFormD = stIn.Normalize(NormalizationForm.FormD);
			StringBuilder sb = new StringBuilder();
			for (int ich = 0; ich < stFormD.Length; ich++)
			{
				UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
				if (uc != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(stFormD[ich]);
				}
			}
			return sb.ToString().Normalize(NormalizationForm.FormC);
		}

		public static bool IsNullOrEmpty(this string s)
		{
			return s == null || s.Trim() == "";
		}

		public static string ComputeHash(this string s)
		{
			using (var md5 = new MD5CryptoServiceProvider())
			{
				return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLowerInvariant();
			}
		}

		public static bool CaseInsensitivelyEquals(this string s, string other)
		{
			return string.Compare(s, other, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}
}