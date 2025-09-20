namespace calculator {
    namespace Interpreter {
        struct Expression {
            public Expression(string expression) {
                this.expression = expression;
            }

            static readonly Dictionary<string, int> precedence = new (){
            {"^", 4},
            {"/", 3},
            {"*", 3},
            {"+", 2},
            {"-", 2},
            {"(", 1}};

            static bool  persistentStack = false;
            static readonly string decimalSeperator =
                        System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;

            static readonly Dictionary<string, Func<Stack<float>, int, Stack<float>>>
                functionArray = new (){
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

                if(argc == 1)  {
                 stack.Push(r);
                 return stack;
                }

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
            {"!", (stack, argc) => {

                if(stack.Count < 1) {
                    throw new Exception(
                        $"Function expected {argc} arguments but got {stack.Count}");
                }

                var r = stack.Pop();
                if (r == 0) {
                    stack.Push(1.0f);
                    return stack;
                }

                if(r < 0) {
                    throw new Exception($"Factor was negative ({r})");
                }

                for(int i = ((int)r)-1; i > 0; --i) {
                    r*=i;
                }

                stack.Push(r);

                return stack;
            }},
        };


            public Expression Tokenize() {
                List<Types.Token> tokens = [];
                string tokenString = "";
                /*
                 * Used for 2x-1, where x is any dyadic infix operator.
                 * E.g. 2^-1. Tells the tokenizer to add parenthesies to the right and the left hand
                 * side of an operator-number pair, so 2x-1 becomes 2x(-1).
                 * 2x-1-1 becomes 2x(-1)-1. TODO: remove this hack and use the precedence Dictionary<>
                 * instead.
                */
                //bool addLeftParen = false;
                int addLeftParen = 0;

                foreach(var c in expression) {
                    if(char.IsNumber(c) || c == decimalSeperator[0]) {
                        tokenString += c;
                        continue;
                    }

                    if(c == ' ') {
                        if(tokenString != string.Empty) {
                            if(!float.TryParse(tokenString, out var number)) {
                                Console.WriteLine("Not a number!");
                                throw new Exception("No number!");
                            }

                            tokens.Add(new Types.Number(number));
                            tokenString = "";
                        }
                        continue;
                    }

                    // Add number
                    if(tokenString != string.Empty) {
                        if(!float.TryParse(tokenString, out var number)) {
                            Console.WriteLine("Not a number!");
                            throw new Exception("No number!");
                        }

                        tokens.Add(new Types.Number(number));
                        tokenString = "";

                        //if(addLeftParen) {
                        if(addLeftParen > 0) {
                            tokens.Add(new Types.LeftParen());
                            //addLeftParen = false;
                            addLeftParen--;
                        }
                    }


                    if(c == '(') {
                        tokens.Add(new Types.RightParen());
                        continue;
                    } else if(c == ')') {
                        tokens.Add(new Types.LeftParen());
                        continue;
                    } else if(c == 'e') {
                        tokens.Add(new Types.Number(float.E));
                        continue;
                    } else if(c == 'p') {
                        tokens.Add(new Types.Number(float.Pi));
                        continue;
                    }

                    // Else the token is a function
                    if((tokens.Count == 0) ||
                        ((char)tokens.Last().Get == '(') ||
                        tokens.Last().IsFunction()) {

                        // Add '(' to 2x-1 = 2x(-
                        if(tokens.Count != 0) {
                            if((char)tokens.Last().Get != ')') {
                                tokens.Add(new Types.RightParen());
                                //addLeftParen = true;
                                addLeftParen++;
                            }
                        }

                        // TODO: remove hard-coded argument count
                        tokens.Add(new Types.OperatorToken(c.ToString(), 1));

                        continue;
                    }

                    // The default amount of arguments for an infix operator is 2
                    // TODO: remove hard-coded argument count
                    tokens.Add(new Types.OperatorToken(c.ToString(), 2));
                }

                // TODO: somehow remove this repetition
                if(tokenString != string.Empty) {
                    if(!float.TryParse(tokenString, out var number)) {
                        Console.WriteLine("Not a number!");
                        throw new Exception("No number!");
                    }

                    tokens.Add(new Types.Number(number));
                    //if(addLeftParen) {
                    //    tokens.Add(new Types.LeftParen());
                    //    addLeftParen = false;
                    //}
                }

                for(int i = addLeftParen; i > 0; --i) {
                        tokens.Add(new Types.LeftParen());
                }

                infixTokens = tokens;
                return this;
            }

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
                            stack = func(stack, ((Types.OperatorToken)token).argumentCount);
                        } catch(Exception e) {
                            Console.WriteLine($"Error: {e.Message}");
                            return float.NegativeInfinity;
                        }
                        continue;
                    }
                    stack.Push(token.Get);
                }

                if(stack.Count <= 0) {
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
                Stack<Types.Token> stack = new();
                List<Types.Token> postfixTokens = [];

                stack.Push(new Types.RightParen());
                infixTokens.Add(new Types.LeftParen());

                foreach(Types.Token token in infixTokens) {
                    if(token.IsParen()) {
                        if((char)token.Get == '(') {
                            stack.Push(token);
                        } else if((char)token.Get == ')') {
                            // Pop all the operators from the stack
                            while(stack.Count != 0 && stack.First().Get != '(') {
                                postfixTokens.Add(stack.Pop());
                            }

                            if(stack.Count != 0) {
                                stack.Pop();
                            }
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

                        continue;
                    }

                    // Token is a number
                    postfixTokens.Add(token);
                }

                tokens = postfixTokens;
                return this;
            }

            static Stack<float> stack = new();
            string expression = "";
            List<Types.Token> infixTokens = [];
            List<Types.Token> tokens = [];
        }
    }
}
