namespace TailBlazer.Infrastucture
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public class FileNamer
    {
        #region Fields
        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        #endregion
        #region Constructors
        public FileNamer(IEnumerable<string> paths)
        {
            foreach (var path in paths)
                this.Insert(path);
        }
        #endregion
        #region Properties
        private Node Root { get; } = new Node(string.Empty);
        #endregion
        #region Methods
        private static string CombinePath(Stack<string> path)
        {
            var parts = new List<string>(3);
            parts.Add(path.First());
            if (path.Count > 1)
            {
                if (path.Count > 2)
                    parts.Add("..");
                parts.Add(path.Last());
            }
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }
        private static Stack<string> GetName(Node node, Stack<string> path) => FileNamer.GetName(node, path, new Stack<string>());
        private static Stack<string> GetName(Node node, Stack<string> path, Stack<string> result)
        {
            if (result == null)
                result = new Stack<string>();
            if (path.Count == 0)
                return result;
            var part = path.Pop();
            result.Push(part);
            Node childNode;
            if (node.Children.TryGetValue(part, out childNode) && childNode.Count > 1)
                return FileNamer.GetName(childNode, path, result);
            return result;
        }
        private static void Insert(Node node, Stack<string> path)
        {
            if (path.Count == 0)
                return;
            var part = path.Pop();
            Node childNode;
            if (!node.Children.TryGetValue(part, out childNode))
                node.Children.Add(part, childNode = new Node(part));
            childNode.Count++;
            FileNamer.Insert(childNode, path);
        }
        public string GetName(string path) => FileNamer.CombinePath(FileNamer.GetName(this.Root, new Stack<string>(path.Split(DirectorySeparators))));
        public void Insert(string path) { FileNamer.Insert(this.Root, new Stack<string>(path.Split(DirectorySeparators))); }
        #endregion
        #region Classes
        private class Node
        {
            #region Constructors
            public Node(string value)
            {
                this.Value = value;
                this.Children = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
            }
            #endregion
            #region Properties
            public IDictionary<string, Node> Children { get; }
            public int Count { get; set; }
            private string Value { get; }
            #endregion
            #region Methods
            public override string ToString() => $"Value: {this.Value}, Children: {this.Children.Count}";
            #endregion
        }
        #endregion
    }
}