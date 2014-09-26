using System;
using System.IO;
using System.Collections;

namespace Eresys
{
	public class HLBSPMap: SceneObject
	{
		#region Public Exceptions

		public class BSPVersionException: Exception
		{
			public override string Message { get { return "Version of " + bspFileName + " is not supported!"; } }
			public BSPVersionException(string bspFileName) { this.bspFileName = bspFileName; }
			private string bspFileName;
		}

		public class BSPCorruptedException: Exception
		{
			public override string Message { get { return "Contents of " + bspFileName + " is corrupted!"; } }
			public BSPCorruptedException(string bspFileName) { this.bspFileName = bspFileName; }
			private string bspFileName;
		}

		#endregion

		#region Public Classes

		public class ScriptValue
		{
			public string nam, val;
		}

		public class ScriptBlock
		{
			public string this [string i]
			{
				get
				{
					for(int j = 0; j < vals.Count; j ++)
						if(((ScriptValue)vals[j]).nam == i) return ((ScriptValue)vals[j]).val;
					throw new System.IndexOutOfRangeException();
				}
			}

			public string this [int i]
			{
				get
				{
					return ((ScriptValue)vals[i]).val;
				}
			}

			public int Count
			{
				get
				{
					return vals.Count;
				}
			}

			public ScriptBlock(string script)
			{
				vals = new ArrayList();
				this.script = script;
				idx = 0;
				while(idx < script.Length)
				{
					ScriptValue val = new ScriptValue();
					try
					{
						val.nam = ReadWord();
						val.val = ReadWord();
					}
					catch(Exception)
					{
						break;
					}
					vals.Add(val);
				}
			}

			private string ReadWord()
			{
				string res = "";
				while(script[idx] != '"') idx ++;
				idx ++;
				while(script[idx] != '"')
				{
					res += script[idx];
					idx ++;
				}
				idx ++;
				return res;
			}

			private int idx;
			private string script;
			private ArrayList vals;
		}

		public class Script
		{
			public ScriptBlock[] this [string i]
			{
				get
				{
					string[] classes = i.Split(';');
					Queue queue = new Queue();
					for(int j = 0; j < blocks.Count; j ++)
					{
						for(int k = 0; k < classes.Length; k ++)
						{
							if(((ScriptBlock)blocks[j])["classname"] == classes[k]) queue.Enqueue(blocks[j]);
						}
					}
					ScriptBlock[] res = new ScriptBlock[queue.Count];
					for(int j = 0; j < res.Length; j ++) res[j] = (ScriptBlock)queue.Dequeue();
					return res;
				}
			}

			public ScriptBlock this [int i]
			{
				get
				{
					return (ScriptBlock)blocks[i];
				}
			}

			public int Count
			{
				get
				{
					return blocks.Count;
				}
			}

			public Script(string script)
			{
				int i = 0;
				blocks = new ArrayList();
				while(i < script.Length)
				{
					try
					{
						while(script[i] != '{') i ++;
						i ++;
						int start = i;
						while(script[i] != '}') i ++;
						int stop = i - 1;
						i ++;
						blocks.Add(new ScriptBlock(script.Substring(start, stop - start + 1)));
					}
					catch(Exception)
					{
						break;
					}
				}
			}
			private ArrayList blocks;
		}

		#endregion

		#region Public Members

		public static int BSPVersion = 30;

		public override bool Solid
		{
			get { return true; }
		}

		public string FileName
		{
			get { return fileName; }
		}

		public bool BSPRendering;

		public bool FrustumCulling;

		public Script EntityScript
		{
			get { return script; }
		}

		#endregion

		#region Public Methods

		public HLBSPMap(string fileName)
		{
			BSPRendering = true;
			FrustumCulling = true;
			this.fileName = fileName;
			Load();
		}

