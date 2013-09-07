using System;
using System.Collections.Generic;
using System.Text;

namespace Silk
{
    public static unsafe class Cil
    {
        public static void Label(string label) { throw new Exception("CilTK Rewriter not run."); }
        
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
        public static void Beq(string label) { throw new Exception("CilTK Rewriter not run."); }

        public static void Bge(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Bge_Un(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Bgt(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Bgt_Un(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Ble(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Ble_Un(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Blt(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Blt_Un(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Bne_Un(string label) { throw new Exception("CilTK Rewriter not run."); }

        public static void Box() { throw new Exception("CilTK Rewriter not run."); }
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

        public static void Brfalse(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Brtrue(string label) { throw new Exception("CilTK Rewriter not run."); }

        public static void Call<T>(T method) { throw new Exception("CilTK Rewriter not run."); }

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

        public static void Castclass() { throw new Exception("CilTK Rewriter not run."); }

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


        public static void Cpblk() { throw new Exception("CilTK Rewriter not run."); }
        public static void Cpobj() { throw new Exception("CilTK Rewriter not run."); }


        public static void Div() { throw new Exception("CilTK Rewriter not run."); }

        public static void Div_Un() { throw new Exception("CilTK Rewriter not run."); }


        public static void Dup() { throw new Exception("CilTK Rewriter not run."); }
        public static void Endfilter() { throw new Exception("CilTK Rewriter not run."); }
        public static void Endfinally() { throw new Exception("CilTK Rewriter not run."); }
        public static void Initblk() { throw new Exception("CilTK Rewriter not run."); }
        public static void Initobj() { throw new Exception("CilTK Rewriter not run."); }
        public static void Isinst() { throw new Exception("CilTK Rewriter not run."); }


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

        public static void Ldelem_Any() { throw new Exception("CilTK Rewriter not run."); }
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
        public static void Ldnull() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldobj() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldsfld() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldsflda() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldstr() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldtoken() { throw new Exception("CilTK Rewriter not run."); }
        public static void Ldvirtftn() { throw new Exception("CilTK Rewriter not run."); }
        public static void Leave(string label) { throw new Exception("CilTK Rewriter not run."); }
        public static void Localloc() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mkrefany() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mul() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mul_Ovf() { throw new Exception("CilTK Rewriter not run."); }
        public static void Mul_Ovf_Un() { throw new Exception("CilTK Rewriter not run."); }
        public static void Neg() { throw new Exception("CilTK Rewriter not run."); }
        public static void Newarr() { throw new Exception("CilTK Rewriter not run."); }
        public static void Newobj() { throw new Exception("CilTK Rewriter not run."); }
        public static void No() { throw new Exception("CilTK Rewriter not run."); }
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
        public static void Sizeof() { throw new Exception("CilTK Rewriter not run."); }
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

        public static void Stobj() { throw new Exception("CilTK Rewriter not run."); }
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
