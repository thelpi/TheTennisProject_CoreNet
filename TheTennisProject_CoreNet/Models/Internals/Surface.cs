namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Enumération des types de surface.
    /// </summary>
    public enum Surface
    {
        /// <summary>
        /// Moquette.
        /// </summary>
        [Translation("Moquette")]
        Carpet = 1,
        /// <summary>
        /// Terre battue.
        /// </summary>
        [Translation("Terre battue")]
        Clay = 2,
        /// <summary>
        /// Herbe.
        /// </summary>
        [Translation("Herbe")]
        Grass = 3,
        /// <summary>
        /// Dur.
        /// </summary>
        [Translation("Dur")]
        Hard = 4,
        /// <summary>
        /// Indéterminée.
        /// </summary>
        [Translation("Indéterminée")]
        Unknown = 5
    }
}
