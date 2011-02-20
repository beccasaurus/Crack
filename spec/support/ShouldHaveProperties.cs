using System;
using System.Reflection;

namespace NUnit.Framework {
	public static class ShouldHavePropertiesExtension {

		/// <summary>For checking lots of properties on an object</summary>
		/// <remarks>
		/// Lets you say:
		/// <code>
		/// foo.ShouldHaveProperties(new {
		///		Id  = "This",
		///		Foo = 15.89,
		///		Hi  = "Something"
		/// });
		/// </code>
		///
		/// Instead of having to say:
		/// <code>
		/// foo.Id.ShouldEqual("This");
		/// foo.Foo.ShouldEqual(15.89);
		/// foo.Hi.ShouldEqual("Something");
		/// </code>
		/// </remarks>
		public static void ShouldHaveProperties(this object o, object anonymousObjectOfAttributes) {
			var type = o.GetType();
			foreach (var propertyItem in anonymousObjectOfAttributes.ToDictionary()) {
				var property = type.GetProperty(propertyItem.Key);
				var expected = propertyItem.Value;
				var actual   = property.GetValue(o, new object[] {});
				actual.ShouldEqual(expected);
			}
		}
	}
}
