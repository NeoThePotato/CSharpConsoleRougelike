﻿using static Game.World.Point2D;
using System.Diagnostics;

namespace Game.World
{
	class Level
	{
		private Map _map;
		private List<MapEntity> _entities;
		private MapMetadata _metadata;
		private List<MapEntity> _entitiesList;

		public Map Map
		{ get => _map; private set => _map = value; }
		public List<MapEntity> Entities
		{ get => _entities; private set => _entities = value; }
        public MapMetadata Metadata
		{ get => _metadata; private set => _metadata = value; }

        public Level(Map map, MapMetadata metadata)
		{
			_map = map;
			_entities = new List<MapEntity>();
			_metadata = metadata;
			_entitiesList = new List<MapEntity>(_entities.Count);
		}

		#region ENTITY_SPAWNING
		public MapEntity AddEntity(Entity entity, Point2D pos)
		{
			var mapEntity = new MapEntity(entity, pos);
			AddEntity(mapEntity);

			return mapEntity;
		}

		public void AddEntity(MapEntity entity)
		{
			Entities.Add(entity);
        }

        public MapEntity AddEntityAtEntryTile(Entity entity)
        {
            return AddEntity(entity, Metadata.entryTile);
		}

		public MapEntity AddEntityAtExitTile(Entity entity)
		{
			return AddEntity(entity, Metadata.exitTile);
		}

		public MapEntity AddEntityAtRandomValidPoint(Entity entity)
		{
			Point2D randP;

			do
				randP = Map.GetRandomPoint();
			while (!TileTraversable(randP));

			return AddEntity(entity, randP);
		}
		#endregion

		#region ENTITY_MOVEMENT
		public bool MoveEntity(MapEntity entity, Direction direction, out List<MapEntity> occupiedBy)
		{
			if (CanEntityMoveTo(entity, direction, out occupiedBy))
			{
				entity.Move(direction);

				return true;
			}
			else
			{
				MoveEntityToEdgeOfTile(entity, direction);

				return false;
			}
        }

        public bool MoveEntity(MapEntity entity, out List<MapEntity> occupiedBy)
        {
			return MoveEntity(entity, entity.Dir, out occupiedBy);
        }

		private void MoveEntityToEdgeOfTile(MapEntity entity, Direction direction)
		{
			var projectedPoint = entity.ProjectedNewLocation(direction);
			var actualJ = Utility.ClampRange(projectedPoint.PointJ, TileToPoint(entity.Pos.TileJ), TileToPoint(entity.Pos.TileJ + 1) - 1);
			var actualI = Utility.ClampRange(projectedPoint.PointI, TileToPoint(entity.Pos.TileI), TileToPoint(entity.Pos.TileI + 1) - 1);
			entity.Pos = new Point2D(actualJ, actualI);
		}
		#endregion

		#region SPATIAL_CHECKS
		public bool CanEntityMoveTo(MapEntity entity, Direction targetDir, out List<MapEntity> occupiedBy)
		{
			return CanEntityMoveTo(entity, entity.Pos, targetDir, out occupiedBy);
		}
		
		public bool CanEntityMoveTo(MapEntity entity, Point2D startingPoint, Direction targetDir, out List<MapEntity> occupiedBy)
		{
			_entitiesList.Clear();
			occupiedBy = _entitiesList;
			var newPos = entity.ProjectedNewLocation(startingPoint, targetDir);

			if (TileTraversable(newPos))
			{
				if (!TileOccupied(newPos, entity) || AllPassable(occupiedBy))
					return true;
			}

			return false;
		}

		public bool CanEntityMoveTo(MapEntity entity, MapEntity other)
		{
			Debug.Assert(entity != other);
			bool inLineOfSight = CanEntityMoveTo(entity, other.Pos, out var occupiedBy);

			return inLineOfSight || occupiedBy.Contains(other);
		}

		/// <summary>
		/// Checks if there is a direct line-of-sight/movement between this entity and targetPos.
		/// </summary>
		/// <param name="entity">MapEntity to check movement for.</param>
		/// <param name="targetPos">Target point to check if entity can move to.</param>
		/// <param name="occupiedBy">Returns entity standing in the way, null if there isn't any.</param>
		/// <returns>true</returns>
		public bool CanEntityMoveTo(MapEntity entity, Point2D targetPos, out List<MapEntity> occupiedBy)
		{
			Debug.Assert(entity.Moveable);
			int stepsCounter = 0;
			int maxSteps = (entity.DetectionRange/entity.MovementSpeed) + 1;
			Point2D currentLocation = entity.Pos;
			Direction currentTrajectory = new Direction(entity.Pos, targetPos);

			while (!SameTile(currentLocation, targetPos))
			{
				if (CanEntityMoveTo(entity, currentLocation, currentTrajectory, out occupiedBy) && stepsCounter < maxSteps)
				{
					currentLocation = entity.ProjectedNewLocation(currentLocation, currentTrajectory);
					currentTrajectory = new Direction(currentLocation, targetPos);
					stepsCounter++;
				}
				else
				{
					Debug.WriteIf(stepsCounter >= maxSteps, $"{entity} took {stepsCounter} steps and failed to reach its destination");

					return false;
				}
			}
			occupiedBy = GetEntitiesAt(targetPos);

			return true;
		}

		private bool TileTraversable(Point2D pos)
		{
			return !(PointOutOfBounds(pos) || TileImpassable(pos));
		}

		private bool PointOutOfBounds(Point2D pos)
		{
			return pos.PointJ >= TileToPoint(Map.TileSizeJ) || pos.PointJ < 0 || pos.PointI >= TileToPoint(Map.TileSizeI) || pos.PointI < 0;
		}

		private bool TileImpassable(Point2D pos)
		{
			return !Map.GetTileInfo(pos).passable;
		}

		private bool TileOccupied(Point2D pos)
		{
			var occupiedBy = GetEntitiesAt(pos);

			return occupiedBy.Any();
		}

		private bool TileOccupied(Point2D pos, MapEntity exceptFor)
		{
			var occupiedBy = GetEntitiesAt(pos, exceptFor);

			return occupiedBy.Any();
		}

		private List<MapEntity> GetEntitiesAt(Point2D pos)
		{
			_entitiesList.Clear();

			foreach (var entity in Entities)
			{
				if (SameTile(entity.Pos, pos))
					_entitiesList.Add(entity);
			}

			return _entitiesList;
		}

		private List<MapEntity> GetEntitiesAt(Point2D pos, MapEntity exceptFor)
		{
			_entitiesList = GetEntitiesAt(pos);
			_entitiesList.Remove(exceptFor);

			return _entitiesList;
		}

		private bool AllPassable(List<MapEntity> entities)
		{
			return entities.All(e => e.Passable);
		}
		#endregion
	}
}
