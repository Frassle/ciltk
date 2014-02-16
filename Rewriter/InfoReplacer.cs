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
        public static string[] SplitName(string name)
        {
            // Name = Identifer
            // Type = NAMESPACE.Name | Type/Name | Type`Integer
            // Field = Type Type::Name
            // Parameters = "" | Type | Parameters,Type
            // Method = Type Type::Name(Parameters)
            return name.Split(new string[] { "::", "/", " "}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static FieldReference FindField(ModuleDefinition module, string name)
        {
            var fields = module.GetTypes().SelectMany(type => type.Fields);
            FieldReference fieldReference = null;

            foreach (var field in fields)
            {
                var fieldName = field.FullName.Split(' ')[1];
                if (fieldName == name)
                {
                    fieldReference = field;
                    break;
                }
            }

            if (fieldReference == null)
            {
                throw new Exception(string.Format("Field {0} not found.", name));
            }

            return fieldReference;
        }

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
            throw new NotImplementedException();
        }

        private void ReplaceVariable(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var name = instruction.Previous.Operand as string;

            var variable = ilProcessor.Body.Variables.FirstOrDefault(v => v.Name == name);

            if (variable == null)
            {
                throw new Exception(string.Format("Variable {0} not found.", name));
            }
        }

        private void ReplaceField(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var module = ilProcessor.Body.Method.Module;

            var name = instruction.Previous.Operand as string;

            var fieldReference = FindField(module, name);

            var getFieldFromHandle = module.Import(
                typeof(System.Reflection.FieldInfo).GetMethod("GetFieldFromHandle",
                new[] { typeof(RuntimeFieldHandle) }));

            ilProcessor.InsertAfter(instruction, Instruction.Create(OpCodes.Call, getFieldFromHandle));
            ilProcessor.InsertAfter(instruction, Instruction.Create(OpCodes.Ldtoken, fieldReference));

            ilProcessor.Remove(instruction.Previous);
            ilProcessor.Remove(instruction);
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
