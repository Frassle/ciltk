module Silk.Loom.Parser

open FParsec

type Parser<'T> = Parser<'T, unit>

type Bound = uint32 option * uint32 option

type Type = 
    | Class of string
    | ManagedPointer of Type
    | UnmanagedPointer of Type
    | GenericType of Type * Type list
    | Array of Type * Bound list
    | Pinned of Type

 type private Modifer = 
    | ReferenceModifer
    | PointerModifer
    | GenericTypeModifer of Type list
    | ArrayModifer of Bound list
    | PinnedModifer

let pint32 : Parser<_> =
    (pstring "0x" >>. many1 hex 
        |>> (fun s -> System.UInt32.Parse(new System.String(List.toArray s), System.Globalization.NumberStyles.HexNumber)))
    <|>
    (many1 digit |>> (fun s -> System.UInt32.Parse(new System.String(List.toArray s))))

let pid : Parser<_> =
    let startsWith = (fun ch -> 
        ('A' <= ch && ch <= 'Z') ||
        ('a' <= ch && ch <= 'z') ||
        (ch = '_' || ch = '$' || ch = '@' || ch = '`' || ch = '?'))
    let endsWith = (fun ch -> 
        startsWith ch ||
        ('0' <= ch && ch <= '9'))

    many1Chars2 (satisfy startsWith) (satisfy endsWith)

let pdottedName : Parser<_> =
    sepBy1 pid (pchar '.') |>> String.concat "."

let ptypeRefernce : Parser<_> =
    sepBy1 pdottedName (pchar '/') |>> String.concat "/"

let pbound : Parser<_> =
    choiceL [
        (pipe3 pint32 (pstring "...") pint32 (fun l _ u -> (Some l, Some u)))
        (pipe2 pint32 (pstring "...") (fun l _ -> (Some l, None)))
        (pipe2 (pstring "...") pint32 (fun _ u -> (None, Some u)))
        (pstring "..." >>% (None, None))
        (preturn (None, None))
    ] "Bound"

let ptype : Parser<_> =
    let (Type : Parser<_>), TypeRef = createParserForwardedToRef()

    let primative = 
        choice [ 
            pstring "bool" >>% "System.Boolean"
            pstring "char" >>% "System.Char"
            pstring "float" >>% "System.Single"
            pstring "double" >>% "System.Double"
            pstring "sbyte" >>% "System.SByte"
            pstring "short" >>% "System.Int16"
            pstring "int" >>% "System.Int32"
            pstring "long" >>% "System.Int64"
            pstring "IntPtr" >>% "System.IntPtr"
            pstring "UIntPtr" >>% "System.UIntPtr"
            pstring "object" >>% "System.Object"
            pstring "string" >>% "System.String"
            pstring "TypedRef" >>% "System.Typedref"
            pstring "byte" >>% "System.Byte"
            pstring "ushort" >>% "System.UInt16"
            pstring "uint" >>% "System.UInt32"
            pstring "ulong" >>% "System.UInt64"
            pstring "void" >>% "System.Void"
        ] 

    let ty = 
        choiceL [
            primative 
            ptypeRefernce
        ] "Type" |>> Class

    let array = 
        between (pchar '[') (pchar ']') (sepBy pbound (pchar ',')) |>> ArrayModifer

    let genargs = 
        between (pchar '<') (pchar '>') (sepBy1 Type (pchar ',')) |>> GenericTypeModifer

    let modifer = 
        choiceL [
            pstring "&" >>% ReferenceModifer
            pstring "*" >>% PointerModifer
            pstring " pinned" >>% PinnedModifer
            array
            genargs
        ] "Type"

    TypeRef := pipe2 ty (many modifer) (fun ty modifers -> 
                Seq.fold (fun ty modifer -> 
                    match modifer with
                    | ReferenceModifer -> ManagedPointer ty
                    | PointerModifer -> UnmanagedPointer ty
                    | PinnedModifer -> Pinned ty
                    | ArrayModifer bounds -> Array (ty, bounds)
                    | GenericTypeModifer args -> GenericType (ty, args)
                ) ty modifers)
    Type

let pfield : Parser<_> =
    tuple2 
        (ptype .>> pstring "::")
        pid

let pproperty : Parser<_> = 
    let parameters = 
        between (pchar '(') (pchar ')') (sepBy ptype (pchar ','))

    tuple3
        (ptype .>> pstring "::")
        pid
        (opt parameters)

let pmethod : Parser<_> =
    let genargs = 
        between (pchar '<') (pchar '>') (sepBy1 ptype (pchar ','))
    let parameters = 
        between (pchar '(') (pchar ')') (sepBy ptype (pchar ','))

    tuple4
        (ptype .>> pstring "::")
        pid
        (opt genargs)
        parameters
     
let run parser str = 
     match FParsec.CharParsers.run parser str with
     | Success (result, _, _) -> result
     | Failure (message, _, _) -> failwith message