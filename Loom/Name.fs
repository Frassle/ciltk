namespace Silk.Loom

module ReflectionNames =

    let rec TypeName (ty : System.Type) =
        match  ty.DeclaringType with
        | null -> ty.FullName
        | _ -> TypeName (ty.DeclaringType) + "/" + ty.Name

    let rec MethodName (meth : System.Reflection.MethodInfo) =    
        sprintf "%s::%s(%s)" 
            (TypeName meth.DeclaringType)
            meth.Name
            (String.concat "," (meth.GetParameters() |> Seq.map (fun p -> TypeName p.ParameterType)))

module CecilNames = 
    
    let rec TypeName (ty : Mono.Cecil.TypeReference) =
        match  ty.DeclaringType with
        | null -> ty.FullName
        | _ -> TypeName (ty.DeclaringType) + "/" + ty.Name

    let rec MethodName (meth : Mono.Cecil.MethodReference) =    
        sprintf "%s::%s(%s)" 
            (TypeName meth.DeclaringType)
            meth.Name
            (String.concat "," (meth.Parameters |> Seq.map (fun p -> TypeName p.ParameterType)))