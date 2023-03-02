namespace StoneAssemblies.Extensibility.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using NUnit.Framework;

    using AssemblyExtensions = StoneAssemblies.Extensibility.AssemblyExtensions;

    /// <summary>
    ///     The assembly extensions facts.
    /// </summary>
    [TestFixture]
    public class AssemblyExtensionsFacts
    {
        [TestFixture]
        public class The_EnumReferences_Method
        {
            [Test]
            public void Returns_Distinc_Assemblies_When_Cross_Assembly_Cache_Is_Used()
            {
                var assemblies = new List<Assembly>();
                var cache = new HashSet<string>();
                var assemblyNames = new[]
                                        {
                                            typeof(AssemblyExtensions).Assembly.GetName(),
                                            typeof(AssemblyExtensions).Assembly.GetName()
                                        };

                foreach (var assemblyName in assemblyNames)
                {
                    assemblies.AddRange(typeof(AssemblyExtensions).Assembly.EnumReferences(cache));
                }

                var enumerable = assemblies.Distinct().ToList();
                Assert.That(assemblies, Is.EquivalentTo(enumerable));
            }

            [Test]
            public void Returns_Duplicate_Assemblies_When_Cross_Assembly_Cache_Is_Not_Used()
            {
                var assemblies = new List<Assembly>();
                var assemblyNames = new[]
                                        {
                                            typeof(AssemblyExtensions).Assembly.GetName(),
                                            typeof(AssemblyExtensions).Assembly.GetName()
                                        };
                foreach (var assemblyName in assemblyNames)
                {
                    assemblies.AddRange(typeof(AssemblyExtensions).Assembly.EnumReferences());
                }

                var enumerable = assemblies.Distinct().ToList();
                Assert.That(assemblies, !Is.EquivalentTo(enumerable));
            }
        }
    }
}