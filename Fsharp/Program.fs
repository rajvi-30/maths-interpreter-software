module Mexer
open System
open System.Collections.Generic
open System.Globalization


// BNF (as implemented)
// <program>      ::= <statement>
// <statement>    ::= <assignment> | <expression>
// <assignment>   ::= <identifier> "=" <expression> [";"]   // semicolon optional
// <expression>   ::= <term> { ("+" | "-") <term> }
// <term>         ::= <factor> { ("*" | "/" | "%" | IMPLICIT_MUL) <factor> }
// <factor>       ::= <primary> { "^" <primary> }

// <primary>      ::= "-" <primary>                         // unary minus
//                  | <number>
//                  | <identifier>
//                  | "(" <expression> ")"
//                  | <identifier> "=" <expression> [";"]   // assignment is also allowed as an atom
// <identifier>   ::= <letter> { <letter> | <digit> }
// <implicit-mul> ::= IMPLICIT_MUL                          // inserted by lexer between number/")" and "("
    
// =============================
// TYPES
// =============================
type Expr =
    | NumConst of float
    | Var of string
    | Add of Expr * Expr
    | Sub of Expr * Expr
    | Mul of Expr * Expr
    | Div of Expr * Expr
    | Pow of Expr * Expr

type Stmt =
    | Assign of string * Expr

type Token =
    | Add | Sub | Mul | Div | Mod | Pow | Lpar | Rpar 
    | Num of float
    | Assign              
    | Semicolon 
    | Ident of string
    | ImplicitMul 

// Symbol table for variables
let variables = Dictionary<string, float>()

// =============================
// LEXER
// =============================
let lexer (input: string) = 
    let str2lst (s:string) = [for c in s -> c]
    let isblank c = System.Char.IsWhiteSpace c
    let isdigit c = System.Char.IsDigit c

    let rec scNum (iStr: char list, accStr: string) : char list * float = 
        match iStr with
        | '.' :: tail when not (accStr.Contains(".")) ->
            scNum(tail, accStr + ".")
        | c :: tail when isdigit c ->
            scNum(tail, accStr + string c)
        | _ -> (iStr, float accStr)
    
    let rec scan input (prevToken: Token option) =
        match input with
        | [] -> []
        | '+'::tail -> Add :: scan tail (Some Add)
        | '-'::tail -> Sub :: scan tail (Some Sub)
        | '*'::tail -> Mul :: scan tail (Some Mul)
        | '/'::tail -> Div :: scan tail (Some Div)
        | '%'::tail -> Mod :: scan tail (Some Mod)
        | '^'::tail -> Pow :: scan tail (Some Pow)
        | '='::tail -> Assign :: scan tail (Some Assign)
        | ';'::tail -> Semicolon :: scan tail (Some Semicolon)
        | '('::tail ->
            match prevToken with
            | Some (Num _) -> ImplicitMul :: Lpar :: scan tail (Some Lpar)
            | Some Rpar -> ImplicitMul :: Lpar :: scan tail (Some Lpar)
            | _ -> Lpar :: scan tail (Some Lpar)
        | ')'::tail -> Rpar :: scan tail (Some Rpar)
        | c :: tail when isblank c -> scan tail prevToken
        | c :: tail when isdigit c ->
            let (iStr, numVal) = scNum(tail, string c)
            Num numVal :: scan iStr (Some (Num numVal))
        | c :: tail when Char.IsLetter c ->
            let rec getIdent acc rest =
                match rest with
                | ch :: t when Char.IsLetterOrDigit ch -> getIdent (acc + string ch) t
                | _ -> (acc, rest)
            let (name, remaining) = getIdent (string c) tail
            Ident name :: scan remaining (Some (Ident name))
        | _ -> failwith "Lexer error: Unexpected character"

    scan (str2lst input) None

