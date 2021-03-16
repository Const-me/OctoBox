using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace OctoBoxDemo
{
	/// <summary>Utility class to write an SVG file with a few polygons</summary>
	struct SvgWriter: IDisposable
	{
		XmlWriter writer;

		const string xmlns = "http://www.w3.org/2000/svg";

		public SvgWriter( string path, Vector2 size )
		{
			XmlWriterSettings xws = new XmlWriterSettings() { Indent = true, IndentChars = "\t" };
			writer = XmlWriter.Create( path, xws );

			writer.WriteDocType( "svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null );

			writer.WriteStartElement( "svg", xmlns );
			writer.WriteAttributeString( "version", "1.1" );
			string w = size.X.ToString( CultureInfo.InvariantCulture );
			string h = size.Y.ToString( CultureInfo.InvariantCulture );
			writer.WriteAttributeString( "width", w );
			writer.WriteAttributeString( "height", h );
			writer.WriteAttributeString( "viewBox", $"0 0 { w } { h }" );
		}

		public void Dispose()
		{
			writer?.Close();
			writer?.Dispose();
		}

		class CloseElement: IDisposable
		{
			readonly XmlWriter writer;
			public CloseElement( XmlWriter w ) { writer = w; }

			public void Dispose()
			{
				writer.WriteEndElement();
			}
		}
		public IDisposable group( string color )
		{
			writer.WriteStartElement( "g", xmlns );
			writer.WriteAttributeString( "style", $"stroke-width:1; fill-opacity: 0.11; stroke-opacity: 0.33; stroke:{ color }; fill:{ color }; " );
			return new CloseElement( writer );
		}

		static string printPoint( Vector2 v )
		{
			return $"{ v.X.ToString( CultureInfo.InvariantCulture ) },{ v.Y.ToString( CultureInfo.InvariantCulture ) }";
		}

		public void polygon( IEnumerable<Vector2> points )
		{
			writer.WriteStartElement( "polygon", xmlns );
			writer.WriteAttributeString( "points", string.Join( ' ', points.Select( printPoint ) ) );
			writer.WriteEndElement();
		}
	}
}