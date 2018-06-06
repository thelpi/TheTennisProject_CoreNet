using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Barème de répartition des points ATP suivant le niveau de compétition et le tour.
    /// </summary>
    /// <remarks>Ce barème est celui de 2015. Les jeux olympiques et la coupe Davis sont ignorés.</remarks>
    public class PointsAtpScale
    {
        #region Champs et propriétés

        // Liste de toutes les instances.
        private static List<PointsAtpScale> _pointsAtpSystems = new List<PointsAtpScale>();

        /// <summary>
        /// Niveau de compétition.
        /// </summary>
        public Level Level { get; private set; }
        /// <summary>
        /// Tour associé.
        /// </summary>
        public Round Round { get; private set; }
        /// <summary>
        /// Nombre de points pour le vainqueur.
        /// </summary>
        public uint WPoints { get; private set; }
        /// <summary>
        /// Nombre de points pour le vaincu.
        /// </summary>
        public uint LPoints { get; private set; }
        /// <summary>
        /// Nombre de points pour le vaincu s'il était exempt au tour précédent.
        /// </summary>
        public uint LPointsEx { get; private set; }
        /// <summary>
        /// Indique si les points obtenus à ce tour sont cumulatifs avec les précédents.
        /// </summary>
        public bool IsCumuled { get; private set; }

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="level">Niveau de compétition.</param>
        /// <param name="round">Tour associé.</param>
        /// <param name="wPoints">Nombre de points pour le vainqueur.</param>
        /// <param name="lPoints">Nombre de points pour le vaincu.</param>
        /// <param name="lPointsEx">Nombre de points pour le vaincu s'il était exempt au tour précédent.</param>
        /// <param name="isCumuled">Indique si les points obtenus à ce tour sont cumulatifs avec les précédents.</param>
        public PointsAtpScale(Level level, Round round, uint wPoints, uint lPoints, uint lPointsEx, bool isCumuled)
        {
            Level = level;
            Round = round;
            WPoints = wPoints;
            LPoints = lPoints;
            LPointsEx = lPointsEx;
            IsCumuled = isCumuled;

            _pointsAtpSystems.Add(this);
        }

        /// <summary>
        /// Charge le barème d'un niveau de compétition.
        /// </summary>
        /// <param name="level">Le niveau de compétition.</param>
        /// <param name="round">Le tour de la compétition.</param>
        /// <returns>Le barème associé aux paramètres fournis.</returns>
        public static ReadOnlyCollection<PointsAtpScale> GetLevelScale(Level level, Round? round)
        {
            List<PointsAtpScale> tempScale = _pointsAtpSystems.Where(item => item.Level == level && (!round.HasValue || item.Round == round.Value)).ToList();
            if (!tempScale.Any())
            {
                // HACK : à retirer dés que possible
                tempScale.Add(new PointsAtpScale(level, round.HasValue ? round.Value : Round.R128, 0, 0, 0, false));
            }

            return tempScale.AsReadOnly();
        }

        /// <summary>
        /// Détermine le nombre de points récoltés par un joueur pour un match donné.
        /// </summary>
        /// <remarks>Cette fonction ne lève pas d'exceptions, toute anomalie entrainera un retour à 0.</remarks>
        /// <param name="match">Le match concerné.</param>
        /// <param name="player">Le joueur concerné.</param>
        /// <param name="wasPreviousExempt">Détermine si le joueur était exempt au tour précédent.</param>
        /// <returns>Le nombre de points pour ce match.</returns>
        public static uint GetPoints(Match match, Player player, bool wasPreviousExempt)
        {
            if (match == null || player == null)
            {
                return 0;
            }

            PointsAtpScale scale = GetLevelScale(match.Edition.TournamentLevel, match.Round)[0];

            if (match.Winner == player)
            {
                return scale.WPoints;
            }
            else if (match.Loser == player)
            {
                return wasPreviousExempt ? scale.LPointsEx : scale.LPoints;
            }
            
            return 0;
        }
    }
}
