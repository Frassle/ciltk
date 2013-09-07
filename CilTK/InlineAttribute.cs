using System;
using System.Collections.Generic;
using System.Text;

namespace Silk
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class InlineAttribute : Attribute
    {
        public InlineAttribute()
        {
        }
    }
}
