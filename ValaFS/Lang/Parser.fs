module Parser

open FParsec
open FParsec.Primitives
open FParsec.CharParsers
open System
open Ast

type UserState = unit // doesn't have to be unit, of course
type Parser<'t> = Parser<'t, UserState>

// Collects a parsed item
let pcollect (p:Parser<'r, 'u>) : Parser<'r,'u> =
    fun stream ->
        let reply = p stream
        Reply(reply.Status, reply.Result, reply.Error)

let insertItem (p:Parser<'r, 'u>) =
    fun (stream:CharStream<'r>) ->
        let state = stream.State
        printf "Name: %s" stream.Name
        printf "Line %d\n" state.Line
        printf "LineBegin %d\n" state.LineBegin
        p stream

let nl_eof =
    skipNewline <|> eof

let lineComment =
    pstring "//" >>.
    manyCharsTill anyChar nl_eof |>>
    fun(comment) ->
        Annotation.Comment(comment)

let blockComment = 
    skipString "/*" >>.
    manyCharsTill anyChar (skipString "*/") |>>
    fun(comment) ->
        Annotation.Comment(comment)

let emptyStatement = skipChar ';' >>. preturn Stmt.Empty

//let simpleStatement = manyCharsTill anyChar (skipChar ';') |>> fun(stmt) ->
//    Stmt.PlaceHolder(stmt)

//let StatementBlock = pchar '{' >>. manyTill StatementList (pchar '}')


let annotation = choice[lineComment; blockComment] |>> fun(annotation) ->
    Symbol.Annotation(annotation)

let statement, statementRef = createParserForwardedToRef<Stmt, unit>()
//let expression, expressionRef = createParserForwardedToRef<Expr, unit>()

let intLiteral = pint32 |>> fun(x) -> Literal.Integer(x)
let floatLiteral = pfloat |>> fun(x) -> Literal.Float(x)
let charLiteral =
    between (skipChar '\'') (skipChar '\'') anyChar |>>
    fun(x) -> Literal.Char(x)

let stringLiteral =
    skipChar '"' >>.
    manyCharsTill anyChar (skipChar '"') |>>
    fun(x) -> Literal.String(x)

let boolLiteral = 
    ((stringReturn "true" true) <|> (stringReturn "false" false)) |>>
    fun(x) -> Literal.Boolean(x)


let literal : Parser<Literal, unit> = 
    choice [
        intLiteral
        floatLiteral
        boolLiteral
        charLiteral
        stringLiteral
    ]

let expression = 
    let lit = literal |>> fun(lit) -> Expr.Literal(lit)
    lit

let ws = many(skipNewline <|> spaces1)

let trimSpaces p = ws >>. p .>> ws

let betweenParens p =
    between (skipChar '(' |> trimSpaces) (skipChar ')' |> trimSpaces) p

let betweenCurly p =
    between (skipChar '{' |> trimSpaces) (skipChar '}' |> trimSpaces) p

let parensExpression = expression |> betweenParens

let curlyBlock =
    many statement |> betweenCurly |>> fun(lst) -> Stmt.Block(lst)

let breakStatement = skipString "break" >>. ws >>. emptyStatement >>. preturn Stmt.Break
let continueStatement = skipString "continue" >>. ws >>. emptyStatement >>. preturn Stmt.Continue

//handles case   label: body
let catchClause = 
    let label = skipString "case" >>. ws >>. expression .>> skipChar ':'
    label .>>. (many statement)

let switchStatement =
    let switchExpr = skipString "switch" >>. ws >>. parensExpression
    let inner = between (skipChar '{' |> trimSpaces) (skipChar '}' |> trimSpaces) (many catchClause)

    switchExpr .>>. inner |>>
    fun(switchExp, body) ->
        Stmt.Switch(switchExp, body)


let whileStatement = 
    let condition = skipString "while" >>. ws >>. parensExpression
    let body = (attempt curlyBlock) <|> statement

    condition .>>. body |>> fun(cond, body) ->
        Stmt.While(cond, body)

let doWhileStatement =
    let condition = skipString "while" >>. ws >>. parensExpression
    skipString "do" >>. ws >>. curlyBlock .>>. condition |>>
    fun(body, cond) ->
        Stmt.DoWhile(body, cond)

//if(cond) { .. body }
let preIfStatement =
    let condition = skipString "if" >>. ws >>. parensExpression
    // { .. } or single statement
    let body = (attempt curlyBlock) <|> statement
    
    condition .>>. body |>> fun(cond, body) -> 
        Stmt.If(cond, body, None)
   
//..else..[if..]
let postIfStatement p = 
    let optElseIf = (attempt preIfStatement) <|> (attempt curlyBlock) <|> statement
    let elseBlock = opt (skipString "else" >>. ws >>. optElseIf)
    p .>>. elseBlock

// Builds an if statement from if-else blocks
let rec makeIf preIf postIf = 
    match preIf with
    | Stmt.If(cond, body, _) ->
        match postIf with
        // else
        | Some(stmt) ->
            match stmt with
            // else-if
            | Stmt.If(_, _, _) -> Stmt.If(cond, body, Some(makeIf stmt None))
            // else with body or single statement
            | _ -> Stmt.If(cond, body, postIf)
        // if without else
        | _ -> Stmt.If(cond, body, None)

let ifStatement =
    preIfStatement |> postIfStatement |>>
    fun(preIf, postIf) ->
        makeIf preIf postIf



let validIdentifierChar = letter <|> digit <|> (pchar '_')
let identifier = manyCharsTill anyChar (notFollowedBy validIdentifierChar)

let variableDeclStatement =
    //= something
    let varDef = skipChar '=' >>. ws >>. expression .>> ws

    //valid identifier for variable name
    let identSpaces = identifier .>> ws
    
    //x [=foo];
    let restVar = identSpaces .>>. (opt varDef) .>> emptyStatement

    //var ...
    let implicitType = pstring "var" .>> ws

    //int|string ...
    let explicitType = identSpaces .>> followedBy restVar

    //var|fooType
    let varLeft = ((attempt implicitType) <|> (attempt explicitType)) |> notEmpty
    
    let makeVar varType varName varDecl _ =
        printf "Var '%s' '%s' '%s' end\n" varType varName (string varDecl)
        Stmt.PlaceHolder(varType + varName)

    pipe4 varLeft identSpaces (opt varDef) emptyStatement makeVar

let forStatement =
    let rec makeInitializers init =
        match init with
        | p::xs -> 
            let left = match p with
            | Some(stmt) -> [stmt]
            | None -> []
            left @ makeInitializers xs
        | _ -> []
    
    let rec makeIters iters =
        match iters with
        | p::xs ->
            let left = match p with
            | Some(expr) -> [expr]
            | None -> []
            left @ makeIters xs
        | _ -> []

    let initList = (sepBy (opt variableDeclStatement) (skipChar ',')) .>> followedBy (skipChar ';')
    let initCond = (opt expression) .>> followedBy (skipChar ';')
    let iterList = (sepBy (opt expression) (skipChar ','))

    let initParams =
        between (skipChar '(') (skipChar ')') (initList .>>. initCond .>>. iterList) |>>
        fun((initList, initCond), iterList) -> (initList, initCond, iterList)

    (skipString "for" >>. ws >>. initParams) .>>. curlyBlock |>>
    fun ((initList, initCond, iterList), body) ->
        Stmt.For(initList, initCond, iterList, body)

//https://wiki.gnome.org/Projects/Vala/Manual/Statements
statementRef :=
    choice [
        emptyStatement
        ifStatement
        whileStatement
        doWhileStatement
        switchStatement
        breakStatement
        continueStatement
        variableDeclStatement
        //forStatement
    ] |> trimSpaces

let symbol =
    let stmt = statement |>> fun(stmt) -> Symbol.Stmt(stmt)

    choice[
        annotation
        stmt
    ] |> trimSpaces

let symbolList = many symbol

let toplevel = 
    many symbol

let parse = ws >>. toplevel .>> eof

let runParser file =
    let res = runParserOnFile parse () file System.Text.Encoding.Default
    match res with
    | Success(r,_,_) -> printfn "%A" r
    | Failure(str, exn, _) -> printfn "Failure: %s" str

[<EntryPoint>]
let main args =
    if Array.length args = 0 then
        printfn "Usage"
        printfn "  %s [vala_file]" (System.IO.Path.GetFileName (System.Environment.GetCommandLineArgs().[0]))
    else
        Array.iter runParser args
    0