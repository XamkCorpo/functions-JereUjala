namespace calculator {
    namespace Interpreter {
        namespace Types {
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

        }
    }
}
