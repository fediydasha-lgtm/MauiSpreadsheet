grammar Calculator;

compileUnit: expr EOF;

expr: NUMBER                                               # NumberExpr
    | ID                                                   # IdentifierExpr
    | '(' expr ')'                                         # ParenthesesExpr
    | operator=('+' | '-') expr                            # UnarExpr
    | 'inc' '(' expr ')'                                   # IncExpr
    | 'dec' '(' expr ')'                                   # DecExpr
    | left=expr operator=('*' | '/' | 'mod' | 'div') right=expr # MultiplicativeExpr
    | left=expr operator=('+' | '-') right=expr            # AdditiveExpr
    ;

NUMBER: [0-9]+ ;                     // Цілі числа 
ID:     [A-Za-z]+[0-9]+ ;            // Назви клітинок 
WS:     [ \t\r\n]+ -> skip ;         // Ігнорування пробілів
