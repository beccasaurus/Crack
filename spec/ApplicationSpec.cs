using System;
using System.Reflection;
using NUnit.Framework;
using ConsoleRack;

namespace ConsoleRack.Specs {

	[TestFixture]
	public class ApplicationSpec : Spec {

		public static Response Foo(Request req) {
			return new Response("You requested: {0}", string.Join(", ", req.Arguments));
		}

		MethodInfo FooMethod;

		[SetUp]
		public void Before() {
			FooMethod = this.GetType().GetMethod("Foo");
		}

		[Test]
		public void can_get_an_Application_given_a_MethodInfo() {
			var app = new Application(FooMethod);
			app.Name.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.Foo");
			app.Description.Should(Be.Null);
			app.Invoke(new string[]{ "hello", "world" }).Text.ShouldEqual("You requested: hello, world\n");
		}

		[Test][Ignore]
		public void raises_Exception_if_MethodInfo_doesnt_have_correct_parameters() {
		}

		[Test][Ignore]
		public void Name_defaults_to_full_method_name() {
		}

		[Test][Ignore]
		public void Description_defaults_to_null() {
		}

		[Test][Ignore]
		public void Name_can_be_set() {
		}

		[Test][Ignore]
		public void Description_can_be_set() {
		}

		[Test][Ignore]
		public void can_get_Application_by_name_from_a_ApplicationList() {
		}
	}
}
