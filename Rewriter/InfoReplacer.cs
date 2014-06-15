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

            var methodReference = Silk.Loom.References.FindMethod(module, ilProcessor.Body, name);

            var getMethodFromHandle = module.Import(
                typeof(System.Reflection.MethodBase).GetMethod("GetMethodFromHandle",
                new[] { typeof(RuntimeMethodHandle) }));

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

            var fieldReference = Silk.Loom.References.FindField(module, ilProcessor.Body, name);

            var getFieldFromHandle = module.Import(
                typeof(System.Reflection.FieldInfo).GetMethod("GetFieldFromHandle",
                new[] { typeof(RuntimeFieldHandle) }));

            var ldtoken = Instruction.Create(OpCodes.Ldtoken, fieldReference);
            var call = Instruction.Create(OpCodes.Call, getFieldFromHandle);

            ilProcessor.Replace(instruction.Previous, ldtoken);
            ilProcessor.Replace(instruction, call);
        }

        private void ReplaceProperty(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            throw new NotImplementedException();
        }

        private void ReplaceParameter(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            throw new NotImplementedException();
        }
    }
}
