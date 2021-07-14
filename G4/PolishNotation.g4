grammar PolishNotation;
@parser::members {
    public string Name, Description;
}

fragment SINGLE_SPACE   : '\u0020'; // ' '
fragment TABULATION     : '\u0009'; // '\t'
fragment LINE_FEED      : '\u000A'; // '\n'
fragment CARRAIGE_RETURN: '\u000D'; // '\r'
fragment TILDE          : '~';
fragment HASH           : '#';
fragment UP             : '/\\';
fragment DOWN           : '\\/';
fragment DOT            : '.';
fragment FRACTION       : DOT [0-9]* '1'..'9';
fragment LETTER         : 'a'..'z';
fragment CAPITAL        : 'A'..'Z';


Add           : '+';
Subtract      : '-';
Multiply      : '*';
Divide        : '/';
Colon         : ':' -> type(Divide);
Reminder      : '%';
FloorDivide   : '//';
Power         : '^';
DoubleMultiply: '**' -> type(Power);
Max           : 'max';
Min           : 'min';
Integer       : (TILDE | HASH | UP | DOWN);
Truncation    : Integer* HASH;
Round         : Integer* TILDE;
Ceiling       : Integer* UP;
Floor         : Integer* DOWN;
IntPart       : '0'
              | [1-9]+ [0-9]*
              ;
PointFloat    : IntPart? FRACTION;
Word		  : ( CAPITAL | LETTER )+;
WhiteSpace    : ( SINGLE_SPACE | TABULATION )+ -> channel(HIDDEN);
NewLine       : ( CARRAIGE_RETURN | LINE_FEED )+ -> skip;
Semicolon	  : ';';
Desc		  : 'description:';


negate	  : Subtract (Subtract Subtract)*;
argument  : number           # Wieheiﬂter
          | binary           # Wieheiﬂter
          | unary            # Wieheiﬂter
          | Word	         # VarArg
          | '(' argument ')' # EnclosedArg
          ;
number    : IntPart | PointFloat;
binary    : op=( Add
               | Subtract
               | Multiply
               | Divide
               | FloorDivide
               | Reminder
               | Power
               | Max
               | Min ) argument argument;
unary     : ( negate
            | Round
            | Ceiling
            | Floor
            | Truncation) argument;
name      : 'name:' Word Semicolon { Name = $Word.text; };
desc      : Desc ~Semicolon+ Semicolon { var dsc = $text; Description = dsc.Substring(12, dsc.Length - 13).Trim(); };
expression: name desc? tree=argument;
