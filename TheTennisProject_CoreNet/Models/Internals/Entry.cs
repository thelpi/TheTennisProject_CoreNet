namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente la codification de l'entrée d'un joueur dans un tournoi.
    /// </summary>
    public enum Entry
    {
        /// <summary>
        /// Repêché (lucky loser).
        /// </summary>
        [Translation("Repêché")]
        LL,
        /// <summary>
        /// Exempté (special exempt).
        /// </summary>
        [Translation("Exempté")]
        SE,
        /// <summary>
        /// Qualifié (qualified).
        /// </summary>
        [Translation("Qualifié")]
        Q,
        /// <summary>
        /// Invité (wild-card).
        /// </summary>
        [Translation("Invité")]
        WC,
        /// <summary>
        /// Classement protégé (protected ranking).
        /// </summary>
        [Translation("Protégé")]
        PR,
        /// <summary>
        /// Remplaçant (alternative).
        /// </summary>
        [Translation("Remplaçant")]
        Alt
    }
}
