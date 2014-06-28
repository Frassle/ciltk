using Mono.Cecil;
using Mono.Cecil.Cil;
using Silk.Loom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave
{
    class InfoReplacer : InstructionVisitor
    {
        protected override bool ShouldVisit(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var calledMethod = instruction.Operand as MethodReference;

                if (calledMethod != null && calledMethod.DeclaringType.FullName == "Silk.Info")
                {
                    return true;
                }
            }

            return false;
        }

        protected override Instruction Visit(ILProcessor ilProcessor, Instruction instruction)
        {
            var calledMethod = instruction.Operand as MethodReference;
            var next = instruction.Next;

            if (calledMethod.Name == "Parameter")
            {
                ReplaceParameter(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Variable")
            {
                ReplaceVariable(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Field")
            {
                ReplaceField(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Property")
            {
                ReplaceProperty(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Method")
            {
                ReplaceMethod(ilProcessor, instruction, calledMethod);
            }

            return next;
        }

        private void ReplaceMethod(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var module = ilProcessor.Body.Method.Module;

            var name = instruction.Previous.Operand as string;

            var methodReference = Silk.Loom.References.FindMethod(module, ilProcessor.Body.Method, name);

            var getMethodFromHandle = Silk.Loom.References.FindMethod(module, null,
                "System.Reflection.MethodBase System.Reflection.MethodBase::GetMethodFromHandle(System.RuntimeMethodHandle)");

            var ldtoken = Instruction.Create(OpCodes.Ldtoken, methodReference);
            var call = Instruction.Create(OpCodes.Call, getMethodFromHandle);

            ilProcessor.Replace(instruction.Previous, ldtoken);
            ilProcessor.Replace(instruction, call);
        }

        private void ReplaceVariable(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            throw new NotImplementedException();
        }

        private void ReplaceField(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var module = ilProcessor.Body.Method.Module;

            var name = instruction.Previous.Operand as string;

            var fieldReference = Silk.Loom.References.FindField(module, ilProcessor.Body.Method, name);

            var getFieldFromHandle = Silk.Loom.References.FindMethod(module, null,
                "System.Reflection.FieldInfo System.Reflection.FieldInfo::GetFieldFromHandle(System.RuntimeFieldHandle)");

            var ldtoken = Instruction.Create(OpCodes.Ldtoken, fieldReference);
            var call = Instruction.Create(OpCodes.Call, getFieldFromHandle);

            ilProcessor.Replace(instruction.Previous, ldtoken);
            ilProcessor.Replace(instruction, call);
        }

        private void ReplaceProperty(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var module = ilProcessor.Body.Method.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;

            var propertyReference = Silk.Loom.References.FindProperty(module, ilProcessor.Body.Method, name);

            var systemType = Silk.Loom.References.FindType(module, null, "System.Type");

            var getTypeFromHandle = Silk.Loom.References.FindMethod(module, null,
                "System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)");

            MethodReference getProperty;
            if (propertyReference.Parameters.Count != 0)
            {
                getProperty = Silk.Loom.References.FindMethod(module, null,
                    "System.Reflection.PropertyInfo System.Type::GetProperty(System.String,System.Type,System.Type[])");
            }
            else
            {
                getProperty = Silk.Loom.References.FindMethod(module, null,
                    "System.Reflection.PropertyInfo System.Type::GetProperty(System.String,System.Type)");
            }

            var insert_before = instruction.Next;
            // Push Declaring Type onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, propertyReference.DeclaringType));
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getTypeFromHandle));
            // Push name onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldstr, propertyReference.Name));
            // Push return type onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, propertyReference.PropertyType));
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getTypeFromHandle));
            if (propertyReference.Parameters.Count != 0)
            {
                // Push property array onto stack
                ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldc_I4, propertyReference.Parameters.Count));
                ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Newarr, systemType));
                // Assign property elems
                for (int i = 0; i < propertyReference.Parameters.Count; ++i)
                {
                    var param_type = propertyReference.Parameters[i].ParameterType;
                    // Duplicate the array value
                    ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Dup));
                    ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldc_I4, i));
                    ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, param_type));
                    ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getTypeFromHandle));
                    ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Stelem_Any, systemType));
                }
            }
            // Call GetProperty
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getProperty));
            
            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
        }

        private void ReplaceParameter(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var module = ilProcessor.Body.Method.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;

            var callingMethod = ilProcessor.Body.Method;

            var parameter = callingMethod.Parameters.FirstOrDefault(var => var.Name == name);

            if (parameter == null)
            {
                throw new Exception(string.Format("Could not find parameter '{0}'.", name));
            }
            
            var parameterInfo = Silk.Loom.References.FindType(module, null, "System.Reflection.ParameterInfo");

            var methodReference = Silk.Loom.References.FindMethod(module, ilProcessor.Body.Method, callingMethod.FullName);

            var getMethodFromHandle = Silk.Loom.References.FindMethod(module, null,
                "System.Reflection.MethodBase System.Reflection.MethodBase::GetMethodFromHandle(System.RuntimeMethodHandle)");
            
            var getParameters = Silk.Loom.References.FindMethod(module, null,
                "System.Reflection.ParameterInfo[] System.Reflection.MethodBase::GetParameters()");

            var insert_before = instruction.Next;
            // Push calling method onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, methodReference));
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getMethodFromHandle));
            // Get parameter array
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Callvirt, getParameters));
            // Push parameter index onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldc_I4, parameter.Index));
            // Get local variable
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldelem_Any, parameterInfo));

            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
        }
    }
}
