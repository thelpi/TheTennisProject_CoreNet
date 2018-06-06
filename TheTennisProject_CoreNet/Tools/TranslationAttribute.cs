using System;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Attribut de traduction des valeurs d'énumération.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TranslationAttribute : Attribute
    {
        /// <summary>
        /// La traduction.
        /// </summary>
        public string Translation { get; } = string.Empty;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="translation">La traduction.</param>
        public TranslationAttribute(string translation)
        {
            Translation = translation;
        }
    }
}
