using System;
using System.Collections.Generic;
using System.Linq;


namespace KCEngine.Instance;

public abstract class Instance
{
    public string Name { get; set; }
    
    private Instance? _parent;
    
    public bool IsRoot => _parent == null;
    
    public Instance Root
    {
        get
        {
            if (IsRoot)
            { return this; }

            Instance current = this;
            while (current._parent != null)
            { current = current._parent; }

            return current;
        }
    }
    
    public Instance? Parent
    {
        get => _parent;
        set
        {
            if (value == null)
            { throw new ArgumentNullException(nameof(value), "Parent cannot be set to null. Use IsRoot property to check if an instance is a root."); }

            if (value == _parent)
            { return; }

            if (value.IsDescendantOf(this))
            { throw new InvalidOperationException("Cannot set Parent to a descendant of this instance."); }

            _parent?._children.Remove(this);
            _parent = value;
            value._children.Add(this);
        }
    }
    
    public Instance(string name, Instance parent)
    {
        Name = name;

        if (parent.IsDescendantOf(this))
        { throw new InvalidOperationException("Cannot set Parent to a descendant of this instance."); }

        Parent = parent;
    }
    
    private List<Instance> _children = [];
    
    public IReadOnlyList<Instance> Children => _children.AsReadOnly();
    
    public uint UniqueId { get; init; }
    
    public List<string> Tags = [];
    
    public void ClearAllChildren()
    {
        foreach (Instance child in Children)
        {
            child.Destroy();
        }
    }
    
    protected virtual void CopyPropertiesFrom(Instance source)
    {
        Name = source.Name;
        Parent = source.Parent;
        Tags = new List<string>(source.Tags);
    }
    
    public Instance Clone()
    {
        Instance clone = (Activator.CreateInstance(GetType()) as Instance)!;

        clone.CopyPropertiesFrom(this);

        foreach (Instance child in Children)
        {
            Instance childClone = child.Clone();

            childClone.Parent = clone;
        }

        return clone;
    }
    
    public void Destroy()
    {
        ClearAllChildren();

        Parent?._children.Remove(this);
    }
    
    public Instance? FindFirstAncestor(string name)
    {
        Instance current = this;

        while (!current.IsRoot)
        {
            if (current.Name == name)
            { return current; }

            current = current.Parent!;
        }

        return null;
    }
    
    public Instance? FindFirstAncestorOfClass<T>() where T : Instance
    {
        Instance current = this;

        while (!current.IsRoot)
        {
            if (current.GetType() == typeof(T))
            { return current; }

            current = current.Parent!;
        }

        return null;
    }
    
    public Instance? FindFirstAncestorWhichIsA<T>() where T : Instance
    {
        Instance current = this;

        while (!current.IsRoot)
        {
            if (current is T)
            { return current; }

            current = current.Parent!;
        }

        return null;
    }
    
    public Instance? FindFirstChild(string name)
    { return FindFirstChild(name, false); }
    
    public Instance? FindFirstChild(string name, bool recursive)
    {
        foreach (Instance child in Children)
        {
            if (child.Name == name)
            { return child; }

            if (!recursive)
            { continue; }

            Instance? descendant = child.FindFirstChild(name, true);
            if (descendant != null)
            {  return descendant; }
        }

        return null;
    }
    
    public Instance? FindFirstChildOfClass<T>() where T : Instance
    { return FindFirstChildOfClass<T>(false); }
    
    public Instance? FindFirstChildOfClass<T>(bool recursive) where T : Instance
    {
        foreach (Instance child in Children)
        {
            if (child.GetType() == typeof(T))
            { return child; }

            if (!recursive)
            { continue; }

            Instance? descendant = child.FindFirstChildOfClass<T>(true);
            if (descendant != null)
            { return descendant; }
        }

        return null;
    }
    
    public Instance? FindFirstChildWhichIsA<T>() where T : Instance
    { return FindFirstChildWhichIsA<T>(false); }
    
    public Instance? FindFirstChildWhichIsA<T>(bool recursive) where T : Instance
    {
        foreach (Instance child in Children)
        {
            if (child is T)
            { return child; }

            if (!recursive)
            { continue; }

            Instance? descendant = child.FindFirstChildWhichIsA<T>(true);
            if (descendant != null)
            { return descendant; }
        }

        return null;
    }
    
    public Instance? FindFirstDescendant(string name)
    { return FindFirstChild(name, true); }
    
    public IReadOnlyList<Instance> GetChildren()
    { return Children; }
    
    public IReadOnlyList<Instance> GetDescendants()
    {
        List<Instance> descendants = new List<Instance>(Children);

        foreach (Instance child in Children)
        { descendants.AddRange(child.GetDescendants()); }

        return descendants.AsReadOnly();
    }
    
    public IReadOnlyList<Instance> GetAncestors()
    {
        List<Instance> ancestors = new List<Instance>();
        Instance current = this;

        while (!current.IsRoot)
        {
            ancestors.Add(current.Parent!);
            current = current.Parent!;
        }

        return ancestors.AsReadOnly();
    }
    
    public string GetFullName()
    { return String.Join(".", GetAncestors().Reverse().Select(a => a.Name).Append(Name)); }
    
    public IReadOnlyList<string> GetTags()
    { return Tags.AsReadOnly(); }
    
    public bool HasTag(string tag)
    { return Tags.Contains(tag); }
    
    public bool IsAncestorOf(Instance descendant)
    {
        return descendant.IsDescendantOf(this);
    }
    
    public bool IsDescendantOf(Instance ancestor)
    {
        Instance current = this;
        
        while (!current.IsRoot)
        {
            if (current.Parent == ancestor)
            { return true; }

            current = current.Parent!;
        }

        return false;
    }
    
    public void RemoveTag(string tag)
    { Tags.Remove(tag); }
}

