using System.Collections.Generic;
using System.IO;
namespace DLR_hosting
{
	using System.Reflection;
	using System;
	using Microsoft.Scripting;
	using Microsoft.Scripting.Hosting;

	static class Programm
	{		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main ()
		{
			//ScriptFactoryTest.Run();
			snippet_sample_for_dotnet_snippets_dot_de();
		}
		
		static void snippet_sample_for_dotnet_snippets_dot_de()		
		{
			//Details zur DLR siehe http://dlr.codeplex.com/
			//Spezifikationen zu finden unter: http://dlr.codeplex.com/Wiki/View.aspx?title=Docs%20and%20specs
			var pythonConfigName = Assembly.LoadFile(Path.GetFullPath("IronPython.dll")).GetName();
			var rubyConfigName = Assembly.LoadFile(Path.GetFullPath("IronRuby.dll")).GetName();
            var javaScriptConfigName = Assembly.LoadFile(Path.GetFullPath("IronJS.dll")).GetName();
			var scriptingConfigName = Assembly.LoadFile(Path.GetFullPath("Microsoft.Scripting.dll")).GetName();

			//Für Fehlerbehandlung siehe als Beispiel ScriptCommand.Execute()
			
			//1.
			ScriptRuntime runtime = ScriptRuntime.CreateFromConfiguration();

			//2. alle Engines, die es so auf der DLR gibt ... muss aber nicht sein.
			//wir können die auch später implizit erstellen lassen, z.B. über die 
			//Dateierweiterung.
			ScriptEngine ruby = runtime.GetEngine("IronRuby");
			ScriptEngine python = runtime.GetEngine("IronPython");
            //LanguageContext ist noch nicht implementiert
            //ScriptEngine javascript = runtime.GetEngine("IronJS");

			//Jetzt registrieren wir unsere Instanzen, die wir im Script bearbeiten
			//wollen für die ScriptEngine in unserem gerade erstellten Scope
			//Dann können wir ...
			CSharpClass testInstance = new CSharpClass("creating from csharp");
			ScriptScope scope = runtime.CreateScope();
			scope.SetVariable("sharpClass", testInstance);

			//nun führen wir ein paar scripts aus. 
			//im init wird eine variable erzeugt. auf diese können wir dann
			//anschiessend im scope zugreifen. python erzeugt die variablen
			//in dem scope der mitgegeben wird. das funktioniert in ruby 
			//noch nicht perfekt...  (Stand IronRuby 0.91)
			//Funktionen und Klassen die erzeugt wurden bleiben bestehen, String variablen nicht.
			ScriptSource initSource = python.CreateScriptSourceFromFile (@"script\init.py");
			//ScriptSource initSource = ruby.CreateScriptSourceFromFile (@"script\init.rb");
			CompiledCode initCode = initSource.Compile();
			initCode.Execute(scope);
			IEnumerable<string> variables = scope.GetVariableNames();


			//... aber in ruby können wir trotzdem die variablen schon bearbeiten.
			//ScriptSource rbFromFileSource = ruby.CreateScriptSourceFromFile (@"script\test.rb");
			//CompiledCode rbFromFileCode = rbFromFileSource.Compile();
			//die folgende Anweisung können wir nicht ausführen.
			//ironruby 0.3 implementiert das noch nicht (oder nicht richtig)
			//rbFromFileCode.Execute(scope);
			//rbFromFileSource.Execute(scope);

			//und in python funktioniert das sowieso ...
			ScriptSource pyFromFileSource = python.CreateScriptSourceFromFile (@"script\test.py");
			CompiledCode pyFromFileCode = pyFromFileSource.Compile();
			pyFromFileCode.Execute (scope);

			//nun einfach ein paar anweisungen ausführen, die wieder variablen aus dem scope
			//in die Mangel nimmt.
			string pyMultiLineStatementText = "sharpClass.Var = \"Hallo Statement in Python\"\nprint sharpClass.Var";
			ScriptSource pyMultiLineStatementSource = python.CreateScriptSourceFromString(
						pyMultiLineStatementText, SourceCodeKind.Statements);
			CompiledCode pyMultiLineStatementCode = pyMultiLineStatementSource.Compile();
			pyMultiLineStatementCode.Execute (scope);

			//und zum schluss ein statement, um uns die variable aus dem scope zu holen.
			string pyExpressionText = "scriptScopeVariable";
			ScriptSource pyExpressionSource = python.CreateScriptSourceFromString(pyExpressionText, SourceCodeKind.Expression);
			CompiledCode pyExpressionCode = pyExpressionSource.Compile();
			string scriptScopeVariable = pyExpressionCode.Execute<string>(scope);
			Console.WriteLine(scriptScopeVariable + " in C#");
		}
	}
}
