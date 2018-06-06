using System.ComponentModel;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Enumération des niveaux de compétition.
    /// </summary>
    public enum Level
    {
        /// <summary>
        /// Grand chelem.
        /// </summary>
        [Description("Grand slam"), Translation("Grand chelem")]
        grand_slam = 1,
        /// <summary>
        /// Masters 1000.
        /// </summary>
        [Description("Masters 1000"), Translation("Masters 1000")]
        masters_1000 = 2,
        /// <summary>
        /// ATP 500.
        /// </summary>
        [Description("ATP 500"), Translation("ATP 500")]
        atp_500 = 3,
        /// <summary>
        /// ATP 250.
        /// </summary>
        [Description("ATP 250"), Translation("ATP 250")]
        atp_250 = 4,
        /// <summary>
        /// Masters.
        /// </summary>
        [Description("Masters"), Translation("Masters")]
        masters = 5,
        /// <summary>
        /// Challenger.
        /// </summary>
        [Description("Challenger"), Translation("Challenger")]
        challenger = 6,
        /// <summary>
        /// Jeux olympiques.
        /// </summary>
        [Description("Olympics games"), Translation("Jeux olympiques")]
        olympics_games = 7,
        /// <summary>
        /// Coupe Davis.
        /// </summary>
        [Description("Davis cup"), Translation("Coupe Davis")]
        davis_cup = 8,
        /// <summary>
        /// Autre.
        /// </summary>
        [Description("Other"), Translation("Autre")]
        other = 9,
        /// <summary>
        /// Coupe du grand chelem.
        /// </summary>
        [Description("Grand slam cup"), Translation("Coupe du grand chelem")]
        grand_slam_cup = 10,
        /// <summary>
        /// World championship tennis.
        /// </summary>
        [Description("WCT"), Translation("WCT")]
        wct = 11
    }
}
