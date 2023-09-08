
grammar Milo;

program: line* EOF;

line: statement | ifBlock | whileBlock ;

statement: (assignment | reAssignment | functionCall ) ';' ;

ifBlock: 'if' expr block ('else' elseIfBlock)?;

elseIfBlock: block | ifBlock;

whileBlock: WHILE expr block;

WHILE: 'while' ;
DEF: 'def';
LOCAL: 'local' ;


DATATYPE: 'int' | 'float' | 'str' | 'any' | 'bool';

assignment: LOCAL? ((IDENTIFIER ':' DATATYPE '=' expr) | (DATATYPE IDENTIFIER '=' expr)) ;

reAssignment: IDENTIFIER '=' expr;

functionCall: IDENTIFIER '(' (expr (',' expr)*)? ')';

functionDef: LOCAL? DEF IDENTIFIER '(' (expr (',' expr)*)? ')' '->' DATATYPE block;


expr: constant				#valueExpr
	| IDENTIFIER			#identExpr
	| functionCall			#funcCallExpr
	| '(' expr ')'			#parenExpr
	| ('!'|NOT) expr		#notExpr
	| expr multOp expr		#multOpExpr
	| expr addOp expr		#addOpExpr
	| expr compOp expr		#compOpExpr
	| expr boolOp expr		#boolOpExpr
	;
	
multOp: '*' | '/' | '%';
addOp: '+' | '-';
compOp: '==' | '!=' | '>' | '<' | '>=' | '<=';
boolOp: BOOL_OPERATOR | ('&&' | '||');

BOOL_OPERATOR: 'and' | 'or';

constant: INTEGER | FLOAT | STRING | BOOL | NULL;

BOOL: 'true' | 'false';

NOT: 'not';

INTEGER: [0-9]+ ('_' [0-9]+)*;
FLOAT:  ([0-9]* | ([0-9] ('_' [0-9]+)*)) '.' ([0-9]+ ('_' [0-9]+)*);
STRING: ('"' ~'"'* '"') | ('\'' ~'\''* '\'');

NULL: 'null';


block: '{' line* '}';



WS: [ \t\r\n]+ -> skip;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;


/*

local x = 10;

public def main() -> int {

	return 0;
}


*/