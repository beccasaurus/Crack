using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Represents a console application command, eg. "help" or "commands"</summary>
	/// <remarks>
	/// At the moment, this is nothing more than a regular Application!
	/// </remarks>
	public class Command : Application {

		// TODO add support for setting the Parent of a command, giving us subcommands (with subcommands, and so on)

		/// <summary>Command constructor.  Making an command requires a MethodInfo.</summary>
		public Command(MethodInfo method) : base(method) {}

		/// <summary>Override the exception that gets thrown</summary>
		public override void ThrowInvalidException(string message) {
			throw new InvalidCommandException(message);
		}

		/// <summary>Returns all of the Command found in the given assemblies (see <c>AllFromAssembly</c></summary>
		public static CommandList AllFromAssemblies(params Assembly[] assemblies) {
			var commands = new CommandList();
			foreach (var assembly in assemblies) commands.AddRange(AllFromAssembly(assembly));
			return commands;
		}

		/// <summary>Returns all of the Command found in the given Assembly (my looking for public static methods decorated with [Command]</summary>
		public static CommandList AllFromAssembly(Assembly assembly) {
			var all = Crack.GetMethodInfos<CommandAttribute>(assembly).Select(method => new Command(method)).ToList();
			return new CommandList(all);
		}
	}
}
