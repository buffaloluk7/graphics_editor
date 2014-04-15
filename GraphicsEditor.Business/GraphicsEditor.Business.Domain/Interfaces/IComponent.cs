using System.Windows;

public interface IComponent
{
	void Add(IComponent Component);

	void Remove(IComponent Component);

	void Move(Point location);

    void Resize(Point size);
}