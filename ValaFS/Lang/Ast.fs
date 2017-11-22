module Ast

type Annotation = 
    Comment of string

type DataType = 
    | Array
    | Boolean
    | Class
    | Delegate
    | Enum
    | Error
    | FieldPrototype
    | Floating
    | Generic
    | Integer
    | Interface
    | Invalid
    | Method
    | Null
    | Object
    | Pointer
    | Reference
    | Signal
    | StructValue
    | Token
    | Unresolved
    | Value
    | Void

type Literal = 
    | Integer of int
    | Float of double
    | Char of char
    | String of string
    | Boolean of bool

// type - name
type Identifier = DataType * string

type UnaryExpr =
    | Sign
    | LogicalNot
    | BitwiseNot
    | Prefix
    | OwnershipTransfer
    | Cast
    | Pointer

type LogicalAnd = 
    Logical

type NullableExpr = 
    | LogicalOr

type CoalescingExpr =
    | Nullable of NullableExpr

type Expr = 
    //// Primary
    | Literal of Literal
    | Template
    //[expr].identifier
    | MemberAccess of Expr * string
    | PointerMemberAccess
    //container [indexes]
    | ElementAccess of Expr * Expr
    | Postfix
    | ClassInstantiation
    | ArrayInstantiation
    | StructInstantiation
    | Invocation
    | Sizeof
    | Typeof
    // Unary
    | Unary of Expr
    | Binary of Expr * Expr
    // (cond) ? whenTrue : whenFalse
    | Conditional of CoalescingExpr * Expr * Expr
    | Assignment
    | Lambda
    | PlaceHolder of string

//[int|var] foo [= something];
//type VariableDecl = DataType * string * Expr option
type VariableDecl = string * string * Expr option

type Stmt = 
    | PlaceHolder of string
    | Empty
    | Simple
    | Break
    | Continue
    | Block of Stmt list
    | VariableDecl of VariableDecl
    //if(cond) { block } [else]
    | If of Expr * Stmt * Stmt option
    //switch(expr){ case label: stmts... }
    | Switch of Expr * (Expr * Stmt list) list
    | While of Expr * Stmt
    // block (cond)
    | DoWhile of Stmt * Expr
    //for(init_list; cond; iter_list){ block }
    | For of
        Stmt option list *
        Expr option *
        Expr option list *
        Stmt
    | Foreach
    | Return
    | Throw
    | Try
    | Lock
    | Embedded of Stmt

type Symbol = 
    | Annotation of Annotation
    | Stmt of Stmt
    | Expr of Expr
    | PlaceHolder of string