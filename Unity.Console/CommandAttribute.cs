using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.Console
{
    [AttributeUsage(AttributeTargets.All)]
    public class CommandAttribute : Attribute
    {
        public static readonly CommandAttribute Default = new CommandAttribute();

        public CommandAttribute()
        {
            this.DescriptionValue = string.Empty;
        }

        public CommandAttribute(string name)
        {
            this.DescriptionValue = name;
        }

        public override bool Equals(object obj)
        {
            return ((obj is CommandAttribute) && ((obj == this) || (((CommandAttribute)obj).Description == this.DescriptionValue)));
        }

        public override int GetHashCode()
        {
            return this.DescriptionValue.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (this == Default);
        }

        public virtual string Description => this.DescriptionValue;

        protected string DescriptionValue { get; }
    }
}
