using System;
using System.Collections.Generic;
using System.Text;

namespace Silk
{
    public static unsafe class Cil
    {
        /// <summary>
        /// Defines a new label in the instruction stream.
        /// </summary>
        /// <param name="label">The label name.</param>
        public static void Label(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Keeps a value alive to this point.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to keep alive.</param>
        public static void KeepAlive<T>(T value) { }

        /// <summary>
        /// Loads a variable onto the top of the execution stack.
        /// </summary>
        /// <typeparam name="T">The type to load.</typeparam>
        /// <param name="value">The variable to load.</param>
        public static void Load<T>(T value) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Stores the value at the top of the execution stack to a variable.
        /// </summary>
        /// <typeparam name="T">The type to store.</typeparam>
        /// <param name="value">The variable to store to.</param>
        public static void Store<T>(out T value) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Add two values, returning a new value.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The add instruction adds value2 to value1 and pushes the result on the stack. Overflow is not 
        /// detected for integral operations (but see add.ovf); floating-point overflow returns +inf or -inf.
        /// </remarks>
        public static void Add() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Add signed integer values with overflow check.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The add.ovf instruction adds value1 and value2 and pushes the result on the stack. The 
        /// acceptable operand types and their corresponding result data type are encapsulated in 
        /// Table 7: Overflow Arithmetic Operations.
        /// 
        /// Exceptions:
        /// System.OverflowException is thrown if the result cannot be represented in the result type.
        /// </remarks>
        public static void Add_Ovf() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Add unsigned integer values with overflow check.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The add.ovf instruction adds value1 and value2 and pushes the result on the stack. The 
        /// acceptable operand types and their corresponding result data type are encapsulated in 
        /// Table 7: Overflow Arithmetic Operations.
        /// 
        /// Exceptions:
        /// System.OverflowException is thrown if the result cannot be represented in the result type.
        /// </remarks>
        public static void Add_Ovf_Un() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Bitwise AND of two integral values, returns an integral value.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The and instruction computes the bitwise AND of value1 and value2and pushes the result on the 
        /// stack. The acceptable operand types and their corresponding result data type are encapsulated in 
        /// Table 5: Integer Operations.
        /// </remarks>
        public static void And() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Return argument list handle for the current method.
        /// 
        /// Stack Transition:
        /// ... -> ..., argListHandle
        /// </summary>
        /// <remarks>
        /// The arglist instruction returns an opaque handle (having type System.RuntimeArgumentHandle) 
        /// representing the argument list of the current method. This handle is valid only during the lifetime 
        /// of the current method. The handle can, however, be passed to other methods as long as the 
        /// current method is on the thread of control. The arglist instruction can only be executed within a 
        /// method that takes a variable number of arguments. 
        /// [Rationale: This instruction is needed to implement the C ‘va_*’ macros used to implement 
        /// procedures like ‘printf’. It is intended for use with the class library implementation of 
        /// System.ArgIterator. end rationale]
        /// </remarks>
        public static void Arglist() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if equal.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The beq instruction transfers control to target if value1 is equal to value2. The effect is identical 
        /// to performing a ceq instruction followed by a brtrue target. target is represented as a signed 
        /// offset (4 bytes for beq, 1 byte for beq.s) from the beginning of the instruction following the 
        /// current instruction. 
        /// The acceptable operand types are encapsulated in 
        /// Table 4: Binary Comparison or Branch Operations. 
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first 
        /// of these prefixes. 
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this 
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see 
        /// Partition I for details). 
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Beq(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if greater than or equal to.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The bge instruction transfers control to target if value1 is greater than or equal to value2. The
        /// effect is identical to performing a clt.un instruction followed by a brfalse target. target is
        /// represented as a signed offset (4 bytes for bge, 1 byte for bge.s) from the beginning of the
        /// instruction following the current instruction.
        /// The effect of a “bge target” instruction is identical to:
        /// - If stack operands are integers, then clt followed by a brfalse target
        /// - If stack operands are floating-point, then clt.un followed by a brfalse target
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Bge(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if greater than or equal to (unsigned or unordered).
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The bge.un instruction transfers control to target if value1 is greater than or equal to value2,
        /// when compared unsigned (for integer values) or unordered (for floating-point values).
        /// target is represented as a signed offset (4 bytes for bge.un, 1 byte for bge.un.s) from the
        /// beginning of the instruction following the current instruction.
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Bge_Un(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if greater than.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The bgt instruction transfers control to target if value1 is greater than value2. The effect is
        /// identical to performing a cgt instruction followed by a brtrue target. target is represented as a
        /// signed offset (4 bytes for bgt, 1 byte for bgt.s) from the beginning of the instruction following
        /// the current instruction.
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Bgt(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if greater than (unsigned or unordered).
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ... 
        /// </summary>
        /// <remarks>
        /// The bgt.un instruction transfers control to target if value1 is greater than value2, when compared
        /// unsigned (for integer values) or unordered (for floating-point values). The effect is identical to
        /// performing a cgt.un instruction followed by a brtrue target. target is represented as a signed
        /// offset (4 bytes for bgt.un, 1 byte for bgt.un.s) from the beginning of the instruction following
        /// the current instruction.
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Bgt_Un(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if less than or equal to.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ... 
        /// </summary>
        /// <remarks>
        /// The ble instruction transfers control to target if value1 is less than or equal to value2. target is
        /// represented as a signed offset (4 bytes for ble, 1 byte for ble.s) from the beginning of the
        /// instruction following the current instruction.
        /// The effect of a “ble target” instruction is identical to:
        /// - If stack operands are integers, then : cgt followed by a brfalse target
        /// - If stack operands are floating-point, then : cgt.un followed by a brfalse target
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Ble(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if less than or equal to (unsigned or unordered).
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The ble.un instruction transfers control to target if value1 is less than or equal to value2, when
        /// compared unsigned (for integer values) or unordered (for floating-point values). target is
        /// represented as a signed offset (4 bytes for ble.un, 1 byte for ble.un.s) from the beginning of the
        /// instruction following the current instruction.
        /// The effect of a “ble.un target” instruction is identical to:
        /// - If stack operands are integers, then cgt.un followed by a brfalse target
        /// - If stack operands are floating-point, then cgt followed by a brfalse target
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Ble_Un(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if less than.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The blt instruction transfers control to target if value1 is less than value2. The effect is identical
        /// to performing a clt instruction followed by a brtrue target. target is represented as a signed
        /// offset (4 bytes for blt, 1 byte for blt.s) from the beginning of the instruction following the current
        /// instruction.
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Blt(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if less than (unsigned or unordered).
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The blt.un instruction transfers control to target if value1 is less than value2, when compared
        /// unsigned (for integer values) or unordered (for floating-point values). The effect is identical to
        /// performing a clt.un instruction followed by a brtrue target. target is represented as a signed
        /// offset (4 bytes for blt.un, 1 byte for blt.un.s) from the beginning of the instruction following the
        /// current instruction.
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Blt_Un(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if unequal or unordered, short form.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ...
        /// </summary>
        /// <remarks>
        /// The bne.un instruction transfers control to target if value1 is not equal to value2, when
        /// compared unsigned (for integer values) or unordered (for floating-point values). The effect is
        /// identical to performing a ceq instruction followed by a brfalse target. target is represented as a
        /// signed offset (4 bytes for bne.un, 1 byte for bne.un.s) from the beginning of the instruction
        /// following the current instruction.
        /// The acceptable operand types are encapsulated in
        /// Table 4: Binary Comparison or Branch Operations.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Bne_Un(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Convert a boxable value to its boxed form.
        /// 
        /// Stack Transition:
        /// ..., val -> ..., obj
        /// </summary>
        /// <remarks>
        /// If typeTok is a value type, the box instruction converts val to its boxed form. When typeTok is a
        /// non-nullable type (§I.8.2.4), this is done by creating a new object and copying the data from val
        /// into the newly allocated object. If it is a nullable type, this is done by inspecting val’s HasValue
        /// property; if it is false, a null reference is pushed onto the stack; otherwise, the result of boxing
        /// val’s Value property is pushed onto the stack.
        /// If typeTok is a reference type, the box instruction does returns val unchanged as obj.
        /// If typeTok is a generic parameter, the behavior of box instruction depends on the actual type at
        /// runtime. If this type is a value type it is boxed as above, if it is a reference type then val is not
        /// changed. However the type tracked by verification is always “boxed” typeTok for generic
        /// parameters, regardless of whether the actual type at runtime is a value or reference type.
        /// typeTok is a metadata token (a typedef, typeref, or typespec) indicating the type of val.
        /// typeTok can represent a value type, a reference type, or a generic parameter.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Box<T>() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target.
        /// 
        /// Stack Transition:
        /// ..., -> ... 
        /// </summary>
        /// <remarks>
        /// The br instruction unconditionally transfers control to target. target is represented as a signed
        /// offset (4 bytes for br, 1 byte for br.s) from the beginning of the instruction following the current
        /// instruction.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// [Rationale: While a leave instruction can be used instead of a br instruction when the evaluation
        /// stack is empty, doing so might increase the resources required to compile from CIL to native
        /// code and/or lead to inferior native code. Therefore CIL generators should use a br instruction in
        /// preference to a leave instruction when both are valid. end rationale]
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Br(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Inform a debugger that a breakpoint has been reached.
        /// 
        /// Stack Transition:
        /// ..., -> ...
        /// </summary>
        /// <remarks>
        /// The break instruction is for debugging support. It signals the CLI to inform the debugger that a 
        /// break point has been tripped. It has no other effect on the interpreter state. 
        /// The break instruction has the smallest possible instruction size so that code can be patched with 
        /// a breakpoint with minimal disturbance to the surrounding code. 
        /// The break instruction might trap to a debugger, do nothing, or raise a security exception: the 
        /// exact behavior is implementation-defined. 
        /// </remarks>
        public static void Break() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if value is zero (false).
        /// 
        /// Stack Transition:
        /// ..., value -> ...
        /// </summary>
        /// <remarks>
        /// The brfalse instruction transfers control to target if value (of type int32, int64, object
        /// reference, managed pointer, unmanaged pointer or native int) is zero (false). If value is nonzero
        /// (true), execution continues at the next instruction.
        /// Target is represented as a signed offset (4 bytes for brfalse, 1 byte for brfalse.s) from the
        /// beginning of the instruction following the current instruction.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Brfalse(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Branch to target if value is non-zero (true).
        /// 
        /// Stack Transition:
        /// ..., value -> ...
        /// </summary>
        /// <remarks>
        /// The brtrue instruction transfers control to target if value (of type native int) is nonzero (true).
        /// If value is zero (false) execution continues at the next instruction.
        /// If the value is an object reference (type O) then brinst (an alias for brtrue) transfers control if it
        /// represents an instance of an object (i.e., isn’t the null object reference, see ldnull).
        /// Target is represented as a signed offset (4 bytes for brtrue, 1 byte for brtrue.s) from the
        /// beginning of the instruction following the current instruction.
        /// If the target instruction has one or more prefix codes, control can only be transferred to the first
        /// of these prefixes.
        /// Control transfers into and out of try, catch, filter, and finally blocks cannot be performed by this
        /// instruction. (Such transfers are severely restricted and shall use the leave instruction instead; see
        /// Partition I for details).
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Brtrue(string label) { throw new Exception("CilTK Rewriter not run."); }

        public static void Call(object method) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Call method indicated on the stack with arguments described by 
        /// callsitedescr.
        /// 
        /// Stack Transition:
        /// ..., arg0, arg1 ... argN, ftn -> ..., retVal (not always returned)
        /// </summary>
        /// <remarks>
        /// The calli instruction calls ftn (a pointer to a method entry point) with the arguments arg0 … argN. 
        /// The types of these arguments are described by the signature callsitedescr. (See Partition I for a 
        /// description of the CIL calling sequence.) The calli instruction can be immediately preceded by a 
        /// tail. prefix to specify that the current method state should be released before transferring control. 
        /// If the call would transfer control to a method of higher trust than the originating method the stack 
        /// frame will not be released; instead, the execution will continue silently as if the tail. prefix had 
        /// not been supplied. 
        /// [A callee of “higher trust” is defined as one whose permission grant-set is a strict superset of the 
        /// grant-set of the caller.] 
        /// The ftn argument must be a method pointer to a method that can be legitimately called with the 
        /// arguments described by callsitedescr (a metadata token for a stand-alone signature). Such a 
        /// pointer can be created using the ldftn or ldvirtftn instructions, or could have been passed in from 
        /// native code. 
        /// The standalone signature specifies the number and type of parameters being passed, as well as 
        /// the calling convention (See Partition II) The calling convention is not checked dynamically, so 
        /// code that uses a calli instruction will not work correctly if the destination does not actually use 
        /// the specified calling convention. 
        /// The arguments are placed on the stack in left-to-right order. That is, the first argument is 
        /// computed and placed on the stack, then the second argument, and so on. The argument-building 
        /// code sequence for an instance or virtual method shall push that instance reference (the this
        /// pointer, which shall not be null) first. [Note: for calls to methods on value types, the this
        /// pointer is a managed pointer, not an instance reference. §I.8.6.1.5. end note] 
        /// The arguments are passed as though by implicit starg (§III.3.61) instructions, see Implicit 
        /// argument coercion §III.1.6. 
        /// calli pops the this pointer, if any, and the arguments off the evaluation stack before calling the 
        /// method. If the method has a return value, it is pushed on the stack upon method completion. On 
        /// the callee side, the arg0 parameter/this pointer is accessed as argument 0, arg1 as argument 1, 
        /// and so on. 
        /// </remarks>
        public static void Calli(System.Runtime.InteropServices.CallingConvention callingConvention, Type returnType, params Type[] parameterTypes)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static void Callvirt() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Cast obj to typeTok.
        /// 
        /// Stack Transition:
        /// ..., obj -> ..., obj2
        /// </summary>
        /// <remarks>
        /// typeTok is a metadata token (a typeref, typedef or typespec), indicating the desired
        /// class. If typeTok is a non-nullable value type or a generic parameter type it is interpreted
        /// as “boxed” typeTok. If typeTok is a nullable type, Nullable&lt;T&gt;, it is interpreted as
        /// “boxed” T.
        /// The castclass instruction determines if obj (of type O) is an instance of the type typeTok, termed
        /// “casting”.
        /// If the actual type (not the verifier tracked type) of obj is verifier-assignable-to the type typeTok
        /// the cast succeeds and obj (as obj2) is returned unchanged while verification tracks its type as
        /// typeTok.
        /// Unlike coercions (§III.1.6) and conversions (§III.3.27), a cast never changes the actual
        /// type of an object and preserves object identity (see Partition I).
        /// If the cast fails then an InvalidCastException is thrown.
        /// If obj is null, castclass succeeds and returns null. This behavior differs semantically from isinst
        /// where if obj is null, isinst fails and returns null.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Castclass<T>() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push 1 (of type int32) if value1 equals value2, else push 0
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The ceq instruction compares value1 and value2. If value1 is equal to value2, then 1 (of type 
        /// int32) is pushed on the stack. Otherwise, 0 (of type int32) is pushed on the stack. 
        /// For floating-point numbers, ceq will return 0 if the numbers are unordered (either or both are 
        /// NaN). The infinite values are equal to themselves. 
        /// The acceptable operand types are encapsulated in 
        /// Table 4: Binary Comparison or Branch Operations.
        /// </remarks>
        public static void Ceq() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push 1 (of type int32) if value1 > value2, else push 0.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The cgt instruction compares value1 and value2. If value1 is strictly greater than value2, then 1 
        /// (of type int32) is pushed on the stack. Otherwise, 0 (of type int32) is pushed on the stack. 
        /// For floating-point numbers, cgt returns 0 if the numbers are unordered (that is, if one or both of 
        /// the arguments are NaN). 
        /// As with IEC 60559:1989, infinite values are ordered with respect to normal numbers (e.g., 
        /// +infinity > 5.0 > -infinity). 
        /// The acceptable operand types are encapsulated in 
        /// Table 4: Binary Comparison or Branch Operations. 
        /// </remarks>
        public static void Cgt() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push 1 (of type int32) if value1 > value2, unsigned or 
        /// unordered, else push 0.
        /// 
        /// Stack Transition:
        /// ..., value1, value2 -> ..., result
        /// </summary>
        /// <remarks>
        /// The cgt.un instruction compares value1 and value2. A value of 1 (of type int32) is pushed on 
        /// the stack if 
        ///  for floating-point numbers, either value1 is strictly greater than value2, or value1 is 
        /// not ordered with respect to value2.
        ///  for integer values, value1 is strictly greater than value2 when considered as 
        /// unsigned numbers. 
        /// Otherwise, 0 (of type int32) is pushed on the stack. 
        /// As per IEC 60559:1989, infinite values are ordered with respect to normal numbers (e.g., 
        /// +infinity > 5.0 > -infinity). 
        /// The acceptable operand types are encapsulated in 
        /// Table 4: Binary Comparison or Branch Operations.
        /// </remarks>
        public static void Cgt_Un() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Throw ArithmeticException if value is not a finite number.
        /// 
        /// Stack Transition:
        /// ..., value -> ..., result
        /// </summary>
        /// <remarks>
        /// The ckfinite instruction throws ArithmeticException if value (a floating-point number) is 
        /// either a “not a number” value (NaN) or +/- infinity value. ckfinite leaves the value on the stack if 
        /// no exception is thrown. Execution behavior is unspecified if value is not a floating-point number. 
        /// </remarks>
        public static void Ckfinite() { throw new Exception("CilTK Rewriter not run."); }
        public static void Clt() { throw new Exception("CilTK Rewriter not run."); }
        public static void Clt_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Constrained() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_I() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_I1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_I2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_I4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_I8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I1_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I2_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I4_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_I8_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U1_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U2_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U4_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_Ovf_U8_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_R_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_R4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_R8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_U() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_U1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_U2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_U4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Conv_U8() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Copy data from memory to memory.
        /// 
        /// Stack Transition:
        /// ..., destaddr, srcaddr, size -> ...,
        /// </summary>
        /// <remarks>
        /// The cpblk instruction copies size (of type unsigned int32) bytes from address srcaddr (of type
        /// native int, or &amp;) to address destaddr (of type native int, or &amp;). The behavior of cpblk is
        /// unspecified if the source and destination areas overlap.
        /// cpblk assumes that both destaddr and srcaddr are aligned to the natural size of the machine (but
        /// see the unaligned. prefix instruction). The operation of the cpblk instruction can be altered by
        /// an immediately preceding volatile. or unaligned. prefix instruction.
        /// [Rationale: cpblk is intended for copying structures (rather than arbitrary byte-runs). All such
        /// structures, allocated by the CLI, are naturally aligned for the current platform. Therefore, there is
        /// no need for the compiler that generates cpblk instructions to be aware of whether the code will
        /// eventually execute on a 32-bit or 64-bit platform. end rationale]
        /// </remarks>
        public static void Cpblk() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Copy a value type from src to dest.
        /// 
        /// Stack Transition:
        /// ..., dest, src -> ...,
        /// </summary>
        /// <remarks>
        /// The cpobj instruction copies the value at the address specified by src (an unmanaged pointer,
        /// native int, or a managed pointer, &amp;) to the address specified by dest (also a pointer). typeTok
        /// can be a typedef, typeref, or typespec. The behavior is unspecified if
        /// the type of the location referenced by src is not assignable-to (§I.8.7.3)
        /// the type of the location referenced by dest.
        /// If typeTok is a reference type, the cpobj instruction has the same effect as ldind.ref followed by
        /// stind.ref.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Cpobj<T>() { throw new Exception("CilTK Rewriter not run."); }


        public static void Div() { throw new Exception("CilTK Rewriter not run."); }

        public static void Div_Un() { throw new Exception("CilTK Rewriter not run."); }

        public static void Dup() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// End an exception handling filter clause.
        /// 
        /// Stack Transition:
        /// ..., value -> ...
        /// </summary>
        /// <remarks>
        /// Used to return from the filter clause of an exception (see the Exception Handling subclause of
        /// Partition I for a discussion of exceptions). value (which shall be of type int32 and one of a
        /// specific set of values) is returned from the filter clause. It should be one of:
        /// - exception_continue_search (0) to continue searching for an exception handler
        /// - exception_execute_handler (1) to start the second phase of exception handling
        /// where finally blocks are run until the handler associated with this filter clause is
        /// located. Then the handler is executed.
        /// The result of using any other integer value is unspecified.
        /// The entry point of a filter, as shown in the method’s exception table, shall be the (lexically) first
        /// instruction in the filter’s code block. The endfilter shall be the (lexically) last instruction in the
        /// filter’s code block (hence there can only be one endfilter for any single filter block). After
        /// executing the endfilter instruction, control logically flows back to the CLI exception handling
        /// mechanism.
        /// Control cannot be transferred into a filter block except through the exception mechanism.
        /// Control cannot be transferred out of a filter block except through the use of a throw instruction or
        /// executing the final endfilter instruction. In particular, it is not valid to execute a ret or leave
        /// instruction within a filter block. It is not valid to embed a try block within a filter block. If
        /// an exception is thrown inside the filter block, it is intercepted and a value of
        /// exception_continue_search is returned.
        /// </remarks>
        public static void Endfilter() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// End finally clause of an exception block.
        /// 
        /// Stack Transition:
        /// ... -> ...
        /// </summary>
        /// <remarks>
        /// Return from the finally or fault clause of an exception block (see the Exception Handling
        /// subclause of Partition I for details).
        /// Signals the end of the finally or fault clause so that stack unwinding can continue until the
        /// exception handler is invoked. The endfinally or endfault instruction transfers control back to the
        /// CLI exception mechanism. This then searches for the next finally clause in the chain, if the
        /// protected block was exited with a leave instruction. If the protected block was exited with an
        /// exception, the CLI will search for the next finally or fault, or enter the exception handler
        /// chosen during the first pass of exception handling.
        /// An endfinally instruction can only appear lexically within a finally block. Unlike the endfilter
        /// instruction, there is no requirement that the block end with an endfinally instruction, and there
        /// can be as many endfinally instructions within the block as required. These same restrictions
        /// apply to the endfault instruction and the fault block, mutatis mutandis.
        /// Control cannot be transferred into a finally (or fault block) except through the exception
        /// mechanism. Control cannot be transferred out of a finally (or fault) block except through the
        /// use of a throw instruction or executing the endfinally (or endfault) instruction. In particular, it is
        /// not valid to “fall out” of a finally (or fault) block or to execute a ret or leave instruction
        /// within a finally (or fault) block.
        /// Note that the endfault and endfinally instructions are aliases—they correspond to the same
        /// opcode.
        /// endfinally empties the evaluation stack as a side-effect.
        /// </remarks>
        public static void Endfinally() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Set all bytes in a block of memory to a given byte value.
        /// 
        /// Stack Transition:
        /// ..., addr, value, size -> ...,
        /// </summary>
        /// <remarks>
        /// The initblk instruction sets size (of type unsigned int32) bytes starting at addr (of type native
        /// int, or &amp;) to value (of type unsigned int8). initblk assumes that addr is aligned to the natural
        /// size of the machine (but see the unaligned. prefix instruction).
        /// [Rationale: initblk is intended for initializing structures (rather than arbitrary byte-runs). All such
        /// structures, allocated by the CLI, are naturally aligned for the current platform. Therefore, there is
        /// no need for the compiler that generates initblk instructions to be aware of whether the code will
        /// eventually execute on a 32-bit or 64-bit platform. end rationale]
        /// The operation of the initblk instructions can be altered by an immediately preceding volatile. or
        /// unaligned. prefix instruction.
        /// </remarks>
        public static void Initblk() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Initialize the value at address dest.
        /// 
        /// Stack Transition:
        /// ..., dest -> ...,
        /// </summary>
        /// <remarks>
        /// The initobj instruction initializes an address with a default value. typeTok is a metadata token (a
        /// typedef, typeref, or typespec). dest is an unmanaged pointer (native int), or a managed
        /// pointer (&amp;). If typeTok is a value type, the initobj instruction initializes each field of dest to null
        /// or a zero of the appropriate built-in type. If typeTok is a value type, then after this instruction is
        /// executed, the instance is ready for a constructor method to be called. If typeTok is a reference
        /// type, the initobj instruction has the same effect as ldnull followed by stind.ref.
        /// Unlike newobj, the initobj instruction does not call any constructor method.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Initobj<T>() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Test if obj is an instance of typeTok, returning null or an instance of
        /// that class or interface.
        /// 
        /// Stack Transition:
        /// ..., obj -> ..., result
        /// </summary>
        /// <remarks>
        /// typeTok is a metadata token (a typeref, typedef or typespec), indicating the desired
        /// class. If typeTok is a non-nullable value type or a generic parameter type it is interpreted
        /// as “boxed” typeTok. If typeTok is a nullable type, Nullable&lt;T&gt;, it is interpreted as
        /// “boxed” T.
        /// The isinst instruction tests whether obj (type O) is an instance of the type typeTok.
        /// If the actual type (not the verifier tracked type) of obj is verifier-assignable-to the type
        /// typeTok then isinst succeeds and obj (as result) is returned unchanged while verification
        /// tracks its type as typeTok. Unlike coercions (§III.1.6) and conversions (§III.3.27), isinst
        /// never changes the actual type of an object and preserves object identity (see Partition I).
        /// If obj is null, or obj is not verifier-assignable-to the type typeTok, isinst fails and returns null.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Isinst<T>() { throw new Exception("CilTK Rewriter not run."); }


        public static void Jmp() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Load argument numbered num onto the stack.
        /// 
        /// Stack Transition:
        /// ... -> ..., value
        /// </summary>
        /// <remarks>
        /// The ldarg num instruction pushes onto the evaluation stack, the num’th incoming argument, 
        /// where arguments are numbered 0 onwards (see Partition I). The type of the value on the stack is 
        /// tracked by verification as the intermediate type (§I.8.7) of the argument type, as specified by the 
        /// current method’s signature.
        /// The ldarg.0, ldarg.1, ldarg.2, and ldarg.3 instructions are efficient encodings for loading any 
        /// one of the first 4 arguments. The ldarg.s instruction is an efficient encoding for loading 
        /// argument numbers 4–255.
        /// For procedures that take a variable-length argument list, the ldarg instructions can be used only 
        /// for the initial fixed arguments, not those in the variable part of the signature. (See the arglist
        /// instruction.) 
        /// If required, arguments are converted to the representation of their intermediate type (§I.8.7) 
        /// when loaded onto the stack (§III.1.1.1). 
        /// [Note: that is arguments that hold an integer value smaller than 4 bytes, a boolean, or a character 
        /// are converted to 4 bytes by sign or zero-extension as appropriate. Floating-point values are 
        /// converted to their native size (type F). end note] 
        /// </remarks>
        public static void Ldarg(int num) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Fetch the address of argument argNum.
        /// 
        /// Stack Transition:
        /// ..., -> ..., address of argument number argNum
        /// </summary>
        /// <remarks>
        /// The ldarga instruction fetches the address (of type &amp;, i.e., managed pointer) of the argNum’th 
        /// argument, where arguments are numbered 0 onwards. The address will always be aligned to a 
        /// natural boundary on the target machine (cf. cpblk and initblk). The short form (ldarga.s) should 
        /// be used for argument numbers 0–255. The result is a managed pointer (type &amp;). 
        /// For procedures that take a variable-length argument list, the ldarga instructions can be used only 
        /// for the initial fixed arguments, not those in the variable part of the signature. 
        /// [Rationale: ldarga is used for byref parameter passing (see Partition I). In other cases, ldarg and 
        /// starg should be used. end rationale]
        /// </remarks>
        public static void Ldarga(int argNum) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push num of type int32 onto the stack as int32.
        /// 
        /// Stack Transition:
        /// ... -> ..., num
        /// </summary>
        /// <remarks>
        /// The ldc num instruction pushes number num or some constant onto the stack. There are special 
        /// short encodings for the integers –128 through 127 (with especially short encodings for –1 
        /// through 8). All short encodings push 4-byte integers on the stack. Longer encodings are used for 
        /// 8-byte integers and 4- and 8-byte floating-point numbers, as well as 4-byte values that do not fit 
        /// in the short forms. 
        /// There are three ways to push an 8-byte integer constant onto the stack 
        /// 4. For constants that shall be expressed in more than 32 bits, use the ldc.i8 instruction. 
        /// 5. For constants that require 9–32 bits, use the ldc.i4 instruction followed by a 
        /// conv.i8. 
        /// 6. For constants that can be expressed in 8 or fewer bits, use a short form instruction 
        /// followed by a conv.i8.
        /// There is no way to express a floating-point constant that has a larger range or greater precision 
        /// than a 64-bit IEC 60559:1989 number, since these representations are not portable across 
        /// architectures. 
        /// </remarks>
        public static void Ldc_I4(int num) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push num of type int64 onto the stack as int64.
        /// 
        /// Stack Transition:
        /// ... -> ..., num
        /// </summary>
        /// <remarks>
        /// The ldc num instruction pushes number num or some constant onto the stack. There are special 
        /// short encodings for the integers –128 through 127 (with especially short encodings for –1 
        /// through 8). All short encodings push 4-byte integers on the stack. Longer encodings are used for 
        /// 8-byte integers and 4- and 8-byte floating-point numbers, as well as 4-byte values that do not fit 
        /// in the short forms. 
        /// There are three ways to push an 8-byte integer constant onto the stack 
        /// 4. For constants that shall be expressed in more than 32 bits, use the ldc.i8 instruction. 
        /// 5. For constants that require 9–32 bits, use the ldc.i4 instruction followed by a 
        /// conv.i8. 
        /// 6. For constants that can be expressed in 8 or fewer bits, use a short form instruction 
        /// followed by a conv.i8.
        /// There is no way to express a floating-point constant that has a larger range or greater precision 
        /// than a 64-bit IEC 60559:1989 number, since these representations are not portable across 
        /// architectures. 
        /// </remarks>
        public static void Ldc_I8(long num) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push num of type float32 onto the stack as float32.
        /// 
        /// Stack Transition:
        /// ... -> ..., num
        /// </summary>
        /// <remarks>
        /// The ldc num instruction pushes number num or some constant onto the stack. There are special 
        /// short encodings for the integers –128 through 127 (with especially short encodings for –1 
        /// through 8). All short encodings push 4-byte integers on the stack. Longer encodings are used for 
        /// 8-byte integers and 4- and 8-byte floating-point numbers, as well as 4-byte values that do not fit 
        /// in the short forms. 
        /// There are three ways to push an 8-byte integer constant onto the stack 
        /// 4. For constants that shall be expressed in more than 32 bits, use the ldc.i8 instruction. 
        /// 5. For constants that require 9–32 bits, use the ldc.i4 instruction followed by a 
        /// conv.i8. 
        /// 6. For constants that can be expressed in 8 or fewer bits, use a short form instruction 
        /// followed by a conv.i8.
        /// There is no way to express a floating-point constant that has a larger range or greater precision 
        /// than a 64-bit IEC 60559:1989 number, since these representations are not portable across 
        /// architectures. 
        /// </remarks>
        public static void Ldc_R4(float num) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push num of type float64 onto the stack as float64.
        /// 
        /// Stack Transition:
        /// ... -> ..., num
        /// </summary>
        /// <remarks>
        /// The ldc num instruction pushes number num or some constant onto the stack. There are special 
        /// short encodings for the integers –128 through 127 (with especially short encodings for –1 
        /// through 8). All short encodings push 4-byte integers on the stack. Longer encodings are used for 
        /// 8-byte integers and 4- and 8-byte floating-point numbers, as well as 4-byte values that do not fit 
        /// in the short forms. 
        /// There are three ways to push an 8-byte integer constant onto the stack 
        /// 4. For constants that shall be expressed in more than 32 bits, use the ldc.i8 instruction. 
        /// 5. For constants that require 9–32 bits, use the ldc.i4 instruction followed by a 
        /// conv.i8. 
        /// 6. For constants that can be expressed in 8 or fewer bits, use a short form instruction 
        /// followed by a conv.i8.
        /// There is no way to express a floating-point constant that has a larger range or greater precision 
        /// than a 64-bit IEC 60559:1989 number, since these representations are not portable across 
        /// architectures. 
        /// </remarks>
        public static void Ldc_R8(double num) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Load the element at index onto the top of the stack.
        /// 
        /// Stack Transition:
        /// ..., array, index -> ..., value
        /// </summary>
        /// <remarks>
        /// The ldelem instruction loads the value of the element with index index (of type native int or
        /// int32) in the zero-based one-dimensional array array, and places it on the top of the stack. The
        /// type of the return value is indicated by the type token typeTok in the instruction.
        /// If required elements are converted to the representation of their intermediate type (§I.8.7) when
        /// loaded onto the stack (§III.1.1.1).
        /// [Note: that is elements that are smaller than 4 bytes, a boolean or a character are converted to 4
        /// bytes by sign or zero-extension as appropriate. Floating-point values are converted to their native
        /// size (type F). end note]
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Ldelem<T>() { throw new Exception("CilTK Rewriter not run."); }

        public static void Ldelem_I() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_I1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_I2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_I4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_I8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_R4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_R8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_Ref() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_U1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_U2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelem_U4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldelema() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldfld() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldflda() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push a pointer to a method referenced by method, on the stack
        /// 
        /// Stack Transition:
        /// ... -> ..., ftn
        /// </summary>
        /// <remarks>
        /// The ldftn instruction pushes a method pointer (§II.14.5) to the native code implementing the 
        /// method described by method (a metadata token, either a methoddef or methodref (see 
        /// Partition II)), or to some other implementation-specific description of method (see Note) onto the 
        /// stack). The value pushed can be called using the calli instruction if it references a managed 
        /// method (or a stub that transitions from managed to unmanaged code). It may also be used to 
        /// construct a delegate, stored in a variable, etc. 
        /// The CLI resolves the method pointer according to the rules specified in §I.12.4.1.3 (Computed 
        /// destinations), except that the destination is computed with respect to the class specified by 
        /// method. 
        /// The value returned points to native code (see Note) using the calling convention specified by 
        /// method. Thus a method pointer can be passed to unmanaged native code (e.g., as a callback 
        /// routine). Note that the address computed by this instruction can be to a thunk produced specially 
        /// for this purpose (for example, to re-enter the CIL interpreter when a native version of the method 
        /// isn’t available).
        /// [Note: There are many options for implementing this instruction. Conceptually, this instruction 
        /// places on the virtual machine’s evaluation stack a representation of the address of the method 
        /// specified. In terms of native code this can be an address (as specified), a data structure that 
        /// contains the address, or any value that can be used to compute the address, depending on the 
        /// architecture of the underlying machine, the native calling conventions, and the implementation 
        /// technology of the VES (JIT, interpreter, threaded code, etc.). end note] 
        /// </remarks>
        public static void Ldftn(System.Reflection.MethodInfo method) { throw new Exception("CilTK Rewriter not run."); }

        public static void Ldind_I() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_I1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_I2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_I4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_I8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_R4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_R8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_Ref() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_U1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_U2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldind_U4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldlen() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Load local variable of index indx onto stack.
        ///
        /// Stack Transition:
        /// ... -> ..., value
        /// </summary>
        /// <remarks>
        /// The ldloc indx instruction pushes the contents of the local variable number indx onto the 
        /// evaluation stack, where local variables are numbered 0 onwards. Local variables are initialized to 
        /// 0 before entering the method only if the localsinit on the method is true (see Partition I). The 
        /// ldloc.0, ldloc.1, ldloc.2, and ldloc.3 instructions provide an efficient encoding for accessing the 
        /// first 4 local variables. The ldloc.s instruction provides an efficient encoding for accessing local 
        /// variables 4–255.
        /// The type of the value on the stack is tracked by verification as the intermediate type (§I.8.7) of 
        /// the local variable type, which is specified in the method header. See Partition I. 
        /// If required, local variables are converted to the representation of their intermediate type (§I.8.7) 
        /// when loaded onto the stack (§III.1.1.1) 
        /// [Note: that is local variables smaller than 4 bytes, a boolean or a character are converted to 4 
        /// bytes by sign or zero-extension as appropriate. Floating-point values are converted to their native 
        /// size (type F). end note] 
        /// </remarks>
        public static void Ldloc(int indx) { throw new Exception("CilTK Rewriter not run."); }

        public static void Ldloca(int indx) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push a null reference on the stack.
        /// 
        /// Stack Transition:
        /// ... -> ..., null value
        /// </summary>
        /// <remarks>
        /// The ldnull pushes a null reference (type O) on the stack. This is used to initialize locations before
        /// they become live or when they become dead.
        /// [Rationale: It might be thought that ldnull is redundant: why not use ldc.i4.0 or ldc.i8.0 instead?
        /// The answer is that ldnull provides a size-agnostic null – analogous to an ldc.i instruction, which
        /// does not exist. However, even if CIL were to include an ldc.i instruction it would still benefit
        /// verification algorithms to retain the ldnull instruction because it makes type tracking easier. end
        /// rationale]
        /// </remarks>
        public static void Ldnull() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Copy the value stored at address src to the stack.
        /// 
        /// Stack Transition:
        /// ..., src -> ..., val
        /// </summary>
        /// <remarks>
        /// The ldobj instruction copies a value to the evaluation stack. typeTok is a metadata token (a
        /// typedef, typeref, or typespec). src is an unmanaged pointer (native int), or a managed
        /// pointer (&amp;). If typeTok is not a generic parameter and either a reference type or a built-in value
        /// class, then the ldind instruction provides a shorthand for the ldobj instruction..
        /// [Rationale: The ldobj instruction can be used to pass a value type as an argument. end rationale]
        /// If required values are converted to the representation of the intermediate type (§I.8.7) of typeTok
        /// when loaded onto the stack (§III.1.1.1).
        /// [Note: That is integer values of less than 4 bytes, a boolean or a character are converted to 4
        /// bytes by sign or zero-extension as appropriate. Floating-point values are converted to F type. end
        /// note]
        /// The operation of the ldobj instruction can be altered by an immediately preceding volatile. or
        /// unaligned. prefix instruction.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Ldobj<T>() { throw new Exception("CilTK Rewriter not run."); }

        public static void Ldsfld() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldsflda() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push a string object for the literal str.
        /// 
        /// Stack Transition:
        /// ... -> ..., str
        /// </summary>
        /// <remarks>
        /// The ldstr instruction pushes a new string object representing the literal stored in the metadata as
        /// string (which is a string literal).
        /// By default, the CLI guarantees that the result of two ldstr instructions referring to two metadata
        /// tokens that have the same sequence of characters, return precisely the same string object (a
        /// process known as “string interning”). This behavior can be controlled using the
        /// System.Runtime.CompilerServices. CompilationRelaxationsAttribute and the
        /// System.Runtime.CompilerServices. CompilationRelaxations.NoStringInterning (see
        /// Partition IV).
        /// </remarks>
        /// <param name="str">The literal string to load.</param>
        public static void Ldstr(string str) { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldtoken() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldvirtftn() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Exit a protected region of code.
        /// 
        /// Stack Transition:
        /// ..., -> 
        /// </summary>
        /// <remarks>
        /// The leave instruction unconditionally transfers control to target. target is represented as a signed
        /// offset (4 bytes for leave, 1 byte for leave.s) from the beginning of the instruction following the
        /// current instruction.
        /// The leave instruction is similar to the br instruction, but the former can be used to exit a try,
        /// filter, or catch block whereas the ordinary branch instructions can only be used in such a
        /// block to transfer control within it. The leave instruction empties the evaluation stack and ensures
        /// that the appropriate surrounding finally blocks are executed.
        /// It is not valid to use a leave instruction to exit a finally block. To ease code generation for
        /// exception handlers it is valid from within a catch block to use a leave instruction to transfer
        /// control to any instruction within the associated try block.
        /// The leave instruction can be used to exit multiple nested blocks (see Partition I).
        /// If an instruction has one or more prefix codes, control can only be transferred to the first of these
        /// prefixes.
        /// </remarks>
        /// <param name="label">The label to branch to.</param>
        public static void Leave(string label) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Allocate space from the local memory pool.
        /// 
        /// Stack Transition:
        /// ..., size -> ..., address
        /// </summary>
        /// <remarks>
        /// The localloc instruction allocates size (type native unsigned int or U4) bytes from the local
        /// dynamic memory pool and returns the address (an unmanaged pointer, type native int) of the first
        /// allocated byte. If the localsinit flag on the method is true, the block of memory returned is
        /// initialized to 0; otherwise, the initial value of that block of memory is unspecified. The area of
        /// memory is newly allocated. When the current method returns, the local memory pool is available
        /// for reuse.
        /// address is aligned so that any built-in data type can be stored there using the stind instructions
        /// and loaded using the ldind instructions.
        /// The localloc instruction cannot occur within an exception block: filter, catch, finally, or
        /// fault.
        /// [Rationale: localloc is used to create local aggregates whose size shall be computed at runtime.
        /// It can be used for C’s intrinsic alloca method. end rationale]
        /// </remarks>
        public static void Localloc() { throw new Exception("CilTK Rewriter not run."); }

        public static void Mkrefany() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mul() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mul_Ovf() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mul_Ovf_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Neg() { throw new Exception("CilTK Rewriter not run."); }
        public static void Newarr() { throw new Exception("CilTK Rewriter not run."); }
        public static void Newobj() { throw new Exception("CilTK Rewriter not run."); }
        public static void No() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Do nothing.
        /// 
        /// Stack Transition:
        /// ..., -> ...,
        /// </summary>
        /// <remarks>
        /// The nop instruction does nothing. It is intended to fill in space if bytecodes are patched.
        /// </remarks>
        public static void Nop() { throw new Exception("CilTK Rewriter not run."); }

        public static void Not() { throw new Exception("CilTK Rewriter not run."); }
        public static void Or() { throw new Exception("CilTK Rewriter not run."); }
        public static void Pop() { throw new Exception("CilTK Rewriter not run."); }
        public static void Readonly() { throw new Exception("CilTK Rewriter not run."); }
        public static void Refanytype() { throw new Exception("CilTK Rewriter not run."); }
        public static void Refanyval() { throw new Exception("CilTK Rewriter not run."); }
        public static void Rem() { throw new Exception("CilTK Rewriter not run."); }
        public static void Rem_Un() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Return from method, possibly with a value.
        /// 
        /// Stack Transition:
        ///  retVal on callee evaluation stack (not always present) ->
        ///  ..., retVal on caller evaluation stack (not always present) 
        /// </summary>
        /// <remarks>
        /// Return from the current method. The return type, if any, of the current method determines the 
        /// type of value to be fetched from the top of the stack and copied onto the stack of the method that 
        /// called the current method. The evaluation stack for the current method shall be empty except for 
        /// the value to be returned. 
        /// The ret instruction cannot be used to transfer control out of a try, filter, catch, or finally
        /// block. From within a try or catch, use the leave instruction with a destination of a ret
        /// instruction that is outside all enclosing exception blocks. Because the filter and finally blocks 
        /// are logically part of exception handling, not the method in which their code is embedded, 
        /// correctly generated CIL does not perform a method return from within a filter or finally. See 
        /// Partition I. 
        /// </remarks>
        public static void Ret() { throw new Exception("CilTK Rewriter not run."); }

        public static void Rethrow() { throw new Exception("CilTK Rewriter not run."); }
        public static void Shl() { throw new Exception("CilTK Rewriter not run."); }
        public static void Shr() { throw new Exception("CilTK Rewriter not run."); }
        public static void Shr_Un() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Push the size, in bytes, of a type as an unsigned int32.
        /// 
        /// Stack Transition:
        /// ..., -> ..., size (4 bytes, unsigned)
        /// </summary>
        /// <remarks>
        /// Returns the size, in bytes, of a type. typeTok can be a generic parameter, a reference type or a
        /// value type.
        /// For a reference type, the size returned is the size of a reference value of the corresponding type,
        /// not the size of the data stored in objects referred to by a reference value.
        /// [Rationale: The definition of a value type can change between the time the CIL is generated and
        /// the time that it is loaded for execution. Thus, the size of the type is not always known when the
        /// CIL is generated. The sizeof instruction allows CIL code to determine the size at runtime
        /// without the need to call into the Framework class library. The computation can occur entirely at
        /// runtime or at CIL-to-native-code compilation time. sizeof returns the total size that would be
        /// occupied by each element in an array of this type – including any padding the implementation
        /// chooses to add. Specifically, array elements lie sizeof bytes apart. end rationale]
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Sizeof<T>() { throw new Exception("CilTK Rewriter not run."); }

        public static void Starg(int num) { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_Any() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_I() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_I1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_I2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_I4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_I8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_R4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_R8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stelem_Ref() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stfld() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_I() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_I1() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_I2() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_I4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_I8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_R4() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_R8() { throw new Exception("CilTK Rewriter not run."); }
        public static void Stind_Ref() { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Pop a value from stack into local variable indx
        /// 
        /// Stack Transition:
        ///  ..., value -> ...
        /// </summary>
        /// <remarks>
        /// The stloc indx instruction pops the top value off the evaluation stack and moves it into local 
        /// variable number indx (see Partition I), where local variables are numbered 0 onwards. The type 
        /// of value shall match the type of the local variable as specified in the current method’s locals 
        /// signature. The stloc.0, stloc.1, stloc.2, and stloc.3 instructions provide an efficient encoding 
        /// for the first 4 local variables; the stloc.s instruction provides an efficient encoding for local 
        /// variables 4–255.
        /// Storing into locals that hold a value smaller than 4 bytes long truncates the value as it moves 
        /// from the stack to the local variable. Floating-point values are rounded from their native size 
        /// (type F) to the size associated with the argument. (See §III.1.1.1, Numeric data types.)
        /// </remarks>
        public static void Stloc(int indx) { throw new Exception("CilTK Rewriter not run."); }

        /// <summary>
        /// Store a value of type typeTok at an address.
        /// 
        /// Stack Transition:
        /// ..., dest, src -> ...
        /// </summary>
        /// <remarks>
        /// The stobj instruction copies the value src to the address dest. If typeTok is not a generic
        /// parameter and either a reference type or a built-in value class, then the stind instruction provides
        /// a shorthand for the stobj instruction.
        /// Storing values smaller than 4 bytes truncates the value as it moves from the stack to memory.
        /// Floating-point values are rounded from their native size (type F) to the size associated with
        /// typeTok. (See §III.1.1.1, Numeric data types.)
        /// The operation of the stobj instruction can be altered by an immediately preceding volatile. or
        /// unaligned. prefix instruction.
        /// </remarks>
        /// <typeparam name="T">typeTok</typeparam>
        public static void Stobj<T>() { throw new Exception("CilTK Rewriter not run."); }

        public static void Stsfld() { throw new Exception("CilTK Rewriter not run."); }
        public static void Sub() { throw new Exception("CilTK Rewriter not run."); }
        public static void Sub_Ovf() { throw new Exception("CilTK Rewriter not run."); }
        public static void Sub_Ovf_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Switch() { throw new Exception("CilTK Rewriter not run."); }
        public static void Tail() { throw new Exception("CilTK Rewriter not run."); }
        public static void Throw() { throw new Exception("CilTK Rewriter not run."); }
        public static void Unaligned() { throw new Exception("CilTK Rewriter not run."); }
        public static void Unbox() { throw new Exception("CilTK Rewriter not run."); }
        public static void Unbox_Any() { throw new Exception("CilTK Rewriter not run."); }
        public static void Volatile() { throw new Exception("CilTK Rewriter not run."); }
        public static void Xor() { throw new Exception("CilTK Rewriter not run."); }
    }
}
