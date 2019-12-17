using System;
using System.Collections.Generic;
using System.Text;

namespace Ojb500.EcfLms
{
    public struct Grade
    {
        private ushort _grade;
        private char _category;


        public Grade(int value, char category)
        {
            _grade = (ushort)value;
            _category = category;
        }

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

        public int Value { get => _grade; set => _grade = (ushort)value; }
        public char Category { get => _category; set => _category = value; }
    }
    public struct Player
    {
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
                GivenName = null;
                FamilyName = lastFirst[0];
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
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public Grade Grade { get; set; }
        public bool IsDefault => FamilyName == "Default" && string.IsNullOrEmpty(GivenName);
        public override string ToString()
        {
            if (GivenName == null)
            {
                return FamilyName;
            }
            return $"{FamilyName}, {GivenName} ({Grade})";
        }
    }
}
