public class BufferCircular<T>
{
	T[] buffer;
	private int startingCell = 0, lastCell = 0;
	private readonly int size;

	public BufferCircular(int size)
	{
		buffer = new T[size];
		this.size = size;
	}

	public void Add(T elem)
	{
		buffer[lastCell] = elem;

		lastCell++;
		//Desplaza la última celda a la siguiente posición
		if (lastCell == size) lastCell = 0;
		//Desplaza la celda inicial si la última la alcanza
		if (lastCell == startingCell) startingCell = startingCell == size ? 0 : startingCell + 1;
	}

	public void Clear()
	{
		for(int i = 0; i < size; i++)
		{
			buffer[i] = default;
		}
		startingCell = 0;
		lastCell = 0;
	}

	public bool Vacio() { return startingCell == lastCell; }
	/// <summary>
	/// Good idea to cache this value into another variable
	/// </summary>
	public int SizeUsed() { return startingCell > lastCell ? lastCell + size - startingCell : lastCell - startingCell; }

	public T ValueAt(int i)
	{
		if (SizeUsed() < i) return default;
		else return buffer[(startingCell + i) % size];
	}
	public T ValueAt_FAST_AND_DANGER(int i)
	{
		return buffer[(startingCell + i) % size];
	}

	public T[] ToArray()
	{
		int n = buffer.Length;
		T[] vector = new T[n];
		int s = SizeUsed();
		for(int i = 0; i < s; i++)
		{
			vector[i] = buffer[(startingCell + i) % n];
		}
		return vector;
	}
}
