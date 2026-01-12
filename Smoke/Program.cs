using System;
using System.Collections.Generic;

class Program
{
    private static void Run(string expr, string? expect = null, string? expectContains = null)
    {
        string res = Mexer.evaluateexpression(expr);
        bool ok = false;
        if (expect != null)
            ok = string.Equals(res, expect, StringComparison.Ordinal);
        else if (expectContains != null)
            ok = res.Contains(expectContains, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"{expr} => {res} | PASS={ok}");
    }

    static void Main(string[] args)
    {
        var tests = new (string expr, string? expect, string? expectContains)[]
        {
            ("2+3*4", "14", null),
            ("(2+3)*4", "20", null),
            ("x = 5; x*2", "10", null),
            // Define function in one call, then call it in the next
            ("f(a)=a^2;", "0", null),
            ("f(3)", "9", null),
            ("2(3+4)", "14", null),
            ("10/0", null, "Division by zero"),
            ("5%0", null, "Modulo by zero"),
            ("1e3+2", "1002", null),
        };

        foreach (var t in tests)
        {
            Run(t.expr, t.expect, t.expectContains);
        }
    }
}
