namespace calculator {
    // TODO: this in the future https://blog.ndepend.com/csharp-unions/
    // Source for the tokenize and evaluat function:
    // https://algotree.org/algorithms/stack_based/evaluate_infix/

    internal class Program {
        static Interpreter.Expression? GetExpression() {
            Console.Write(">");
            var input = Console.ReadLine() ?? "";

            if(input is null || input == "q") {
                return null;
            }

            return new(input);
        }

        static void Main() {
            for(; ; ) {
                //new Expression("1+2*4+1+2*5").
                //new Expression("1+2*4+1").
                //new Expression("$").
                //new Expression("2^5*(3-4)").
                //new Expression("(-1-2-3)").//-6
                //new Expression("-2").//-6
                //new Expression("(-1)+(-1)").
                //new Expression("(-1)^(-1)").
                //new Expression("(-1)^(-2)").
                //new Expression("(2-1)/(1-2)").
                //new Expression("(-2)^(1/2)").
                //new Expression("-(2)^(1/2)").
                //new Expression("(10-1)^(-1+2-2)"). // 0,111111
                //new Expression("(10-1)^(-1)"). // 0,111111
                //new Expression("(9)^(-1)"). // 0,111111
                //new Expression("9^(-1)"). // 0,111111
                //new Expression("9^-1"). // 0,111111
                //new Expression("2^(-1)").
                //new Expression("2^-1").
                //new Expression("2^-1-1").
                //new Expression("2*(-1)").
                //new Expression("(1/4)^(-1)").
                //new Expression("-1").

                var expression = GetExpression();
                if(expression is null) {
                    break;
                }

                var result = expression?.Tokenize().InfixToPostfix().Evaluate();

                if(result != float.NegativeInfinity) {
                    Console.WriteLine(result);
                }
            }

        }
    }
}
