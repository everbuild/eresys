using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Eresys
{
	/// <summary>
	/// DirectX implementatie van IGraphics. Zie IGraphics voor meer info.
	/// </summary>
	public class DXGraphics: IGraphics
	{
		public bool WireFrame
		{
			get
			{
				return wire;
			}
			set
			{
				if(wire == value) return;
				wire = value;
				SetStates();
			}
		}

		public bool Lighting
		{
			get
			{
				return light;
			}
			set
			{
				if(light == value) return;
				light = value;
				SetStates();
			}
		}

		public bool AlphaBlending
		{
			get
			{
				return alphaBlend;
			}
			set
			{
				if(alphaBlend == value) return;
				alphaBlend = value;
				SetStates();
			}
		}

		public bool TextureAlpha
		{
			get
			{
				return texAlpha;
			}
			set
			{
				if(texAlpha == value) return;
				texAlpha = value;
				SetStates();
			}
		}

		public byte Alpha
		{
			get
			{
				return alpha;
			}
			set
			{
				if(alpha == value) return;
				alpha = value;
				device.TextureState[0].ConstantColor = System.Drawing.Color.FromArgb(alpha, device.TextureState[0].ConstantColor);
			}
		}

		public bool DepthBuffer
		{
			get
			{
				return zbuffer;
			}
			set
			{
				if(zbuffer == value) return;
				zbuffer = value;
				SetStates();
			}
		}

		public bool Filtering
		{
			get
			{
				return filter;
			}
			set
			{
				if(filter == value) return;
				filter = value;
				SetStates();
			}
		}

		public Camera Camera
		{
			get
			{
				return camera;
			}
			set
			{
				camera = value;
			}
		}

		public Matrix WorldMatrix
		{
			get
			{
				return DXM2EM(device.Transform.World);
			}
			set
			{
				device.Transform.World = EM2DXM(value);
			}
		}

		public float Brightness
		{
			get
			{
				return brightness;
			}
			set
			{
				if(value >= 1.0f) brightness = 1.0f;
				else if(value <= 0.0f) brightness = 0.0f;
				else brightness = value;
				gammaRamp = BuildGammaRamp(brightness, contrast, gamma);
				reloadTextures = true;
			}
		}

		public float Contrast
		{
			get
			{
				return contrast;
			}
			set
			{
				if(value >= 1.0f) contrast = 1.0f;
				else if(value <= 0.0f) contrast = 0.0f;
				else contrast = value;
				gammaRamp = BuildGammaRamp(brightness, contrast, gamma);
				reloadTextures = true;
			}
		}

		public float Gamma
		{
			get
			{
				return gamma;
			}
			set
			{
				gamma = value;
				gammaRamp = BuildGammaRamp(brightness, contrast, gamma);
				reloadTextures = true;
			}
		}

		public bool FrameClearing
		{
			get
			{
				return clear;
			}
			set
			{
				clear = value;
			}
		}

		public const VertexFormats VERTEX_FVF = VertexFormats.Position | VertexFormats.Texture2;
		public const int VERTEX_SIZE = 20;

		public DXGraphics()
		{
			// init members
			wire = false;
			light = true;
			alphaBlend = false;
			texAlpha = true;
			alpha = 255;
			zbuffer = true;
			filter = true;
			camera = null;
			reloadTextures = false;
			rendering = false;
			vertexPools = new ArrayList();
			textures = new ArrayList();
			fonts = new ArrayList();
			clear = false;

			// get settings
			fullscreen = Boolean.Parse(Kernel.Settings["fullscreen"]);
			width = Int32.Parse(Kernel.Settings["width"]);
			height = Int32.Parse(Kernel.Settings["height"]);
			depth = Int32.Parse(Kernel.Settings["depth"]);
			hal = Boolean.Parse(Kernel.Settings["hal"]);
			tnl = Boolean.Parse(Kernel.Settings["tnl"]);
			vsync = Boolean.Parse(Kernel.Settings["vsync"]);
			System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
			nfi.NumberDecimalSeparator = ".";
			brightness = Single.Parse(Kernel.Settings["brightness"], nfi);
			contrast = Single.Parse(Kernel.Settings["contrast"], nfi);
			gamma = Single.Parse(Kernel.Settings["gamma"], nfi);
			gammaRamp = BuildGammaRamp(brightness, contrast, gamma);

			// Log
			Kernel.Log.WriteLine("Initialising graphics...");
			Kernel.Log.WriteLine("  Display Mode:       " + width + "x" + height + "x" + depth + " " + (fullscreen ? "fullscreen" : "windowed"));
			Kernel.Log.WriteLine("  Using HAL:          " + (hal ? "yes" : "no (software emulation with Microsoft DirectX Reference Driver)"));
			Kernel.Log.WriteLine("  Using hardware T&L: " + (tnl ? "yes" : "no"));
			Kernel.Log.WriteLine();

			// init window
			Kernel.Form.Text = "Eresys";
			Kernel.Form.Icon = new Icon(this.GetType(), "Eresys.ico");
			Kernel.Form.BackColor = System.Drawing.Color.Black;
			if(!fullscreen)
			{
				Kernel.Form.ClientSize = new Size(width, height);
				Kernel.Form.StartPosition = FormStartPosition.CenterScreen;
			}

			// init D3D

			// get display format
			Format format = Format.Unknown;
			if(fullscreen)
				switch(depth)
				{
					case 16:
						format = Format.R5G6B5;
						break;
					case 24:
						format = Format.R8G8B8;
						break;
					case 32:
						format = Format.X8R8G8B8;
						break;
				}

			// init device
			PresentParameters presentParams = new PresentParameters();
			presentParams.Windowed = !fullscreen;
			presentParams.SwapEffect = SwapEffect.Copy;
			presentParams.BackBufferCount = 1;
			presentParams.BackBufferFormat = format;
			presentParams.EnableAutoDepthStencil = true;
			presentParams.AutoDepthStencilFormat = DepthFormat.D16;
			presentParams.PresentationInterval = vsync ? PresentInterval.One : PresentInterval.Immediate;
			if(fullscreen)
			{
				presentParams.BackBufferWidth = width;
				presentParams.BackBufferHeight = height;
				presentParams.FullScreenRefreshRateInHz = PresentParameters.DefaultPresentRate;
			}
			device = new Device(0, hal ? DeviceType.Hardware : DeviceType.Reference, Kernel.Form, tnl ? CreateFlags.HardwareVertexProcessing : CreateFlags.SoftwareVertexProcessing, presentParams);

			// set states
			device.RenderState.SourceBlend = Blend.SourceAlpha;
			device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
			device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
			device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
			device.TextureState[0].ConstantColor = System.Drawing.Color.FromArgb(0, 0, 255);
			SetStates();

			// init 2d vertex buffer
			vb2d = new VertexBuffer ( typeof ( CustomVertex.PositionTextured ) , 4 , device , Usage.WriteOnly , 	CustomVertex.PositionTextured.Format , Pool.Default );
			
			CustomVertex.PositionTextured[] data = new CustomVertex.PositionTextured[4];

			data[0].X  = -0.5f; 
			data[0].Y  = -0.5f;
			data[0].Z  =  0.0f;
			data[0].Tu =  0.0f;
			data[0].Tv =  1.0f;

			data[1].X  = -0.5f;
			data[1].Y  =  0.5f;
			data[1].Z  =  0.0f;
			data[1].Tu =  0.0f;
			data[1].Tv =  0.0f;

			data[2].X  =  0.5f; 
			data[2].Y  =  0.5f;
			data[2].Z  =  0.0f;
			data[2].Tu =  1.0f;
			data[2].Tv =  0.0f;

			data[3].X  =  0.5f; 
			data[3].Y  = -0.5f;
			data[3].Z  =  0.0f;
			data[3].Tu =  1.0f;
			data[3].Tv =  1.0f;

			vb2d.SetData(data, 0, 0);

			// show window
			Cursor.Hide();
			Kernel.Form.Show();
		}

		public int AddVertexPool(VertexPool vertexPool)
		{
			// create buffer
			VertexBuffer vb = new VertexBuffer(typeof(Vertex), vertexPool.Size, device, Usage.Dynamic, VERTEX_FVF, Pool.Default);

			// fill buffer
			vb.SetData(vertexPool.GetData(), 0, 0);

			// add vertexPool
			return vertexPools.Add(new VPListElement(vertexPool, vb));
		}

		public void RemoveVertexPool(int vertexPoolIdx)
		{
			((VPListElement)vertexPools[vertexPoolIdx]).vb.Dispose();
			vertexPools.RemoveAt(vertexPoolIdx);
		}

		public int AddTexture(Texture texture)
		{
			int w, nw, h, nh, size, idx;

			// calculate new dimensions
			w = texture.Width;
			h = texture.Height;
			nw = 1;
			nh = 1;
			while(nw < w) nw = nw << 1;
			while(nh < h) nh = nh << 1;
			size = nw*nh;

			// create texture
			Microsoft.DirectX.Direct3D.Texture dxtex = new Microsoft.DirectX.Direct3D.Texture(device, nw, nh, 1, 0, Format.A8R8G8B8, Pool.Managed);
			//Microsoft.DirectX.Direct3D.Texture dxtex = new Microsoft.DirectX.Direct3D.Texture(device, nw, nh, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);

			// add texture
			idx = textures.Add(new TexListElement(texture, dxtex));

			// load texture
			LoadTexture(idx);

			return idx;
		}

		private const int LogPixelsY = 90;

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern int GetDeviceCaps(IntPtr hdc, int cap);

		public int AddFont(string name, float size, bool bold, bool italic)
		{
			int height;
			int logPixels;

			Graphics g = Kernel.Form.CreateGraphics();
			System.IntPtr dc = g.GetHdc();
			logPixels = GetDeviceCaps(dc, LogPixelsY);
			g.ReleaseHdc(dc);
			g.Dispose();

			height = (int)(-size*(float)logPixels/72.0f);

			Microsoft.DirectX.Direct3D.Font font = new Microsoft.DirectX.Direct3D.Font(device, height, 0, bold ? FontWeight.Bold : FontWeight.Regular, 0, italic, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch | PitchAndFamily.FamilyDoNotCare, name);

			return fonts.Add(new FontListElement(font));
		}

		public void RemoveTexture(int textureIdx)
		{
			((TexListElement)textures[textureIdx]).dxtex.Dispose();
			textures.RemoveAt(textureIdx);
		}

		public void RenderTriangleFan(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
		{
			if(!rendering) return;

			VertexBuffer vb = null;
			Microsoft.DirectX.Direct3D.Texture dxtex = null;
			Microsoft.DirectX.Direct3D.Texture dxlm = null;

			// get vertex buffer
			vb = ((VPListElement)vertexPools[vertexPoolIdx]).vb;

			// get texture & lightmap
			dxtex = ((TexListElement)textures[textureIdx]).dxtex;
			if(light) dxlm = ((TexListElement)textures[lightmapIdx]).dxtex;

			// set stream source (= VB), and FVF
			device.SetStreamSource(0, vb, 0);
			device.VertexFormat = VERTEX_FVF;

			// set textures
			if(!wire)
			{
				device.SetTexture(0, dxtex);
				if(light) device.SetTexture(1, dxlm);
			}

			// render
			device.DrawPrimitives(PrimitiveType.TriangleFan, (int)first, (int)count - 2);
		}

		public void RenderTriangleStrip(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
		{
			if(!rendering) return;

			VertexBuffer vb = null;
			Microsoft.DirectX.Direct3D.Texture dxtex = null;
			Microsoft.DirectX.Direct3D.Texture dxlm = null;

			// get vertex buffer
			vb = ((VPListElement)vertexPools[vertexPoolIdx]).vb;

			// get texture & lightmap
			dxtex = ((TexListElement)textures[textureIdx]).dxtex;
			if(light) dxlm = ((TexListElement)textures[lightmapIdx]).dxtex;

			// set stream source (= VB), and FVF
			device.SetStreamSource(0, vb, 0);
			device.VertexFormat = VERTEX_FVF;

			// set textures
			if(!wire)
			{
				device.SetTexture(0, dxtex);
				if(light) device.SetTexture(1, dxlm);
			}

			// render
			device.DrawPrimitives(PrimitiveType.TriangleStrip, (int)first, (int)count - 2);
		}

		public void RenderText(int fontIdx, Color color, Point2D position, string text)
		{
			if(!rendering) return;

			Microsoft.DirectX.Direct3D.Font font = null;

			font = ((FontListElement)fonts[fontIdx]).font;

			font.DrawText(null, text, new Rectangle((int)position.x, (int)position.y, 0, 0), DrawTextFormat.NoClip, color.ToColorCode());
		}

		public void RenderTexture(int textureIdx, float left, float top, float width, float height, float depth)
		{
			if(!rendering) return;
			if(wire) return;

			Microsoft.DirectX.Direct3D.Texture dxtex = null;
			Microsoft.DirectX.Matrix mProj, mView, mWorld;
			bool zbuffer;

			// get texture
			dxtex = ((TexListElement)textures[textureIdx]).dxtex;

			// backup transformation matrices
			mProj = device.Transform.Projection;
			mView = device.Transform.View;
			mWorld = device.Transform.World;

			// Setup an orthographic perspective
			device.Transform.Projection = Microsoft.DirectX.Matrix.OrthoLH(1.0f, 1.0f, 0.0f, 1.0f);
			device.Transform.View = Microsoft.DirectX.Matrix.Identity;
			device.Transform.World = Microsoft.DirectX.Matrix.Scaling(width, height, 1.0f)*Microsoft.DirectX.Matrix.Translation(left, top, depth);

			// backup and set depthbuffer
			zbuffer = DepthBuffer;
			DepthBuffer = false;

			// Set render and texture stage states
			device.RenderState.AlphaBlendEnable = true;
			device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
			device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
			device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
			device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
			device.TextureState[1].ColorOperation = TextureOperation.Disable;

			// set texture
			device.SetTexture(0, dxtex);

			// set stream source (= VB), and FVF
			device.SetStreamSource(0, vb2d, 0);
			device.VertexFormat = CustomVertex.PositionTextured.Format;
			
			// Render texture
			device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);

			// Restore states
			SetStates();

			// restire DepthBuffer
			DepthBuffer = zbuffer;

			// Restore transformation matrices
			device.Transform.Projection = mProj;
			device.Transform.View = mView;
			device.Transform.World = mWorld;
		}

		public void BeginFrame()
		{
			if(rendering) return;

			if(reloadTextures)
			{
				for(int i = 0; i < textures.Count; i ++) LoadTexture(i);
				reloadTextures = false;
			}

			// update matrices if neccesary
			if(camera != null)
			{
				device.Transform.View = EM2DXM(camera.ViewMatrix);
				device.Transform.Projection = EM2DXM(camera.ProjectionMatrix);
				device.Transform.World = Microsoft.DirectX.Matrix.Identity;
			}

			ClearFlags flags = ClearFlags.ZBuffer;
			if(clear) flags |= ClearFlags.Target;
			device.Clear(flags, 0, Camera.DISTANCE, 0);
			device.BeginScene();

			rendering = true;
		}

		public void EndFrame()
		{
			if(!rendering) return;

			device.EndScene();
			device.Present();
			rendering = false;
		}

		public void Dispose()
		{
			vb2d.Dispose();
			while(textures.Count > 0) RemoveTexture(0);
			while(vertexPools.Count > 0) RemoveVertexPool(0);
			device.Dispose();
			Kernel.Form.Close();
			Cursor.Show();
		}

		public Texture TakeScreenshot()
		{
			DisplayMode dm = device.DisplayMode;
			Microsoft.DirectX.Direct3D.Texture shot = new Microsoft.DirectX.Direct3D.Texture(device, dm.Width, dm.Height, 1, 0, Format.A8R8G8B8, Pool.SystemMemory);
			device.GetFrontBufferData(0, shot.GetSurfaceLevel(0));
			System.Drawing.Rectangle rect;
			if(fullscreen) rect = new Rectangle(0, 0, width, height);
			else rect = new Rectangle(Kernel.Form.PointToScreen(new System.Drawing.Point(0, 0)), new Size(width, height));
			int[] pixels = (int[])shot.LockRectangle(typeof(int), 0, LockFlags.None, dm.Width*dm.Height);
			Texture res = new Texture(width, height);
			int i = 0;
			for(int y = rect.Top; y < rect.Bottom; y ++) for(int x = rect.Left; x < rect.Right; x ++) res[i ++] = Color.FromColorCode(pixels[x + y*dm.Width]);
			shot.UnlockRectangle(0);
			return res;
		}

		private void SetStates()
		{
			device.RenderState.ZBufferEnable = zbuffer && !wire;
			device.RenderState.AlphaBlendEnable = alphaBlend && !wire;
			if(wire)
			{
				device.RenderState.FillMode = FillMode.WireFrame;
				device.RenderState.CullMode = Cull.None;
				device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
				device.TextureState[0].ColorArgument1 = TextureArgument.Constant;
			}
			else
			{
				device.RenderState.FillMode = FillMode.Solid;
				device.RenderState.CullMode = Cull.None; //Cull.CounterClockwise;
				device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
				device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
				if(filter)
				{
					device.SamplerState[0].MinFilter = TextureFilter.Linear;
					device.SamplerState[0].MagFilter = TextureFilter.Linear;
				}
				else
				{
					device.SamplerState[0].MinFilter = TextureFilter.None;
					device.SamplerState[0].MagFilter = TextureFilter.None;
				}
				if(texAlpha)
				{
					device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
					device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
				}
				else
				{
					device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
					device.TextureState[0].AlphaArgument1 = TextureArgument.Constant;
				}
			}
			if(light && !wire)
			{
				device.TextureState[1].ColorOperation = TextureOperation.Modulate;
				device.TextureState[1].ColorArgument1 = TextureArgument.Current;
				device.TextureState[1].ColorArgument2 = TextureArgument.TextureColor;
				device.SamplerState[1].MagFilter = TextureFilter.Linear;
			}
			else
			{
				device.TextureState[1].ColorOperation = TextureOperation.Disable;
			}
		}

		private byte[] BuildGammaRamp(float brightness, float contrast, float gamma)
		{
			/*
			 * brightness: 0.0 ~ 1.0 std = 0.5
			 * contrast:   0.0 ~ 1.0 std = 0.5
			 * gamma:      x.x ~ x.x std = 1.0
			 */

			byte[] table = new byte[256];

			double m = 512*brightness - 128; // m = Middle: het middelpunt van de grafiek op de y-as. Dit wordt bepaald door brightness. Hoe helderder, hoe hoger dit zal liggen; en omgekeerd.
			double v; // v = Value: de y-waarde/hoogte van de grafiek op een bepaald punt
			int i;

			if((contrast + 0.001) >= 1.0)
			{
				for(i = 0; i < 256; i ++)
				{
					// calc value using contrast and brightness
					v = i < 128 ? (m - 128) : (m + 128);
					// gamma correction
					v = 255.0*System.Math.Pow((v + 0.5)/255.5, gamma) + 0.5;
					if(v > 255) v = 255;
					if(v < 0) v = 0;
					table[i] = (byte)v;
				}
			}
			else
			{
				double dv = System.Math.Tan(contrast*(float)System.Math.PI/2.0f);
				for(i = 0; i < 256; i ++)
				{
					v = (i - 128)*dv + m;
					v = 255.0*System.Math.Pow((v + 0.5)/255.5, gamma) + 0.5;
					if(v > 255) v = 255;
					if(v < 0) v = 0;
					table[i] = (byte)v;
				}
			}

			return table;
		}

		private void LoadTexture(int i)
		{
			int w, nw, h, nh, x, y, ip, size;
			int[] pixels;
			float ox, oy, dx, dy, fox, foy, a, r, g, b, fw, fh, otl, otr, obl, obr;
			int ifox, ifoy;
			TexListElement tex;
			SurfaceDescription desc;
			Color pixel, opx;

			// get texture and calculate new dimensions
			tex = (TexListElement)textures[i];
			desc = tex.dxtex.GetLevelDescription(0);
			w = tex.tex.Width;
			h = tex.tex.Height;
			nw = desc.Width;
			nh = desc.Height;
			size = nw*nh;

			// init some vars
			pixel = new Color(0, 0, 0, 0);
			ip = 0;
			oy = foy = 0.0f;
			ifoy = 0;
			dx = (float)w/(float)nw;
			dy = (float)h/(float)nh;
			fw = (float)w;
			fh = (float)h;

			// lock texture for writing
			pixels = (int[])tex.dxtex.LockRectangle(typeof(int), 0, LockFlags.None, size);

			for(y = 0; y < nh; y ++)
			{
				ox = fox = 0.0f;
				ifox = 0;
				for(x = 0; x < nw; x ++)
				{
					// bepaal oppervlaktes

					// top left pixel
					otl = (fox - ox + 1.0f)*(foy - oy + 1.0f);

					// top right pixel
					otr = (ox - fox)*(foy - oy + 1.0f);

					// bottom left pixel
					obl = (fox - ox + 1.0f)*(oy - foy);

					// bottom right pixel
					obr = (ox - fox)*(oy - foy);

					if(fox + 1 == fw)
					{
						otl += otr;
						otr = 0.0f;
						obl += obr;
						obr = 0.0f;
					}
					if(foy + 1 == fh)
					{
						otl += obl;
						obl = 0.0f;
						otr += obr;
						obr = 0.0f;
					}

					// zet pixelwarden

					a = r = g = b = 0.0f;

					// top left pixel
					if(otl > 0.0f)
					{
						opx = tex.tex[ifox, ifoy];
						a += otl*(float)opx.a;
						r += otl*(float)opx.r;
						g += otl*(float)opx.g;
						b += otl*(float)opx.b;
					}

					// top right pixel
					if(otr > 0.0f)
					{
						opx = tex.tex[ifox + 1, ifoy];
						a += otr*(float)opx.a;
						r += otr*(float)opx.r;
						g += otr*(float)opx.g;
						b += otr*(float)opx.b;
					}

					// bottom left pixel
					if(obl > 0.0f)
					{
						opx = tex.tex[ifox, ifoy + 1];
						a += obl*(float)opx.a;
						r += obl*(float)opx.r;
						g += obl*(float)opx.g;
						b += obl*(float)opx.b;
					}

					// bottom right pixel
					if(obr > 0.0f)
					{
						opx = tex.tex[ifox + 1, ifoy + 1];
						a += obr*(float)opx.a;
						r += obr*(float)opx.r;
						g += obr*(float)opx.g;
						b += obr*(float)opx.b;
					}

					// implement gamma ramp

					pixel.a = (byte)a;
					pixel.r = gammaRamp[(byte)r];
					pixel.g = gammaRamp[(byte)g];
					pixel.b = gammaRamp[(byte)b];

					// set pixel

					pixels[ip ++] = pixel.ToColorCode();

					ox += dx;
					fox = (float)(int)ox;
					ifox = (int)fox;
				}
				oy += dy;
				foy = (float)(int)oy;
				ifoy = (int)foy;
			}
			tex.dxtex.UnlockRectangle(0);

			/* THIS IS THE OLDER, MORE SIMPLISTIC ALGORITM
			int w, nw, h, nh, size, x, y, ip;
			int[] pixels;
			float ox, oy, dox, doy;
			TexListElement tex;
			SurfaceDescription desc;

			tex = (TexListElement)textures[i];
			desc = tex.dxtex.GetLevelDescription(0);
			w = tex.tex.Width;
			h = tex.tex.Height;
			nw = desc.Width;
			nh = desc.Height;
			size = nw*nh;

			dox = (float)(w)/(float)nw;
			doy = (float)(h)/(float)nh;
			pixels = (int[])tex.dxtex.LockRectangle(typeof(int), 0, LockFlags.None, size);
			ip = 0;
			for(y = 0, oy = 0.0f; y < nh; y ++, oy += doy)
				for(x = 0, ox = 0.0f; x < nw; x ++, ox += dox)
				{
					Color pixel = tex.tex[(int)System.Math.Round(ox), (int)System.Math.Round(oy)];
					pixel.r = gammaRamp[pixel.r];
					pixel.g = gammaRamp[pixel.g];
					pixel.b = gammaRamp[pixel.b];
					pixels[ip ++] = pixel.ToColorCode();
				}
			tex.dxtex.UnlockRectangle(0);
			*/
		}

		private class VPListElement
		{
			public VertexPool vp;
			public Microsoft.DirectX.Direct3D.VertexBuffer vb;

			public VPListElement(VertexPool vp, Microsoft.DirectX.Direct3D.VertexBuffer vb)
			{
				this.vp = vp;
				this.vb = vb;
			}
		}

		private class TexListElement
		{
			public Texture tex;
			public Microsoft.DirectX.Direct3D.Texture dxtex;

			public TexListElement(Texture tex, Microsoft.DirectX.Direct3D.Texture dxtex)
			{
				this.tex = tex;
				this.dxtex = dxtex;
			}
		}

		private class FontListElement
		{
			public Microsoft.DirectX.Direct3D.Font font;

			public FontListElement(Microsoft.DirectX.Direct3D.Font font)
			{
				this.font = font;
			}
		}

		private static Matrix DXM2EM(Microsoft.DirectX.Matrix from)
		{
			Matrix to = new Matrix();

			to.element[0, 0] = from.M11;
			to.element[1, 0] = from.M12;
			to.element[2, 0] = from.M13;
			to.element[3, 0] = from.M14;

			to.element[0, 1] = from.M21;
			to.element[1, 1] = from.M22;
			to.element[2, 1] = from.M23;
			to.element[3, 1] = from.M24;

			to.element[0, 2] = from.M31;
			to.element[1, 2] = from.M32;
			to.element[2, 2] = from.M33;
			to.element[3, 2] = from.M34;

			to.element[0, 3] = from.M41;
			to.element[1, 3] = from.M42;
			to.element[2, 3] = from.M43;
			to.element[3, 3] = from.M44;

			return to;
		}

		private static Microsoft.DirectX.Matrix EM2DXM(Matrix from)
		{
			Microsoft.DirectX.Matrix to = new Microsoft.DirectX.Matrix();

			to.M11 = from.element[0, 0];
			to.M12 = from.element[1, 0];
			to.M13 = from.element[2, 0];
			to.M14 = from.element[3, 0];

			to.M21 = from.element[0, 1];
			to.M22 = from.element[1, 1];
			to.M23 = from.element[2, 1];
			to.M24 = from.element[3, 1];

			to.M31 = from.element[0, 2];
			to.M32 = from.element[1, 2];
			to.M33 = from.element[2, 2];
			to.M34 = from.element[3, 2];

			to.M41 = from.element[0, 3];
			to.M42 = from.element[1, 3];
			to.M43 = from.element[2, 3];
			to.M44 = from.element[3, 3];

			return to;
		}

		private bool fullscreen, hal, tnl;
		private int width, height, depth;
		private bool wire, light, alphaBlend, texAlpha, zbuffer, vsync, filter;
		private byte alpha;
		private float brightness, contrast, gamma;
		private bool rendering;
		private bool clear;

		private byte[] gammaRamp;
		private bool reloadTextures;

		private Camera camera;

		private ArrayList vertexPools;
		private ArrayList textures;
		private ArrayList fonts;

		private Device device;

		private VertexBuffer vb2d; // vertex buffer used for 2D texture drawing
	}
}
