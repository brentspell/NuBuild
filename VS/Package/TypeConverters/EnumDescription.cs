using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuBuild.VS
{
   public class EnumDescription
   {
      public int Key { get; private set; }
      public string Value { get; private set; }
      public string Description { get; private set; }

      public EnumDescription(int key, string value, string description)
      {
         this.Key = key;
         this.Value = value;
         this.Description = description;
      }

      public override string ToString()
      {
         return Value;
      }
   }
}
