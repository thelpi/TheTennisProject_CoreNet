using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente une nationalité.
    /// </summary>
    public struct Country
    {
        // liste de toutes les instances
        private static List<Country> _countries = new List<Country>();

        /// <summary>
        /// Liste de toutes les instances en lecture seule.
        /// </summary>
        public static ReadOnlyCollection<Country> Countries
        {
            get
            {
                return _countries.AsReadOnly();
            }
        }

        /// <summary>
        /// Code ISO sur deux caractères.
        /// </summary>
        public string CodeIso2 { get; private set; }
        /// <summary>
        /// Code ISO sur trois caractères.
        /// </summary>
        public string CodeIso3 { get; private set; }
        /// <summary>
        /// Nom anglais.
        /// </summary>
        public string NameEn { get; private set; }
        /// <summary>
        /// Nom français.
        /// </summary>
        public string NameFr { get; private set; }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="codeIso2">Code ISO sur deux caractères.</param>
        /// <param name="codeIso3">Code ISO sur trois caractères.</param>
        /// <param name="nameEn">Nom anglais.</param>
        /// <param name="nameFr">Nom français.</param>
        public Country(string codeIso2, string codeIso3, string nameEn, string nameFr)
        {
            CodeIso2 = codeIso2;
            CodeIso3 = codeIso3;
            NameEn = nameEn;
            NameFr = nameFr;
            if (!_countries.Any(c => c.CodeIso2?.Equals(codeIso2, System.StringComparison.InvariantCultureIgnoreCase) == true))
            {
                _countries.Add(this);
            }
        }
    }
}
