using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ConsoleRack;

namespace ConsoleRack.Specs {

	// NOTE most of this is directly copy/pasted from ApplicationSpec ... not ideal ... but it works ...
	[TestFixture]
	public class CommandSpec : Spec {

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
		public void can_get_a_Command_given_a_MethodInfo() {
			var app = new Command(Method("Foo"));
			app.Name.ShouldEqual("ConsoleRack.Specs.CommandSpec.Foo");
			app.Description.Should(Be.Null);
			app.Invoke(new string[]{ "hello", "world" }).Text.ShouldEqual("You requested: hello, world\n");
		}

		[Test]
		public void raises_Exception_if_MethodInfo_doesnt_have_correct_parameters_and_whatnot() {
			Command command;

			Should.Throw<InvalidCommandException>("cannot be used as Command. Must be static.", () => {
				command = new Command(Method("InstanceMethod"));
			});

			Should.Throw<InvalidCommandException>("cannot be used as Command. Must return a Response.", () => {
				command = new Command(Method("WrongReturnType"));
			});

			Should.Throw<InvalidCommandException>("cannot be used as Command. Must take 1 parameter (Request).", () => {
				command = new Command(Method("NoParams"));
			});

			Should.Throw<InvalidCommandException>("cannot be used as Command. Must take 1 parameter (Request).", () => {
				command = new Command(Method("TooManyParams"));
			});

			Should.Throw<InvalidCommandException>("cannot be used as Command. Parameter must be a Request.", () => {
				command = new Command(Method("WrongParamType"));
			});

			// if the parameter is an object, that's OK, because a Request is an object
			command = new Command(Method("ObjectParam"));
			command.Invoke("hello", "object").Text.ShouldEqual("You passed args: hello, object\n");
		}

		[Test]
		public void Name_defaults_to_full_method_name() {
			new Command(Method("Foo")).Name.ShouldEqual("ConsoleRack.Specs.CommandSpec.Foo");
			new Command(Method("ObjectParam")).Name.ShouldEqual("ConsoleRack.Specs.CommandSpec.ObjectParam");
		}

		[Test]
		public void Description_defaults_to_null() {
			new Command(Method("Foo")).Description.Should(Be.Null);
			new Command(Method("ObjectParam")).Description.Should(Be.Null);
		}

		[Test]
		public void Name_can_be_set_manually() {
			var app = new Command(Method("Foo"));
			app.Name.ShouldEqual("ConsoleRack.Specs.CommandSpec.Foo");
			app.Name = "Overriden";
			app.Name.ShouldEqual("Overriden");
		}

		[Test]
		public void Description_can_be_set_manually() {
			var app = new Command(Method("Foo"));
			app.Description.Should(Be.Null);
			app.Description = "Overriden";
			app.Description.ShouldEqual("Overriden");
		}

		[Command("my description")]
		public static Response WithAttribute1(Request req){ return new Response(); }

		[Command("CustomName", "my description")]
		public static Response WithAttribute2(Request req){ return new Response(); }

		[Command(Name = "CustomName")]
		public static Response WithAttribute3(Request req){ return new Response(); }

		[Command(Name = "CustomName", Description = "my description")]
		public static Response WithAttribute4(Request req){ return new Response(); }

		[Test]
		public void Name_can_be_set_via_attribute() {
			new Command(Method("WithAttribute1")).Name.ShouldEqual("ConsoleRack.Specs.CommandSpec.WithAttribute1");
			new Command(Method("WithAttribute2")).Name.ShouldEqual("CustomName");
			new Command(Method("WithAttribute3")).Name.ShouldEqual("CustomName");
			new Command(Method("WithAttribute4")).Name.ShouldEqual("CustomName");
		}

		[Test]
		public void Description_can_be_set_via_attribute() {
			new Command(Method("WithAttribute1")).Description.ShouldEqual("my description");
			new Command(Method("WithAttribute2")).Description.ShouldEqual("my description");
			new Command(Method("WithAttribute3")).Description.Should(Be.Null);
			new Command(Method("WithAttribute4")).Description.ShouldEqual("my description");
		}

		[Test]
		public void can_get_Command_by_name_from_a_CommandList() {
			var list = new CommandList();
			list.Add(new Command(Method("Foo")));
			list.Add(new Command(Method("ObjectParam")));
			list.Add(new Command(Method("WithAttribute2")));

			list["Does not exist"].Should(Be.Null);
			list["CustomName"].MethodFullName.ShouldEqual("ConsoleRack.Specs.CommandSpec.WithAttribute2");
			list["ConsoleRack.Specs.CommandSpec.Foo"].Invoke("hello", "foo").Text.ShouldEqual("You requested: hello, foo\n");
		}

		[Test]
		public void can_get_all_command_starting_with_certain_text_from_a_CommandList() {
			var list = new CommandList();

			list.Add(new Command(Method("Foo")){ Name = "abc" });
			list.Add(new Command(Method("Foo")){ Name = "alpha" });
			list.Add(new Command(Method("Foo")){ Name = "beta" });
			list.Add(new Command(Method("Foo")){ Name = "beer" });
			list.Add(new Command(Method("Foo")){ Name = "booze" });
			list.Add(new Command(Method("Foo")){ Name = "zebra" });

			list.StartingWith("a").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "abc", "alpha" });
			list.StartingWith("ab").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "abc" });
			list.StartingWith("al").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "alpha" });

			list.StartingWith("b").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "beer", "beta", "booze" });
			list.StartingWith("bo").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "booze" });
			list.StartingWith("be").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "beer", "beta" });
			list.StartingWith("bet").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "beta" });
			list.StartingWith("bee").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "beer" });

			list.StartingWith("z").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "zebra" });
			list.StartingWith("zeb").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "zebra" });
			list.StartingWith("zebra").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "zebra" });

			list.StartingWith("x").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ });
		}

		[Test]
		public void can_match_commands_with_certain_text_from_a_CommandList() {
			var list = new CommandList();

			list.Add(new Command(Method("Foo")){ Name = "foo" });
			list.Add(new Command(Method("Foo")){ Name = "foot" });

			list.Match("f").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "foo", "foot" });
			list.Match("fo").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "foo", "foot" });
			list.Match("foo").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "foo" }); // EXACT MATCH, so we don't return foot // TODO change this ... if using Match, user can check if any of the results are a command via list.IsCommand("") ... or we need to add something like that.  this result is currently unintuitive.
			list.Match("foot").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{ "foot" });
			list.Match("foott").Select(cmd => cmd.Name).ToArray().ShouldEqual(new string[]{  });
		}

		[Test][Ignore]
		public void can_get_all_commands_in_an_assembly() {
		}
	}
}
