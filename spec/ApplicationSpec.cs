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

			Should.Throw<InvalidApplicationException>("cannot be used as an Application. Must be static.", () => {
				app = new Application(Method("InstanceMethod"));
			});

			Should.Throw<InvalidApplicationException>("cannot be used as an Application. Must return a Response.", () => {
				app = new Application(Method("WrongReturnType"));
			});

			Should.Throw<InvalidApplicationException>("cannot be used as an Application. Must take 1 parameter (Request).", () => {
				app = new Application(Method("NoParams"));
			});

			Should.Throw<InvalidApplicationException>("cannot be used as an Application. Must take 1 parameter (Request).", () => {
				app = new Application(Method("TooManyParams"));
			});

			Should.Throw<InvalidApplicationException>("cannot be used as an Application. Parameter must be a Request.", () => {
				app = new Application(Method("WrongParamType"));
			});

			// if the parameter is an object, that's OK, because a Request is an object
			app = new Application(Method("ObjectParam"));
			app.Invoke("hello", "object").Text.ShouldEqual("You passed args: hello, object\n");
		}

		[Test]
		public void Name_defaults_to_full_method_name() {
			new Application(Method("Foo")).Name.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.Foo");
			new Application(Method("ObjectParam")).Name.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.ObjectParam");
		}

		[Test]
		public void Description_defaults_to_null() {
			new Application(Method("Foo")).Description.Should(Be.Null);
			new Application(Method("ObjectParam")).Description.Should(Be.Null);
		}

		[Test]
		public void Name_can_be_set_manually() {
			var app = new Application(Method("Foo"));
			app.Name.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.Foo");
			app.Name = "Overriden";
			app.Name.ShouldEqual("Overriden");
		}

		[Test]
		public void Description_can_be_set_manually() {
			var app = new Application(Method("Foo"));
			app.Description.Should(Be.Null);
			app.Description = "Overriden";
			app.Description.ShouldEqual("Overriden");
		}

		[Application("my description")]
		public static Response WithAttribute1(Request req){ return new Response(); }

		[Application("CustomName", "my description")]
		public static Response WithAttribute2(Request req){ return new Response(); }

		[Application(Name = "CustomName")]
		public static Response WithAttribute3(Request req){ return new Response(); }

		[Application(Name = "CustomName", Description = "my description")]
		public static Response WithAttribute4(Request req){ return new Response(); }

		[Test]
		public void Name_can_be_set_via_attribute() {
			new Application(Method("WithAttribute1")).Name.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.WithAttribute1");
			new Application(Method("WithAttribute2")).Name.ShouldEqual("CustomName");
			new Application(Method("WithAttribute3")).Name.ShouldEqual("CustomName");
			new Application(Method("WithAttribute4")).Name.ShouldEqual("CustomName");
		}

		[Test]
		public void Description_can_be_set_via_attribute() {
			new Application(Method("WithAttribute1")).Description.ShouldEqual("my description");
			new Application(Method("WithAttribute2")).Description.ShouldEqual("my description");
			new Application(Method("WithAttribute3")).Description.Should(Be.Null);
			new Application(Method("WithAttribute4")).Description.ShouldEqual("my description");
		}

		[Test]
		public void can_get_Application_by_name_from_a_ApplicationList() {
			var list = new ApplicationList();
			list.Add(new Application(Method("Foo")));
			list.Add(new Application(Method("ObjectParam")));
			list.Add(new Application(Method("WithAttribute2")));

			list["Does not exist"].Should(Be.Null);
			list["CustomName"].MethodFullName.ShouldEqual("ConsoleRack.Specs.ApplicationSpec.WithAttribute2");
			list["ConsoleRack.Specs.ApplicationSpec.Foo"].Invoke("hello", "foo").Text.ShouldEqual("You requested: hello, foo\n");
		}

		[Middleware]
		public static Response WriteBeforeAndAfter(Request request, Application app) {
			var response = app.Invoke(request);
			response.Text = string.Format("BEFORE\n{0}\nAFTER", response.Text);
			return response;
		}

		[Test]
		public void can_by_run_given_1_middleware() {
			var response = new Application(Method("Foo")).Invoke(new Request("hello"), new Middleware(Method("WriteBeforeAndAfter")));
			response.Text.ShouldEqual("BEFORE\nYou requested: hello\n\nAFTER");
		}

		[Test]
		public void can_by_run_given_2_middleware() {
			var middleware1 = new Middleware(Method("WriteBeforeAndAfter"));
			var middleware2 = new Middleware(Method("WriteBeforeAndAfter"));
			var response    = new Application(Method("Foo")).Invoke(new Request("hello"), middleware1, middleware2);
			response.Text.ShouldEqual("BEFORE\nBEFORE\nYou requested: hello\n\nAFTER\nAFTER");
		}

		[Test][Ignore]
		public void can_by_run_given_a_list_of_middleware() {
		}

		[Test][Ignore]
		public void can_get_all_Application_in_a_given_assembly() {
		}
	}
}