// =============================
// PARSER + EVALUATOR
// =============================
let parseAndEval (input: string) =
    let tokens = lexer input

    let rec E t = T t |> Eopt
    and Eopt(tList, v) =
        match tList with
        | Add :: tail ->
            let (rest, tVal) = T tail
            Eopt(rest, v + tVal)
        | Sub :: tail ->
            let (rest, tVal) = T tail
            Eopt(rest, v - tVal)
        | _ -> (tList, v)

    and T t = F t |> Topt
    and Topt(tList, v) =
        match tList with
        | Mul :: tail ->
            let (rest, fVal) = F tail
            Topt(rest, v * fVal)
        | Div :: tail ->
            let (rest, fVal) = F tail
            if fVal = 0.0 then failwith "Division by zero"
            Topt(rest, v / fVal)
        | Mod :: tail ->
            let (rest, fVal) = F tail
            if fVal = 0.0 then failwith "Modulo by zero"
            Topt(rest, v % fVal)
        | ImplicitMul :: tail ->
            let (rest, fVal) = F tail
            Topt(rest, v * fVal)
        | _ -> (tList, v)

    and F t = NR t |> Fopt
    and Fopt(tList, v) =
        match tList with
        | Pow :: tail ->
            let (rest, nVal) = NR tail
            Fopt(rest, v ** nVal)
        | _ -> (tList, v)

    and NR tList =
        match tList with
        // Unary minus for numbers, variables, or expressions
        | Sub :: Num v :: tail -> (tail, -v)
        | Sub :: Ident name :: tail ->
            if variables.ContainsKey(name) then
                (tail, -variables.[name])
            else failwithf "Undefined variable: %s" name
        | Sub :: Lpar :: tail ->
            let (rest, v) = NR (Lpar::tail)
            (rest, -v)

        // Numbers
        | Num v :: tail -> (tail, v)

        // Variable assignment
        | Ident name :: Assign :: tail ->
            let (rest, value) = E tail
            variables.[name] <- value
            match rest with
            | Semicolon :: t -> (t, value)
            | _ -> (rest, value)

        // Variable usage
        | Ident name :: tail ->
            if variables.ContainsKey(name) then
                (tail, variables.[name])
            else failwithf "Undefined variable: %s" name

        // Parentheses
        | Lpar :: tail ->
            let (rest, v) = E tail
            match rest with
            | Rpar :: t -> (t, v)
            | _ -> failwith "Missing closing parenthesis"

        | _ -> failwith "Parser error: Expected number, variable, or parentheses"

    // Start parsing
    let (remain, res) = E tokens
    if remain <> [] then
        failwith "Parser error: Incomplete expression"
    res


let evaluateexpression (input: string) =
    try
        let r = parseAndEval input
        if r % 1.0 = 0.0 then $"{int r}" else $"{r}"
    with ex -> $"Error: {ex.Message}"

/// Evaluate the expression for many x values and return the y array (NaN on failure per point)
let evaluateManyD (expr: string, xs: double[]) : double[] =
    let results = Array.zeroCreate<double> xs.Length
    for i = 0 to xs.Length - 1 do
        try 
            // Set variable x and evaluate
            variables.["x"] <- xs[i]
            let v = parseAndEval expr
            results.[i] <- v
        with _ ->
            results.[i] <- Double.NaN
    results

// =============================
// GUI 1 — Console interface
// =============================
let  rec  gui1 () =
    printfn "==================================="
    printfn "        GUI 1 (Interpreter)"
    printfn "==================================="
    printfn "Examples:"
    printfn "   3+5*(2-1)"
    printfn "   x = 10;"
    printfn "   (x+3)^2"
    printfn "Commands: plot, exit"
    printfn "-----------------------------------"

    let rec loop () =
        printf "\nInput: "
        let input = Console.ReadLine()
        match input with
        | "exit" -> printfn "Goodbye!"
        | "plot" -> gui2(); loop()
        | expr ->
            let output = evaluateexpression expr
            printfn "Result: %s" output
            loop()
    loop()

// =============================
// GUI 2 — ASCII Plotter
// =============================
and gui2 () =
    printfn "\n=========== GUI 2: PLOTTER ==========="
    printf "Enter f(x): "
    let expr = Console.ReadLine()

    printf "X-start: "
    let a = float (Console.ReadLine())
    printf "X-end: "
    let b = float (Console.ReadLine())
    printf "Step (0.1 recommended): "
    let step = float (Console.ReadLine())

    let mutable points = []

    let rec generate x =
        if x <= b then
            try
                let y = parseAndEval (expr.Replace("x", x.ToString()))
                points <- (x, y) :: points
            with _ -> ()
            generate (x + step)
    generate a

    let ymax = points |> List.map snd |> List.max |> ceil
    let ymin = points |> List.map snd |> List.min |> floor

    for y in ymax .. -1.0 .. ymin do
        for x in a .. step .. b do
            let fx =
                match points |> List.tryFind(fun(px,py) -> abs(py - y) < 0.3) with
                | Some _ -> "*"
                | None -> " "
            printf "%s" fx
        printfn ""

    printfn "--------------------------------------"
    printfn "Plot complete.\n"


// =============================
// MAIN ENTRY POINT
// =============================
[<EntryPoint>]
let main _ =
    gui1()
    0
