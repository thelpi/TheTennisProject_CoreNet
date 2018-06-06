using System.ComponentModel;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Enumération des tours d'une compétition.
    /// </summary>
    public enum Round
    {
        /// <summary>
        /// Finale.
        /// </summary>
        [Description("Final"), Translation("Finale")]
        F = 1,
        /// <summary>
        /// Demi-finale.
        /// </summary>
        [Description("Semi-final"), Translation("Demi-finale")]
        SF = 2,
        /// <summary>
        /// Quart de finale.
        /// </summary>
        [Description("Quart-final"), Translation("Quart de finale")]
        QF = 3,
        /// <summary>
        /// Huitième de finale.
        /// </summary>
        [Description("8 matches round"), Translation("Huitième de finale")]
        R16 = 4,
        /// <summary>
        /// 16ème de finale.
        /// </summary>
        [Description("16 matches round"), Translation("16ème de finale")]
        R32 = 5,
        /// <summary>
        /// 32ème de finale.
        /// </summary>
        [Description("32 matches round"), Translation("32ème de finale")]
        R64 = 6,
        /// <summary>
        /// Premier tour (grand chelem).
        /// </summary>
        [Description("64 matches round"), Translation("Premier tour (grand chelem)")]
        R128 = 7,
        /// <summary>
        /// Round Robin (Masters).
        /// </summary>
        [Description("Round Robin"), Translation("Round Robin (Masters)")]
        RR = 8,
        /// <summary>
        /// Médaille de bronze (jeux olympiques).
        /// </summary>
        [Description("Bronze reward (olympics)"), Translation("Médaille de bronze (jeux olympiques)")]
        BR = 9
    }
}
