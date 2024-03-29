﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Game.World
{
	struct Point2D
	{
		public const int POINTS_PER_TILE = 256;
		public int PointJ
		{ get; set; }
		public int PointI
		{ get; set; }
		public int TileJ
		{ readonly get => PointToTile(PointJ); set => PointJ = TileToPoint(value) + PointRemainder(PointJ); }
		public int TileI
		{ readonly get => PointToTile(PointI); set => PointI = TileToPoint(value) + PointRemainder(PointI); }

		public Point2D(int pointJ, int pointI)
		{
			PointJ = pointJ;
			PointI = pointI;
		}

		public Point2D((int, int) tuple)
		{
			PointJ = tuple.Item1;
			PointI = tuple.Item2;
		}

		public Point2D(Point2D other)
		{
			PointJ = other.PointJ;
			PointI = other.PointI;
		}

		public static Point2D Tile(int tileJ, int tileI)
		{
			return new(TileToPoint(tileJ), TileToPoint(tileI));
		}

		public static Point2D operator +(Point2D p)
			=> p;
									   
		public static Point2D operator -(Point2D p)
			=> new(-p.PointJ, -p.PointI);
									   
		public static Point2D operator +(Point2D p1, Point2D p2)
			=> new(p1.PointJ + p2.PointJ, p1.PointI + p2.PointI);
									   
		public static Point2D operator -(Point2D p1, Point2D p2)
			=> new(p1.PointJ - p2.PointJ, p1.PointI - p2.PointI);
									   
		public static Point2D operator *(Point2D p, int num)
			=> new(p.PointJ * num, p.PointI * num);

		public static Point2D operator /(Point2D p, int num)
			=> new(p.PointJ / num, p.PointI / num);

		public static bool operator ==(Point2D p1, Point2D p2)
			=> SamePoint(p1, p2);

		public static bool operator !=(Point2D p1, Point2D p2)
			=> !SamePoint(p1, p2);

		public static bool SamePoint(Point2D p1, Point2D p2)
			=> (p1.PointJ == p2.PointJ) & (p1.PointI == p2.PointI);

		public static bool SameTile(Point2D p1, Point2D p2)
			=> (p1.TileJ == p2.TileJ) & (p1.TileI == p2.TileI);

		public static bool WithinDistance(Point2D p1, Point2D p2, int distance)
		{
			Debug.Assert(distance >= 0);

			return new Direction(p1, p2).Mag < distance;
		}

		public static int TileToPoint(int tile)
			=> tile * POINTS_PER_TILE;

		public static int PointToTile(int point)
			=> point / POINTS_PER_TILE;

		public static int PointRemainder(int point)
			=> point % POINTS_PER_TILE;

		public override readonly bool Equals([NotNullWhen(true)] object? obj)
		{
			return base.Equals(obj);
		}

		public override readonly int GetHashCode()
		{
			return base.GetHashCode() << 2;
		}

		public override readonly string ToString()
		{
			return $"({TileJ}+{PointRemainder(PointJ)}/{POINTS_PER_TILE}, {TileI}+{PointRemainder(PointI)}/{POINTS_PER_TILE})";
		}
	}
}
