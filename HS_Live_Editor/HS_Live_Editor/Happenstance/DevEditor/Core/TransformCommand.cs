// HS Live Dev Editor (c) 2025 Happenstance Games LLC - MIT License

using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;

namespace Happenstance.SE.DevEditor.Core
{
    public interface IEditorCommand
    {
        void Execute();
        void Undo();
        string Description { get; }
        Entity TargetEntity { get; }
    }


    public class TransformCommand : IEditorCommand
    {
        private readonly Entity _entity;
        private readonly Vector3 _oldPosition, _newPosition;
        private readonly Quaternion _oldRotation, _newRotation;
        private readonly Vector3 _oldScale, _newScale;
        private readonly string _description;

        public Entity TargetEntity => _entity;
        public string Description => _description;

        public TransformCommand(Entity entity,
                              Vector3 oldPos, Vector3 newPos,
                              Quaternion oldRot, Quaternion newRot,
                              Vector3 oldScale, Vector3 newScale,
                              string description)
        {
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
            _oldPosition = oldPos;
            _newPosition = newPos;
            _oldRotation = oldRot;
            _newRotation = newRot;
            _oldScale = oldScale;
            _newScale = newScale;
            _description = description;
        }

        //position-only changes
        public static TransformCommand CreatePositionCommand(Entity entity, Vector3 oldPos, Vector3 newPos, string description = "Move")
        {
            return new TransformCommand(entity, oldPos, newPos,
                                      entity.Transform.Rotation, entity.Transform.Rotation,
                                      entity.Transform.Scale, entity.Transform.Scale,
                                      description);
        }

        //rotation-only changes
        public static TransformCommand CreateRotationCommand(Entity entity, Quaternion oldRot, Quaternion newRot, string description = "Rotate")
        {
            return new TransformCommand(entity,
                                      entity.Transform.Position, entity.Transform.Position,
                                      oldRot, newRot,
                                      entity.Transform.Scale, entity.Transform.Scale,
                                      description);
        }

        //scale-only changes
        public static TransformCommand CreateScaleCommand(Entity entity, Vector3 oldScale, Vector3 newScale, string description = "Scale")
        {
            return new TransformCommand(entity,
                                      entity.Transform.Position, entity.Transform.Position,
                                      entity.Transform.Rotation, entity.Transform.Rotation,
                                      oldScale, newScale,
                                      description);
        }

        public void Execute()
        {
            if (_entity == null) return;

            _entity.Transform.Position = _newPosition;
            _entity.Transform.Rotation = _newRotation;
            _entity.Transform.Scale = _newScale;
        }

        public void Undo()
        {
            if (_entity == null) return;

            _entity.Transform.Position = _oldPosition;
            _entity.Transform.Rotation = _oldRotation;
            _entity.Transform.Scale = _oldScale;
        }

        public static TransformCommand CreatePositionRotationCommand(Entity entity,
    Vector3 oldPos, Vector3 newPos,
    Quaternion oldRot, Quaternion newRot,
    string description)
        {
            return new TransformCommand(entity,
                oldPos, newPos,
                oldRot, newRot,
                entity.Transform.Scale, entity.Transform.Scale, // Scale unchanged
                description);
        }
    }
}