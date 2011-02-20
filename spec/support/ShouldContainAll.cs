using System;
using System.Collections.Generic;

namespace NUnit.Framework {
	public static class ShouldContainAllExtension {
		public static void ShouldContainAll<T>(this IEnumerable<T> a, params T[] items) {
			foreach (var item in items) a.ShouldContain(item);
		}
		public static void ShouldContainAll(this string a, params string[] items) {
			foreach (var item in items) a.ShouldContain(item);
		}
	}
}
