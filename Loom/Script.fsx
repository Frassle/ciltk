// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r @"../packages/FParsec.1.0.1/lib/net40-client/FParsecCS.dll"
#r @"../packages/FParsec.1.0.1/lib/net40-client/FParsec.dll"

#load "Parser.fs"

open Loom.Parser

run Loom.Parser.Int32 "0x123"

run Loom.Parser.DottedName "System.Int32"

run Loom.Parser.ResolutionScope "[.module Test.dll]"
run Loom.Parser.ResolutionScope "[mscorlib]"

run Loom.Parser.TypeReference "[mscorlib]System.Type"
run Loom.Parser.TypeReference "Loom.Parser/Type"

run Loom.Parser.Type "int32"
run Loom.Parser.Type "int32& pinned"
run Loom.Parser.Type "class [mscorlib]System.Type& pinned"
run Loom.Parser.Type "class [mscorlib]List<int32>"

run Loom.Parser.TypeSpec "int32"
run Loom.Parser.TypeSpec "int32& pinned"
run Loom.Parser.TypeSpec "class [mscorlib]System.Type& pinned"
run Loom.Parser.TypeSpec "class [mscorlib]List<int32>"

run Loom.Parser.Field "int32 class [mscorlib]List<int32>::id"