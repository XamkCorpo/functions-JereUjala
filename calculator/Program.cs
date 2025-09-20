namespace calculator {

    // TODO: this https://blog.ndepend.com/csharp-unions/
    // Source for the tokenize and evaluat function:
    // https://algotree.org/algorithms/stack_based/evaluate_infix/

    class Token {
        public bool IsFunction() {
            return isFunction;
        }
        public bool IsParen() {
            return isParen;
        }
        virtual public float Get {
            get;
        }
        protected bool isFunction = false;
        protected bool isParen = false;
    }

    class Number : Token {
        public Number(float num) {
            this.val = num;
        }
        public override float Get => val;
        readonly float val = 0;
    }

    class LeftParen : Token {
        public LeftParen() {
            isParen = true;
        }
        public override float Get => ')';
    }

    class RightParen : Token {
        public RightParen() {
            isParen = true;
        }
        public override float Get => '(';
    }

    class OperatorToken : Token {

        public OperatorToken(string op, int argumentCount) {
            this.isFunction = true;
            this.op = op[0];
            this.argumentCount=argumentCount;
        }

        public override float Get => op;
        public int argumentCount = 0;
        readonly char op;
    }

    struct Expression {
        public Expression(string expression) {
            this.expression = expression;
        }

        public Expression Tokenize() {
            List<Token> tokens = [];
            string tokenString = "";
            /*
             * Used for 2x-1, where x is any dyadic infix operator.
             * E.g. 2^-1. Tells the tokenizer to add parenthesies to the right and the left hand
             * side of an operator-number pair, so 2x-1 becomes 2x(-1).
             * 2x-1-1 becomes 2x(-1)-1. TODO: remove this hack and use the precedence Dictionary<>
             * instead.
            */
            bool addLeftParen = false;

            foreach(var c in expression) {
                if(char.IsNumber(c)) {
                    tokenString += c;
                    continue;
                }

                if(tokenString != string.Empty) {
                    if(!float.TryParse(tokenString, out var number)) {
                        Console.WriteLine("Not a number!");
                        throw new Exception("No number!");
                    }

                    tokens.Add(new Number(number));
                    tokenString = "";

                    if(addLeftParen) {
                        tokens.Add(new LeftParen());
                        addLeftParen = false;
                    }
                }

                if(c == '(') {
                    tokens.Add(new RightParen());
                } else if(c == ')') {
                    tokens.Add(new LeftParen());
                } else {
                    if((tokens.Count == 0) ||
                        ((char)tokens.Last().Get == '(') ||
                        tokens.Last().IsFunction()) {

                        if(tokens.Count != 0) {
                            if((char)tokens.Last().Get != ')') {
                                tokens.Add(new RightParen());
                                addLeftParen = true;
                            }
                        }
                        // TODO: remove hard-coded argument count
                        tokens.Add(new OperatorToken(c.ToString(), 1));

                    } else {
                        // The default amount of arguments for an infix operator is 2
                        // TODO: remove hard-coded argument count
                        tokens.Add(new OperatorToken(c.ToString(), 2));
                    }
                }
            }

            // TODO: somehow remove this repetition
            if(tokenString != string.Empty) {
                if(!float.TryParse(tokenString, out var number)) {
                    Console.WriteLine("Not a number!");
                    throw new Exception("No number!");
                }

                tokens.Add(new Number(number));
                if(addLeftParen) {
                    tokens.Add(new LeftParen());
                    addLeftParen = false;
                }
            }

            infixTokens = tokens;
            return this;
        }

        static Dictionary<string, int> precedence = new (){
            {"^", 4},
            {"/", 3},
            {"*", 3},
            {"+", 2},
            {"-", 2},
            {"(", 1}};

        static bool persistentStack = false;


        static readonly Dictionary<string, Func<Stack<float>, int, Stack<float>>> functionArray
        = new (){
            // Persistent stack
            {"s", (stack, _) => {
                persistentStack = !persistentStack;
                Console.WriteLine($"Persistent stack: turned {(persistentStack ? "on" : "off")}");
                return stack; } },
            // Pop from stack
            {".", (stack, _) => {

                if(stack.Count == 0) {
                    Console.WriteLine("Stack is already empty!");
                } else {
                    stack.Pop();
                }
                return stack;
            }},
            // Print stack
            {"$", (stack, _) => {
                int i = 0;
                if(stack.Count == 0) {
                    Console.WriteLine("Stack is empty!");
                }

                foreach(var token in stack) {
                    Console.WriteLine($"{i}) {token}");
                    i++;
                }
                return stack;
            }},
            {"-", (stack, argc) => {
                if(stack.Count < argc) {
                    throw new Exception(
                        $"Function expected {argc} arguments but got {stack.Count}");
                }

                var r = stack.Pop();

                if(argc == 1) {
                    stack.Push(-r);
                } else if (argc == 2) {
                    stack.Push(stack.Pop() - r);
                }

                return stack;
            }},
            {"^", (stack, argc) => {
                if(stack.Count < argc) {
                    throw new Exception(
                        $"Function expected {argc} arguments but got {stack.Count}");
                }

                var r =  stack.Pop();

                stack.Push(float.Pow(stack.Pop(), r));

                return stack;
            }},

            {"+", (stack, argc) => {
                if(stack.Count < argc) {
                    throw new Exception(
                        $"Function expected {argc} arguments but got {stack.Count}");
                }

                var r = stack.Pop();

                stack.Push(stack.Pop() + r);

                return stack;
            }},
            {"*", (stack, argc) => {
                if(stack.Count < argc) {
                    throw new Exception(
                        $"Function expected {argc} arguments but got {stack.Count}");
                }
                var r = stack.Pop();

                stack.Push(stack.Pop() * r);

                return stack;
            }},
            {"/", (stack, argc) => {
                if(stack.Count < argc) {
                    throw new Exception(
                        $"Function expected {argc} arguments but got {stack.Count}");
                }

                var r = stack.Pop();

                if(r == 0) {
                    throw new Exception("Divide by zero.");
                }

                stack.Push(stack.Pop() / r);

                return stack;
            }},
        };

        static Stack<float> stack = new();
        public float Evaluate() {

            if(!persistentStack) {
                stack.Clear();
            }

            foreach(var token in tokens) {
                if(token.IsFunction()) {

                    if(!functionArray.TryGetValue(
                        ((char)token.Get).ToString(), out var func)) {
                        Console.WriteLine($"No function {(char)token.Get}");
                        return float.NegativeInfinity;
                    }

                    try {
                        stack = func(stack, ((OperatorToken)token).argumentCount);
                    } catch(Exception e) {
                        Console.WriteLine($"Error: {e.Message}");
                        return float.NegativeInfinity;
                    }
                    continue;
                }
                stack.Push(token.Get);
            }

            if(stack.Count <= 0 /* || stack.Peek() == 40 */) {
                return float.NegativeInfinity;
            }

            return stack.Peek();
        }

        static bool Precedence(string l, string r) {
            if(!precedence.TryGetValue(l, out int lhs))
                lhs = 0;

            if(!precedence.TryGetValue(r, out int rhs))
                rhs = 0;

            return lhs >= rhs;
        }

        public Expression InfixToPostfix() {
            Stack<Token> stack = new();
            List<Token> postfixTokens = [];

            // For the popping operator
            stack.Push(new RightParen());
            infixTokens.Add(new LeftParen());

            foreach(Token token in infixTokens) {
                if(token.IsParen()) {
                    if((char)token.Get == '(') {
                        stack.Push(token);
                    } else if((char)token.Get == ')') {
                        // Pop all the operators from the stack
                        while(stack.Count != 0 && stack.First().Get != '(') {
                            postfixTokens.Add(stack.Pop());
                        }

                        if(stack.Count != 0)
                            stack.Pop();
                    }
                    continue;
                } else if(token.IsFunction()) {

                    while(stack.Count != 0
                        && stack.Peek().Get != '('
                        && Precedence(((char)stack.Peek().Get).ToString(),
                        ((char)token.Get).ToString())) {
                        postfixTokens.Add(stack.Pop());
                    }
                    stack.Push(token);

                } else {
                    // Is a number
                    postfixTokens.Add(token);
                }
            }

            tokens = postfixTokens;
            return this;
        }

        string expression = "";
        List<Token> infixTokens = [];
        List<Token> tokens = [];
    }

    internal class Program {
        static Expression GetExpression() {
            Console.Write(">");
            return new(Console.ReadLine() ?? "");
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

                var result = GetExpression().Tokenize().InfixToPostfix().Evaluate();

                if(result != float.NegativeInfinity) {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
