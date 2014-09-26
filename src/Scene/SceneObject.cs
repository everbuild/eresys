using System;

namespace Eresys
{
	/// <summary>
	/// Summary description for SceneObject.
	/// </summary>
	public class SceneObject
	{
		public Point3D Position
		{
			get { return position; }
			set { position = value; changed = true; }
		}

		public Point3D Direction
		{
			get { return direction; }
			set { direction = value; changed = true; }
		}

		public Vector Speed
		{
			get { return speed; }
			set { speed = value; }
		}

		public Vector Rotation
		{
			get { return rotation; }
			set { rotation = value; }
		}

		public virtual bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}

		public virtual bool Solid
		{
			get { return false; }
		}

		public Matrix WorldMatrix
		{
			get
			{
				return mworld;
			}
		}

		public SceneObject()
		{
			this.position	= new Point3D();
			this.direction	= new Point3D();
			this.speed		= new Vector();
			this.rotation	= new Vector();
			this.changed	= true;
			this.visible	= false;
			Kernel.Scene.AddObject(this);
			mworld = new Matrix();
		}

		public SceneObject(Point3D position, Point3D direction)
		{
			this.position	= position;
			this.direction	= direction;
			this.speed		= new Vector();
			this.rotation	= new Vector();
			this.changed	= true;
			Kernel.Scene.AddObject(this);
		}

		public SceneObject(Point3D position, Point3D direction, Vector speed, Vector rotation)
		{
			this.position	= position;
			this.direction	= direction;
			this.speed		= speed;
			this.rotation	= rotation;
			this.changed	= true;
			Kernel.Scene.AddObject(this);
		}

		public virtual void Update()
		{
			Position += Speed*(float)Kernel.Interval;
			Direction += Rotation*(float)Kernel.Interval;
		}

		public virtual void Render(Camera camera)
		{
		}

		protected virtual void OnChange()
		{
			// mworld = ...
		}

		public void Change()
		{
			if(changed)
			{
				OnChange();
				changed = false;
			}
		}

		public virtual Vector CheckCollision(Point3D start, Vector movement, float sphere)
		{
			return movement;
		}

		protected bool changed;

		private Point3D position;
		private Point3D direction;
		private Vector speed;
		private Vector rotation;
		protected bool visible;
		private Matrix mworld;
	}
}