		public override void Render(Camera camera)
		{
			int i, j;
			float d;
			Plane plane;
			bool[] pvs;
			ArrayList visFaces;
			Face face;
			bool visible;
			bool firstAlphaFace;

			Kernel.Profiler.StartSample("HLBSPMap.Render");

			// create visible faces list
			visFaces = new ArrayList();

			// add faces to visible faces list
			if(BSPRendering)
			{
				// get camera node
				Kernel.Profiler.StartSample("get camera leaf");
				i = 0;
				while(i >= 0)
				{
					plane = planes[nodes[i].plane];
					d = plane.Distance(camera.Position);
					if(d >= 0.0f) i = nodes[i].frontChild;
					else i = nodes[i].backChild;
				}
				Kernel.Profiler.StopSample();

				// get pvs for current leaf (~i => -(i + 1))
				Kernel.Profiler.StartSample("decompress pvs");
				i = leaves[~i].pvs;
				pvs = DecompressPvs(i);
				Kernel.Profiler.StopSample();
				
				// add visible faces
				Kernel.Profiler.StartSample("add visible faces");
				for(i = 0; i < leaves.Length; i ++) // go through all leaves
				{
					visible = i == 0;
					if(!visible) visible = pvs[i - 1];
					if(visible) // if leaf is visible
					{
						for(j = 0; j < leaves[i].numMarkFaces; j ++) // go through all faces of leaf ..
						{
							face = faces[markFaces[leaves[i].firstMarkFace + j]];
							if(face.texture < 0) continue;
							if(face.model) continue;
							visFaces.Add(face); // .. and add them
						}
					}
				}
				Kernel.Profiler.StopSample();

				// add all model faces
				Kernel.Profiler.StartSample("add model faces");
				visFaces.AddRange(modelFaces);
				Kernel.Profiler.StopSample();
			}
			else
			{
				Kernel.Profiler.StartSample("add all faces");
				for(i = 0; i < faces.Length; i ++)
				{
					face = faces[i];
					if(face.texture < 0) continue;
					visFaces.Add(face);
				}
				Kernel.Profiler.StopSample();
			}

			// perform frustum culling on visible faces
			if(FrustumCulling)
			{
				Kernel.Profiler.StartSample("frustum culling");
				for(i = visFaces.Count - 1; i >= 0; i --)
				{
					face = (Face)visFaces[i];
					if(!camera.InsideFrustum(face.center, face.radius)) visFaces.RemoveAt(i);
				}
				Kernel.Profiler.StopSample();
			}

			// update camera distance of visible faces
			Kernel.Profiler.StartSample("update face distance");
			for(i = 0; i < visFaces.Count; i ++) ((Face)visFaces[i]).UpdateCamDist(camera);
			Kernel.Profiler.StopSample();

			// sort visible faces
			Kernel.Profiler.StartSample("sort faces");
			visFaces.Sort();
			Kernel.Profiler.StopSample();

			// render visible faces + sky somewher ebetween the transparant and the solid faces
			Kernel.Profiler.StartSample("render faces");
			firstAlphaFace = false;
			for(i = 0; i < visFaces.Count; i ++)
			{
				face = (Face)visFaces[i];
				if(face.IsTransparant())
				{
					if(!firstAlphaFace && sky != null)
					{
						// render skybox
						Kernel.Profiler.StartSample("render sky");
						sky.Render(camera);
						Kernel.Profiler.StopSample();
						firstAlphaFace = true;
					}
					Kernel.Graphics.AlphaBlending = true;
					Kernel.Graphics.Alpha = face.alpha;
					Kernel.Graphics.TextureAlpha = face.textureAlpha;
				}
				else
				{
					Kernel.Graphics.AlphaBlending = false;
				}
				Kernel.Graphics.Lighting = face.lightmap >= 0;
				Kernel.Graphics.RenderTriangleFan(vertexPoolI, face.firstVertex, face.numVertices, face.texture, face.lightmap);
			}
			if(!firstAlphaFace && sky != null)
			{
				// render skybox
				Kernel.Profiler.StartSample("render sky");
				sky.Render(camera);
				Kernel.Profiler.StopSample();
			}
			Kernel.Profiler.StopSample();

			Kernel.Profiler.StopSample();
		}

		private Point3D ClosestPointOnLine(Point3D a, Point3D b, Point3D p)
		{
			// Determine t (the length of the vector from ‘a’ to ‘p’)

			Vector c = p - a;
			Vector v = b - a;
			float d = v.Magnitude();
			v.Normalize();
			float t = v.Dot(c);

			// Check to see if ‘t’ is beyond the extents of the line segment

			if (t < 0) return a;
			if (t > d) return b;
 
			// Return the point between ‘a’ and ‘b’

			v *= t;
			return a + v;
		}

		private Point3D ClosestPointOnFace(int faceIdx, Point3D p)
		{
			Face face = faces[faceIdx];
			float mag = float.MaxValue;
			Point3D res = p;
			for(int i = 0; i < face.numVertices; i ++)
			{
				int prevI = i - 1;
				if(prevI < 0) prevI += face.numVertices;
				Point3D q = ClosestPointOnLine(vertexPool[prevI + face.firstVertex].position, vertexPool[i + face.firstVertex].position, p);
				float d = ((Vector)(q - p)).Magnitude();
				if(d < mag)
				{
					res = q;
					mag = d;
				}
			}
			return res;
		}

		private bool PointInFace(int faceIdx, Point3D point)
		{
			Face face = faces[faceIdx];
			Vector v, w;
			w = vertexPool[face.firstVertex + face.numVertices - 1].position - point;
			w.Normalize();
			float angle = 0.0f;
			for(int i = 0; i < face.numVertices; i ++)
			{
				v = w;
				w = vertexPool[i + face.firstVertex].position - point;
				w.Normalize();
				angle += (float)System.Math.Acos(v.Dot(w));
			}
			return angle > 6.2828f;
		}

