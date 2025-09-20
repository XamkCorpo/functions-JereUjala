namespace calculator {

    enum Operator {
        Add, Subtract, Multiply, Divide, Power,
        // Metaoperators
        LeftParen, RightParen
    }

    // TODO: this https://blog.ndepend.com/csharp-unions/
    class Node {
        public Node? left = null, right = null;
        public Token? token = null;

        public void Print(int num = 0) {
            for(int i = 0; i < num; i++)
                Console.Write(" ");

            Console.WriteLine(token.IsFunction() ? (Operator)token.Get : token.Get);
            if(left != null)
                left.Print(num++);
            if(right != null)
                right.Print(num++);
            Console.WriteLine();

        }

        public float Eval(Dictionary<Operator, Func<float, float, float>> opsArray) {

            //if(token is null)
            //    if(left is null)
            //        return Evaltoken.Get;
            //    else
            //if(right is null)
            //        return token.Get;
            //    else
            //        return null;

            if(token is null) {
                if(left is null && right is not null)
                    return right.Eval(opsArray);

                if(right is null  && left is not null)
                    return left.Eval(opsArray);

                return 0.0f;
            }

            if(!token.IsFunction())
                return token.Get;

            return opsArray[(Operator)token.Get]
                (right.Eval(opsArray), left.Eval(opsArray));
        }

    }

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
        float val = 0;
    }

    class LeftParen : Token {
        public LeftParen() {
            isParen = true;
        }
        //public override float Get => ')';
        public override float Get => (float)Operator.LeftParen;
    }


    class RightParen : Token {
        public RightParen() {
            isParen = true;
        }
        //public override float Get => '(';
        public override float Get => (float)Operator.RightParen;

    }


    class OperatorToken : Token {

        public OperatorToken(string op, int argumentCount) {
            this.isFunction = true;
            this.op = (Operator)op[0];
            this.argumentCount=argumentCount;
        }

        public OperatorToken(Operator op, int argumentCount) {
            this.isFunction = true;
            this.op = op;
            this.argumentCount=argumentCount;
        }
        public override float Get => (int)op;
        public int argumentCount = 0;
        Operator op;
    }

    //class DyadicOperator : Token {
    //    public DyadicOperator(Operator op) {
    //        this.isFunction = true;
    //        this.op = op;
    //    }
    //    public override float Get => (int)op;
    //    Operator op;
    //}

    //class UnaryOperator : Token {
    //    public UnaryOperator(Operator op) {
    //        this.isFunction = true;
    //        this.op = op;
    //    }
    //    public override float Get => (int)op;
    //    Operator op;
    //}


    struct Expression {
        public Expression(string expression) {
            this.expression = expression;
        }

        public List<Token> MakeTokens(Dictionary<string, Operator>
            stringToOpsTable) {
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
                Console.WriteLine($"Token {tokenString}");
                if(char.IsNumber(c)) {
                    tokenString += c;
                    continue;
                }

                if(tokenString != string.Empty) {
                    tokens.Add(new Number(float.Parse(tokenString)));
                    tokenString = "";
                    if(addLeftParen) {
                        tokens.Add(new LeftParen());
                        addLeftParen = false;
                    }
                }

                if(c == '(')
                    tokens.Add(new RightParen());
                else if(c == ')')
                    tokens.Add(new LeftParen());
                else {
                    if((tokens.Count == 0) ||
                        ((Operator)tokens.Last().Get == Operator.RightParen) ||
                        tokens.Last().IsFunction()) {

                        if(tokens.Count != 0) {
                            if((Operator)tokens.Last().Get != Operator.RightParen) {
                                tokens.Add(new RightParen());
                                addLeftParen = true;
                            }
                        }
                        //tokens.Add(new OperatorToken(stringToOpsTable [c.ToString()], 1));
                        tokens.Add(new OperatorToken(c.ToString(), 1));

                    } else {
                        // The default amount of arguments for an infix operator is 2
                        //tokens.Add(new OperatorToken(stringToOpsTable
                        //    [c.ToString()], 2));
                        tokens.Add(new OperatorToken(
                            c.ToString(), 2));
                    }
                }
            }

            // TODO: somehow remove this repetition
            if(tokenString != string.Empty) {
                tokens.Add(new Number(
                    float.Parse(tokenString)));
                if(addLeftParen) {
                    tokens.Add(new LeftParen());
                    addLeftParen = false;
                }
            }

            return tokens;
        }

        string expression = "";
    }

    internal class Program {


        static Dictionary<Operator, int> precedence = new ()
            {
            {Operator.Power, 4},
            {Operator.Divide, 3},
            {Operator.Multiply, 3},
            {Operator.Add, 2},
            {Operator.Subtract, 2},
            {Operator.RightParen, 1},
        };

        static readonly Dictionary<string, Func<Stack<float>, int, Stack<float>>> functionArray
        = new (){
            // Pop from stack
            {".", (Stack<float> stack, int _) => { stack.Pop(); return stack; } },
            // Print stack
            {"$", (Stack<float> stack, int _) => {
                int i = 0;
                foreach(var token in stack) {
                    Console.WriteLine($"{i}) {token.ToString()}");
                    i++;
                } return stack; } },
            {"-", (Stack<float> stack, int argc) => {
                var r = stack.Pop();
                if(argc == 1)
                    stack.Push(-r);

                else if (argc == 2)
                    stack.Push(stack.Pop() - r); 

                return stack;
            }
            },
        };

        public static float Evaluate(List<Token> tokens//,
           // Dictionary<Operator, Func<float, float, float>> ops
            ) {

            Stack<float> stack = new();

            foreach(var token in tokens) {

                if(token.IsFunction()) {
                    var r = stack.Pop();

                    //if(((OperatorToken)token).argumentCount == 1) {
                    //    stack.Push(
                    //        ops[(Operator)token.Get]
                    //            (0, r));
                    //} else if(((OperatorToken)token).argumentCount == 2) {
                    //    var l = stack.Pop();
                    //    stack.Push(
                    //        ops[(Operator)token.Get]
                    //                (l, r));
                    //    // 0 amount of arguments isn't support neither
                    //} else /* if(((OperatorToken)token).argumentCount > 2) */ {
                    //    throw new Exception("Unsupported amount of arguments to a function!");
                    //}


                    //stack.Push(
                    //ops[(Operator)token.Get]
                    //    (stack)
                    //(0, r)
                   // )
                    ;
                    functionArray[token.Get.ToString()] (stack, ((OperatorToken)token).argumentCount);



                    continue;
                }

                stack.Push(token.Get);
            }

            if(stack.Count <= 0) {
                return float.NegativeInfinity;
            }


            return stack.Pop();

        }

        static readonly Dictionary<Operator, Func<float, float, float>> opsArray
        = new (){
            {Operator.Add, (float a, float b) => a+b },
            {Operator.Subtract, (float a, float b) => a-b },
            {Operator.Multiply, (float a, float b) => a*b },
            {Operator.Divide, (float a, float b) => a/b },
            {Operator.Power, float.Pow }
        };

        static readonly Dictionary<string, Operator> stringToOpsTable
        = new (){
            {"+", Operator.Add},
            {"-", Operator.Subtract},
            {"*", Operator.Multiply},
            {"/", Operator.Divide},
            {"^", Operator.Power},
            {"(", Operator.RightParen},
        };



        static List<Token> InfixToPostfix(List<Token> infixTokens,
        Dictionary<string, Operator> stringToOpsTable) {
            Stack<Token> stack = new();
            List<Token> postfixTokens = new();

            // For the popping operator
            stack.Push(new RightParen());
            infixTokens.Add(new LeftParen());

            foreach(Token token in infixTokens) {
                if(token.IsParen()) {
                    //if((char)token.Get == '(') {
                    if((Operator)token.Get == Operator.RightParen) {
                        stack.Push(token);
                        //} else if((char)token.Get == ')') {
                    } else if((Operator)token.Get == Operator.LeftParen) {
                        // Pop all the operators from the stack
                        //while(stack.Peek().Get != '(') {
                        while((Operator)stack.Peek().Get
                            != Operator.RightParen) {
                            postfixTokens.Add(stack.Pop());
                        }
                        stack.Pop();
                    }
                } else if(token.IsFunction()) {

                    while(stack.Count != 0 &&
                        (precedence[
                            (Operator)stack.Peek()
                                             .Get]
                        >= precedence[
                            (Operator)token.Get])) {
                        postfixTokens.Add(stack.Pop());
                    }
                    stack.Push(token);

                } else {
                    // Is a number
                    postfixTokens.Add(token);
                }
            }

            return postfixTokens;
        }

        static Expression GetExpression() =>
            new(Console.ReadLine() ?? "");

        static void Main() {
            for(; ; ) {
                var t = Evaluate(
            //new Expression("1+2*4+1+2*5").
            //new Expression("1+2*4+1").
            InfixToPostfix(
             GetExpression().
            // new Expression("2^5*(3-4)").
                //new Expression("(-1-2-3)").//-6
                //new Expression("-2").//-6
                //new Expression("(-1)+(-1)").
                //new Expression("(-1)^(-1)").
                //new Expression("(-1)^(-2)").
                //new Expression("(2-1)/(1-2)").
                //new Expression("(-2)^(1/2)").
                //new Expression("-(2)^(1/2)").
                //new Expression("(10-1)^(-1+2-2)"). // 0,111111
                //new Expression("2^(-1)").

                //new Expression("2^-1").
            //    new Expression("2^-1-1").
                //new Expression("2*(-1)").

                //new Expression("(1/4)^(-1)").
            MakeTokens(stringToOpsTable), stringToOpsTable)
           // , opsArray
            );

                //t.Print();
                //var t2 = t.Eval(opsArray);
                //Console.WriteLine(t2);
                Console.WriteLine(t);
            }
        }
    }
}
