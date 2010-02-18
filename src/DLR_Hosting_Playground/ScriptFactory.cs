namespace DLR_hosting
{
	using System;
	using System.IO;
	using System.Windows.Input;
	using Microsoft.Scripting;
	using Microsoft.Scripting.Hosting;

	public abstract class ScriptFactory
	{
		protected readonly ScriptRuntime runtime;

		protected ScriptFactory()
			: this(ScriptRuntime.CreateFromConfiguration())
		{
		}

		protected ScriptFactory(ScriptRuntime runtime)
		{
     this.runtime = runtime;  
		}
	}

	public static class ScriptFactoryTest
	{
		public static void Run()
		{
			

			// Ein Modul/Namespace in der Runtime anlegen, in dem wir isoliert Aktionen ausführen können
			var scriptCommandFactory = new ScriptCommandFactory();

			//var myClass = new CSharpClass();
			//scriptCommandFactory.SetVariable("sharpClass", myClass);
			
			var initScriptPyCmd = scriptCommandFactory.CreateFromFile(@"script\init.py");
			initScriptPyCmd.Execute(null);

//			var initScriptRbCmd = scriptCommandFactory.CreateFromFile(@"script\init.rb");
//			initScriptRbCmd.Execute(null);

			var rbTestCommand = scriptCommandFactory.CreateFromFile(@"script\test.rb");
			rbTestCommand.Execute(null);
			//test = rbTestCommand.GetVariable("scriptScopeVariable");

			var pyTestCommand = scriptCommandFactory.CreateFromFile(@"script\test.py");
			pyTestCommand.Execute(null);

			string pyStatementText = "sharpClass.Var = \"Hallo statement from C# in Python\"\nprint sharpClass.Var";
			var pyStatementCommand = scriptCommandFactory.CreateFromStatement(pyStatementText, "IronPython");
			if (pyStatementCommand.CanExecute(null))
				pyStatementCommand.Execute(null);
				
			Console.ReadLine();
		}	
	}

	public class ScriptCommandFactory : ScriptFactory
	{
		private ScriptEngine currentEngine;
		private ScriptScope moduleScope;

		public ScriptCommandFactory()
		{
			moduleScope = runtime.CreateScope();
		}

		public ICommand CreateFromFile(string fileName)
		{
			ScriptSource scriptSource = createScriptSource(fileName);
			return new ScriptCommand(scriptSource, moduleScope);
		}

		private ScriptSource createScriptSource(string fileName)
		{
			currentEngine = runtime.GetEngineByFileExtension(Path.GetExtension(fileName));
			return currentEngine.CreateScriptSourceFromFile(fileName);
		}

		public ICommand CreateFromStatement(string scriptCodeStatement, string engineName)
		{
			ScriptSource scriptSource = createScriptSourceFromStatementsAndEngine(
						scriptCodeStatement, engineName);

			return new ScriptCommand(scriptSource, moduleScope);
		}

		//public ICommand CreateFromStreamContentProvider(StreamContentProvider scp)
		//{
		//    //3. aus einem Provider (wird wohl eher von den einzelnen Scriptimplementierungen verwendet)
		//    //TODO: Provider erstellen.
		//    //StreamContentProvider scp;
		//    var scriptSourceFromStreamProvider = engine.CreateScriptSource(scp);
		//    //var scriptSourceFromTextProvider = engine.CreateScriptSource(tcp);

		//    //4. aus dem CodeDom (eingeschränkt)
		//    //CodeObject codeObject;
		//    //TODO: codeObject erzeugen.
		//    //var scriptSourceFromCodeDom = pythonEngine.CreateScriptSource( codeObject);
		//}

		private ScriptSource createScriptSourceFromStatementsAndEngine(string statement, string engineName)
		{
			ScriptEngine engine = runtime.GetEngine(engineName);
			ScriptSource scriptSource = engine.CreateScriptSourceFromString(statement, SourceCodeKind.Statements);
			return scriptSource;
		}

		public void SetVariable(string nameInScript, object @instance)
		{
			moduleScope.SetVariable(nameInScript, @instance);
		}
	}

	public abstract class ScriptCommandBase : ICommand
	{
		protected ScriptSource source;
		protected ScriptScope scope;
		protected CompiledCode code;

		public ScriptCommandBase(ScriptSource scriptSource, ScriptScope scriptScope)
		{
			this.source = scriptSource;
			this.scope = scriptScope;
		}

		public event EventHandler CanExecuteChanged;

		public virtual void Execute(object parameter)
		{
			if (!CanExecute(parameter))
				return;

			//BUG: compiled code can't be executed with scope in ruby atm, use scriptScope
			var engineName = source.Engine.Setup.DisplayName;
			if (engineName == "IronRuby 0.3")
				source.Execute(scope);
			else
				code.Execute(scope);
		}

		public virtual bool CanExecute(object parameter)
		{
			try
			{
				code = source.Compile();
				return true;
			}
			catch (SyntaxErrorException e)
			{
				string msg = "Syntax error in \"{0}\"";
				showError(msg, Path.GetFileName(source.Path), e);
			}
			catch (Exception e)
			{
				string msg = "Error executing file \"{0}\"";
				showError(msg, Path.GetFileName(source.Path), e);
			}

			return false;
		}

		protected abstract void showError(string msg, string name, Exception exception);
	}

	public class ScriptCommand : ScriptCommandBase
	{
		public ScriptCommand(ScriptSource scriptSource, ScriptScope scriptScope)
			: base(scriptSource, scriptScope)
		{
		}

		protected override void showError(string title, string name, Exception e)
		{
			string caption = String.Format(title, name);
			var eo = source.Engine.GetService<ExceptionOperations>();
			string error = eo.FormatException(e);
			Console.Write(caption + ": " + error);
		}
	}
}