		private Collision CheckFace(int faceIdx, Point3D start, Vector movement, float startFraction, float endFraction, float sphere)
		{
			Face face = faces[faceIdx];
			//Plane plane = planes[face.plane];

			// get accurate plane
			Plane plane = planes[face.plane];
			if(face.inversePlane)
			{
				plane = new Plane(-plane.a, -plane.b, -plane.c, -plane.d);
			}

			// calculate distance of start- and endpoint to plane
			float ds = plane.Distance(start + movement*startFraction);
			float de = plane.Distance(start + movement*endFraction);

			// if the starting point of the sphere lies behind the plane, no collision should occur, because in this case the plane faces away from us and is supposed to be invisible
			if(ds < sphere) return null;

			// check if we are moving away from the plane (with our back towards the front of the plane) or parallel to the plane
			// in neither of these cases a collision will occur
			if(ds <= de) return null;

			// finally, if this check fails we know for sure there is a collision
			// it tests if the endpoint is on or beyond the plane
			if(de >= sphere) return null;

			// calculate fraction of the movement vector where we hit the plane
			float fraction = (ds - sphere)/(ds - de);

			// apply a window to this fraction value, just to be sure it is valid
			if(fraction < 0.0f) fraction = 0.0f;
			if(fraction > 1.0f) fraction = 1.0f;

			// scale this fraction to fit on the real movement vector
			fraction = startFraction + fraction*(endFraction - startFraction);

			// we now know for sure we intersect the plane, but do we intersect it inside the face that lies on top of it?
			// to find that out we first need to calculate an intersection point
			Point3D intersection = start + movement*fraction - plane.GetNormal()*sphere;
			if(!PointInFace(faceIdx, intersection))
			{
				// if this point doesnt lie within the face, perhaps a part of our sphere does
				// to check this we get the closest point on one of the edges of the face to our intersection point
				Point3D closestPoint = ClosestPointOnFace(faceIdx, intersection);
				// and then we check the distance between those 2 points to se if a part of our sphere does hit the face
				Vector distance = intersection - closestPoint;
				if(distance.MagnitudeSquare() > sphere*sphere) return null;
			}

			return new Collision(fraction, plane.GetNormal());
		}

		private Collision ClosestCollision(Collision c1, Collision c2)
		{
			Collision res;
			if(c1 == null && c2 == null) res = null;
			else if(c1 == null) res = c2;
			else if(c2 == null) res = c1;
			else if(System.Math.Abs(c1.fraction - c2.fraction) < collisionMargin)
			{
				// when 2 collision are almost equal, we merge them
				c1.fraction = System.Math.Min(c1.fraction, c2.fraction);
				c1.response += c2.response;
				c1.response.Normalize();
				res = c1;
			}
			else if(c1.fraction < c2.fraction) res = c1;
			else res = c2;
			if(res == null) return null;
			// adjust small and big fraction values so we won't get ridiculously many microscopic collisions no one would ever notice
			if(res.fraction < 1.0f) res.fraction = 0.0f;
			if(res.fraction > 0.9f) res.fraction = 1.0f;
			return res;
		}

		private Collision CheckLeaf(int leafIdx, Point3D start, Vector movement, float startFraction, float endFraction, float sphere)
		{
			Leaf leaf = leaves[leafIdx];
			Collision c = null;
			for(int i = 0; i < leaf.numMarkFaces; i ++)
			{
				c = ClosestCollision(c, CheckFace(markFaces[leaf.firstMarkFace + i], start, movement, startFraction, endFraction, sphere));
			}
			return c;
		}

		private Collision CheckNode(int nodeIdx, Point3D start, Vector movement, float startFraction, float endFraction, float sphere)
		{
			// calculate start and end point
			Point3D s = start + movement*startFraction;
			Point3D e = start + movement*endFraction;

			// if this node is a leaf, check for collision within that leaf
			if(nodeIdx < 0)
			{
				return CheckLeaf(~nodeIdx, start, movement, startFraction, endFraction, sphere);
			}

			// get node
			Node node = nodes[nodeIdx];

			// get splitting plane
			Plane plane = planes[node.plane];

			// calculate distance of start and end point to plane
			float ds = plane.Distance(s);
			float de = plane.Distance(e);

			if(ds > sphere && de > sphere)
			{
				// both points are in front of the plane
				return CheckNode(node.frontChild, start, movement, startFraction, endFraction, sphere);
			}
			else if(ds < -sphere && de < -sphere)
			{
				// both points are behind the plane
				return CheckNode(node.backChild, start, movement, startFraction, endFraction, sphere);
			}
			else
			{
				float f1, f2;

				float ads = System.Math.Abs(ds);
				float ade = System.Math.Abs(de);
				float ad = ads + ade;
				float frac = endFraction - startFraction;

				f1 = frac*(ads + sphere)/ad + collisionMargin;
				f2 = frac*(ade + sphere)/ad + collisionMargin;

				Collision c1 = CheckNode(node.frontChild, start, movement, startFraction, startFraction + f1, sphere);
				Collision c2 = CheckNode(node.backChild, start, movement, endFraction - f2, endFraction, sphere);

				return ClosestCollision(c1, c2);
			}
		}

		public override Vector CheckCollision(Point3D start, Vector movement, float sphere)
		{
			if(movement.IsNullVector()) return movement;

			Vector res = new Vector();
			int numCollisions = 0;

			for(;;)
			{
				Collision collision = null;
				//for(int i = 0; i < faces.Length; i ++) collision = ClosestCollision(collision, CheckFace(i, start, movement, 0.0f, 1.0f, sphere));
				collision = CheckNode(0, start, movement, 0.0f, 1.0f, sphere);
				for(int i = 0; i < modelFaces.Count; i ++) collision = ClosestCollision(collision, CheckFace(((Face)modelFaces[i]).index, start, movement, 0.0f, 1.0f, sphere));

				if(collision == null) break;
				if(collision.fraction == 1.0f) break;

				float movMag = movement.Magnitude();
				movement.Normalize();

				float fracMag = collision.fraction*movMag - collisionMargin;
				if(fracMag < 0.0f) fracMag = 0.0f;
				res += movement*fracMag;

				numCollisions ++;
				if(numCollisions == maxCollisions)
				{
					movement = new Vector();
					break;
				}

				start += res;

				movement = collision.response.Cross(movement).Cross(collision.response)*(1.0f - collision.fraction)*movMag;
			}

			res += movement;

			return res;
		}

