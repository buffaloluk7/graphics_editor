using System.Windows;

public interface IComponent
{
	void Add(IComponent Component);

	void Remove(IComponent Component);

	void Move(Vector translation);

    // resizing is only available for the bottom right corner, therefore we can describe it with a vector
    void Resize(Vector translation);
}