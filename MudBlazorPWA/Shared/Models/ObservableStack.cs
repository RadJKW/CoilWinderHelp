namespace MudBlazorPWA.Shared.Models;
public class ObservableStack<T> : Stack<T>
{
	public event Action? StackChanged;

	public ObservableStack() { }

	// Add this constructor
	public ObservableStack(IEnumerable<T> collection) : base(collection) { }

	public new void Push(T item)
	{
		base.Push(item);
		StackChanged?.Invoke();
	}

	public new T Pop()
	{
		var item = base.Pop();
		StackChanged?.Invoke();
		return item;
	}
}
