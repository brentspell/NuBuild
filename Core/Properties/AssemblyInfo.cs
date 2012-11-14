//===========================================================================
// MODULE:  AssemblyInfo.cs
// PURPOSE: assembly configuration properties
// 
// Copyright © 2012
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
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// Project References

[assembly:Guid("DF1AC33F-7D8D-473C-AD88-2CA4EDAD3B70")]
[assembly:AssemblyProduct("NuGet Project System")]
[assembly:AssemblyTitle("NuGet Project System Core Library")]
[assembly:AssemblyCompany("Brent M. Spell")]
[assembly:AssemblyCopyright("Copyright © 2012 Brent M. Spell. All Rights Reserved.")]
[assembly:CLSCompliant(false)]
[assembly:ComVisible(false)]
#if DEBUG
[assembly:AssemblyConfiguration("Debug")]
#else
[assembly:AssemblyConfiguration("Release")]
#endif
