using System;
using System.Text.RegularExpressions;

namespace NUnit.Framework {
	public static class ShouldMatchExtension {
		public static void ShouldMatch(this string input, string regex) {
			if (! Regex.IsMatch(input, regex))
				Assert.Fail("Expected \"{0}\" to match /{1}/", input, regex);
		}
	}
}
