module Silk.Loom.Reference

type Scope = 
    | MethodScope of Mono.Cecil.MethodDefinition
    | TypeScope of Mono.Cecil.TypeDefinition
    | ModuleScope of Mono.Cecil.ModuleDefinition

let private GetModule scope =
    match scope with
    | MethodScope m -> m.Module
    | TypeScope t -> t.Module
    | ModuleScope m -> m

let private FindType scope (name : string) =
    let genericParameters = 
        match scope with
        | MethodScope m -> Seq.concat [m.GenericParameters; m.DeclaringType.GenericParameters] |> Seq.toList
        | TypeScope t -> t.GenericParameters |> Seq.toList
        | ModuleScope m -> []

    let generic = genericParameters |> Seq.tryFind (fun p -> p.Name = name)
    match generic with
    | Some generic -> generic :> Mono.Cecil.TypeReference
    | None ->
        let moddef = 
            match scope with 
            | MethodScope m -> m.Module
            | TypeScope t -> t.Module
            | ModuleScope m -> m

        let extern_modules = moddef.AssemblyReferences |> Seq.map (moddef.AssemblyResolver.Resolve >> (fun a -> a.Modules)) |> Seq.concat
        let intern_modules = moddef.Assembly.Modules :> seq<Mono.Cecil.ModuleDefinition>
        let modules = Seq.concat [intern_modules; extern_modules]
        let types = modules |> Seq.map (fun m -> m.Types) |> Seq.concat

        let rec matchType (name : string) (ty : Mono.Cecil.TypeDefinition) =
            if name = ty.FullName then
                Some ty
            else if name.StartsWith(ty.FullName) then
                ty.NestedTypes |> Seq.tryPick (matchType name)
            else
                None

        let ty = 
            match types |> Seq.tryPick (matchType name) with
            | Some ty -> ty :> Mono.Cecil.TypeReference
            | None -> failwithf "Type '%s' not found" name

        let currentModule = GetModule scope
        if ty.Module = currentModule then
            ty
        else
            currentModule.Import(ty)
        

let rec private ResolveType scope ty =
    match ty with
    | Parser.Class name -> FindType scope name
    | Parser.ManagedPointer ty -> Mono.Cecil.ByReferenceType(ResolveType scope ty) :> Mono.Cecil.TypeReference
    | Parser.UnmanagedPointer ty -> Mono.Cecil.PointerType(ResolveType scope ty) :> Mono.Cecil.TypeReference
    | Parser.Pinned ty -> Mono.Cecil.PinnedType(ResolveType scope ty) :> Mono.Cecil.TypeReference
    | Parser.Array (ty, bounds) -> Mono.Cecil.ArrayType(ResolveType scope ty, Seq.length bounds) :> Mono.Cecil.TypeReference
    | Parser.GenericType (ty, args) ->
        let ty = Mono.Cecil.GenericInstanceType(ResolveType scope ty)
        args |> Seq.iter (fun arg -> ty.GenericArguments.Add(ResolveType scope arg))
        ty :> Mono.Cecil.TypeReference

let ParseTypeReference scope reference : Mono.Cecil.TypeReference =
    ResolveType scope (Parser.run Parser.ptype reference)

let ParseFieldReference scope reference : Mono.Cecil.FieldReference = 
    let (ty, name) = Parser.run Parser.pfield reference

    let ty = ResolveType scope ty
    let def = ty.Resolve()

    let fields = def.Fields |> Seq.filter (fun f -> f.Name = name) |> Seq.toList

    let field = 
        match fields with  
        | [f] -> f
        | _ -> failwithf "Could not find unique field %s" reference
        
    let currentModule = GetModule scope
    let field =
        if field.Module = currentModule then
            field :> Mono.Cecil.FieldReference
        else
            currentModule.Import(field)
    field.DeclaringType <- ty
    field

let ParseMethodReference scope reference : Mono.Cecil.MethodReference = 
    let (ty, name, genargs, args) = Parser.run Parser.pmethod reference

    let ty = ResolveType scope ty
    let def = ty.Resolve()

    let genargs = genargs |> Option.bind(Seq.map (ResolveType scope) >> Some)
    let methods = def.Methods |> Seq.filter (fun m -> m.Name = name)

    let meth = 
        methods
        |> Seq.filter(fun m ->
            let args = args |> Seq.map (ResolveType (MethodScope m))
            let parameters = m.Parameters |> Seq.map(fun p -> p.ParameterType)
            (Seq.compareWith(fun (arg : Mono.Cecil.TypeReference) (param : Mono.Cecil.TypeReference) -> if arg.FullName = param.FullName then 0 else 1) args parameters) = 0)
        |> Seq.toList

    let meth = 
        match meth with  
        | [m] -> m
        | _ -> failwithf "Could not find unique method %s" reference
    
    let currentModule = GetModule scope
    let meth =
        if meth.Module = currentModule then
            meth :> Mono.Cecil.MethodReference
        else
            currentModule.Import(meth)

    meth.DeclaringType <- ty        

    match genargs with 
    | None -> meth
    | Some genargs ->
        let meth = Mono.Cecil.GenericInstanceMethod(meth)
        genargs |> Seq.iter(fun arg -> meth.GenericArguments.Add(arg))
        meth :> Mono.Cecil.MethodReference


let ParsePropertyReference scope reference = 
    let (ty, name, args) = Parser.run Parser.pproperty reference

    let ty = ResolveType scope ty
    let def = ty.Resolve()

    let properties = def.Properties |> Seq.filter (fun p -> p.Name = name)

    let property = 
        properties
        |> Seq.filter(fun p ->
            let args = 
                match args with
                | Some args -> args |> Seq.map (ResolveType (TypeScope p.DeclaringType))
                | None -> Seq.empty
            let parameters = p.Parameters |> Seq.map(fun p -> p.ParameterType)
            (Seq.compareWith(fun (arg : Mono.Cecil.TypeReference) (param : Mono.Cecil.TypeReference) -> if arg.FullName = param.FullName then 0 else 1) args parameters) = 0)
        |> Seq.toList

    let property = 
        match property with  
        | [p] -> p :> Mono.Cecil.PropertyReference
        | _ -> failwithf "Could not find unique property %s" reference
    
    property.DeclaringType <- ty
    property