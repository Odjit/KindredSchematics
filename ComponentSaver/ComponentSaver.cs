using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;



// ComponentsaveDataer Attribute
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ComponentTypeAttribute : Attribute
{
    public Type Component { get; }
    public ComponentTypeAttribute(Type component)
    {
        Component = component;
    }
}

abstract class ComponentSaver
{
    static readonly Dictionary<int, ComponentSaver> componentSavers = [];
    static readonly Dictionary<string, ComponentSaver> componentSaversByName = [];
    static readonly Dictionary<string, Type> componentTypes = [];

    public static void PopulateComponentSavers()
    {
        componentSavers.Clear();
        var types = Assembly.GetAssembly(typeof(ComponentSaver)).GetTypes().Where(t => t.IsSubclassOf(typeof(ComponentSaver)));
        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<ComponentTypeAttribute>();
            if (attr != null)
            {
                var componentId = new ComponentType(Il2CppType.From(attr.Component)).TypeIndex;
                var componentSaver = (ComponentSaver)Activator.CreateInstance(type);
                componentSavers[componentId] = componentSaver;
                componentSaversByName[attr.Component.Name] = componentSaver;
            }
        }
    }

    public static ComponentSaver GetComponentSaver(int componentId)
    {
        if (componentSavers.TryGetValue(componentId, out var componentSaver))
        {
            return componentSaver;
        }
        return null;
    }

    public static ComponentSaver GetComponentSaver(string componentName)
    {
        if (componentSaversByName.TryGetValue(componentName, out var componentSaver))
        {
            return componentSaver;
        }
        return null;
    }

    public abstract object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper);

    public abstract object SaveComponent(Entity entity, EntityMapper entityMapper);
    public abstract void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded);

    public virtual int[] GetDependencies(JsonElement data)
    {
        return [];
    }

    public static void ApplyComponentData(Entity entity, ComponentData[] additions, Entity[] entitiesBeingLoaded)
    {
        if(additions == null)
            return;
        foreach (var addition in additions)
        {
            var componentSaver = GetComponentSaver(addition.component);
            if (componentSaver != null)
            {
                componentSaver.ApplyComponentData(entity, (JsonElement)addition.data, entitiesBeingLoaded);
            }
        }
    }

    public static void ApplyRemovals(Entity entity, int[] removals)
    {
        if(removals == null)
            return;
        foreach (var removal in removals)
        {
            var ct = new ComponentType();
            ct.TypeIndex = removal;
            if(Core.EntityManager.HasComponent(entity, ct))
                Core.EntityManager.RemoveComponent(entity, ct);
        }
    }

    public static int[] GetDependencies(ComponentData[] additions)
    {
        if (additions == null)
            return [];

        var dependencies = new List<int>();
        foreach (var addition in additions)
        {
            var componentSaver = GetComponentSaver(addition.component);
            if (componentSaver != null)
            {
                var componentDependencies = componentSaver.GetDependencies((JsonElement)addition.data);
                if (componentDependencies.Any(x => x==0))
                    Core.Log.LogError($"Component {addition.component} has a dependency on 0");
                dependencies.AddRange(componentDependencies.Where(x => x!=0));
            }
        }
        return dependencies.ToArray();
    }
}