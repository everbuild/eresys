using System;
using System.IO;

namespace Eresys {

	#region WAD Basis

	/// <summary>
	/// Deze abstracte klasse beschrijft de basis die over alle WAD-versies heen dezelfde is.
	/// Elke specifieke WAD implementatie hoort deze basis te implementeren.
	/// </summary>
	public abstract class WadBase 
	{
		/// <summary>
		/// De default constructor.
		/// </summary>
		protected WadBase () {
			this.wadname    = null;
			this.byteReader = null;
			this.header     = new WadHeader();
		}

		/// <summary>
		/// Deze constructor maakt een nieuw WAD object aan met de meegegeven WAD.
		/// </summary>
		/// <param name="wadname">De bestandsnaam (inclusief pad) van de te interpreteren WAD.</param>
		protected WadBase ( string wadname ) {
			this.wadname = wadname;
			this.byteReader = new BinaryReader ( new FileStream ( wadname , FileMode.Open , FileAccess.Read ) );
			this.header     = new WadHeader ( new string ( byteReader.ReadChars ( 4 ) ) , 
					byteReader.ReadInt32() , byteReader.ReadInt32() );
		}

		/// <summary>
		/// Een property die de header van de WAD terug geeft.
		/// </summary>
		public WadHeader Header {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.header; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.header = value; }
		}

		/// <summary>
		/// Een methode die een reeks bytes uit de WAD rechtstreeks terug geeft.
		/// </summary>
		/// <param name="offset">De positie van de eerste byte.</param>
		/// <param name="length">De lengte van de reeks bytes die verwacht wordt.</param>
		/// <returns></returns>
		public byte [] GetBytes ( int offset , int length ) {
			this.byteReader.BaseStream.Seek ( offset , SeekOrigin.Begin );
			return this.byteReader.ReadBytes ( length );
		}

