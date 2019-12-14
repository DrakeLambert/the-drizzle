## C# 8 Switch Expressions with Pattern Matching

Pattern matching has been a long standing concept in many functional languages like Haskell, F#, Common Lisp (with libraries).

Haskell

```haskell

```

```fsharp

```

```lisp
(defun myfunc (x)
  (match x
    ((foo (bar b) (baz :a))
     ...body1...)
    ((list* :a b _)
     ...body2...)))
```

 was introduced in C# 7.

### Additional Resources

- <https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-7.0/pattern-matching>
