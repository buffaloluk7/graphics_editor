using System;
using System.Collections.Generic;
using System.Windows;

public class Composite : IComponent
{
    private List<IComponent> children;

    public Composite()
    {
        children = new List<IComponent>();
    }

    public virtual void Add(IComponent component)
    {
        children.Add(component);
    }

    public virtual void Remove(IComponent component)
    {
        children.Remove(component);
    }

    public void Move(Vector translation)
    {
        // maybe use parallels here
        foreach(var child in children)
        {
            child.Move(translation);
        }
    }

    // resizing is difficult, because some objects need to be moved as well
    public void Resize(Vector translation)
    {
        throw new NotImplementedException();
    }
}

