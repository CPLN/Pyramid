using System;
using ExpressionEvaluator;
using System.Windows.Forms;

namespace Pyramid
{
    public class Evaluator
    {
        public Actor Actor;
        public string Code;

        public Evaluator()
        {
            
        }

        public void Run()
        {
            var registry = new TypeRegistry();
            registry.RegisterDefaultTypes();
            registry.RegisterType("Console", typeof(Console));

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
            catch (ArgumentException e) {
                MessageBox.Show(e.Message);
            }
        }
    }
}