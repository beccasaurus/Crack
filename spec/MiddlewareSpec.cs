using System;
using System.Reflection;
using NUnit.Framework;

namespace ConsoleRack.Specs {

	[TestFixture]
	public class MiddlewareSpec : Spec {

		public static Response Foo(Request req, Application app) {
			return new Response("You requested: {0}", string.Join(", ", req.Arguments));
		}

		public        Response InstanceMethod(Request req, Application app)                { return new Response(); }
		public static object   WrongReturnType(Request req, Application app)               { return new Response(); }
		public static Response NoParams()                                                  { return new Response(); }
		public static Response TooManyParams(Request req, Application app, object another) { return new Response(); }
		public static Response WrongParamType1(string req, Application app)                { return new Response(); }
		public static Response WrongParamType2(Request req, string app)                    { return new Response(); }

		public static Response ObjectParams(object req, object app) {
			return new Response("You passed args: {0}", string.Join(", ", (req as Request).Arguments));
		}

		MethodInfo Method(string name) {
			return this.GetType().GetMethod(name);
		}

		[Test]
		public void can_get_an_Middleware_given_a_MethodInfo() {
			var mw = new Middleware(Method("Foo"));
			mw.Name.ShouldEqual("ConsoleRack.Specs.MiddlewareSpec.Foo");
			mw.Description.Should(Be.Null);
			mw.Invoke(new string[]{ "hello", "world" }).Text.ShouldEqual("You requested: hello, world\n");
		}

		[Test]
		public void raises_Exception_if_MethodInfo_doesnt_have_correct_parameters_and_whatnot() {
			Middleware mw;

			Should.Throw<InvalidMiddlewareException>("This method cannot be used as a Middleware. Must be static.", () => {
				mw = new Middleware(Method("InstanceMethod"));
			});

			Should.Throw<InvalidMiddlewareException>("This method cannot be used as a Middleware. Must return a Response.", () => {
				mw = new Middleware(Method("WrongReturnType"));
			});

			Should.Throw<InvalidMiddlewareException>("This method cannot be used as a Middleware. Must take 2 parameters (Request, Application).", () => {
				mw = new Middleware(Method("NoParams"));
			});

			Should.Throw<InvalidMiddlewareException>("This method cannot be used as a Middleware. Must take 2 parameters (Request, Application).", () => {
				mw = new Middleware(Method("TooManyParams"));
			});

			Should.Throw<InvalidMiddlewareException>("This method cannot be used as a Middleware. Parameter 1 must be a Request.", () => {
				mw = new Middleware(Method("WrongParamType1"));
			});

			Should.Throw<InvalidMiddlewareException>("This method cannot be used as a Middleware. Parameter 2 must be an Application.", () => {
				mw = new Middleware(Method("WrongParamType2"));
			});

			// if the parameter is an object, that's OK, because a Request is an object
			mw = new Middleware(Method("ObjectParams"));
			mw.Invoke("hello", "object").Text.ShouldEqual("You passed args: hello, object\n");
		}

		[Test]
		public void Name_defaults_to_full_method_name() {
			new Middleware(Method("Foo")).Name.ShouldEqual("ConsoleRack.Specs.MiddlewareSpec.Foo");
			new Middleware(Method("ObjectParams")).Name.ShouldEqual("ConsoleRack.Specs.MiddlewareSpec.ObjectParams");
		}

		[Test]
		public void Description_defaults_to_null() {
			new Middleware(Method("Foo")).Description.Should(Be.Null);
			new Middleware(Method("ObjectParams")).Description.Should(Be.Null);
		}

		[Test]
		public void Name_can_be_set_manually() {
			var mw = new Middleware(Method("Foo"));
			mw.Name.ShouldEqual("ConsoleRack.Specs.MiddlewareSpec.Foo");
			mw.Name = "Overriden";
			mw.Name.ShouldEqual("Overriden");
		}

		[Test]
		public void Description_can_be_set_manually() {
			var mw = new Middleware(Method("Foo"));
			mw.Description.Should(Be.Null);
			mw.Description = "Overriden";
			mw.Description.ShouldEqual("Overriden");
		}

		[Middleware("my description")]
		public static Response WithAttribute1(Request req, Application app){ return new Response(); }

		[Middleware("CustomName", "my description")]
		public static Response WithAttribute2(Request req, Application app){ return new Response(); }

		[Middleware(Name = "CustomName")]
		public static Response WithAttribute3(Request req, Application app){ return new Response(); }

		[Middleware(Name = "CustomName", Description = "my description")]
		public static Response WithAttribute4(Request req, Application app){ return new Response(); }

		[Test]
		public void Name_can_be_set_via_attribute() {
			new Middleware(Method("WithAttribute1")).Name.ShouldEqual("ConsoleRack.Specs.MiddlewareSpec.WithAttribute1");
			new Middleware(Method("WithAttribute2")).Name.ShouldEqual("CustomName");
			new Middleware(Method("WithAttribute3")).Name.ShouldEqual("CustomName");
			new Middleware(Method("WithAttribute4")).Name.ShouldEqual("CustomName");
		}

		[Test]
		public void Description_can_be_set_via_attribute() {
			new Middleware(Method("WithAttribute1")).Description.ShouldEqual("my description");
			new Middleware(Method("WithAttribute2")).Description.ShouldEqual("my description");
			new Middleware(Method("WithAttribute3")).Description.Should(Be.Null);
			new Middleware(Method("WithAttribute4")).Description.ShouldEqual("my description");
		}

		[Test]
		public void can_get_Middleware_by_name_from_a_MiddlewareList() {
			var list = new MiddlewareList();
			list.Add(new Middleware(Method("Foo")));
			list.Add(new Middleware(Method("ObjectParams")));
			list.Add(new Middleware(Method("WithAttribute2")));

			list["Does not exist"].Should(Be.Null);
			list["CustomName"].MethodFullName.ShouldEqual("ConsoleRack.Specs.MiddlewareSpec.WithAttribute2");
			list["ConsoleRack.Specs.MiddlewareSpec.Foo"].Invoke("hello", "foo").Text.ShouldEqual("You requested: hello, foo\n");
		}

		[Middleware(Name = "Bar")]
		public static Response Bar(Request req, Application app) {
			//return app.Invoke(req).Prepend("Bar");
			return null;
		}

		[Middleware(Name = "Awesome")]
		public static Response Awesome(Request req, Application app) {
			return null;
		}

		[Middleware(Name = "GoesBeforeAwesome", Before = "")]
		public static Response GoesBeforeAwesome(Request req, Application app) {
			return null;
		}

		[Middleware(Name= "GoesAfterBar", After = "Bar")]
		public static Response GoesAfterBar(Request req, Application app) {
			return null;
		}

		[Test][Ignore]
		public void can_specify_the_name_of_a_Middleware_that_this_should_be_put_Before() {
			
		}

		[Test][Ignore]
		public void can_specify_the_name_of_a_Middleware_that_this_should_be_put_After() {
		}

		[Test][Ignore]
		public void can_specify_that_Middleware_should_be_run_First() {
		}

		[Test][Ignore]
		public void can_specify_that_Middleware_should_be_run_Last() {
		}
	}
}
