using System;

namespace Eresys
{
	public class SkyBox: SceneObject
	{
		public SkyBox(float width, float height, float depth, Texture texture)
		{
			float x, y, z;
			VertexPool vp;

			x = width/2.0f;
			y = height/2.0f;
			z = depth/2.0f;

			vp = new VertexPool(NumVertices);

			vp[0]  = new Vertex(new Point3D(-x, -y, -z), new Point2D(0.0000f, 0.5f));
			vp[1]  = new Vertex(new Point3D(-x,  y, -z), new Point2D(0.0000f, 0.0f));
			vp[2]  = new Vertex(new Point3D(-x, -y,  z), new Point2D(0.3333f, 0.5f));
			vp[3]  = new Vertex(new Point3D(-x,  y,  z), new Point2D(0.3333f, 0.0f));
			vp[4]  = new Vertex(new Point3D( x, -y,  z), new Point2D(0.6666f, 0.5f));
			vp[5]  = new Vertex(new Point3D( x,  y,  z), new Point2D(0.6666f, 0.0f));
			vp[6]  = new Vertex(new Point3D( x, -y, -z), new Point2D(1.0000f, 0.5f));
			vp[7]  = new Vertex(new Point3D( x,  y, -z), new Point2D(1.0000f, 0.0f));

			vp[8]  = new Vertex(new Point3D(-x, -y,  z), new Point2D(0.0000f, 1.0f));
			vp[9]  = new Vertex(new Point3D( x, -y,  z), new Point2D(0.0000f, 0.5f));
			vp[10] = new Vertex(new Point3D(-x, -y, -z), new Point2D(0.3333f, 1.0f));
			vp[11] = new Vertex(new Point3D( x, -y, -z), new Point2D(0.3333f, 0.5f));
			vp[12] = new Vertex(new Point3D(-x,  y, -z), new Point2D(0.6666f, 1.0f));
			vp[13] = new Vertex(new Point3D( x,  y, -z), new Point2D(0.6666f, 0.5f));
			vp[14] = new Vertex(new Point3D(-x,  y,  z), new Point2D(1.0000f, 1.0f));
			vp[15] = new Vertex(new Point3D( x,  y,  z), new Point2D(1.0000f, 0.5f));

			/*
			vp[0]  = new Vertex(new Point3D(-x, -y, -z), new Point2D(1.0000f, 0.5f));
			vp[1]  = new Vertex(new Point3D(-x,  y, -z), new Point2D(1.0000f, 0.0f));
			vp[2]  = new Vertex(new Point3D(-x, -y,  z), new Point2D(0.6666f, 0.5f));
			vp[3]  = new Vertex(new Point3D(-x,  y,  z), new Point2D(0.6666f, 0.0f));
			vp[4]  = new Vertex(new Point3D( x, -y,  z), new Point2D(0.3333f, 0.5f));
			vp[5]  = new Vertex(new Point3D( x,  y,  z), new Point2D(0.3333f, 0.0f));
			vp[6]  = new Vertex(new Point3D( x, -y, -z), new Point2D(0.0000f, 0.5f));
			vp[7]  = new Vertex(new Point3D( x,  y, -z), new Point2D(0.0000f, 0.0f));

			vp[8]  = new Vertex(new Point3D(-x, -y,  z), new Point2D(1.0000f, 1.0f));
			vp[9]  = new Vertex(new Point3D( x, -y,  z), new Point2D(1.0000f, 0.5f));
			vp[10] = new Vertex(new Point3D(-x, -y, -z), new Point2D(0.6666f, 1.0f));
			vp[11] = new Vertex(new Point3D( x, -y, -z), new Point2D(0.6666f, 0.5f));
			vp[12] = new Vertex(new Point3D(-x,  y, -z), new Point2D(0.3333f, 1.0f));
			vp[13] = new Vertex(new Point3D( x,  y, -z), new Point2D(0.3333f, 0.5f));
			vp[14] = new Vertex(new Point3D(-x,  y,  z), new Point2D(0.0000f, 1.0f));
			vp[15] = new Vertex(new Point3D( x,  y,  z), new Point2D(0.0000f, 0.5f));
			*/

			vpi = Kernel.Graphics.AddVertexPool(vp);

			texi = Kernel.Graphics.AddTexture(texture);
		}

		public override void Render(Camera camera)
		{
			Matrix oldMatrix;

			Kernel.Graphics.Lighting = false;

			oldMatrix = Kernel.Graphics.WorldMatrix;
			Kernel.Graphics.WorldMatrix = Matrix.Translation(camera.Position.x, camera.Position.y, camera.Position.z);

			Kernel.Graphics.Filtering = false;

			Kernel.Graphics.RenderTriangleStrip(vpi, 0, 8, texi, -1);
			Kernel.Graphics.RenderTriangleStrip(vpi, 8, 8, texi, -1);

			Kernel.Graphics.Filtering = true;

			Kernel.Graphics.WorldMatrix = oldMatrix;
		}

		private static ushort NumVertices = 16;
		private int vpi, texi;
	}
}
