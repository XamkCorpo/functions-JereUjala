namespace calculator {
    // TODO: this in the future https://blog.ndepend.com/csharp-unions/

    // Source for the tokenize and evaluator function:
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

                var expression = GetExpression();
                if(expression is null) {
                    break;
                }

                var result = expression?.Evaluate();

                if(result != float.NegativeInfinity) {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
