using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ConsoleRack {

	/// <summary>Custom List of Middleware that lets you easily get an Middleware by name</summary>
	public class MiddlewareList : List<Middleware>, IList<Middleware>, IEnumerable<Middleware> {
		public MiddlewareList() : base(){}
		public MiddlewareList(IEnumerable<Middleware> middlewares) : base(middlewares){}

		public virtual Middleware this[string name] {
			get { return this.FirstOrDefault(mw => mw.Name == name); }
		}

		public virtual void MoveToTop(Middleware middleware) {
			Remove(middleware);
			Insert(0, middleware);
		}

		public virtual void MoveToBottom(Middleware middleware) {
			Remove(middleware);
			Insert(Count, middleware);
		}

		public virtual void MoveToBefore(Middleware middleware, string beforeName) {
			Remove(middleware);

			var other = this[beforeName];
			if (other == null) return;

			Insert(IndexOf(other), middleware);
		}

		public virtual void MoveToAfter(Middleware middleware, string afterName) {
			Remove(middleware);

			var other = this[afterName];
			if (other == null) return;

			Insert(IndexOf(other) + 1, middleware);
		}

		/// <summary>Given this Request and Application, we sort all of our middleware in order and invoke the first one!</summary>
		public virtual Response Invoke(Request request, Application app) {
			if (Count == 0)
				return app.Invoke(request);

			// Put all of our middleware in the right order
			Middleware.Sort(this);

			// Set the Application property on all middlewares, given the order that they're currently in
			Middleware.SetInnerApplications(this);

			this.Last().Application = app;
			
			return this.First().Invoke(request);
		}
	}
}
