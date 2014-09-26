using System;

namespace Eresys
{
	/// <summary>
	/// Summary description for Player.
	/// </summary>
	public class Player: SceneObject
	{
		public override bool Visible
		{
			get { return false; }
		}

		public override bool Solid
		{
			get { return true; }
		}

		public Camera Camera
		{
			get { return camera; }
		}

		public Player()
		{
			camera = new Camera();
		}

		protected override void OnChange()
		{
			camera.Position = Position;
			camera.Direction = Direction;
		}

		private Camera camera;
	}
}
