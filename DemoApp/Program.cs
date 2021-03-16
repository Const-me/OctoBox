using System;
using System.Numerics;

namespace OctoBoxDemo
{
	static class Program
	{
		const string path = @"C:\Temp\2remove\1.svg";
		const int boxesCount = 22;

		static readonly Random random = new Random( 0 );

		static Vector2 randomPoint()
		{
			double x = random.NextDouble() * ( 1920 - 40 ) + 20;
			double y = random.NextDouble() * ( 1080 - 40 ) + 20;
			return new Vector2( (float)x, (float)y );
		}
		static OctoBox randomBox( int count = 3 )
		{
			OctoBox box = OctoBox.Empty;
			for( int j = 0; j < 3; j++ )
				box.extend( randomPoint() );
			return box;
		}

		static void mainImpl()
		{
			OctoBox[] boxes = new OctoBox[ boxesCount ];
			for( int i = 0; i < boxes.Length; i++ )
				boxes[ i ] = randomBox();

			using( var svg = new SvgWriter( path, new Vector2( 1920, 1080 ) ) )
			{
				using( var g = svg.group( "green" ) )
				{
					foreach( var b in boxes )
						svg.polygon( b.makeVertices() );
				}
				using( var g = svg.group( "blue" ) )
				{
					foreach( var b in boxes )
						svg.polygon( b.makeBoxVertices() );
				}
			}
		}

		static void Main( string[] args )
		{
			try
			{
				mainImpl();
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.ToString() );
			}
		}
	}
}