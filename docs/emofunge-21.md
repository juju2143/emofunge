# Emofunge-21

Emofunge-21 (or Emofunge for short) is a programming language inspired by Befunge, but with native Unicode support. All of the instructions have been replaced with emoji and other special characters, leaving you the entirety of the ASCII plane to do whatever you want in the comments. It also adds many commands not found in the original Befunge, as well as command modifiers.

## Commands

A cell consists of a leading character (henceforth a command) followed by any characters Unicode considers a combining character (henceforth modifiers), which modifies the behavior of the command. Most of the commands found in Befunge-93 are mostly unchanged, so it should be compatible (but with different characters, and no guarantees).

| Opcode | Unicode | Befunge-93 | Comment |
|---|---|---|---|
| |U+0020|` `|No-op. Purposely defined.
|⠀|U+2800|`0`|Pushes 0 on the stack (note: might look like a space in some fonts)
|⁝|⁝|⁝|⁝
|⠉|U+2809|`9`|Pushes 9 on the stack
|⠊|U+280A||Pushes 10 on the stack
|⁝|⁝|⁝|⁝
|⣿|U+28FF||Pushes 255 on the stack
|➕|U+2795|`+`|Addition: Pop a and b, then push a+b
|➖|U+2796|`-`|Subtraction: Pop a and b, then push b-a
|✖️|U+2716|`*`||Multiplication: Pop a and b, then push a*b
|➗|U+2797|`/`|Integer division: Pop a and b, then push b/a, rounded towards 0.
|💠|U+1F4A0|`%`|Modulo: Pop a and b, then push the remainder of the integer division of b/a.
|🚫|U+1F6AB|`!`|Logical NOT: Pop a value. If the value is zero, push 1; otherwise, push zero.
|🔍|U+1F50D|`````|Greater than: Pop a and b, then push 1 if b>a, otherwise zero.
|➡️|U+27A1|`>`|Start moving east
|⬅️|U+2B05|`<`|Start moving west
|⬆️|U+2B06|`^`|Start moving north
|⬇️|U+2B07|`v`|Start moving south
|↖️|U+2196||Start moving northwest
|↗️|U+2197||Start moving northeast
|↘️|U+2198||Start moving southeast
|↙️|U+2199||Start moving southwest
|↺|U+21BA|`[`|Turn anticlockwise 90 degrees
|↻|U+21BB|`]`|Turn clockwise 90 degrees
|🧭|U+1F9ED|`?`|Start moving in a random cardinal direction
|↔️|U+2194|`_`|Pop a value; move right if value=0, left otherwise
|↕️|U+2195|``|``|Pop a value; move down if value=0, up otherwise
|⤡|U+2921||Pop a value; move southeast if value=0, northwest otherwise
|⤢|U+2922||Pop a value; move southwest if value=0, northeast otherwise
|🧵|U+1F9F5|`"`|Start string mode: push each character's Unicode value all the way up to the next 🧵
|🪞|U+1FA9E|`:`|Duplicate value on top of the stack
|🔀|U+1F500|`\`|Swap two values on top of the stack
|🗑️|U+1F5D1|`$`|Pop value from the stack and discard it
|📟|U+1F4DF|`.`|Pop value and output as an integer followed by a space
|🖨️|U+1F5A8|`,`|Pop value and output as ASCII character
|🌉|U+1F309|`#`|Bridge: Skip next cell
|🧮|U+1F9EE|`&`|Ask user for a number and push it
|⌨️|U+2328|`~`|Ask user for a character and push its Unicode value
|↩️|U+21A9||return statement (loads previously saved program counter and direction register from the macro stack)
|🛑|U+1F6D1|`@`|End program
|⌚|U+231A||push current UNIX timestamp

### To do

| Opcode | Unicode | Befunge-93 | Comment |
|---|---|---|---|
||U+||Power: Pop a and b, push a**b
|🚮|U+1F6A9|`p`|A "put" call (a way to store a value for later use). Pop y, x, and v, then change the character at (x,y) in the program to the character with ASCII value v
|👀|U+1F440|`g`|A "get" call (a way to retrieve data in storage). Pop y and x, then push Unicode values of the character at that position in the program
|any undefined character|||No-op. If a macro definition character exists somewhere in the program, saves program counter and direction register on the macro stack and resets said registers, pushes any combining characters on the stack, then teleport to its macro definition. (Note: Emoji with multiple code points joined by ZWJs might not work correctly, only the first codepoint will be recognized as a command and the rest are recognized as modifiers.)
|📺|U+1F4FA||Pop w and h, init graphics
|🟥|U+||pop value and set red component of current pixel
|🟩|U+||pop value and set green component of current pixel
|🟦|U+||pop value and set blue component of current pixel
|⬜|U+||pop b, g, r and set current pixel
|🔴|U+||push red component of current pixel on stack
|🟢|U+||push green component of current pixel on stack
|🔵|U+||push blue component of current pixel on stack
|⚪|U+||push r, g, b of current pixel on stack

## Modifiers
### With any command
| Opcode | Unicode | Comment |
|---|---|---|
||U+FE0F|Reserved (some emojis require it so it's displayed with emoji presentation, best to ignore it)
### With any undefined command
| Opcode | Unicode | Comment |
|---|---|---|
|⃞|U+20DE|Macro definition, also turns in a no-op
### With U+2800-U+28FF (to do)
| Opcode | Unicode | Comment |
|---|---|---|
|||Pop a, then push a+num
|||Pop a, push a-num
|||Pop a, push num-a
|||Pop a, push a*num
|||Pop a, push a/num
|||Pop a, push num/a
|||Pop a, then push the remainder of the integer division of a/num.
|||Push num*256 on stack (can be stacked)