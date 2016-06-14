using System;

using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Pyramid
{
    public class Globals
    {
        public Hero Hero;

        async public void Run(string code)
        {
            try
            {
                await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System"), globals: this);
            }
            catch (CompilationErrorException e)
            {
                // FIXME: do something UIesque with this.
                await Console.Error.WriteLineAsync(String.Join(Environment.NewLine, e.Diagnostics));
            }
        }
    }
}