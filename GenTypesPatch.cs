using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace OptimizerHarmony
{
    [StaticConstructorOnStartup]
    public class GenTypesPatch
    {
        static readonly List<Assembly> AllActiveAssembliesCache;
        static readonly List<Type> AllTypesCache;
        static readonly Dictionary<Type, List<Type>> AllSubclassesCache = new Dictionary<Type, List<Type>>();
        static readonly Dictionary<Type, List<Type>> AllSubclassesNonAbstractCache = new Dictionary<Type, List<Type>>();
        static readonly Dictionary<Type, List<Type>> AllLeafSubclassesCache = new Dictionary<Type, List<Type>>();
        static readonly Dictionary<Type, List<Type>> InstantiableDescendantsAndSelfCache = new Dictionary<Type, List<Type>>();

        static GenTypesPatch()
        {
            var h = Main.H;
            AllActiveAssembliesCache = GenTypes.AllActiveAssemblies.ToList();
            h.Patch(AccessTools.PropertyGetter(typeof(GenTypes), nameof(GenTypes.AllActiveAssemblies)), prefix: new HarmonyMethod(AccessTools.Method(typeof(GenTypesPatch), nameof(AllActiveAssemblies))));

            AllTypesCache = GenTypes.AllTypes.ToList();
            h.Patch(AccessTools.PropertyGetter(typeof(GenTypes), nameof(GenTypes.AllTypes)), prefix: new HarmonyMethod(AccessTools.Method(typeof(GenTypesPatch), nameof(AllTypes))));

            h.Patch(AccessTools.Method(typeof(GenTypes), nameof(GenTypes.AllSubclasses)), prefix: new HarmonyMethod(AccessTools.Method(typeof(GenTypesPatch), nameof(AllSubclasses))));
            h.Patch(AccessTools.Method(typeof(GenTypes), nameof(GenTypes.AllSubclassesNonAbstract)), prefix: new HarmonyMethod(AccessTools.Method(typeof(GenTypesPatch), nameof(AllSubclassesNonAbstract))));
            h.Patch(AccessTools.Method(typeof(GenTypes), nameof(GenTypes.AllLeafSubclasses)), prefix: new HarmonyMethod(AccessTools.Method(typeof(GenTypesPatch), nameof(AllLeafSubclasses))));
            h.Patch(AccessTools.Method(typeof(GenTypes), nameof(GenTypes.InstantiableDescendantsAndSelf)), prefix: new HarmonyMethod(AccessTools.Method(typeof(GenTypesPatch), nameof(InstantiableDescendantsAndSelf))));
        }

        static bool AllTypes(ref IEnumerable<Type> __result) {
            __result = AllTypesCache;
            return false;
        }

        static bool AllActiveAssemblies(ref IEnumerable<Assembly> __result)
		{
            __result = AllActiveAssembliesCache;
            return false;
		}

        static bool AllSubclasses(Type baseType, ref IEnumerable<Type> __result)
        {
            if (!AllSubclassesCache.TryGetValue(baseType, out var cache)) {
                cache = GenTypes.AllTypes.Where(x => x.IsSubclassOf(baseType)).ToList();
                AllSubclassesCache.Add(baseType, cache);
            }
            __result = cache; 
            return false;
        }

  //       public static IEnumerable<Type> AllTypesWithAttribute<TAttr>() where TAttr : Attribute
		// {
		// 	return Enumerable.Where<Type>(GenTypes.AllTypes, (Type x) => x.HasAttribute<TAttr>());
		// }

		

        static bool AllSubclassesNonAbstract(Type baseType, ref IEnumerable<Type> __result)
		{
            if (!AllSubclassesNonAbstractCache.TryGetValue(baseType, out var cache)) {
                cache = GenTypes.AllTypes.Where(x => x.IsSubclassOf(baseType) && !x.IsAbstract).ToList();
                AllSubclassesNonAbstractCache.Add(baseType, cache);
            }
            __result = cache; 
            return false;
		}

        static bool AllLeafSubclasses(Type baseType, ref IEnumerable<Type> __result)
        {
            if (!AllLeafSubclassesCache.TryGetValue(baseType, out var cache)) {
                cache = baseType.AllSubclasses().Where(x => !x.AllSubclasses().Any()).ToList();
                AllLeafSubclassesCache.Add(baseType, cache);
            }
            __result = cache; 
            return false;
        }

        static bool InstantiableDescendantsAndSelf(Type baseType, ref IEnumerable<Type> __result) {
            IEnumerable<Type> InstantiableDescendantsAndSelf(Type t) {
                if (!t.IsAbstract)
                    yield return t;
                foreach (Type type in t.AllSubclasses()) 
                    if (!type.IsAbstract) 
                        yield return type;
            }

            if (!InstantiableDescendantsAndSelfCache.TryGetValue(baseType, out var cache)) {
                cache = InstantiableDescendantsAndSelf(baseType).ToList();
                InstantiableDescendantsAndSelfCache.Add(baseType, cache);
            }
            __result = cache; 
            return false;
        }

		// public static Type GetTypeInAnyAssembly(string typeName, string namespaceIfAmbiguous = null)
		// {
		// 	GenTypes.TypeCacheKey key = new GenTypes.TypeCacheKey(typeName, namespaceIfAmbiguous);
		// 	Type type = null;
		// 	if (!GenTypes.typeCache.TryGetValue(key, out type))
		// 	{
		// 		type = GenTypes.GetTypeInAnyAssemblyInt(typeName, namespaceIfAmbiguous);
		// 		GenTypes.typeCache.Add(key, type);
		// 	}
		// 	return type;
		// }
		//
		// private static Type GetTypeInAnyAssemblyInt(string typeName, string namespaceIfAmbiguous = null)
		// {
		// 	Type typeInAnyAssemblyRaw = GenTypes.GetTypeInAnyAssemblyRaw(typeName);
		// 	if (typeInAnyAssemblyRaw != null)
		// 	{
		// 		return typeInAnyAssemblyRaw;
		// 	}
		// 	if (!namespaceIfAmbiguous.NullOrEmpty() && GenTypes.IgnoredNamespaceNames.Contains(namespaceIfAmbiguous))
		// 	{
		// 		typeInAnyAssemblyRaw = GenTypes.GetTypeInAnyAssemblyRaw(namespaceIfAmbiguous + "." + typeName);
		// 		if (typeInAnyAssemblyRaw != null)
		// 		{
		// 			return typeInAnyAssemblyRaw;
		// 		}
		// 	}
		// 	for (int i = 0; i < GenTypes.IgnoredNamespaceNames.Count; i++)
		// 	{
		// 		typeInAnyAssemblyRaw = GenTypes.GetTypeInAnyAssemblyRaw(GenTypes.IgnoredNamespaceNames[i] + "." + typeName);
		// 		if (typeInAnyAssemblyRaw != null)
		// 		{
		// 			return typeInAnyAssemblyRaw;
		// 		}
		// 	}
		// 	return null;
		// }
		//
		// private static Type GetTypeInAnyAssemblyRaw(string typeName)
		// {
		// 	uint num = <PrivateImplementationDetails>.ComputeStringHash(typeName);
		// 	if (num <= 2299065237u)
		// 	{
		// 		if (num <= 1092586446u)
		// 		{
		// 			if (num <= 431052896u)
		// 			{
		// 				if (num != 296283782u)
		// 				{
		// 					if (num != 398550328u)
		// 					{
		// 						if (num == 431052896u)
		// 						{
		// 							if (typeName == "byte?")
		// 							{
		// 								return typeof(byte?);
		// 							}
		// 						}
		// 					}
		// 					else if (typeName == "string")
		// 					{
		// 						return typeof(string);
		// 					}
		// 				}
		// 				else if (typeName == "char?")
		// 				{
		// 					return typeof(char?);
		// 				}
		// 			}
		// 			else if (num != 513669818u)
		// 			{
		// 				if (num != 520654156u)
		// 				{
		// 					if (num == 1092586446u)
		// 					{
		// 						if (typeName == "float?")
		// 						{
		// 							return typeof(float?);
		// 						}
		// 					}
		// 				}
		// 				else if (typeName == "decimal")
		// 				{
		// 					return typeof(decimal);
		// 				}
		// 			}
		// 			else if (typeName == "uint?")
		// 			{
		// 				return typeof(uint?);
		// 			}
		// 		}
		// 		else if (num <= 1454009365u)
		// 		{
		// 			if (num != 1189328644u)
		// 			{
		// 				if (num != 1299622921u)
		// 				{
		// 					if (num == 1454009365u)
		// 					{
		// 						if (typeName == "sbyte?")
		// 						{
		// 							return typeof(sbyte?);
		// 						}
		// 					}
		// 				}
		// 				else if (typeName == "decimal?")
		// 				{
		// 					return typeof(decimal?);
		// 				}
		// 			}
		// 			else if (typeName == "long?")
		// 			{
		// 				return typeof(long?);
		// 			}
		// 		}
		// 		else if (num <= 1630192034u)
		// 		{
		// 			if (num != 1603400371u)
		// 			{
		// 				if (num == 1630192034u)
		// 				{
		// 					if (typeName == "ushort")
		// 					{
		// 						return typeof(ushort);
		// 					}
		// 				}
		// 			}
		// 			else if (typeName == "int?")
		// 			{
		// 				return typeof(int?);
		// 			}
		// 		}
		// 		else if (num != 1683620383u)
		// 		{
		// 			if (num == 2299065237u)
		// 			{
		// 				if (typeName == "double?")
		// 				{
		// 					return typeof(double?);
		// 				}
		// 			}
		// 		}
		// 		else if (typeName == "byte")
		// 		{
		// 			return typeof(byte);
		// 		}
		// 	}
		// 	else if (num <= 2823553821u)
		// 	{
		// 		if (num <= 2515107422u)
		// 		{
		// 			if (num != 2471414311u)
		// 			{
		// 				if (num != 2508976771u)
		// 				{
		// 					if (num == 2515107422u)
		// 					{
		// 						if (typeName == "int")
		// 						{
		// 							return typeof(int);
		// 						}
		// 					}
		// 				}
		// 				else if (typeName == "ulong?")
		// 				{
		// 					return typeof(ulong?);
		// 				}
		// 			}
		// 			else if (typeName == "ushort?")
		// 			{
		// 				return typeof(ushort?);
		// 			}
		// 		}
		// 		else if (num <= 2699759368u)
		// 		{
		// 			if (num != 2667225454u)
		// 			{
		// 				if (num == 2699759368u)
		// 				{
		// 					if (typeName == "double")
		// 					{
		// 						return typeof(double);
		// 					}
		// 				}
		// 			}
		// 			else if (typeName == "ulong")
		// 			{
		// 				return typeof(ulong);
		// 			}
		// 		}
		// 		else if (num != 2797886853u)
		// 		{
		// 			if (num == 2823553821u)
		// 			{
		// 				if (typeName == "char")
		// 				{
		// 					return typeof(char);
		// 				}
		// 			}
		// 		}
		// 		else if (typeName == "float")
		// 		{
		// 			return typeof(float);
		// 		}
		// 	}
		// 	else if (num <= 3286667814u)
		// 	{
		// 		if (num != 3122818005u)
		// 		{
		// 			if (num != 3270303571u)
		// 			{
		// 				if (num == 3286667814u)
		// 				{
		// 					if (typeName == "bool?")
		// 					{
		// 						return typeof(bool?);
		// 					}
		// 				}
		// 			}
		// 			else if (typeName == "long")
		// 			{
		// 				return typeof(long);
		// 			}
		// 		}
		// 		else if (typeName == "short")
		// 		{
		// 			return typeof(short);
		// 		}
		// 	}
		// 	else if (num <= 3415750305u)
		// 	{
		// 		if (num != 3365180733u)
		// 		{
		// 			if (num == 3415750305u)
		// 			{
		// 				if (typeName == "uint")
		// 				{
		// 					return typeof(uint);
		// 				}
		// 			}
		// 		}
		// 		else if (typeName == "bool")
		// 		{
		// 			return typeof(bool);
		// 		}
		// 	}
		// 	else if (num != 3996115294u)
		// 	{
		// 		if (num == 4088464520u)
		// 		{
		// 			if (typeName == "sbyte")
		// 			{
		// 				return typeof(sbyte);
		// 			}
		// 		}
		// 	}
		// 	else if (typeName == "short?")
		// 	{
		// 		return typeof(short?);
		// 	}
		// 	foreach (Assembly assembly in GenTypes.AllActiveAssemblies)
		// 	{
		// 		Type type = assembly.GetType(typeName, false, true);
		// 		if (type != null)
		// 		{
		// 			return type;
		// 		}
		// 	}
		// 	Type type2 = Type.GetType(typeName, false, true);
		// 	if (type2 != null)
		// 	{
		// 		return type2;
		// 	}
		// 	return null;
		// }
		//
		// public static string GetTypeNameWithoutIgnoredNamespaces(Type type)
		// {
		// 	if (type.IsGenericType)
		// 	{
		// 		return type.ToString();
		// 	}
		// 	for (int i = 0; i < GenTypes.IgnoredNamespaceNames.Count; i++)
		// 	{
		// 		if (type.Namespace == GenTypes.IgnoredNamespaceNames[i])
		// 		{
		// 			return type.Name;
		// 		}
		// 	}
		// 	return type.FullName;
		// }
		//
		// public static bool IsCustomType(Type type)
		// {
		// 	string @namespace = type.Namespace;
		// 	return !@namespace.StartsWith("System") && !@namespace.StartsWith("UnityEngine") && !@namespace.StartsWith("Steamworks");
		// }
		//
		// public static readonly List<string> IgnoredNamespaceNames = new List<string>
		// {
		// 	"RimWorld",
		// 	"Verse",
		// 	"Verse.AI",
		// 	"Verse.Sound",
		// 	"Verse.Grammar",
		// 	"RimWorld.Planet",
		// 	"RimWorld.BaseGen",
		// 	"RimWorld.QuestGen",
		// 	"RimWorld.SketchGen",
		// 	"System"
		// };
		//
		// private static Dictionary<GenTypes.TypeCacheKey, Type> typeCache = new Dictionary<GenTypes.TypeCacheKey, Type>(EqualityComparer<GenTypes.TypeCacheKey>.Default);
		//
		// private struct TypeCacheKey : IEquatable<GenTypes.TypeCacheKey>
		// {
		// 	public override int GetHashCode()
		// 	{
		// 		if (this.namespaceIfAmbiguous == null)
		// 		{
		// 			return this.typeName.GetHashCode();
		// 		}
		// 		return (17 * 31 + this.typeName.GetHashCode()) * 31 + this.namespaceIfAmbiguous.GetHashCode();
		// 	}
		//
		// 	public bool Equals(GenTypes.TypeCacheKey other)
		// 	{
		// 		return string.Equals(this.typeName, other.typeName) && string.Equals(this.namespaceIfAmbiguous, other.namespaceIfAmbiguous);
		// 	}
		//
		// 	public override bool Equals(object obj)
		// 	{
		// 		return obj is GenTypes.TypeCacheKey && this.Equals((GenTypes.TypeCacheKey)obj);
		// 	}
		//
		// 	public TypeCacheKey(string typeName, string namespaceIfAmbigous = null)
		// 	{
		// 		this.typeName = typeName;
		// 		this.namespaceIfAmbiguous = namespaceIfAmbigous;
		// 	}
		//
		// 	public string typeName;
		//
		// 	public string namespaceIfAmbiguous;
		// }
    }
}