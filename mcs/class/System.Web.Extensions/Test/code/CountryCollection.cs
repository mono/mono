using System;
using System.Collections.Generic;

namespace TestCode
{
    public class CountryCollection
    {
        static List<Country> db;
        static CountryCollection()
        {
            db = new List<Country>();

            db.Add(new Country(1, "Poland", "Warsaw", 38116000.0));
            db.Add(new Country(2,"Portugal", "Lisbon", 10617575.0));
            db.Add(new Country(3,"Australia", "Canberra", 21468700.0));
            db.Add(new Country(4,"Austria", "Vienna", 8316487.0));
            db.Add(new Country(5,"Belgium", "Brussels", 10666866.0));
            db.Add(new Country(6,"Brazil", "Brasilia", 190132630.0));
            db.Add(new Country(7,"China", "Bejing", 1321000000.0));
            db.Add(new Country(8,"Chad", "N'Djamena", 10780600.0));
            db.Add(new Country(9,"Venezuela", "Caracas", 28199822.0));
            db.Add(new Country(10,"Vietnam", "Hanoi", 86116559.0));
            db.Add(new Country(11,"New Zealand", "Wellington", 4268000.0));
            db.Add(new Country(12,"Nigeria", "Abuja", 148000000.0));
            db.Add(new Country(13,"Oman", "Muscat", 2577000.0));
            db.Add(new Country(14,"Quatar", "Doha", 1450000.0));
            db.Add(new Country(15,"Denmark", "Copenhagen", 5505995.0));
            db.Add(new Country(16,"Dominican Republic", "Santo Domingo de Guzman", 9904000.0));
            db.Add(new Country(17,"France", "Paris", 64473140.0));
            db.Add(new Country(18,"United States of America", "Washington", 305619000.0));
            db.Add(new Country(19,"Latvia", "Riga", 2270700.0));
        }

        public List<Country> GetCountries(string sortExpression)
        {
            var ret = new List<Country>();
            ret.AddRange(db);
            ret.Sort(new CountryComparer(sortExpression));

            return ret;
        }

        public int Update(int id, string name, string capital, double population)
        {
            if (String.IsNullOrEmpty(name))
                return 0;

            int updated = 0;
            foreach (Country c in db)
            {
                if (c.ID != id)
                    continue;
                updated++;
                c.Name = name;
                c.Capital = capital;
                c.Population = population;
            }

            return updated;
        }

        public int Insert(int id, string name, string capital, double population)
        {
            if (String.IsNullOrEmpty(name))
                return 0;

            db.Add(new Country(id, name, capital, population));
            return 1;
        }

        public int Delete(int id)
        {
            var toDelete = new List<Country> ();
            foreach (Country c in db)
            {
                if (c.ID != id)
                    continue;
                toDelete.Add(c);
            }

            foreach (Country c in toDelete)
                db.Remove(c);

            return toDelete.Count;
        }
    }

    sealed class CountryComparer : IComparer<Country>
    {
        string sortProperty;
        bool descending;

        public CountryComparer(string sortExpression)
        {
            descending = sortExpression.ToLowerInvariant().EndsWith(" desc");
            if (descending)
            {
                sortProperty = sortExpression.Substring(0, sortExpression.Length - 5);
            }
            else
            {
                if (sortExpression.ToLowerInvariant().EndsWith(" asc"))
                    sortProperty = sortExpression.Substring(0, sortExpression.Length - 4);
                else
                    sortProperty = sortExpression;
            }
        }

        public int Compare(Country a, Country b)
        {
            int retVal = 0;
            switch (sortProperty)
            {
                case "Name":
                    retVal = String.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case "Capital":
                    retVal = String.Compare(a.Capital, b.Capital, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case "Population":
                    retVal = (int)(a.Population - b.Population);
                    break;
            }

            if (descending)
                return retVal;
            return retVal * -1;
        }
    }

}