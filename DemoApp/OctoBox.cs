using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace OctoBoxDemo
{
	struct OctoBox
	{
		// The following code uses x86 intrinsics.
		// NEON has similar stuff: vrev64_f32 to flip lanes, vcombine_f32 to make 16-byte vectors from halves.
		// No `addsubps` in NEON though, can be emulated with bitwise XOR flipping sign bit of one lane, followed with addition.
		public Vector4 min, max;

		public static OctoBox Empty =>
			new OctoBox { min = new Vector4( float.MaxValue ), max = new Vector4( -float.MaxValue ) };

		public void extend( Vector2 pt )
		{
			Vector128<float> vec = pt.AsVector128();
			// [ x, y, x, y ]; BTW if the source vector is in memory, can do for free with `_mm_loaddup_pd` instruction.
			Vector128<float> lhs = Sse.MoveLowToHigh( vec, vec );
			// [ 0, 0, y, x ]
			Vector128<float> rhs = Sse.Shuffle( Vector128<float>.Zero, vec, 0b00010100 );   // _MM_SHUFFLE( 0, 1, 1, 0 ) 
			// [ x, y, x - y, x + y ]
			Vector4 vals = Sse3.AddSubtract( lhs, rhs ).AsVector4();
			// Now update the box
			min = Vector4.Min( min, vals );
			max = Vector4.Max( max, vals );
		}

		/// <summary>Make a union with another OctoBox</summary>
		public void unionWith( OctoBox that )
		{
			min = Vector4.Min( min, that.min );
			max = Vector4.Max( max, that.max );
		}

		/// <summary>true if the two boxes intersect, or contained within one another, or share an edge</summary>
		public bool intersects( OctoBox that )
		{
			Vector128<float> i1 = min.AsVector128();
			Vector128<float> ax1 = max.AsVector128();

			Vector128<float> i2 = that.min.AsVector128();
			Vector128<float> ax2 = that.max.AsVector128();

			Vector128<float> c1 = Sse.CompareLessThan( ax2, i1 );
			Vector128<float> c2 = Sse.CompareLessThan( ax1, i2 );
			Vector128<float> cmp = Sse.Or( c1, c2 );
			return Sse.MoveMask( cmp ) == 0;    // Avx.TestZ is only marginally faster
		}

		public IEnumerable<Vector2> makeVertices()
		{
			// Doesn't collapse dupes and not too efficient, but good enough for debug visualizations

			// Left edge, top-left diagonal
			yield return new Vector2( min.X, min.W - min.X );
			// Top edge, top-left diagonal
			yield return new Vector2( min.W - min.Y, min.Y );
			// Top edge, top-right diagonal
			yield return new Vector2( min.Y + max.Z, min.Y );
			// Right edge, top-right diagonal
			yield return new Vector2( max.X, max.X - max.Z );
			// Right edge, bottom-right diagonal
			yield return new Vector2( max.X, max.W - max.X );
			// Bottom edge, bottom-right diagonal
			yield return new Vector2( max.W - max.Y, max.Y );
			// Bottom edge, bottom-left diagonal
			yield return new Vector2( max.Y + min.Z, max.Y );
			// Left edge, bottom-left diagonal
			yield return new Vector2( min.X, min.X - min.Z );
		}

		public IEnumerable<Vector2> makeBoxVertices()
		{
			yield return new Vector2( min.X, min.Y );
			yield return new Vector2( max.X, min.Y );
			yield return new Vector2( max.X, max.Y );
			yield return new Vector2( min.X, max.Y );
		}
	}
}