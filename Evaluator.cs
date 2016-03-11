using System;
using System.Windows.Forms;

using ExpressionEvaluator;


namespace Pyramid
{
    public class Evaluator
    {
        public Hero Hero;
        public string Code;

        public Evaluator()
        {
        }

        public void Run()
        {
            var registry = new TypeRegistry();
            registry.RegisterDefaultTypes();
            registry.RegisterType("Console", typeof(Console));
            registry.RegisterType("Actor", typeof(Actor));

            var expression = new CompiledExpression(Code)
            {
                TypeRegistry = registry,
                ExpressionType = CompiledExpressionType.StatementList
            };

            try
            {
                var f = expression.ScopeCompile<Evaluator>();
                f(this);
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }
    }
}