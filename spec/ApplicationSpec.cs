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

		public        Response InstanceMethod(Request req)                { return new Response(); }
		public static object   WrongReturnType(Request req)               { return new Response(); }
		public static Response NoParams()                                 { return new Response(); }
		public static Response TooManyParams(Request req, object another) { return new Response(); }
		public static Response WrongParamType(string req)                 { return new Response(); }

		public static Response ObjectParam(object req) {
			return new Response("You passed args: {0}", string.Join(", ", (req as Request).Arguments));
		}

		MethodInfo Method(string name) {
			return this.GetType().GetMethod(name);
		}

		[Test]
		public void can_get_an_Application_given_a_MethodInfo() {
			var app = new Application(Method("Foo"));
			app.Name.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.Foo");
			app.Description.Should(Be.Null);
			app.Invoke(new string[]{ "hello", "world" }).Text.ShouldEqual("You requested: hello, world\n");
		}

		[Test]
		public void raises_Exception_if_MethodInfo_doesnt_have_correct_parameters_and_whatnot() {
			Application app;

			Should.Throw<InvalidApplicationException>("This method cannot be used as an Application. Must be static.", () => {
				app = new Application(Method("InstanceMethod"));
			});

			Should.Throw<InvalidApplicationException>("This method cannot be used as an Application. Must return a Response.", () => {
				app = new Application(Method("WrongReturnType"));
			});

			Should.Throw<InvalidApplicationException>("This method cannot be used as an Application. Must take 1 parameter (Request).", () => {
				app = new Application(Method("NoParams"));
			});

			Should.Throw<InvalidApplicationException>("This method cannot be used as an Application. Must take 1 parameter (Request).", () => {
				app = new Application(Method("TooManyParams"));
			});

			Should.Throw<InvalidApplicationException>("This method cannot be used as an Application. Parameter must be a Request.", () => {
				app = new Application(Method("WrongParamType"));
			});

			// if the parameter is an object, that's OK, because a Request is an object
			app = new Application(Method("ObjectParam"));
			app.Invoke("hello", "object").Text.ShouldEqual("You passed args: hello, object\n");
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
