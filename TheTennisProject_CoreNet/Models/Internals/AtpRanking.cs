using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente une entrée dans la table "atp_ranking".
    /// </summary>
    public class AtpRanking
    {
        // liste de toutes les instances.
        private static List<AtpRanking> _instances = new List<AtpRanking>();

        /// <summary>
        /// Joueur.
        /// </summary>
        public Player Player { get; private set; }
        /// <summary>
        /// Année.
        /// </summary>
        public uint Year { get; private set; }
        /// <summary>
        /// Numéro de semaine.
        /// </summary>
        public uint WeekNo { get; private set; }
        /// <summary>
        /// Nombre de points cette semaine.
        /// </summary>
        public uint WeekPoints { get; private set; }
        /// <summary>
        /// Nombre cumulé de points à date pour l'année civile.
        /// </summary>
        public uint CalendarPoints { get; private set; }
        /// <summary>
        /// Nombre cumulé de points à date pour l'année glissante.
        /// </summary>
        public uint RollingPoints { get; private set; }
        /// <summary>
        /// Le classement du joueur à date pour l'année civile.
        /// </summary>
        public ushort CalendarRank { get; private set; }
        /// <summary>
        /// Le classement du joueur à date pour l'année glisante.
        /// </summary>
        public ushort RollingRank { get; private set; }
        /// <summary>
        /// Nombre de points ELO.
        /// </summary>
        public ushort Elo { get; private set; }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="playerId">Identifiant du joueur.</param>
        /// <param name="year">Année.</param>
        /// <param name="weekNo">Numéro de semaine.</param>
        /// <param name="weekPoints">Nombre de points cette semaine.</param>
        /// <param name="yearCalendarPoints">Nombre cumulé de points pour cette année civile.</param>
        /// <param name="yearRollingPoints">Nombre cumulé de points pour l'année glissante.</param>
        /// <param name="yearCalendarRanking">Classement à date sur l'année civile.</param>
        /// <param name="yearRollingRanking">Classement à date sur l'année glissante.</param>
        /// <param name="elo">Points ELO à date.</param>
        public AtpRanking(ulong playerId, uint year, uint weekNo, uint weekPoints, uint yearCalendarPoints, uint yearRollingPoints,
            ushort yearCalendarRanking, ushort yearRollingRanking, ushort elo)
        {
            Player = Player.GetById(playerId);
            Year = year;
            WeekNo = weekNo;
            WeekPoints = weekPoints;
            CalendarPoints = yearCalendarPoints;
            RollingPoints = yearRollingPoints;
            CalendarRank = yearCalendarRanking;
            RollingRank = yearRollingRanking;
            Elo = elo;
            _instances.Add(this);
        }

        /// <summary>
        /// Récupère le classement ATP à un moment précis dans le temps.
        /// </summary>
        /// <param name="date">Date du classement (le dimanche).</param>
        /// <param name="rollingSort">Si vrai, le tri est l'année glissante ; sinon, sur l'année civile.</param>
        /// <param name="limit">Nombre de joueurs retournés (0 ou un nombre négatif pour tous les retourner).</param>
        /// <returns>Le classement, trié par performance décroissante.</returns>
        public static ReadOnlyCollection<AtpRanking> GetAtpRankingAtDate(DateTime date, bool rollingSort, int limit)
        {
            int computedWeek = Tools.GetWeekNoFromDate(date);
            int computedYear = date.Year + (computedWeek == 1 && date.Month == 12 ? 1 : ((computedWeek >= 52 && date.Month == 1 ? -1 : 0)));
            return _instances
                        .Where(_ => _.WeekNo == computedWeek && _.Year == computedYear)
                        .OrderBy(_ => (rollingSort ? _.RollingRank : _.CalendarRank))
                        .Take(limit <= 0 ? _instances.Count : limit)
                        .ToList()
                        .AsReadOnly();
        }

        /// <summary>
        /// Récupère le classement ELO (global) à un moment précis dans le temps.
        /// </summary>
        /// <param name="date">Date du classement (le dimanche).</param>
        /// <param name="limit">Nombre de joueurs retournés.</param>
        /// <returns>Le classement, trié par ELO décroissant.</returns>
        public static ReadOnlyCollection<AtpRanking> GetEloRankingAtDate(DateTime date, int limit)
        {
            return _instances
                        .Where(_ => _.WeekNo == Tools.GetWeekNoFromDate(date) && _.Year == date.Year)
                        .OrderByDescending(_ => _.Elo)
                        .Take(limit)
                        .ToList()
                        .AsReadOnly();
        }
    }
}
