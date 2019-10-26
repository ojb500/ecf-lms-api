using System;
using System.Collections.Generic;
using System.Text;

namespace Ojb500.EcfLms
{
    public readonly struct Grade
    {
        private readonly ushort _grade;
        private readonly char _category;

        public Grade(string value)
        {
            value = value.Trim();
            if (value == "000")
            {
                _grade = 0;
                _category = (char) 0;
            }
            else
            {
                _grade = ushort.Parse(value.Substring(0, 3));
                _category = value[3];
            }
        }

        public override string ToString()
        {
            if (_category == '\0')
            {
                return "ug";
            }
            return $"{_grade}{_category}";
        }

        public int Value => _grade;
        public char Category => _category;
    }
    public struct Player
    {
        public Player(string name, string grade) : this(name, new Grade(grade))
        { }

        public Player(string name, Grade grade)
        {
            var lastFirst = name.Split(new char[] { ',' } , StringSplitOptions.RemoveEmptyEntries);
            for (int i=0; i < lastFirst.Length; i++)
            {
                lastFirst[i] = lastFirst[i].Trim(new char[] { '.', ' ' });
            }
            if (lastFirst.Length == 2)
            {
                GivenName = lastFirst[1];
                FamilyName = lastFirst[0];
            }
            else if (lastFirst.Length == 1)
            {
                GivenName = lastFirst[0];
                FamilyName = null;
            }
            else
            {
                throw new InvalidOperationException("Invalid player name");
            }
            Grade = grade;
        }

        public Player(string givenName, string familyName, Grade grade)
        {
            GivenName = givenName;
            FamilyName = familyName;
            Grade = grade;
        }
        public Grade Grade { get; }
        public string GivenName { get; }
        public string FamilyName { get; }

        public override string ToString()
        {
            if (FamilyName == null)
            {
                return GivenName;
            }
            return $"{FamilyName}, {GivenName} ({Grade})";
        }
    }
}
