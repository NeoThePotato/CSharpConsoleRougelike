﻿using static Game.World.Direction;
using static Game.World.Point2D;

namespace Game.World
{
	class MapEntity
	{
		private Point2D _target;

		public const int MAX_MOVEMENT_SPEED = POINTS_PER_TILE;
		public Entity Entity
		{ get; set; }
		public Point2D Pos
		{ get; set; }
		public Direction Dir
		{ get; set; }
		public int MovementSpeed
		{ get => MAX_MOVEMENT_SPEED / 4; } // TODO Replace with entity "SPD" stat or something
		public int DetectionRange
		{ get => TileToPoint(3); } // TODO Replace with entity "INT" stat or something
		public Point2D Target
		{
			get => _target;
			set
			{
				_target = value;
				Dir = new Direction(Pos, _target);
			}
		}
		public bool Passable
		{ get => Entity.Passable; }
		public bool Moveable
		{ get => Entity.Moveable && MovementSpeed > 1; }

		public MapEntity(Entity entity, Point2D pos)
		{
			Entity = entity;
			Pos = pos;
			Dir = TranslateDirection(Directions.None);
		}

		public MapEntity(Entity entity, Point2D pos, Direction dir)
		{
			Entity = entity;
			Pos = pos;
			Dir = dir;
		}

		public void Move()
		{
			Move(Dir);
		}

		public void Move(Direction dir)
		{
			Dir = dir;
			Pos = ProjectedNewLocation(dir);
		}

		public Point2D ProjectedNewLocation(Direction dir)
		{
			return ProjectedNewLocation(Pos, dir);
		}

		public Point2D ProjectedNewLocation(Point2D pos, Direction dir)
		{
			dir.Normalize();
			return pos + EffectiveMovement(dir);
		}

		public bool OtherInDetectionRange(MapEntity other)
		{
			return WithinDistance(Pos, other.Pos, DetectionRange);
		}

		private Direction EffectiveMovement(Direction dir)
		{
			return (dir * MovementSpeed) / MAX_MOVEMENT_SPEED;
		}

		public override string ToString()
		{
			return Entity.ToString();
		}
	}
}