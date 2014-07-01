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

            if (calledMethod.Name == "Type")
            {
                ReplaceType(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Parameter")
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
            var callingMethod = ilProcessor.Body.Method;
            var module = callingMethod.Module;

            var name = instruction.Previous.Operand as string;

            var methodReference = Reference.ParseMethodReference(
                                      Reference.Scope.NewMethodScope(callingMethod), name);

            var getMethodFromHandle = Reference.ParseMethodReference(
                                          Reference.Scope.NewMethodScope(callingMethod),
                                          "System.Reflection.MethodBase::GetMethodFromHandle(System.RuntimeMethodHandle)");

            var ldtoken = Instruction.Create(OpCodes.Ldtoken, methodReference);
            var call = Instruction.Create(OpCodes.Call, getMethodFromHandle);

            ilProcessor.Replace(instruction.Previous, ldtoken);
            ilProcessor.Replace(instruction, call);
        }

        private void ReplaceVariable(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var callingMethod = ilProcessor.Body.Method;
            var module = callingMethod.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;


            var variable = callingMethod.Body.Variables.FirstOrDefault(var => var.Name == name);

            if (variable == null)
            {
                throw new Exception(string.Format("Could not find variable '{0}'.", name));
            }
            
            var localVariableInfo = Reference.ParseTypeReference(
                                        Reference.Scope.NewMethodScope(callingMethod), 
                                        "System.Reflection.LocalVariableInfo");

            var methodReference = Reference.ParseMethodReference(
                                      Reference.Scope.NewMethodScope(callingMethod),
                                      Silk.Loom.CecilNames.MethodName(callingMethod));

            var getMethodFromHandle = Reference.ParseMethodReference(
                                          Reference.Scope.NewMethodScope(callingMethod),
                                          "System.Reflection.MethodBase::GetMethodFromHandle(System.RuntimeMethodHandle)");
            
            var getMethodBody = Reference.ParseMethodReference(
                                    Reference.Scope.NewMethodScope(callingMethod),
                                    "System.Reflection.MethodBase::GetMethodBody()");

            var get_localVariables = Reference.ParseMethodReference(
                                         Reference.Scope.NewMethodScope(callingMethod),
                                         "System.Reflection.MethodBody::get_LocalVariables()");

            var get_ilist_item = Reference.ParseMethodReference(
                                     Reference.Scope.NewMethodScope(callingMethod), 
                                     "System.Collections.Generic.IList`1<System.Reflection.LocalVariableInfo>::get_Item(System.Int32)");

            var insert_before = instruction.Next;
            // Push calling method onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, methodReference));
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getMethodFromHandle));
            // Get method body
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Callvirt, getMethodBody));
            // Get local variable array
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, get_localVariables));
            // Push variable index onto stack
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldc_I4, variable.Index));
            // Get local variable
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Callvirt, get_ilist_item));

            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
        }

        private void ReplaceField(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var callingMethod = ilProcessor.Body.Method;
            var module = callingMethod.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;

            var fieldReference = Reference.ParseFieldReference(
                                     Reference.Scope.NewMethodScope(callingMethod), name);

            var getFieldFromHandle = Reference.ParseMethodReference(
                                         Reference.Scope.NewMethodScope(callingMethod),
                                         "System.Reflection.FieldInfo::GetFieldFromHandle(System.RuntimeFieldHandle)");

            var insert_before = instruction.Next;
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, fieldReference));
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getFieldFromHandle));

            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
        }

        private void ReplaceProperty(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var callingMethod = ilProcessor.Body.Method;
            var module = callingMethod.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;

            var propertyReference = Silk.Loom.Reference.ParsePropertyReference(Reference.Scope.NewMethodScope(callingMethod), name);

            var systemType = Silk.Loom.Reference.ParseTypeReference(Reference.Scope.NewMethodScope(callingMethod), "System.Type");

            var getTypeFromHandle = Silk.Loom.Reference.ParseMethodReference(
                Reference.Scope.NewMethodScope(callingMethod),
                "System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)");

            MethodReference getProperty;
            if (propertyReference.Parameters.Count != 0)
            {
                getProperty = Silk.Loom.Reference.ParseMethodReference(
                    Reference.Scope.NewMethodScope(callingMethod),
                    "System.Type::GetProperty(System.String,System.Type,System.Type[])");
            }
            else
            {
                getProperty = Silk.Loom.Reference.ParseMethodReference(
                    Reference.Scope.NewMethodScope(callingMethod),
                    "System.Type::GetProperty(System.String,System.Type)");
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
            var callingMethod = ilProcessor.Body.Method;
            var module = callingMethod.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;

            var parameter = callingMethod.Parameters.FirstOrDefault(var => var.Name == name);

            if (parameter == null)
            {
                throw new Exception(string.Format("Could not find parameter '{0}'.", name));
            }
            
            var parameterInfo = Reference.ParseTypeReference(
                                    Reference.Scope.NewMethodScope(callingMethod), 
                                    "System.Reflection.ParameterInfo");

            var methodReference = Reference.ParseMethodReference(
                                      Reference.Scope.NewMethodScope(callingMethod),
                                      Silk.Loom.CecilNames.MethodName(callingMethod));

            var getMethodFromHandle = Reference.ParseMethodReference(
                                          Reference.Scope.NewMethodScope(callingMethod), 
                                          "System.Reflection.MethodBase::GetMethodFromHandle(System.RuntimeMethodHandle)");
            
            var getParameters = Reference.ParseMethodReference(
                                    Reference.Scope.NewMethodScope(callingMethod), 
                                    "System.Reflection.MethodBase::GetParameters()");

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

        private void ReplaceType(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var callingMethod = ilProcessor.Body.Method;
            var module = callingMethod.Module;

            var name = Analysis[instruction.Previous].Head.Item2.Value as string;

            var typeReference = Reference.ParseTypeReference(
                                    Reference.Scope.NewMethodScope(callingMethod), name);

            var getTypeFromHandle = Reference.ParseMethodReference(
                                        Reference.Scope.NewMethodScope(callingMethod), 
                                        "System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)");

            var insert_before = instruction.Next;
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Ldtoken, typeReference));
            ilProcessor.InsertBefore(insert_before, Instruction.Create(OpCodes.Call, getTypeFromHandle));
            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
        }
    }
}