		#endregion

		#region Private Classes

		private enum Lumps
		{
			Entities = 0,
			Planes,
			Textures,
			Vertices,
			VisDate,
			Nodes,
			TexInfo,
			Faces,
			Lighting,
			ClipNodes,
			Leaves,
			MarkSurfaces,
			Edges,
			SurfaceEdges,
			Models,
			Count
		}

		private struct Lump
		{
			public int offset;
			public int length;
		}

		public struct Edge
		{
			public ushort v1; // indices into the vertex array
			public ushort v2; //
		}

		private class Texture
		{
			public string			name;
			public uint				width, height;

			public Texture(BinaryReader stream, int lumpOffset, int texOffset)
			{
				char[] cstr;
				int l ;
				
				stream.BaseStream.Position = lumpOffset + texOffset;
				cstr = stream.ReadChars(16);
				l = 0;
				while(cstr[l] != 0) l ++;
				name = new string(cstr, 0, l);
				width = stream.ReadUInt32();
				height = stream.ReadUInt32();
			}
		}

		private struct TextureInfo
		{
			public Point3D	axisU;		// These 2 axes define a plane. Texture coordinates are calculated by projecting
			public float		offsetU;	// the vertices of a face onto this plane. This makes aligning textures to faces
			// a lot easier.
			public Point3D	axisV;		//      Magic forumula:
			public float		offsetV;	//      u = x*axisU.x + y*axisU.y + z*axisU.z + offsetU (dito for v..)
			public int			texture;
		}

		private class Lightmap: System.IComparable
		{
			public int width;
			public int height;
			public int offset;
			public int face;

			public Lightmap(int width, int height, int offset, int face)
			{
				this.width = width;
				this.height = height;
				this.offset = offset;
				this.face = face;
			}

			public int CompareTo(object obj)
			{
				return height - ((Lightmap)obj).height;
			}
		}

		private class Face: IComparable
		{
			public int index;
			public short plane;
			public bool inversePlane;
			public int firstVertex;
			public int numVertices;
			public int texture;
			public int lightmap;
			public float camDist;
			public Point3D center;
			public float radius;
			public byte alpha;
			public bool textureAlpha;
			public bool model;

			public bool IsTransparant()
			{
				return alpha < 255 || textureAlpha;
			}

			public void UpdateCamDist(Camera camera)
			{
				float dx, dy, dz;

				dx = center.x - camera.Position.x;
				dy = center.y - camera.Position.y;
				dz = center.z - camera.Position.z;

				camDist = (float)System.Math.Sqrt(dx*dx + dy*dy + dz*dz);
			}

			public int CompareTo(object obj)
			{
				Face face;
				float dist;
					
				face = (Face)obj;

				if(IsTransparant())
				{
					if(face.IsTransparant())
					{
						dist = face.camDist - camDist;
						if(dist == 0.0f) return 0;
						if(dist > 0.0f) return 1;
						return -1;
					}
					else
					{
						return 1;
					}
				}
				else
				{
					if(face.IsTransparant())
					{
						return -1;
					}
					else
					{
						dist = face.camDist - camDist;
						if(dist == 0.0f) return 0;
						if(dist > 0.0f) return -1;
						return 1;
					}
				}
			}
		}

		private class Node
		{
			public int		plane;		// index of the splitting plane (in the plane array)
			public short	frontChild;	// index of the front child node or leaf (leaf = -(child + 1))
			public short	backChild;	// index of the back child node or leaf (leaf = -(child + 1))
		}

		private class Leaf
		{
			public int		pvs;			// index into the pvs array; -1 = no pvs (--> everything is visible)
			public ushort	firstMarkFace;	// index of the first face (in the face leaf array)
			public ushort	numMarkFaces;	// number of consecutive edges (in the face leaf array)
		}

		public struct Model
		{
			public Point3D origin;
			public int firstFace, numFaces;
		}

		#endregion

		#region Private Members

		private static float collisionMargin = 1.0f/32.0f;
		private static int maxCollisions = 100;


		private string		fileName;			// name of the bsp file
		private Script		script;				// the entity script included with the bsp file
		private Plane[]		planes;				// planes array
		private VertexPool	vertexPool;			// the vertex pool
		private int			vertexPoolI;		// vertexPool index
		private Face[]		faces;				// faces array
		private ushort[]	markFaces;			// markFaces array. these are indices into the faces array
		private ArrayList	modelFaces;			// stores all faces used in models
		private Node[]		nodes;				// nodes array. these are the nodes of the bsp tree
		private Leaf[]		leaves;				// leaves array. these are the leaves of the bsp tree
		private byte[]		pvs;				// PVS (= Potential Visibility Set) array. This is used to determine if a leaf is visible
		private SkyBox		sky;				// class for rendering background sky

		#endregion

		#region Private Methods

