using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Inu.Language
{
    public class StringTable
    {
        public const int InvalidId = 0;

        private class Element : IComparable<Element>
        {
            public readonly int Id;
            public readonly string Name;
            public Element(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int CompareTo(Element? other)
            {
                return string.Compare(Name, other!=null ? other.Name : "", StringComparison.Ordinal);
            }
        }
        private readonly int minId;
        private readonly List<Element> elements = new List<Element>();

        public StringTable(int minId)
        {
            this.minId = minId;
        }

        public StringTable(int minId, ICollection<string> sortedNames)
        {
            this.minId = minId;
            int id = minId;
            foreach (string name in sortedNames) {
                elements.Add(new Element(id++, name));
            }
        }

        public int Add(string name)
        {
            int index = elements.FindIndex((Element e) => string.Compare(e.Name, name, StringComparison.Ordinal) >= 0);
            if (index < 0) {
                int id = minId + elements.Count;
                elements.Add(new Element(id, name));
                return id;
            }
            else if (elements[index].Name.Equals(name)) {
                return elements[index].Id;
            }
            else {
                int id = minId + elements.Count;
                elements.Insert(index, new Element(id, name));
                return id;
            }
        }

        public bool Contains(int id)
        {
            return elements.Any(e => e.Id == id);
        }

        public string? FromId(int id)
        {
            var index = elements.FindIndex(e => e.Id == id);
            return index >= 0 ? elements[index].Name : null;
        }

        public int ToId(string name)
        {
            int index = elements.FindIndex((Element e) => e.Name.Equals(name));
            return index >= 0 ? elements[index].Id : InvalidId;
        }

        public void Write(Stream stream)
        {
            stream.WriteWord(elements.Count);
            foreach (Element element in elements) {
                stream.WriteWord(element.Id);
                stream.WriteString(element.Name);
            }
        }
    }
}
