//===========================================================================
// MODULE:  EnumDescriptionTypeConverter.cs
// PURPOSE: DescriptionAttribute-based type converter for enums
// 
// Copyright © 2013
// Brent M. Spell. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 3 of the License, or 
// (at your option) any later version. This library is distributed in the 
// hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU Lesser General Public License for more details. You should 
// have received a copy of the GNU Lesser General Public License along with 
// this library; if not, write to 
//    Free Software Foundation, Inc. 
//    51 Franklin Street, Fifth Floor 
//    Boston, MA 02110-1301 USA
//===========================================================================
// System References
#region using Directives
// System References
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
// Project References
#endregion

namespace NuBuild.VS
{
   /// <summary>
   /// DescriptionAttribute-based enum type converter
   /// </summary>
   /// <remarks>
   /// This class supports converting enum types to the
   /// corresponding DescriptionAttribute value for
   /// string data binding.
   /// </remarks>
   /// <typeparam name="EnumType"></typeparam>
   public class EnumDescriptionTypeConverter<EnumType> : EnumConverter where EnumType : struct
   {
      /// <summary>
      /// Initializes a new type converter instance
      /// </summary>
      public EnumDescriptionTypeConverter ()
         : base(typeof(EnumType))
      {
      }
      /// <summary>
      /// Converts an enum value to string
      /// </summary>
      /// <param name="context">
      /// The type descriptor context
      /// </param>
      /// <param name="culture">
      /// The culture to convert to
      /// </param>
      /// <param name="value">
      /// The enum value to convert
      /// </param>
      /// <param name="destinationType">
      /// The requested type to convert to
      /// </param>
      /// <returns>
      /// The converted value
      /// </returns>
      public override object ConvertTo (
         ITypeDescriptorContext context,
         System.Globalization.CultureInfo culture,
         Object value,
         Type destinationType)
      {
         if (destinationType == typeof(String))
         {
            // convert the enumeration value to a description
            // . first, attempt to retrieve the custom DescriptionAttribute
            //   for the enumeration value
            // . then, attempt to retrieve the symbolic name for the enum
            // . finally, fall back on the numeric value if the value does
            //   not exist within the enumeration itself
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field != null)
               return field
                  .GetCustomAttributes(true)
                  .OfType<DescriptionAttribute>()
                  .Select(a => a.Description)
                  .Where(d => !String.IsNullOrWhiteSpace(d))
                  .DefaultIfEmpty(field.Name)
                  .First();
            return value.ToString();
         }
         return base.ConvertTo(context, culture, value, destinationType);
      }
      /// <summary>
      /// Converts a string description to an enum value
      /// </summary>
      /// <param name="context">
      /// The type descriptor context
      /// </param>
      /// <param name="culture">
      /// The culture to convert to
      /// </param>
      /// <param name="value">
      /// The enum description to convert
      /// </param>
      /// <returns>
      /// The enumeration value corresponding to the description
      /// </returns>
      public override Object ConvertFrom (
         ITypeDescriptorContext context, 
         System.Globalization.CultureInfo culture, 
         Object value)
      {
         if (value.GetType() == typeof(String))
         {
            // convert the enumeration value from a description
            // . first, attempt to retrieve the custom DescriptionAttribute
            //   for the enumeration value
            // . then, attempt to retrieve the enum matching the description
            foreach (var field in base.EnumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
               var attr = field.GetCustomAttributes(true)
                  .OfType<DescriptionAttribute>()
                  .FirstOrDefault(a => String.Compare(a.Description, (String)value, true) == 0);
               if (attr != null)
                  return Enum.ToObject(base.EnumType, field.GetRawConstantValue());
            }
         }
         return base.ConvertFrom(context, culture, value);
      }
   }
}
