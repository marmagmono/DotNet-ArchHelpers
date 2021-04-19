using System;

namespace ArchHelpers.Core.Utilities.LabeledTree
{
    public readonly struct NodePath : IEquatable<NodePath>
    {
        private readonly string _path;

        public NodePath(string? path)
        {
            _path = path ?? string.Empty;
        }

        public NodePath? GetParentPath()
        {
            if (this.Equals(RootPath))
            {
                return null;
            }

            var lastDot = _path.LastIndexOf('.');
            if (lastDot == -1)
            {
                return RootPath;
            }
            else
            {
                return _path[..lastDot];
            }
        }

        public string GetNodeLabel()
        {
            var lastDot = _path.LastIndexOf('.');
            if (lastDot == -1)
            {
                return _path;
            }
            else if (lastDot == _path.Length - 1)
            {
                return ".";
            }
            else
            {
                return _path[(lastDot+1)..];
            }
        }
        

        public static implicit operator NodePath(string s) => new NodePath(s);

        public override string ToString() => _path;

        public static NodePath RootPath => new NodePath(string.Empty);

        public bool Equals(NodePath other) => _path == other._path;

        public override bool Equals(object? obj) => obj is NodePath other && Equals(other);

        public override int GetHashCode() => _path.GetHashCode();
    }
}