using System.Linq;
using System.Reflection;
using Crucible.Core.Random;
using Crucible.Core.State;
using NUnit.Framework;

namespace Crucible.Tests.Core
{
    public class CoreAssemblyConstraintsTest
    {
        [Test]
        public void CoreAssembly_ReferencedAssemblies_ExcludeUnityEngine()
        {
            var coreAssembly = typeof(Pcg32).Assembly;

            var unityReferences = coreAssembly.GetReferencedAssemblies()
                .Where(name => name.Name.StartsWith("UnityEngine"))
                .Select(name => name.Name)
                .ToArray();

            Assert.That(unityReferences, Is.Empty);
        }

        [Test]
        public void CoreAssembly_StateTypes_HaveNoFloatingPointFields()
        {
            const BindingFlags all = BindingFlags.Instance | BindingFlags.Static |
                                     BindingFlags.Public | BindingFlags.NonPublic;
            var stateTypes = typeof(RunState).Assembly.GetTypes()
                .Where(type => type.Namespace == typeof(RunState).Namespace)
                .ToArray();

            var floatingFields = stateTypes
                .SelectMany(type => type.GetFields(all).Select(field => (type, field)))
                .Where(pair => pair.field.FieldType == typeof(float) || pair.field.FieldType == typeof(double))
                .Select(pair => $"{pair.type.Name}.{pair.field.Name}")
                .ToArray();

            Assert.That(stateTypes, Is.Not.Empty);
            Assert.That(floatingFields, Is.Empty);
        }
    }
}
