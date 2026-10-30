grammar Calculator;

// Головне правило - вираз, який закінчується кінцем файлу
compileUnit: expr EOF;

// Правила для виразів (пріоритет операцій іде зверху вниз)
expr: NUMBER                                               # NumberExpr
    | ID                                                   # IdentifierExpr
    | '(' expr ')'                                         # ParenthesesExpr
    | operator=('+' | '-') expr                            # UnarExpr
    | 'inc' '(' expr ')'                                   # IncExpr
    | 'dec' '(' expr ')'                                   # DecExpr
    | left=expr operator=('*' | '/' | 'mod' | 'div') right=expr # MultiplicativeExpr
    | left=expr operator=('+' | '-') right=expr            # AdditiveExpr
    ;

// Лексичні правила
NUMBER: [0-9]+ ;                     // Цілі числа (для BigInteger)
ID:     [A-Za-z]+[0-9]+ ;            // Назви клітинок (наприклад, A1, B25)
WS:     [ \t\r\n]+ -> skip ;         // Ігнорування пробілів
