using System;

namespace Eresys
{
	/// <remarks>
	/// Schaal: 1m = 40u --> 1u = 2.5cm
	/// breedte 19" scherm = 36cm -> 14.4u
	/// </remarks>
	public class Camera: SceneObject
	{
		public const float FOV = 1.5707963f; // = 90°
		public const float SCREENWIDTH = 32.0f;
		public const float DISTANCE = 80000.0f;
		public const float ASPECT = 1.3333333f;

		public override bool Visible
		{
			get { return false; }
		}

		public float Zoom
		{
			get
			{
				return zoom;
			}
			set
			{
				zoom = value;
				fov = FOV/zoom;
				nearClipZ = SCREENWIDTH/(zoom*2.0f*(float)System.Math.Tan(fov/2.0f));
				changed = true;
			}
		}

		public float NearClippingPlane { get { return nearClipZ; } }
		public float FarClippingPlane { get { return farClipZ; } }

		public Matrix ViewMatrix
		{
			get
			{
				return mview;
			}
		}

		public Matrix ProjectionMatrix
		{
			get
			{
				return mproj;
			}
		}

		public Camera()
		{
			mview = new Matrix();
			mproj = new Matrix();
			Zoom = 1.0f;
			farClipZ = DISTANCE;
			aspect = ASPECT;
			frustumPlanes = new Plane[6];
			for(int i = 0; i < 6; i ++) frustumPlanes[i] = new Plane();
		}

		public bool InsideFrustum(Point3D p, float r)
		{
			for(int i = 0; i < 6; i ++)
			{
				float d = frustumPlanes[i].a*p.x + frustumPlanes[i].b*p.y + frustumPlanes[i].c*p.z + frustumPlanes[i].d;
				if(d <= -r) return false;
			}

			return true;
		}

		protected override void OnChange()
		{
			base.OnChange ();

			// Update frustum planes
			UpdateFrustumPlanes();

			// update view matrix
			Vector up, pos, lookat;

			pos = new Vector(Position.x, Position.y, Position.z);
			up = new Vector(0.0f, 1.0f, 0.0f);
			lookat = new Vector(0.0f, 0.0f, 1.0f);

			Matrix mtrans = Matrix.RotationYawPitchRoll(Direction.y, Direction.x, Direction.z);
					
			up = Vector.TransformNormal(up, mtrans);
			lookat = Vector.TransformNormal(lookat, mtrans);
			lookat += pos;
			mview = Matrix.LookAtLH(pos, lookat, up);

			// update projection matrix
			mproj = Matrix.PerspectiveFovLH(fov/2.0f, aspect, nearClipZ, farClipZ);
		}

		private void UpdateFrustumPlanes()
		{
			Matrix clip = ViewMatrix;
			clip.multiply(ProjectionMatrix);

			// right
			frustumPlanes[0].a = clip.element[3, 0] - clip.element[0, 0];
			frustumPlanes[0].b = clip.element[3, 1] - clip.element[0, 1];
			frustumPlanes[0].c = clip.element[3, 2] - clip.element[0, 2];
			frustumPlanes[0].d = clip.element[3, 3] - clip.element[0, 3];
			frustumPlanes[0].Normalize();

			// left
			frustumPlanes[1].a = clip.element[3, 0] + clip.element[0, 0];
			frustumPlanes[1].b = clip.element[3, 1] + clip.element[0, 1];
			frustumPlanes[1].c = clip.element[3, 2] + clip.element[0, 2];
			frustumPlanes[1].d = clip.element[3, 3] + clip.element[0, 3];
			frustumPlanes[1].Normalize();

			// top
			frustumPlanes[2].a = clip.element[3, 0] - clip.element[1, 0];
			frustumPlanes[2].b = clip.element[3, 1] - clip.element[1, 1];
			frustumPlanes[2].c = clip.element[3, 2] - clip.element[1, 2];
			frustumPlanes[2].d = clip.element[3, 3] - clip.element[1, 3];
			frustumPlanes[2].Normalize();

			// bottom
			frustumPlanes[3].a = clip.element[3, 0] + clip.element[1, 0];
			frustumPlanes[3].b = clip.element[3, 1] + clip.element[1, 1];
			frustumPlanes[3].c = clip.element[3, 2] + clip.element[1, 2];
			frustumPlanes[3].d = clip.element[3, 3] + clip.element[1, 3];
			frustumPlanes[3].Normalize();

			// back
			frustumPlanes[4].a = clip.element[3, 0] - clip.element[2, 0];
			frustumPlanes[4].b = clip.element[3, 1] - clip.element[2, 1];
			frustumPlanes[4].c = clip.element[3, 2] - clip.element[2, 2];
			frustumPlanes[4].d = clip.element[3, 3] - clip.element[2, 3];
			frustumPlanes[4].Normalize();

			// front
			frustumPlanes[5].a = clip.element[3, 0] + clip.element[2, 0];
			frustumPlanes[5].b = clip.element[3, 1] + clip.element[2, 1];
			frustumPlanes[5].c = clip.element[3, 2] + clip.element[2, 2];
			frustumPlanes[5].d = clip.element[3, 3] + clip.element[2, 3];
			frustumPlanes[5].Normalize();
		}

		private Matrix mview, mproj;
		private float zoom, fov, nearClipZ, farClipZ, aspect;
		private Plane[] frustumPlanes;
	}
}
