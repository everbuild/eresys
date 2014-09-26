using System;
using System.Collections;

namespace Eresys
{
	public class Scene
	{
		public Player player;

		public SceneObject this [int index]
		{
			get { return (SceneObject)objects[index]; }
		}

		public int Count
		{
			get { return objects.Count; }
		}

		public Scene()
		{
			Kernel.Log.WriteLine("Initialising Scene...");
			Kernel.Log.WriteLine();
			objects = new ArrayList();
			player = null;
		}

		public void AddObject(SceneObject obj)
		{
			if(!objects.Contains(obj))
				objects.Add(obj);
		}

		public void RemoveObject(SceneObject obj)
		{
			objects.Remove(obj);
		}

		public void Update()
		{
			// update objects
			for(int i = 0; i < objects.Count; i ++)
			{
				((SceneObject)objects[i]).Update();
			}
		}

		public void Render()
		{
			if(player == null) return;
			for(int i = 0; i < objects.Count; i ++)
			{
				((SceneObject)objects[i]).Change();
			}
			Kernel.Graphics.Camera = player.Camera;
			for(int i = 0; i < objects.Count; i ++)
			{
				SceneObject obj = ((SceneObject)(objects[i]));
				Kernel.Graphics.WorldMatrix = obj.WorldMatrix;
				if(obj.Visible) obj.Render(player.Camera);
			}
		}

		private ArrayList objects;
	}
}
