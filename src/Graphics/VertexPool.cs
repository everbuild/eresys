using System;

namespace Eresys
{
	/// <summary>
	/// Stelt een lijst vertices voor. Deze kan mbv. Kernel.Graphics worden ge-upload naar de grafische hardware om gebruikt
	/// te worden bij het renderen.
	/// </summary>
	public class VertexPool
	{
		public Vertex this [int index]
		{
			get { return vertices[index]; }
			set { vertices[index] = value; }
		}

		public int Size { get { return vertices.Length; } }

		public VertexPool(int size)
		{
			vertices = new Vertex[size];
		}

		public object GetData()
		{
			return vertices;
		}

		public Vertex[] vertices;
	}
}
