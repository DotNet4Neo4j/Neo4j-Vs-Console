/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Anabranch.Neo4JConsolePackage;

namespace Neo4JConsolePackage_UnitTests
{
    using FluentAssertions;
    using Xunit;

    public class PackageTest
    {
        [Fact]
        public void CreateInstance()
        {
            Neo4JConsolePackagePackage package = new Neo4JConsolePackagePackage();
        }

        [Fact]
        public void IsIVsPackage()
        {
            Neo4JConsolePackagePackage package = new Neo4JConsolePackagePackage();
            package.Should().NotBeNull("Package doesn't implement IVsPackage");
        }

    }
}