		private void Load()
		{
			FileStream		fs;						// stream used to open the bsp file
			BinaryReader	br;						// reader used to read from it

			int				i, j, k;				// indices
			int				x, gx, y;				// coordinate variables. gx = Global X
			Point3D		p;						// 3D point
			int				n, count;				// to count stuff ... (n = Number)
			char[]			cstr;					// a classic null-terminated character array string
			bool			found;					// indicates if we found something ...
			string			s, t;					// some strings

			Lump[]			lumps;					// A lump is a chunk of data in a BSP file containing a specific type of information, like vertices for example
			Lump			lump;					// used for easy acces to the info in the lumps array
			ArrayList		wads;					// list of wad files being used by this bsp file
			Point3D[]	vertices;				// stores all vertices
			Vertex[]		tempVertices;			// a temporary list with all the vertices used by a single face.
			Queue			faceVertices;			// a temporary list with all the vertices used by faces. this data will be moved to a vertexPoolI later on.
			Vertex			vertex;					// used to create and modify vertices in the vertices, faceVertices
			Edge[]			edges;					// edges (a line between 2 vertices)
			int[]			faceEdges;				// indices into the edge array that indicate the edges used by a face
			Texture[]		textures;				// texture array. contains only info like size and name
			TextureInfo[]	textureInfo;			// texture info array. contains info needed to calculate texture coordinates
			Eresys.Texture	texture;				// texture object
			int				texInfoI;				// index into the textureInfo array
			Point2D	minTexCoor, maxTexCoor;	// smallest and bigest texture coordinate
			float			radius;					// used to calculate the radius of a face
			ArrayList		addedTextures;			// list of all textures that are added. this is used to prevent loading textures multiple times
			byte[]			lightData;				// lightmap data
			ArrayList		lightmaps;				// contains some info for the lightmaps, like size
			Lightmap		lightmap;				// used for easy acces to elements in the lightmaps list
			int				lmw, lmh;				// size of a lightmap
			int				lmOffset;				// offset into the lightData array
			int				firstEdge, numEdges;	// first edge and number of edges of a face
			int				e, v;					// edge (e) and vertex (v) index
			Model[]			models;					// models array
			ScriptBlock[]	modelScripts;			// code blocks in the entityScript containing model info
			string			model;					// model identifier
			byte			mode, amount;			// render mode and render amount. indicates alpha blending and the amount of alpha blending
			float			range;					// used to determine the size of the skybox

			// open file
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			br = new BinaryReader(fs);

			// check bsp version
			if(br.ReadInt32() != BSPVersion) throw new BSPVersionException(fileName);

			// load lumps info
			lumps = new Lump[(int)Lumps.Count];
			for(i = 0; i < (int)Lumps.Count; i ++)
			{
				lumps[i].offset = br.ReadInt32();
				lumps[i].length = br.ReadInt32();
			}

			// load entity script
			lump = lumps[(int)Lumps.Entities];
			br.BaseStream.Position = lump.offset;
			cstr = br.ReadChars(lump.length);
			n = 0;
			while(cstr[n] != 0) n ++;
			s = new string(cstr, 0, n);
			script = new Script(s);

			// export entity script
			(new Log("entityscript.log")).WriteLine(s.Replace("\n", "\r\n"));

			// create wads
			i = 0;
			s = script["worldspawn"][0]["wad"];
			wads = new ArrayList();
			while(i < s.Length)
			{
				j = s.IndexOf(';', i);
				if(j < 0) j = s.Length;
				t = s.Substring(i, j - i);
				k = t.LastIndexOf('\\');
				if(k < 0) k = t.LastIndexOf('/');
				if(k >= 0) t = t.Substring(k + 1);
				try
				{
					wads.Add(new Wad3(t));
				}
				catch(Exception)
				{
					Kernel.Log.WriteLine("WARNING: Couldn't load wad \'" + t + "\'!");
				}
				i = j + 1;
			}

			// load planes
			lump = lumps[(int)Lumps.Planes];
			if(lump.length%20 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/20;
			planes = new Plane[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				planes[i] = new Plane();
				planes[i].a = br.ReadSingle();
				planes[i].c = br.ReadSingle();
				planes[i].b = br.ReadSingle();
				planes[i].d = br.ReadSingle();
				br.BaseStream.Position += 4;
			}

			// load vertices
			lump = lumps[(int)Lumps.Vertices];
			if(lump.length%12 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/12;
			br.BaseStream.Position = lump.offset;
			vertices = new Point3D[n];
			for(i = 0; i < n; i ++)
			{
				vertices[i].x = br.ReadSingle();
				vertices[i].z = br.ReadSingle();
				vertices[i].y = br.ReadSingle();
			}

			// load edges
			lump = lumps[(int)Lumps.Edges];
			if(lump.length%4 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/4;
			edges = new Edge[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				edges[i].v1 = br.ReadUInt16();
				edges[i].v2 = br.ReadUInt16();
			}

			// load face edges
			lump = lumps[(int)Lumps.SurfaceEdges];
			if(lump.length%4 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/4;
			faceEdges = new int[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				faceEdges[i] = br.ReadInt32();
			}

			// load textures
			lump = lumps[(int)Lumps.Textures];
			br.BaseStream.Position = lump.offset;
			n = br.ReadInt32();
			textures = new Texture[n];
			for(i = 0, j = lump.offset + 4; i < n; i ++, j += 4)
			{
				br.BaseStream.Position = j;
				textures[i] = new Texture(br, lump.offset, br.ReadInt32());
			}

			// load texture info
			lump = lumps[(int)Lumps.TexInfo];
			if(lump.length%40 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/40;
			textureInfo = new TextureInfo[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				textureInfo[i].axisU.x = br.ReadSingle();
				textureInfo[i].axisU.z = br.ReadSingle();
				textureInfo[i].axisU.y = br.ReadSingle();
				textureInfo[i].offsetU = br.ReadSingle();
				textureInfo[i].axisV.x = br.ReadSingle();
				textureInfo[i].axisV.z = br.ReadSingle();
				textureInfo[i].axisV.y = br.ReadSingle();
				textureInfo[i].offsetV = br.ReadSingle();
				textureInfo[i].texture = br.ReadInt32();
				br.BaseStream.Position += 4;
			}

			// load lighting data
			lump = lumps[(int)Lumps.Lighting];
			br.BaseStream.Position = lump.offset;
			lightData = br.ReadBytes(lump.length);

			// load faces
			lump = lumps[(int)Lumps.Faces];
			if(lump.length%20 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/20;
			faces = new Face[n];
			addedTextures = new ArrayList();
			lightmaps = new ArrayList();
			faceVertices = new Queue();
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				// create new face
				faces[i] = new Face();
				faces[i].index = i;
				faces[i].model = false;
				faces[i].alpha = 255;
				faces[i].textureAlpha = false;

				// load in some data
				faces[i].plane = br.ReadInt16();
				br.BaseStream.Position += 2;
				firstEdge = br.ReadInt32();
				numEdges = (int)br.ReadInt16();
				texInfoI = (int)br.ReadInt16();
				br.BaseStream.Position += 4;
				lmOffset = br.ReadInt32();

				// create texture

				// get texture info
				TextureInfo texInfo = textureInfo[texInfoI];
				Texture tex = textures[texInfo.texture];

				// is it a sky or a trigger texture?
				if(tex.name == "sky" || tex.name == "aaatrigger")
				{
					faces[i].texture = -1;
				}
				else
				{
					texture = null;

					// look if we didn't already add this texture
					found = false;
					for(j = 0; j < addedTextures.Count; j += 2)
					{
						texture = (Eresys.Texture)addedTextures[j];
						if(texture.Name == tex.name)
						{
							found = true;
							break;
						}
					}

					// If we haven't found the texture, create a new one
					if(!found)
					{
						j = 0;
						texture = null;
						while(j < wads.Count)
						{
							try
							{
								texture = new Eresys.Texture((Wad3)wads[j], tex.name);
								break;
							}
							catch(Exception)
							{
								j ++;
							}
						}
						if(texture == null)
						{
							// subsititute
							texture = new Eresys.Texture(1, 1, tex.name);
							texture[0] = new Color(255, 255, 255);
							Kernel.Log.WriteLine("WARNING: Couldn't load texture \'" + tex.name + "\'!");
						}

						// add new texture to addedTextures and upload it to the graphics hardware
						addedTextures.Add(texture);
						addedTextures.Add(Kernel.Graphics.AddTexture(texture));
					}

					// set face texture index
					faces[i].texture = (int)addedTextures[addedTextures.IndexOf(texture) + 1];
				}

				// create face vertices
				faces[i].firstVertex = faceVertices.Count;
				faces[i].numVertices = numEdges;
				tempVertices = new Vertex[numEdges];
				k = 0;
				minTexCoor.x = minTexCoor.y = System.Single.MaxValue;
				maxTexCoor.x = maxTexCoor.y = System.Single.MinValue;
				faces[i].center.x = 0.0f;
				faces[i].center.y = 0.0f;
				faces[i].center.z = 0.0f;
				for(j = 0; j < numEdges; j ++)
				{
					e = faceEdges[j + firstEdge]; // edge index

					// if e > 0 we should use this edge clockwise, otherwise we use edge -e counterclockwise
					if(e > 0)
						v = edges[e].v1; // clockwise: take first vertex of edge
					else
						v = edges[-e].v2; // counterclockwise: take second vertex of edge

					// set up the new vertex
					vertex = new Vertex();
					vertex.position.x = vertices[v].x;
					vertex.position.y = vertices[v].y;
					vertex.position.z = vertices[v].z;
					faces[i].center.x += vertex.position.x;
					faces[i].center.y += vertex.position.y;
					faces[i].center.z += vertex.position.z;

					// calculate its texture coordinate
					vertex.texCoord.x = (vertex.position.x*texInfo.axisU.x + vertex.position.y*texInfo.axisU.y + vertex.position.z*texInfo.axisU.z + texInfo.offsetU);
					vertex.texCoord.y = (vertex.position.x*texInfo.axisV.x + vertex.position.y*texInfo.axisV.y + vertex.position.z*texInfo.axisV.z + texInfo.offsetV);
					if(vertex.texCoord.x > maxTexCoor.x) maxTexCoor.x = vertex.texCoord.x;
					if(vertex.texCoord.y > maxTexCoor.y) maxTexCoor.y = vertex.texCoord.y;
					if(vertex.texCoord.x < minTexCoor.x) minTexCoor.x = vertex.texCoord.x;
					if(vertex.texCoord.y < minTexCoor.y) minTexCoor.y = vertex.texCoord.y;

					// temporary set lightmap coords to tex coords
					vertex.lightCoord.x = vertex.texCoord.x;
					vertex.lightCoord.y = vertex.texCoord.y;

					// scale texture coordinates to texture space
					vertex.texCoord.x /= (float)tex.width;
					vertex.texCoord.y /= (float)tex.height;

					// add vertex to temporary array
					tempVertices[k ++] = vertex;
				}
				faces[i].center.x /= (float)faces[i].numVertices;
				faces[i].center.y /= (float)faces[i].numVertices;
				faces[i].center.z /= (float)faces[i].numVertices;
				faces[i].radius = 0.0f;
				for(j = 0; j < numEdges; j ++)
				{
					p.x = tempVertices[j].position.x - faces[i].center.x;
					p.y = tempVertices[j].position.y - faces[i].center.y;
					p.z = tempVertices[j].position.z - faces[i].center.z;
					radius = (float)System.Math.Sqrt(p.x*p.x + p.y*p.y + p.z*p.z);
					if(radius > faces[i].radius) faces[i].radius = radius;
				}

				// create lightmap
				if(lmOffset < 0)
				{
					faces[i].lightmap = -1;
				}
				else
				{
					// calculate lightmap dimensions
					lmw = (int)(System.Math.Ceiling(maxTexCoor.x/16.0f) - System.Math.Floor(minTexCoor.x/16.0f) + 1.0);
					lmh = (int)(System.Math.Ceiling(maxTexCoor.y/16.0f) - System.Math.Floor(minTexCoor.y/16.0f) + 1.0);

					// calculate lightmap coordinates (in pixels for now, they will later be translated to texture space)
					for(j = 0; j < tempVertices.Length; j ++)
					{
						vertex = tempVertices[j];
						vertex.lightCoord.x = ((lmw*16.0f) + 2.0f*vertex.lightCoord.x - minTexCoor.x - maxTexCoor.x)/32.0f;
						vertex.lightCoord.y = ((lmh*16.0f) + 2.0f*vertex.lightCoord.y - minTexCoor.y - maxTexCoor.y)/32.0f;
						tempVertices[j] = vertex;
					}

					// create new lightmap
					lightmaps.Add(new Lightmap(lmw, lmh, lmOffset, i));
				}

				// add vertices to faceVertices queue
				for(j = 0; j < tempVertices.Length; j ++) faceVertices.Enqueue(tempVertices[j]);
			}

			// create vertexPool
			vertexPool = new VertexPool(faceVertices.Count);
			for(i = 0; i < vertexPool.Size; i ++) vertexPool[i] = (Vertex)faceVertices.Dequeue();

			// process lightmap data
			//   for enhanced performance we will group multiple lightmaps of more or less similar height together into one
			//   texture. Therefore we first sort them (hint: the Lightmap class implements IComparable for this reason).

			lightmaps.Sort();

			lmh = 2; // lmh = LightMap Height. We start with all maps less then or equal to 2

			// repeat the following operation until all lightmaps are in a texture. Once added to a texture, we will remove
			// a lightmap from the lightmaps array.

			while(lightmaps.Count > 0)
			{
				lmw = 0;
				i = 0;
				count  = 0;

				// this loop checks how many lightmaps are suited to be added to our texture
				while(i < lightmaps.Count)
				{
					lightmap = (Lightmap)lightmaps[i ++];
					if(lightmap.height > lmh) break;
					lmw += lightmap.width;
					if(lmw > 1024)
					{
						lmw -= lightmap.width;
						break;
					}
					count ++;
				}
				if(count == 0)
				{
					lmh = lmh << 1;
					continue;
				}
				i = 1;
				while(i < lmw) i = i << 1;
				lmw = i;
				texture = new Eresys.Texture(lmw, lmh);
				gx = 0;
				for(i = 0; i < count; i ++)
				{
					lightmap = (Lightmap)lightmaps[i];
					j = lightmap.offset;
					for(y = 0; y < lmh; y ++)
					{
						for(x = 0; x < lightmap.width; x ++)
						{
							if(y < lightmap.height)
							{
								try
								{
									texture[gx + x, y] = new Color(lightData[j ++], lightData[j ++], lightData[j ++]);
								}
								catch(Exception)
								{
									texture[gx + x, y] = new Color(255, 255, 255);
								}
							}
							else
							{
								texture[gx + x, y] = new Color(255, 255, 255);
							}
						}
					}
					n = faces[lightmap.face].firstVertex + faces[lightmap.face].numVertices;
					for(j = faces[lightmap.face].firstVertex; j < n; j ++)
					{
						vertex = vertexPool[j];
						vertex.lightCoord.x = (vertex.lightCoord.x + gx)/lmw;
						vertex.lightCoord.y = vertex.lightCoord.y/lmh;
						vertexPool[j] = vertex;
					}

					gx += lightmap.width;
				}
				j = Kernel.Graphics.AddTexture(texture);
				for(i = 0; i < count; i ++) faces[((Lightmap)lightmaps[i]).face].lightmap = j;
				lightmaps.RemoveRange(0, count);
			}

			// load markFaces
			lump = lumps[(int)Lumps.MarkSurfaces];
			if(lump.length%2 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/2;
			markFaces = new ushort[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				markFaces[i] = br.ReadUInt16();
			}

			// load nodes
			lump = lumps[(int)Lumps.Nodes];
			if(lump.length%24 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/24;
			nodes = new Node[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				nodes[i] = new Node();
				nodes[i].plane = br.ReadInt32();
				nodes[i].frontChild = br.ReadInt16();
				nodes[i].backChild = br.ReadInt16();
				br.BaseStream.Position += 16;
			}

			// load leaves
			lump = lumps[(int)Lumps.Leaves];
			if(lump.length%28 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/28;
			leaves = new Leaf[n];
			br.BaseStream.Position = lump.offset + 4;
			for(i = 0; i < n; i ++)
			{
				leaves[i] = new Leaf();
				leaves[i].pvs = br.ReadInt32();
				br.BaseStream.Position += 12;
				leaves[i].firstMarkFace = br.ReadUInt16();
				leaves[i].numMarkFaces = br.ReadUInt16();
				br.BaseStream.Position += 8;
			}

			// load visibility data (PVS sets)
			lump = lumps[(int)Lumps.VisDate];
			br.BaseStream.Position = lump.offset;
			pvs = br.ReadBytes(lump.length);

			// load models
			lump = lumps[(int)Lumps.Models];
			if(lump.length%64 != 0)
			{
				br.Close();
				fs.Close();
				throw new BSPCorruptedException(fileName);
			}
			n = lump.length/64;
			models = new Model[n];
			br.BaseStream.Position = lump.offset;
			for(i = 0; i < n; i ++)
			{
				br.BaseStream.Position += 24;
				models[i].origin.x = br.ReadSingle();
				models[i].origin.z = br.ReadSingle();
				models[i].origin.y = br.ReadSingle();
				br.BaseStream.Position += 20;
				models[i].firstFace = br.ReadInt32();
				models[i].numFaces = br.ReadInt32();
			}

			// process models
			modelScripts = script["func_wall;func_illusionary;func_water;func_breakable"];
			for(i = 0; i < modelScripts.Length; i ++)
			{
				try
				{
					model = modelScripts[i]["model"];
					mode = Byte.Parse(modelScripts[i]["rendermode"]);
					amount = Byte.Parse(modelScripts[i]["renderamt"]);
				}
				catch(IndexOutOfRangeException)
				{
					continue;
				}
				if(model[0] != '*') continue;
				j = Int32.Parse(model.Substring(1));
				for(k = 0; k < models[j].numFaces; k ++)
				{
					Face face = faces[k + models[j].firstFace];
					face.model = true;
					face.textureAlpha = mode == 4;
					face.alpha = amount;
					for(v = 0; v < face.numVertices; v ++)
					{
						vertex = vertexPool[v + face.firstVertex];
						vertex.position.x += models[j].origin.x;
						vertex.position.y += models[j].origin.y;
						vertex.position.z += models[j].origin.z;
						vertexPool[v + face.firstVertex] = vertex;
					}
				}
			}

			// upload vertex pool to the graphics hardware
			vertexPoolI = Kernel.Graphics.AddVertexPool(vertexPool);

			// create model faces array
			modelFaces = new ArrayList();
			for(i = 0; i < faces.Length; i ++)
			{
				Face face = faces[i];
				if(face.texture < 0) continue;
				if(face.model)
				{
					modelFaces.Add(face);
				}
			}

			// create skybox
			try
			{
				range = Int32.Parse(script["worldspawn"][0]["MaxRange"]);
			}
			catch(Exception)
			{
				range = 0x4000;
			}
			string skyname;
			try
			{
				skyname = script["worldspawn"][0]["skyname"] + ".bmp";
			}
			catch(Exception)
			{
				skyname = "";
			}

			Eresys.Texture skyTexture = null;
			if(skyname != "")
			{
				try
				{
					skyTexture = new Eresys.Texture(skyname);
				}
				catch(Exception)
				{
					Kernel.Log.WriteLine("WARNING: Couldn't load sky texture \'" + skyname + "\'!");
					try
					{
						skyTexture = new Eresys.Texture("defaultsky.bmp");
					}
					catch(Exception)
					{
						Kernel.Log.WriteLine("WARNING: Couldn't load default sky texture \'defaultsky.bmp\'!");
						skyTexture = new Eresys.Texture(1, 1);
						skyTexture[0] = new Color(132, 184, 255);
					}
				}
			}
			if(skyTexture != null)
			{
				sky = new SkyBox(range, range, range, skyTexture);
			}
			else
			{
				sky = null;
			}
				
			// check face plane oriëntations
			for(i = 0; i < faces.Length; i ++)
			{
				Face face = faces[i];
				Vector vector1 = vertexPool[face.firstVertex].position - face.center;
				Vector vector2 = vertexPool[face.firstVertex + 1].position - face.center;
				vector1 = vector1.Cross(vector2);
				vector1.Normalize();
				face.inversePlane = vector1.Dot(planes[face.plane].GetNormal()) < 0.0f;
			}

			// close streams
			br.Close();
			fs.Close();
		}

		private bool[] DecompressPvs(int offset)
		{
			int i, v, c, n;
			bool[] res;
			
			v = offset;
			n = leaves.Length - 1;
			res = new bool[n];

			c = 0;
			while(c < n)
			{
				if(v < 0 || v >= pvs.Length)
				{
					res[c ++] = true;
					continue;
				}
				if(pvs[v] == 0) // 0 = repeater byte: next byte contains number of following bytes that are 0.
				{
					v ++;
					i = pvs[v] << 3; // -> x << 3 = x*2^3 = x*8
					while(i > 0 && c < n)
					{
						i --;
						res[c] = false;
						c ++;
					}
					v ++;
					continue;
				}

				// check every bit of the curent byte if it is set
				for(i = 0; i < 8 && c < n; i ++, c ++) res[c] = (pvs[v] & (1 << i)) != 0;
				v ++;
			}

			return res;
		}

		#endregion
	}
}
