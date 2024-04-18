using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;



// ComponentDiffer Attribute
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

    public static void PopulateComponentDiffers()
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

    public abstract object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper);

    public abstract void ApplyDiff(Entity entity, JsonElement diff, Entity[] entitiesBeingLoaded);

    public static void ApplyDiffs(Entity entity, ComponentData[] diffs, Entity[] entitiesBeingLoaded)
    {
        if(diffs == null)
            return;
        foreach (var diff in diffs)
        {
            var differ = GetComponentSaver(diff.component);
            if (differ != null)
            {
                differ.ApplyDiff(entity, (JsonElement)diff.data, entitiesBeingLoaded);
            }
        }
    }

    public abstract object SaveComponent(Entity entity, EntityMapper entityMapper);
    public abstract void AddComponent(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded);

    public static void ApplyAdditions(Entity entity, ComponentData[] additions, Entity[] entitiesBeingLoaded)
    {
        if(additions == null)
            return;
        foreach (var addition in additions)
        {
            var differ = GetComponentSaver(addition.component);
            if (differ != null)
            {
                differ.AddComponent(entity, (JsonElement)addition.data, entitiesBeingLoaded);
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
}