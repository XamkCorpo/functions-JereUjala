namespace Tests;

using Xunit;

public class Unary {
    [Fact]
    public void MinuMinusIsPositive() {
        Assert.Equal(1.0f, new calculator.Interpreter.
                          Expression("--1").Evaluate()
                          );
    }

    [Fact]
    public void MinusIsNegative() {
        Assert.Equal(-1.0f, new calculator.Interpreter.
                          Expression("-1").Evaluate());
        Assert.Equal(-1.0f, new calculator.Interpreter.
        Expression("---1").Evaluate());
    }
}

public class Dyadic {
    [Fact]
    public void ToThePowerOf() {
        Assert.Equal(1.0f/9.0f, new calculator.Interpreter.
            Expression("9^-1").Evaluate());
        Assert.Equal(1.0f/9.0f, new calculator.Interpreter.
        Expression("9^(-1)").Evaluate());

        Assert.Equal(1.0f/9.0f, new calculator.Interpreter.
        Expression("(10-1)^(-1+2-2)")
        .Evaluate());

        Assert.Equal(1.0f/9.0f, new calculator.Interpreter.
        Expression("(10-1)^(-1)")
        .Evaluate());

        Assert.Equal(1.0f/9.0f, new calculator.Interpreter.
        Expression("(9)^(-1)")
        .Evaluate());



    }

    [Fact]
    public void MinuIsNegative() {
        Assert.Equal(-1.0f, new calculator.Interpreter.
                          Expression("-1").
                          Evaluate());
    }
}

public class Constant {
    public void NumericConstants() {
        Assert.Equal(float.E, new calculator.Interpreter.
                          Expression("e").
                          Evaluate());
        Assert.Equal(float.Pi, new calculator.Interpreter.
                          Expression("p").
                          Evaluate());

    }
}


