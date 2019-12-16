# C# 8 Switch Expressions with Pattern Matching

*Written 12/2019*

Most .NET engineers are familiar with the original `switch` statement in C#. Like similar constructs in other object oriented languages, given an arbitrary expression, you can match it's result to a `case`, and execute selected statements.

```csharp
switch (car.Color) {
    case Color.Red:
        Console.WriteLine("Color is red!");
        break;
    case Color.Blue:
        Console.WriteLine("Color is blue!");
        break;
    default:
        Console.WriteLine("Color is not red or blue!");
        break;
}
```

In the past, I've found that `switch` statements were useful for cleaning up long `if else` chains, but I rarely used them in my code. The `switch-case-break` syntax felt bloated with keywords, and, before C# 7, cases only supported the constant pattern. This meant that each case value had to be a compile-time constant.

Fast forward to C# 8, and the lowly `switch` statement has been upgraded with new features that make it much more appealing! Take a look at how we can simplify the above example:

```csharp
Console.WriteLine(car.Color switch
{
    Color.Red => "Color is red!",
    Color.Blue => "Color is blue!",
    _ => "Color is not red or blue!"
});
```

I'll introduce five new *matching patterns* and three other `switch` improvements that can make complex control flow short and readable.

## Contents

- [C# 8 Switch Expressions with Pattern Matching](#c-8-switch-expressions-with-pattern-matching)
  - [Contents](#contents)
  - [C# 7 Features](#c-7-features)
    - [Declaration Pattern](#declaration-pattern)
    - [Var Pattern](#var-pattern)
    - [Case Guards](#case-guards)
  - [C# 8 Features](#c-8-features)
    - [Switch Expressions](#switch-expressions)
    - [Discard Pattern](#discard-pattern)
    - [Positional Pattern](#positional-pattern)
    - [Property Pattern](#property-pattern)
    - [Recursive Pattern Matching](#recursive-pattern-matching)
  - [Conclusion](#conclusion)
  - [Additional Resources](#additional-resources)

## C# 7 Features

Any features in this section will be compatible with .NET Core >=2.0, .NET Standard >=1.0, and all versions of .NET Framework. See the [C# language versioning article](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version) for more information.

### Declaration Pattern

The *declaration pattern* was introduced in C# 7. It enables `case` matching based on the type of value passed in. The syntax is as follows:

```csharp
var person = new { Name = "Drake" };
switch (person.Name)
{
    // type followed by designation
    // variable, name, will always be non-null if matched
    // only matches values assignable from the given type
    case string name:
        Console.WriteLine($"Hello, {name}!");
        break;

    // only matches null values
    case null:
        Console.WriteLine($"Hello, user!");
        break;
}
```

At first glance, its uses appear narrow, but when combined with polymorphism, the pattern is much more powerful. Consider this simple inheritance structure that models shapes:

```csharp
class Square : Shape
{
    public double Height { get; set; }
    public double Width { get; set; }
}

class Circle : Shape
{
    public double Radius { get; set; }
}
```

We can define an extension method for `Shape` that calculates area depending on the type of shape passed in.

```csharp
static double GetArea(this Shape shape)
{
    switch (shape)
    {
        // match Rectangle type
        case Rectangle rectangle:
            // rectangle variable is:
            //   - non-null and of type Rectangle
            //   - in scope within the case
            return rectangle.Height * rectangle.Width;
        case Circle circle:
            return Math.PI * circle.Radius * circle.Radius;
        case null:
            throw new ArgumentNullException(nameof(shape));
        default:
            throw new NotImplementedException();
    }
}
```

### Var Pattern

Expressions always match the *var pattern*. In this way, it's a suitable replacement for the `default` `case` when you have the additional requirement to capture the result of the expression being 'switched' on. **The *var pattern* matches `null` values.** Consider the following example:

```csharp
static bool TestShapeRequirement(this Shape shape)
{
    switch (shape.GetArea())
    {
        // the constant pattern
        case 0:
            Console.WriteLine("A shape with non-zero area is required!");
            return false;

        // catch null case first
        case null:
            Console.WriteLine("A non-null area is required!");
            return false;

        // var followed by designation
        case var area:
            Console.WriteLine($"Shape with area {area} accepted!");
            return true;
    }
}
```

### Case Guards

Case guards give additional control over the execution of case blocks with the `when` clause.

```csharp
static double GetAreaOptimized(this Shape shape)
{
    switch (shape)
    {
                                 // 'when' followed by boolean expression
        case Rectangle rectangle when rectangle.Height is 0 || rectangle.Width is 0:
        case Circle circle when circle.Radius is 0:
            return 0;
        // ...
    }
}
```

Any boolean valued expression can follow the `when` keyword. This gives the developer the ability to conditionally execute each `case` based on an expression evaluated at runtime rather than a compile time constant or other pattern.

By the way, if you haven't seen the `is` keyword before, you'll love it! It takes all the pattern matching goodness seen and this article, and allows us to use it in any context as a boolean valued expression. See [C# 7 Pattern Matching - Is Expression](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-7.0/pattern-matching#is-expression) for more information.

## C# 8 Features

The following features are only available in C# 8 and above. Without changing compiler settings, you'll only be able to use these in .NET Core >=3.0 and .NET Standard >=2.1. Don't fear, it's trivial to get these features working in .NET Framework. To do this, you'll need to add a single compiler setting to your `.csproj`. [See this link](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version#edit-the-project-file).

### Switch Expressions

Introduced in C# 8, the `switch` expression addresses my primary gripe with the `switch` statement, the syntax. `switch` expressions remove the need for the `case`, `break`, and `default` keywords, but they also go one step further by turning the `switch` statement into an expression!

This allows us to convert the `GetArea` method into this much more readable version:

```csharp
static double GetAreaExpression(this Shape shape)
{
    return shape switch
    {
        Rectangle rectangle => rectangle.Height * rectangle.Width,
        Circle circle => Math.PI * circle.Radius * circle.Radius,
        null => throw new ArgumentNullException(nameof(shape)),
        var unknownShape => throw new NotImplementedException(),
    };
}
```

First, take note that the expression or variable to be switched on now proceeds the `switch` keyword, and parentheses are no longer required. Each case takes the form of `(pattern) (optional when clause) => (expression)` where the expression to the right of the `=>` is returned by the switch expression if the pattern matches. Each `case` is ended with a comma.

Each `switch` expression `case` can also incorporate the `when` clause:

```csharp
shape switch {
    Circle circle when circle.Radius is 0 => 0,
    // ...
}
```

If you combine the `switch` expression with C# 6 and 7 expression bodied members, you can really reduce the amount of boilerplate code you have to write:

```csharp
static double GetAreaExpression(this Shape shape) => shape switch
{
    Rectangle rectangle => rectangle.Height * rectangle.Width,
    Circle circle => Math.PI * circle.Radius * circle.Radius,
    null => throw new ArgumentNullException(nameof(shape)),
    var unknownShape => throw new NotImplementedException()
};
```

### Discard Pattern

The *discard pattern* in C# 8 is simple, and fills a similar role as the `default` keyword.

```csharp
shape switch
{
    Rectangle rectangle => rectangle.Height * rectangle.Width,
    Circle circle => Math.PI * circle.Radius * circle.Radius,
    _ => throw new ArgumentException()
};
```

Here, the `_` token matches anything. Unlike the declaration or var pattern, you cannot access the matched value via the `_` token. Use this pattern to replace a `default` case.

You can use the *discard pattern* in conjunction with the *declaration pattern* when you are only concerned with type matching, but not the captured value.

```csharp
shape switch
{
    Rectangle _ => "It's a rectangle!",
    Circle _ => "It's a circle!",
    _ => "I don't know what it is!"
};
```

This pattern is also useful in combination with other patterns seen later in this article.

### Positional Pattern

The *positional pattern* has a tuple-like syntax, and allows pattern matching on a tuple or multiple expressions grouped into a tuple.

The following example shows the ease with which you can write a complex state machine using the positional pattern.

```csharp
var nextState = (GetShipmentState(), action, GetDaysSinceDelivery()) switch
{
    (State.Ordered, Action.CancelOrder, _) => State.Canceled,
    (State.Canceled, Action.CancelOrderCancellation, _) => State.Ordered,
    (State.Delivered, Action.Return, int elapsedDays) when elapsedDays <= 30 => State.Returned,
    (null, Action.Order, _) => State.Ordered,
    (var state, _, _) => state
};
```

Note here that the input to the switch expression is an inline tuple, and each case deconstructs the tuple while performing additional pattern matching on each element.

### Property Pattern

Property patterns are very similar to positional patterns. They differ by pattern matching on named properties and fields of values rather than positional matching on tuple elements.

```csharp
static double GetAreaExpression(this Shape shape) => shape switch
{
    Rectangle { Width: 0 } => 0,
    Rectangle { Height: 0 } => 0,
    Circle { Radius: 0 } circle => circle.Radius,
    Rectangle rectangle => rectangle.Height * rectangle.Width,
    Circle circle => Math.PI * circle.Radius * circle.Radius,
    _ => throw new ArgumentException()
};
```

In this pattern, a type name is followed by `{ ... }` where additional pattern matching can be performed on properties of the matched type inside the `{ }`. The result of a matched type can also be captured by a variable designation following the `{ }`. Matching this pattern ensures the value is not `null`.

If you want to use the *positional pattern* directly on the `switch` input type, you can omit the type before the `{ }`.

```csharp
var person = new { Name = "Drake", Age = 22 };
var hasAccess = person switch
{
    { Name: "Drake" } drake when drake.Age > 18 => true,
    { Name: "Devin" } => true,
    _ => false
};
```

### Recursive Pattern Matching

All of the patterns we have seen so far can be combined in a recursive way! Given an arbitrarily complex object, we can combine property patterns, positional patterns, and the rest to select very specific criteria on values in a switch expression.

```csharp
var message = complexType switch
{
    { ShipmentStatus: Shipment.State.Ordered } => "Congrats on your order!",
    { Address: { State: "LA" } } => "I live there too!",
    { Address: { Zip: null } } => "You forgot to enter a zip code!",
    { ShipmentStatus: Shipment.State.Delivered, Name: (var firstName, _) } => $"Enjoy your package {firstName}!",
    null => throw new ArgumentNullException(),
    _ => "I'm not sure what I'm looking at here."
};
```

## Conclusion

Pattern matching with switch expressions gives C# developers a concise yet powerful way to express complex control flow. I find this is very helpful when writing functional C#, and for easily codifying complex business rules.

Feel free to reach out to me with any questions. I would also greatly appreciate your feedback on this article as I seek to improve it!

- Email: [drake-lambert@outlook.com](mailto:drake-lambert@outlook.com)
- GitHub: [https://github.com/DrakeLambert](https://github.com/DrakeLambert)
- LinkedIn: [https://www.linkedin.com/in/drakelambert](https://www.linkedin.com/in/drakelambert)

## Additional Resources

- [C# Pattern Matching Overview](https://docs.microsoft.com/en-us/dotnet/csharp/pattern-matching)
- [C# 7 Pattern Matching](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-7.0/pattern-matching)
- [C# 8 Recursive Pattern Matching](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/patterns)
- [C# Language Versioning](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)
