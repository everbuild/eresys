using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eresys
{
	/// <summary>
	/// Representatie van een afbeelding. Ondersteunt laden uit verschillende bestandsformaten.
	/// Deze kan mbv. Kernel.Graphics worden ge-upload naar de grafische hardware om gebruikt
	/// te worden bij het renderen.
	/// </summary>
	/// <remarks>Daar sommige methodes in deze class gebruik maken van pointers is het nodig deze
	/// code te compileren met /unsafe.</remarks>
	public class Texture
	{

		#region Constructors

		/// <summary>
		/// Een constructor die een texture aanmaakt met een meegegeven breedte en hoogte.
		/// </summary>
		/// <param name="width">De breedte van de aan te maken texture.</param>
		/// <param name="height">De hoogte van de aan te maken texture.</param>
		public Texture ( int width , int height )
		{
			this.path = "";
			this.archive = "";
			this.name = "";
			this.type = "";
			this.width = width;
			this.height = height;
			this.data = new Color [ width * height ];
		}

		/// <summary>
		/// Een constructor die een texture aanmaakt met een meegegeven naam, breedte en hoogte.
		/// </summary>
		/// <param name="name">De naam van de aan te maken texture.</param>
		/// <param name="width">De breedte van de aan te maken texture.</param>
		/// <param name="height">De hoogte van de aan te maken texture.</param>
		public Texture ( int width , int height , string name )
		{
			this.path = "";
			this.archive = "";
			this.name = name;
			this.type = "";
			this.width = width;
			this.height = height;
			this.data = new Color [ width * height ];
		}

		/// <summary>
		/// Een constructor die een texture aanmaakt van een gegeven kleur, met een meegegeven kleur,
		/// breedte en hoogte.
		/// </summary>
		/// <param name="color">De kleur van de aan te maken texture.</param>
		/// <param name="width">De breedte van de aan te maken texture.</param>
		/// <param name="height">De hoogte van de aan te maken texture.</param>
		public Texture( Color color , int width , int height )
		{
			this.path = "";
			this.archive = "";
			this.name = "";
			this.type = "";
			this.width = width;
			this.height = height;
			this.data = new Color [ width * height ];
			int size = width * height;
			for ( int i = 0 ; i < size ; i++ )
				this.data[i] = color;
		}

		/// <summary>
		/// Een constructor die een texture aanmaakt met een opgegeven bestandsnaam.
		/// </summary>
		/// <param name="texFileName">De bestandsnaam van de te laden texture.</param>
		/// <remarks>Daar in deze functie gebruik wordt gemaakt van pointers is het nodig dat deze code 
		/// gecompileerd wordt met /unsafe. Dit kan voorkomen worden door de Bitmap methode GetPixel te gebruiken
		/// ipv de pointers.</remarks>
		public Texture ( string texFileName )
		{
			// analyze texFileName
			InitAttribs(texFileName);

			// load texture (width, height, data, and eventually mipmaps must be inited by the following code)
			switch(type)
			{
				case ".bmp" :
				case ".exif" :
				case ".gif" :
				case ".jpg" :
				case ".jpeg" :
				case ".png" :
				case ".tiff" :
					Image image = System.Drawing.Image.FromFile ( texFileName );
					this.width = image.Width;
					this.height = image.Height;
					this.data = new Color [ this.width * this.height ];

					Bitmap bitmap = new Bitmap ( image );
					BitmapData bitmapData = bitmap.LockBits ( new Rectangle ( 0 , 0 , this.width , this.height ) , ImageLockMode.ReadWrite , PixelFormat.Format32bppArgb );
			
					int stride = bitmapData.Stride;
					System.IntPtr scan0 = bitmapData.Scan0;

					unsafe 
					{
						byte * p = (byte *)(void *) scan0;
						int nOffset = stride - bitmap.Width * 4;

						for ( int j = 0 ; j < this.height ; ++j ) 
						{
							for ( int k = 0 ; k < this.width ; ++k ) 
							{
								this.data [ j * this.width + k ] = new Color ( p [3] , p [2] , p [1] , p [0] );
								p += 4;
							}
							p += nOffset;
						}
					}

					bitmap.UnlockBits ( bitmapData );
					
					break;
					/* 
					 * De oudere bmp code (ter referentie)
					 * 
					 * FileStream fs = new FileStream ( this.path + this.name , FileMode.Open , FileAccess.Read );
					 * BinaryReader br = new BinaryReader ( fs );
					 * br.BaseStream.Seek ( 18 , SeekOrigin.Begin ); // Skip useless header info...
					 * 
					 * this.width = br.ReadInt32();
					 * this.height = br.ReadInt32();
					 * 
					 * length = this.width * this.height;
					 * 
					 * br.BaseStream.Seek ( 54 , SeekOrigin.Begin ); // Skip to image data...
					 * bytes = br.ReadBytes ( length * 3 );
					 * 
					 * this.data = new Color [ length ];
					 * 
					 * // Eerste is laatste pixel en het is tevens bgr ipv rgb
					 * // De hele array omkeren levert ons de pixels in logische volgorde 
					 * Array.Reverse ( bytes , 0 , bytes.Length );
					 * 
					 * for ( int i = 0 , j = 0 ; i < length ; i++ , j += 3 )
					 *	this.data [ i ] = new Color ( bytes [ j ] , bytes [ j+1 ] , bytes [ j+2 ] );
					 * 
					 * br.Close();
					 * fs.Close();
					 * 
					 */
				default:
					throw new System.IO.FileLoadException("The " + type + " image format or image archive format used by " + texFileName + " is not supported!", texFileName);
			}
		}

		/// <summary>
		/// Een constructor die een texture aanmaakt met een meegegeven WAD3 en een naam.
		/// </summary>
		/// <param name="wad">De WAD3 waaruit de texture moet geladen worden.</param>
		/// <param name="name">De naam van de texture die geladen moet worden.</param>
		public Texture ( Wad3 wad , string name )
		{
			// Variables used for loading
			int length; byte [] bytes;

			this.path = "";
			this.archive = "";
			this.name = name;
			this.type = "";

			Wad3Lump lump = wad.GetLump ( this.name );

			// Set Width and Height of the texture
			this.width = lump.Width;
			this.height = lump.Height;

			// Set length of texture in bytes (8-bit format) & colors
			length = this.width * this.height;

			// Get the bytes for mip level 0 (1) from wad
			bytes = wad.GetBytes ( lump.Offset + lump.Offsets [0] , length );
					
			// Create the color pallet
			Color [] pallet = new Color [ 256 ];
			byte [] palletBytes = wad.GetBytes ( lump.Offset + lump.Offsets [3] + ( length / 64 ) + 2 , 768 );
					
			// Fill the color pallet
			for ( int i = 0 , j = 0 ; i < 256 ; i ++ , j += 3 )
				pallet [ i ] = new Color ( palletBytes [ j ] , palletBytes [ j+1 ] , palletBytes [ j+2 ] );

			// check transparancy
			bool tranparant = name[0] == '{';

			// Create the color array
			this.data = new Color [ length ];
			// Fill the actual data array with the correct (?) values
			for ( int i = 0 ; i < length ; i++ )
			{
				Color pixel = pallet [ bytes [ i ] ];
				if ( tranparant && pixel.ToColorCode() == (int) ColorKey )
					pixel = new Color();
				this.data [ i ] = pixel;
			}
		}

		#endregion

		#region Public Members

		/// <summary>
		/// Een sleutel die gebruikt wordt bij het bepalen van de transparante pixels in een texture.
		/// </summary>
		public static uint ColorKey = 0xFF0000FF;

		#endregion

		#region Properties

		/// <summary>
		/// Naam van de texture (eventueel met de extensie van de afbeelding.)
		/// </summary>
		public string Name { get { return name; } }

		/// <summary>
		/// Type (of extensie) van de texture/afbeelding.
		/// </summary>
		public string Type { get { return type; } }

		/// <summary>
		/// Breedte van de texture/afbeelding.
		/// </summary>
		public int Width { get { return this.width; } }

		/// <summary>
		/// Hoogte van de texture/afbeelding.
		/// </summary>
		public int Height { get { return this.height; } }

		/// <summary>
		/// De array van kleuren van de texture, gegeven van links naar rechts en van boven naar onder.
		/// </summary>
		public Color[] Data { get { return data; } }

		/// <summary>
		/// <code>public Color this [int index]</code><br />
		/// Geeft toegang tot de individuele pixels van de texture met lineaire manie index.
		/// </summary>
		public Color this [ int index ]
		{
			get { return data [ index ]; }
			set { data [ index ] = value; }
		}

		/// <summary>
		/// <code>public Color this [int x, int y]</code><br />
		/// Geeft toegang tot de individuele pixels van de texture met x en y coördinaat.
		/// </summary>
		public Color this [ int x , int y ]
		{
			get { return data [ y * width + x ]; }
			set { data [ y * width + x ] = value; }
		}

		/// <summary>
		/// Pad naar het bestand van deze texture.
		/// </summary>
		public string Path { get { return path; } }

		/// <summary>
		/// Naam van het archief waaruit de texture afkomstig is.
		/// </summary>
		public string Archive { get { return archive; } }

		#endregion

		#region Public Methods

		/// <summary>
		/// Slaat de texture op naar een bestand met een meegegeven naam. Het type van het bestand wordt bepaald
		/// op basis van de opgegeven bestandsnaam en dient een van de ondersteunde types aan te duiden.
		/// (Zie SupportedExtensions)
		/// </summary>
		/// <param name="texFileName">De bestandsnaam naar waar de texture moet opgeslaan worden.</param>
		/// <remarks>Daar in deze functie gebruik wordt gemaakt van pointers is het nodig dat deze code 
		/// gecompileerd wordt met /unsafe. Dit kan voorkomen worden door de Bitmap methode SetPixel te gebruiken
		/// ipv de pointers.</remarks>
		public void SaveToFile ( string texFileName )
		{
			for ( int i = 0 ; i < SupportedExtensions.Length ; i++  ) 
			{
				if ( texFileName.EndsWith ( SupportedExtensions [ i ] ) ) 
				{
					Bitmap bitmap = new Bitmap ( this.width , this.height , PixelFormat.Format24bppRgb );
					BitmapData bitmapData = bitmap.LockBits ( new Rectangle ( 0 , 0 , this.width , this.height ) , ImageLockMode.ReadWrite , PixelFormat.Format24bppRgb );
			
					int stride = bitmapData.Stride;
					System.IntPtr scan0 = bitmapData.Scan0;

					unsafe 
					{
						byte * p = (byte *)(void *) scan0;
						int nOffset = stride - bitmap.Width * 3;

						for ( int j = 0 ; j < this.height ; ++j ) 
						{
							for ( int k = 0 ; k < this.width ; ++k ) 
							{
								p [0] = this.data [ j * this.width + k ].b;
								p [1] = this.data [ j * this.width + k ].g;
								p [2] = this.data [ j * this.width + k ].r;
								p += 3;
							}
							p += nOffset;
						}
					}

					bitmap.UnlockBits ( bitmapData );

					Image image = System.Drawing.Image.FromHbitmap ( bitmap.GetHbitmap() );
					ImageFormat imageFormat;
					if ( texFileName.EndsWith ( ".jpg" ) || texFileName.EndsWith ( ".jpeg" ) ) 
					{
						ImageCodecInfo [] imageEncoders = ImageCodecInfo.GetImageEncoders();
						ImageCodecInfo codecInfo = null;
						foreach ( ImageCodecInfo codec in imageEncoders ) {
							if ( codec.MimeType == "image/jpeg" )
								codecInfo = codec;
						}
						if ( codecInfo != null ) 
						{
							Encoder encoder = Encoder.Quality;
							EncoderParameters encoderParams = new EncoderParameters ( 1 );
							encoderParams.Param [ 0 ] = new EncoderParameter ( encoder , 90L );
							image.Save ( texFileName , codecInfo , encoderParams );
						} else
							throw new FormatException ( "Format for " + texFileName + " not supported!" );
					}
					else 
					{
						if ( texFileName.EndsWith ( ".gif" ) )
							imageFormat = ImageFormat.Gif;
						else if ( texFileName.EndsWith ( ".png" ) )
							imageFormat = ImageFormat.Png;
						else if ( texFileName.EndsWith ( ".tiff" ) )
							imageFormat = ImageFormat.Tiff;
						else if ( texFileName.EndsWith ( ".exif" ) )
							imageFormat = ImageFormat.Exif;
						else
							imageFormat = ImageFormat.Bmp;
						image.Save ( texFileName , imageFormat );
					}
					return;
				}
			}
			if ( texFileName.EndsWith ( ".raw" ) ) {
				FileStream rawFs = new FileStream ( texFileName , FileMode.OpenOrCreate , FileAccess.ReadWrite );
				BinaryWriter rawBw = new BinaryWriter ( rawFs );
				foreach ( Color c in this.data ) 
				{
					rawBw.Write ( c.r );
					rawBw.Write ( c.g );
					rawBw.Write ( c.b );
				}
				rawBw.Close();
				rawFs.Close();
				return;
			}
			throw new FormatException ( "Format for " + texFileName + " not supported!" );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Private.<br />
		/// Deze methode wordt gebruikt om het pad, de naam en het type van een texture dat geladen wordt
		/// vanuit een bestand te initialiseren.
		/// </summary>
		/// <param name="texFileName">De te onderzoeken string.</param>
		private void InitAttribs(string texFileName)
		{
			int i, j, k;

			i = texFileName.LastIndexOf('/');
			if(i < 0) i = texFileName.LastIndexOf('\\');
			if(i < 0)
			{
				path = "";
				name = texFileName;
			}
			else
			{
				path = texFileName.Substring(0, i);
				name = texFileName.Substring(i + 1);
			}
			for(j = 0; j < SupportedArchives.Length; j ++)
			{
				i = path.LastIndexOf(SupportedArchives[j]);
				if(i >= 0) break;
			}
			if(i < 0)
			{
				k = name.LastIndexOf('.');
				if(k <0)
					type = "";
				else
					type = name.Substring(k);
				archive = "";
			}
			else
			{
				type = SupportedArchives[j];
				k = path.LastIndexOf('/', i);
				if(k < 0) k = path.LastIndexOf('\\', i);
				if(k < 0)
				{
					archive = path;
					path = "";
				}
				else
				{
					archive = path.Substring(k + 1);
					path = path.Substring(0, k);
				}
			}
			if ( !path.EndsWith ( @"\" ) && path != "" )
				path += @"\";
		}

		#endregion

		#region Private Members

		/// <summary>
		/// Private.<br />
		/// Deze array van strings bevat de extensies voor de ondersteunde bestandsformaten (met uitzondering van RAW.) De 
		/// ondersteunde formaten zijn bmp, exif, gif, jpg of jpeg, png en tiff.
		/// </summary>
		private static string [] SupportedExtensions = new string [7] { "bmp" , "exif" , "gif" , "jpg" , "jpeg" , "png" , "tiff" };
		/// <summary>
		/// Private.<br />
		/// Deze array van strings bevat de extensies voor de ondersteunde archief formaten.
		/// </summary>
		private static string [] SupportedArchives = new string [1] { ".wad" };
		/// <summary>
		/// Private.
		/// </summary>
		private string name;
		/// <summary>
		/// Private.
		/// </summary>
		private int width, height;
		/// <summary>
		/// Private.
		/// </summary>
		private Color[] data;
		/// <summary>
		/// Private.
		/// </summary>
		private string type;
		/// <summary>
		/// Private.
		/// </summary>
		private string path;
		/// <summary>
		/// Private.
		/// </summary>
		private string archive;

		#endregion
	}
}