		/// <summary>
		/// Protected.
		/// </summary>
		protected string       wadname;
		/// <summary>
		/// Protected.
		/// </summary>
		protected BinaryReader byteReader;
		/// <summary>
		/// Protected.
		/// </summary>
		protected WadHeader    header;
	}

	/// <summary>
	/// Een struct die de structuur van een WAD header beschrijft
	/// </summary>
	public struct WadHeader
	{
		/// <summary>
		/// Maakt een nieuwe WAD header aan met de meegegeven gegevens.
		/// </summary>
		/// <param name="id">De ID van de WAD. Deze heeft steeds de vorm WADx, waarbij x de versie aangeeft. (HL WADs hebben als ID WAD3)</param>
		/// <param name="numLumps">Het aantal lumps dat aanwezig is in de WAD. Lumps zijn dataeenheden, in vroegere versies (Doom) konden deze allerlei soorten data bevatten, tegenwoordig worden deze slechts gebruikt voor textures.</param>
		/// <param name="dirOffset">Geeft de offset van de directory aan (in bytes.)</param>
		public WadHeader ( string id , int numLumps , int dirOffset ) {
			this.id = id;
			this.numLumps = numLumps;
			this.directoryOffset = dirOffset;
		}

		/// <summary>
		/// Een property die de ID van de WAD terug geeft.
		/// </summary>
		public string ID {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.id;}
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.id = value; }
		}

		/// <summary>
		/// Een property die het aantal lumps in de WAD terug geeft.
		/// </summary>
		public int NumberOfLumps {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.numLumps; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.numLumps = value; }
		}

		/// <summary>
		/// Een property die de offset van de WAD directory terug geeft.
		/// </summary>
		public int DirectoryOffset {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.directoryOffset; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.directoryOffset = value; }
		}

		/// <summary>
		/// Private.
		/// </summary>
		private string id;
		/// <summary>
		/// Private.
		/// </summary>
		private int numLumps;
		/// <summary>
		/// Private.
		/// </summary>
		private int directoryOffset;
	}

	#endregion

	#region WAD1 // Partially implemented
	
	/// <summary>
	/// Een struct die de structuur van een WAD Directory Entry (voor WAD1) beschrijft.
	/// </summary>
	public struct Wad1DirEntry 
	{
		/// <summary>
		/// Een constructor die een WAD1 Directory Entry aanmaakt met een gegeven naam als string en een offset
		/// en grootte als integer.
		/// </summary>
		/// <param name="name">De naam van de Directory Entry.</param>
		/// <param name="lumpOffset">De offset naar de eigenlijke Lump in bytes.</param>
		/// <param name="lumpSize">De grootte van de eigenlijke Lump in bytes.</param>
		public Wad1DirEntry ( string name , int lumpOffset , int lumpSize ) {
			this.name = name;
			this.lumpOffset = lumpOffset;
			this.lumpSize = lumpSize;
		}

		/// <summary>
		/// Een constructor die een WAD1 Directory Entry aanmaakt met een gegeven naam als een array van chars 
		/// en een offset en grootte als integer.
		/// </summary>
		/// <param name="name">De naam van de Directory Entry.</param>
		/// <param name="lumpOffset">De offset naar de eigenlijke Lump in bytes.</param>
		/// <param name="lumpSize">De grootte van de eigenlijke Lump in bytes.</param>
		public Wad1DirEntry ( char [] name , int lumpOffset , int lumpSize ) 
		{
			this.name = "";
			foreach ( char c in name )
				if ( c != '\0' ) this.name += c;
				else break;
			this.lumpOffset = lumpOffset;
			this.lumpSize = lumpSize;
		}

		/// <summary>
		/// Een property die de naam van de beschreven Lump weergeeft.
		/// </summary>
		public string Name {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.name; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.name = value; }
		}

		/// <summary>
		/// Een property die de offset in bytes naar de beschreven Lump weergeeft.
		/// </summary>
		public int LumpOffset {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.lumpOffset; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.lumpOffset = value; }
		}

		/// <summary>
		/// Een property die de grootte in bytes van de beschreven Lump weergeeft.
		/// </summary>
		public int LumpSize {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.lumpSize; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.lumpSize = value; }
		}

		/// <summary>
		/// Private.
		/// </summary>
		private string name;
		/// <summary>
		/// Private.
		/// </summary>
		private int lumpOffset;
		/// <summary>
		/// Private.
		/// </summary>
		private int lumpSize;
	}

	
	/// <summary>
	/// Een struct die de structuur van een WAD1 lump beschrijft. (Te implementeren indien nodig (not bloody likely))
	/// </summary>
	public struct Wad1Lump {}

	/// <summary>
	/// Een class die WAD1 beschrijft.
	/// </summary>
	public class Wad : WadBase
	{
		/// <summary>
		/// De default constructor.
		/// </summary>
		public Wad () : base () {
			this.directory = new Wad1DirEntry [ this.header.NumberOfLumps ];
		}

		/// <summary>
		/// Deze constructor maakt een nieuw WAD object aan met de meegegeven WAD.
		/// </summary>
		/// <param name="wadname">De bestandsnaam (inclusief pad) van de te interpreteren WAD.</param>
		public Wad ( string wadname ) : base ( wadname ) {
			this.directory = new Wad1DirEntry [ this.header.NumberOfLumps ];
			this.byteReader.BaseStream.Seek ( this.header.DirectoryOffset , 0 );
			for ( int i = 0 ; i < directory.Length ; i++ )
				directory [i] = new Wad1DirEntry ( this.byteReader.ReadChars ( 8 ) , this.byteReader.ReadInt32() , this.byteReader.ReadInt32() );
		}

		/// <summary>
		/// Een property die de WAD directory terug geeft.
		/// </summary>
		public Wad1DirEntry [] Directory {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.directory; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.directory = value; }
		}

		/// <summary>
		/// Private.
		/// </summary>
		private Wad1DirEntry [] directory;
	}

	#endregion

	#region WAD2 // Not implemented at all

	/// <summary>
	/// Een struct die de structuur van een WAD Directory Entry (voor WAD2) beschrijft. (Niet geïmplementeerd!)
	/// </summary>
	public struct Wad2DirEntry {}
	/// <summary>
	/// Een struct die de structuur van een WAD2 lump beschrijft. (Niet geïmplementeerd!)
	/// </summary>
	public struct Wad2Lump {}
	/// <summary>
	/// Een class die WAD2 beschrijft. (Niet geïmplementeerd!)
	/// </summary>
	public class Wad2 : WadBase {}

	#endregion

	#region WAD3

	/// <summary>
	/// Een struct die de structuur van een WAD Directory Entry (voor WAD3) beschrijft.
	/// </summary>
	public struct Wad3DirEntry 
	{
		/// <summary>
		/// Een constructor die een WAD directory entry aanmaakt met de naam opgegeven als een string.
		/// </summary>
		/// <param name="lumpOffset">De offset van de beschreven lump in bytes.</param>
		/// <param name="lumpDiskSize">De grootte van de beschreven lump op de schijf in bytes. (Gebruikelijk identiek aan de grootte van de lump.)</param>
		/// <param name="lumpSize">De grootte van de beschreven lump in bytes.</param>
		/// <param name="type">Het type van de beschreven lump. (Meestal 67, wat duidt op een mipmap texture.)</param>
		/// <param name="compression">Geeft aan of de lump al dan niet gecomprimeerd is.</param>
		/// <param name="pad1">Een eerste padding byte.</param>
		/// <param name="pad2">Een tweede padding byte.</param>
		/// <param name="name">De naam van de beschreven lump.</param>
		public Wad3DirEntry (int lumpOffset , int lumpDiskSize , int lumpSize , int type , 
				byte compression , byte pad1 , byte pad2 ,  string name  ) {

			this.name = name;
			this.lumpOffset = lumpOffset;
			this.lumpDiskSize = lumpDiskSize;
			this.lumpSize = lumpSize;
			this.type = type;
			this.compression = compression;
			this.pad1 = pad1;
			this.pad2 = pad2;
		}

		/// <summary>
		/// Een constructor die een WAD directory entry aanmaakt met de naam opgegeven als een array van chars.
		/// </summary>
		/// <param name="lumpOffset">De offset van de beschreven lump in bytes.</param>
		/// <param name="lumpDiskSize">De grootte van de beschreven lump op de schijf in bytes. (Gebruikelijk identiek aan de grootte van de lump.)</param>
		/// <param name="lumpSize">De grootte van de beschreven lump in bytes.</param>
		/// <param name="type">Het type van de beschreven lump. (Meestal 67, wat duidt op een mipmap texture.)</param>
		/// <param name="compression">Geeft aan of de lump al dan niet gecomprimeerd is.</param>
		/// <param name="pad1">Een eerste padding byte.</param>
		/// <param name="pad2">Een tweede padding byte.</param>
		/// <param name="name">De naam van de beschreven lump.</param>
		public Wad3DirEntry (int lumpOffset , int lumpDiskSize , int lumpSize , int type , 
				byte compression , byte pad1 , byte pad2 ,  char [] name  ) {

			this.name = "";
			foreach ( char c in name )
				if ( c != '\0' ) this.name += c;
				else break;
			this.lumpOffset = lumpOffset;
			this.lumpDiskSize = lumpDiskSize;
			this.lumpSize = lumpSize;
			this.type = type;
			this.compression = compression;
			this.pad1 = pad1;
			this.pad2 = pad2;
		}

		#region Properties

		/// <summary>
		/// Een property die de naam van de beschreven lump terug geeft.
		/// </summary>
		public string Name 
		{
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.name; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.name = value; }
		}

		/// <summary>
		/// Een property die de offset in bytes naar de beschreven lump terug geeft.
		/// </summary>
		public int LumpOffset {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.lumpOffset; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.lumpOffset = value; }
		}

		/// <summary>
		/// Een property die de grootte op de schijf in bytes van de beschreven lump terug geeft.
		/// </summary>
		public int LumpSizeOnDisk {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.lumpDiskSize; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.lumpDiskSize = value; }
		}

		/// <summary>
		/// Een property die de grootte in bytes van de beschreven lump terug geeft.
		/// </summary>
		public int LumpSize {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.lumpSize; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.lumpSize = value; }
		}

		/// <summary>
		/// Een property dat het type van de beschreven lump terug geeft.
		/// </summary>
		public int Type {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.type; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.type = value; }
		}

		/// <summary>
		/// Een property die instelt of de beschreven lump gebruik maakt van compressie.
		/// </summary>
		public bool Compression {
			/// <summary>
			/// Setter.
			/// </summary>
			set 
			{
				if ( value ) this.compression = (byte) 1;
				else this.compression = (byte) 0;
			} 
		}

		/// <summary>
		/// Een property die aangeeft of de beschreven lump gebruik maakt van compressie.
		/// </summary>
		public bool IsCompressed {
			/// <summary>
			/// Getter.
			/// </summary>
			get 
			{ 
				if ( (int) this.compression == 1 ) return true;
				else return false;
			}
		}

		/// <summary>
		/// Een property die die de eerste padding byte terug geeft.
		/// </summary>
		public int Pad1 {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return (int) this.pad1; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.pad1 = (byte) value; }
		}

		/// <summary>
		/// Een property die die de tweede padding byte terug geeft.
		/// </summary>
		public int Pad2 {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return (int) this.pad2; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.pad2 = (byte) value; }
		}

		#endregion

		/// <summary>
		/// Private.
		/// </summary>
		private string name;
		/// <summary>
		/// Private.
		/// </summary>
		private int lumpOffset;
		/// <summary>
		/// Private.
		/// </summary>
		private int lumpDiskSize;
		/// <summary>
		/// Private.
		/// </summary>
		private int lumpSize;
		/// <summary>
		/// Private.
		/// </summary>
		private int type;
		/// <summary>
		/// Private.
		/// </summary>
		private byte compression;
		/// <summary>
		/// Private.
		/// </summary>
		private byte pad1 , pad2;
	}
	
	/// <summary>
	/// Een struct die de structuur van een WAD3 lump beschrijft.
	/// </summary>
	public struct Wad3Lump
	{
		/// <summary>
		/// Een constructor die een WAD3 lump aanmaakt met de naam opgegeven als een string.
		/// </summary>
		/// <param name="name">De naam van de beschreven lump.</param>
		/// <param name="width">De breedte van de beschreven lump.</param>
		/// <param name="height">De hoogte van de beschreven lump.</param>
		/// <param name="offsets">De offsets in bytes van de 4 verschillende mipmaps binnen de lump.</param>
		/// <param name="offset">De offset in bytes van de lump.</param>
		/// <param name="length">De lengte in bytes van de lump.</param>
		/// <remarks>
		///   <p>De lumps van een WAD3 stellen steeds een texture voor. Deze texture wordt opgeslagen onder 
		///   de vorm van 4 mipmaps, die kunnen benaderd worden op basis van de offsets en de opgegeven 
		///   breedte en hoogte.</p>
		///   <b>Af te leiden gegevens!</b>
		///   <ul>
		///     <li>De lengte in bytes van de verschillende mipmaps is af te leiden uit de 
		///       hoogte en breedte van de beschreven lump. De lengte van de eerste mipmap is de lengte en
		///       breedte zoals opgegeven, de lengte van de volgende mipmaps wordt bekomen door de opgegeven
		///       lengte en breedte telkens te delen door 2.</li>
		///     <li>De pixels van de verschillende mipmaps worden opgeslaan onder de vorm van een 
		///       verwijzing naar het pallet van de beschreven lump. Elke pixel wordt beschreven met precies één
		///       byte, dit is de index binnen het pallet. Verder worden de pixels opgeslaan van links naar rechts
		///       en van boven naar onder.</li>
		///     <li>De locatie van het pallet wordt bekomen door de offset van de laatste mipmap te nemen
		///       en daarbij de lengte van de laatste mipmap en twee padding bytes bij op te tellen. Elk pallet 
		///       bevat 256 kleuren, wat de lengte in bytes op 768 bytes brengt (zie volgende opmerking.)</li>
		///     <li>De kleuren binnen het pallet worden opgeslaan met 24 bits (3 bytes). De bytes stellen
		///       respectievelijk het rode, groene en blauwe kanaal van de kleur voor.</li>
		///   </ul>
		/// </remarks>
		public Wad3Lump ( string name , int width , int height , int [] offsets , int offset , int length ) 
		{
			this.name = name;
			this.width = width;
			this.height = height;
			this.offsets = offsets;
			this.offset = offset;
			this.length = length;
		}

		/// <summary>
		/// Een constructor die een WAD3 lump aanmaakt met de naam opgegeven als een array van chars.
		/// </summary>
		/// <param name="name">De naam van de beschreven lump.</param>
		/// <param name="width">De breedte van de beschreven lump.</param>
		/// <param name="height">De hoogte van de beschreven lump.</param>
		/// <param name="offsets">De offsets in bytes van de 4 verschillende mipmaps binnen de lump.</param>
		/// <param name="offset">De offset in bytes van de lump.</param>
		/// <param name="length">De lengte in bytes van de lump.</param>
		/// <remarks>
		///   <p>De lumps van een WAD3 stellen steeds een texture voor. Deze texture wordt opgeslagen onder 
		///   de vorm van 4 mipmaps, die kunnen benaderd worden op basis van de offsets en de opgegeven 
		///   breedte en hoogte.</p>
		///   <b>Af te leiden gegevens!</b>
		///   <ul>
		///     <li>De lengte in bytes van de verschillende mipmaps is af te leiden uit de 
		///       hoogte en breedte van de beschreven lump. De lengte van de eerste mipmap is de lengte en
		///       breedte zoals opgegeven, de lengte van de volgende mipmaps wordt bekomen door de opgegeven
		///       lengte en breedte telkens te delen door 2.</li>
		///     <li>De pixels van de verschillende mipmaps worden opgeslaan onder de vorm van een 
		///       verwijzing naar het pallet van de beschreven lump. Elke pixel wordt beschreven met precies één
		///       byte, dit is de index binnen het pallet. Verder worden de pixels opgeslaan van links naar rechts
		///       en van boven naar onder.</li>
		///     <li>De locatie van het pallet wordt bekomen door de offset van de laatste mipmap te nemen
		///       en daarbij de lengte van de laatste mipmap en twee padding bytes bij op te tellen. Elk pallet 
		///       bevat 256 kleuren, wat de lengte in bytes op 768 bytes brengt (zie volgende opmerking.)</li>
		///     <li>De kleuren binnen het pallet worden opgeslaan met 24 bits (3 bytes). De bytes stellen
		///       respectievelijk het rode, groene en blauwe kanaal van de kleur voor.</li>
		///   </ul>
		/// </remarks>
		public Wad3Lump ( char [] name , int width , int height , int [] offsets , int offset , int length ) 
		{
			this.name = "";
			foreach ( char c in name )
				if ( c != '\0' ) this.name += c;
				else break;
			this.width = width;
			this.height = height;
			this.offsets = offsets;
			this.offset = offset;
			this.length = length;
		}

		/// <summary>
		/// Een property die de naam van beschreven lump terug geeft.
		/// </summary>
		public string Name {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.name; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.name = value; }
		}

		/// <summary>
		/// Een property die de breedte van de beschreven lump terug geeft.
		/// </summary>
		public int Width {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.width; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.width = value; }
		}

		/// <summary>
		/// Een property die de hoogte van de beschreven lump terug geeft.
		/// </summary>
		public int Height {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.height; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.height = value; }
		}

		/// <summary>
		/// Een property die een array met daarin de offsets in bytes naar de verschillende mipmaps terug geeft.
		/// </summary>
		public int [] Offsets {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.offsets; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.offsets = value; }
		}

		/// <summary>
		/// Een property die de offset in bytes naar de beschreven lump terug geeft.
		/// </summary>
		public int Offset {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.offset; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.offset = value; }
		}

		/// <summary>
		/// Een property die de lengte in bytes van de beschreven lump terug geeft.
		/// </summary>
		public int Length {
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.length; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.length = value; }
		}

		/// <summary>
		/// Private.
		/// </summary>
		private string name;
		/// <summary>
		/// Private.
		/// </summary>
		private int width;
		/// <summary>
		/// Private.
		/// </summary>
		private int height;
		/// <summary>
		/// Private.
		/// </summary>
		private int [] offsets;
		/// <summary>
		/// Private.
		/// </summary>
		private int offset;
		/// <summary>
		/// Private.
		/// </summary>
		private int length;
	}

	/// <summary>
	/// Een class die WAD3 beschrijft.
	/// </summary>
	public class Wad3 : WadBase 
	{
		/// <summary>
		/// De default constructor.
		/// </summary>
		public Wad3 () : base () 
		{
			this.directory = new Wad3DirEntry [ this.header.NumberOfLumps ];
		}

		/// <summary>
		/// Deze constructor maakt een nieuw WAD object aan met de meegegeven WAD.
		/// </summary>
		/// <param name="wadname">De bestandsnaam (inclusief pad) van de te interpreteren WAD.</param>
		public Wad3 ( string wadname ) : base ( wadname ) 
		{
			this.directory = new Wad3DirEntry [ this.header.NumberOfLumps ];
			this.byteReader.BaseStream.Seek ( this.header.DirectoryOffset , 0 );
			for ( int i = 0 ; i < directory.Length ; i++ )
				directory [i] = new Wad3DirEntry ( this.byteReader.ReadInt32() , 
					this.byteReader.ReadInt32() , this.byteReader.ReadInt32() , 
					(int) this.byteReader.ReadByte() , this.byteReader.ReadByte() , 
					this.byteReader.ReadByte() , this.byteReader.ReadByte() , 
					this.byteReader.ReadChars ( 16 ) );
		}

		/// <summary>
		/// Een property die de WAD directory terug geeft.
		/// </summary>
		public Wad3DirEntry [] Directory 
		{
			/// <summary>
			/// Getter.
			/// </summary>
			get { return this.directory; }
			/// <summary>
			/// Setter.
			/// </summary>
			set { this.directory = value; }
		}

		/// <summary>
		/// Een methode die de lump met de opgegeven naam terug geeft.
		/// </summary>
		/// <param name="lumpName">De naam van de gevraagde lump.</param>
		/// <returns>De gevraagde lump.</returns>
		public Wad3Lump GetLump ( string lumpName ) {
			int lumpOffset = -1 , nextLumpOffset = -1;
			for ( int i = 0 ; i < this.directory.Length ; i++ ) {
				if ( this.directory[i].Name.ToLower() == lumpName.ToLower() /*this.directory [ i ].Name == lumpName*/ ) {
					lumpOffset = this.directory [ i ].LumpOffset;
					if((i + 1) < directory.Length) nextLumpOffset = this.directory [ i+1 ].LumpOffset;
					break;
				}
			}
			if ( lumpOffset == -1 )
				throw new Exception ( "Lump " + lumpName + " not found in WAD!" );
			if ( nextLumpOffset == -1 )
				nextLumpOffset = this.Header.DirectoryOffset;
			this.byteReader.BaseStream.Seek ( lumpOffset , SeekOrigin.Begin );
			return new Wad3Lump ( this.byteReader.ReadChars ( 16 ) , this.byteReader.ReadInt32() , this.byteReader.ReadInt32() , new int [4] { this.byteReader.ReadInt32() , this.byteReader.ReadInt32() , this.byteReader.ReadInt32() , this.byteReader.ReadInt32() } , lumpOffset , nextLumpOffset - lumpOffset );
		}

		/// <summary>
		/// Private.
		/// </summary>
		private Wad3DirEntry [] directory;
	}

	#endregion
}