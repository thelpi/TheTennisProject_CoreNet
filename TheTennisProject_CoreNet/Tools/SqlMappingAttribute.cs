using System;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Attribut de conversion en colonne SQL des valeurs d'énumération.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SqlMappingAttribute : Attribute
    {
        /// <summary>
        /// Le nom de colonne SQL.
        /// </summary>
        public string SqlMapping { get; } = string.Empty;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="sqlMapping">Le nom de colonne SQL.</param>
        public SqlMappingAttribute(string sqlMapping)
        {
            SqlMapping = sqlMapping;
        }
    }
}
