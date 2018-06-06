using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente une édition d'un tournoi.
    /// </summary>
    public class Edition : BaseService
    {
        #region Champs et propriétés

        /// <summary>
        /// Nombre minimal de matchs pour qu'une édition de tournoi se déroule sur deux semaines.
        /// </summary>
        public const int TWO_WEEKS_MIN_MATCH_COUNT = 65;

        // statistiques relatives à tous les joueurs ayant participés au tournoi
        private List<Stats> _stats = new List<Stats>();

        /// <summary>
        /// Tournoi.
        /// </summary>
        public Tournament Tournament { get; private set; }
        /// <summary>
        /// Année.
        /// </summary>
        public uint Year { get; private set; }
        /// <summary>
        /// Nombre de joueurs inscrits.
        /// </summary>
        public ushort DrawSize { get; private set; }
        /// <summary>
        /// Date exacte de début du tournoi.
        /// </summary>
        public DateTime DateBegin { get; private set; }
        /// <summary>
        /// Détermine si le tournoi se déroule sur deux semaines.
        /// </summary>
        public bool OnTwoWeeks { get; private set; }
        /// <summary>
        /// Date de fin de tournoi.
        /// </summary>
        /// <remarks>Calculé approximativement lors de l'insertion en base de données.</remarks>
        public DateTime DateEnd { get; private set; }
        /// <summary>
        /// Statistiques relatives à tous les joueurs ayant participés au tournoi.
        /// </summary>
        public ReadOnlyCollection<Stats> Statistics { get { return _stats.AsReadOnly(); } }
        /// <summary>
        /// Indicates if statistics are loaded for this edition.
        /// </summary>
        public bool StatisticsAreCompute { get; private set; }

        #region Données historiques du tournoi associé

        /// <summary>
        /// Détermine le nom du tournoi pour cette édition.
        /// </summary>
        public string TournamentName { get; private set; }
        /// <summary>
        /// Détermine la ville du tournoi pour cette édition.
        /// </summary>
        public string TournamentCity { get; private set; }
        /// <summary>
        /// Détermine le niveau du tournoi pour cette édition.
        /// </summary>
        public Level TournamentLevel { get; private set; }
        /// <summary>
        /// Détermine la surface du tournoi pour cette édition.
        /// </summary>
        public Surface TournamentSurface { get; private set; }
        /// <summary>
        /// Détermine si le tournoi est indoor ou non pour cette édition.
        /// </summary>
        public bool TournamentIsIndoor { get; private set; }
        /// <summary>
        /// Numéro d'ordre du tournoi (si <see cref="Level.masters_1000"/>) pour cette édition.
        /// </summary>
        public byte TournamentSlotOrder { get; private set; }

        #endregion

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        /// <param name="tournamentId">Identifiant du tournoi.</param>
        /// <param name="year">Année.</param>
        /// <param name="drawSize">Nombre de joueurs inscrits.</param>
        /// <param name="dateBegin">Date de début.</param>
        /// <param name="onTwoWeeks">Indique si l'édition se déroule sur deux semaines.</param>
        /// <param name="dateEnd">Date de fin.</param>
        /// <exception cref="BaseService.NotUniqueIdException">L'identifiant n'est pas unique.</exception>
        /// <exception cref="ArgumentException">Une édition avec le même identifiant existe déjà.</exception>
        /// <exception cref="ArgumentException">Une édition du tournoi pour la même année existe déjà.</exception>
        /// <exception cref="ArgumentException">Le tournoi avec l'identifiant spécifié n'a pas pu être trouvé.</exception>
        /// <exception cref="ArgumentException">La date de fin doit être postérieure à la date de début.</exception>
        public Edition(uint id, uint tournamentId, uint year, ushort drawSize, DateTime dateBegin, bool onTwoWeeks, DateTime dateEnd)
            : base(id)
        {
            if (GetByYearAndTournament(tournamentId, year) != null)
            {
                throw new ArgumentException("Une édition du tournoi pour la même année existe déjà.");
            }

            Tournament tournament = GetByID<Tournament>(tournamentId);
            if (tournament == null)
            {
                throw new ArgumentException("Le tournoi avec l'identifiant spécifié n'a pas pu être trouvé.", nameof(tournamentId));
            }

            if (dateBegin > dateEnd)
            {
                throw new ArgumentException("La date de fin doit être postérieure à la date de début.", nameof(dateEnd));
            }

            Tournament = tournament;
            Year = year;
            DrawSize = drawSize;
            DateBegin = dateBegin;
            OnTwoWeeks = onTwoWeeks;
            DateEnd = dateEnd;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        /// <param name="tournamentID">Identifiant du tournoi.</param>
        /// <param name="year">Année.</param>
        /// <param name="drawSize">Nombre de joueurs inscrits.</param>
        /// <param name="dateBegin">Date de début.</param>
        /// <param name="onTwoWeeks">Indique si l'édition se déroule sur deux semaines.</param>
        /// <param name="dateEnd">Date de fin.</param>
        /// <param name="tournamentCity">Ville pour cette édition.</param>
        /// <param name="tournamentIndoor">Environnement pour cette édition.</param>
        /// <param name="tournamentLevel">Niveau pour cette édition.</param>
        /// <param name="tournamentName">Nom pour cette édition.</param>
        /// <param name="tournamentSlotOrder">Créneau pour cette édition.</param>
        /// <param name="tournamentSurface">Surface pour cette édition.</param>
        /// <exception cref="BaseService.NotUniqueIdException">L'identifiant n'est pas unique.</exception>
        /// <exception cref="ArgumentException">Une édition avec le même identifiant existe déjà.</exception>
        /// <exception cref="ArgumentException">Une édition du tournoi pour la même année existe déjà.</exception>
        /// <exception cref="ArgumentException">Le tournoi avec l'identifiant spécifié n'a pas pu être trouvé.</exception>
        /// <exception cref="ArgumentException">La date de fin doit être postérieure à la date de début.</exception>
        public Edition(uint id, uint tournamentID, uint year, ushort drawSize, DateTime dateBegin, bool onTwoWeeks, DateTime dateEnd,
            bool? tournamentIndoor , Level? tournamentLevel, string tournamentName,
            string tournamentCity, byte? tournamentSlotOrder, Surface? tournamentSurface)
            : this(id, tournamentID, year, drawSize, dateBegin, onTwoWeeks, dateEnd)
        {
            TournamentIsIndoor = tournamentIndoor.HasValue ? tournamentIndoor.Value : Tournament.IsIndoor;
            TournamentLevel = tournamentLevel.HasValue ? tournamentLevel.Value : Tournament.Level;
            TournamentName = tournamentName != null ? tournamentName : Tournament.Name;
            TournamentCity = tournamentCity != null ? tournamentCity : Tournament.City;
            TournamentSlotOrder = tournamentSlotOrder.HasValue ? tournamentSlotOrder.Value : Tournament.SlotOrder;
            TournamentSurface = tournamentSurface.HasValue ? tournamentSurface.Value : Tournament.Surface;
        }

        /// <summary>
        /// Ajoute les statistiques relatives à un joueur pour cette édition.
        /// </summary>
        /// <param name="playerId">Identifiant du jouueur.</param>
        /// <param name="statType">Valeur d'énumération indiquant le type de statistique.</param>
        /// <param name="value">Valeur de la statistique.</param>
        public void AddPlayerStatistics(ulong playerId, StatType statType, uint value)
        {
            _stats.Add(new Stats(playerId, statType, value));
            StatisticsAreCompute = true;
        }

        /// <summary>
        /// Recherche une édition à partir de son année et du tournoi associé.
        /// </summary>
        /// <param name="tournamentId">Identifiant du tournoi associé.</param>
        /// <param name="year">Année de l'édition recherchée.</param>
        /// <returns>L'édition associée aux critères donnés, null si elle n'a pas été trouvée.</returns>
        public static Edition GetByYearAndTournament(ulong tournamentId, uint year)
        {
            return GetList<Edition>().FirstOrDefault(item => item.Year == year && item.Tournament.ID == tournamentId);
        }

        /// <summary>
        /// Recherche les éditions d'un tournoi donné.
        /// </summary>
        /// <param name="tournamentId">Identifiant du tournoi associé.</param>
        /// <returns>Liste des éditions du tournoi.</returns>
        public static List<Edition> GetByTournament(ulong tournamentId)
        {
            return GetList<Edition>().Where(_ => _.Tournament.ID == tournamentId).ToList();
        }

        /// <summary>
        /// Récupère le liste des éditions de tournoi ayant eu lieu sur une période de temps donnée et suivant d'autres critères optionnels.
        /// </summary>
        /// <param name="startDate">Date de début (inclusive).</param>
        /// <param name="endDate">Date de fin (exclusive).</param>
        /// <param name="levels">Liste de <see cref="Level"/>.</param>
        /// <param name="surfaces">Liste de <see cref="Surface"/>.</param>
        /// <param name="indoorOnly">Si activé, filtre les éditions indoor ; sinon ne filtre pas (ne récupère pas les éditions outdoor seules).</param>
        /// <returns>Une liste d'éditions de tournois.</returns>
        public static List<Edition> GetByPeriod(DateTime startDate, DateTime endDate, IEnumerable<Level> levels, IEnumerable<Surface> surfaces, bool indoorOnly)
        {
            return GetList<Edition>().Where(e =>
                e.DateEnd >= startDate
                && e.DateEnd < endDate
                && (levels?.Any() ==true ? levels.Contains(e.TournamentLevel) : true)
                && (surfaces?.Any() ==true ? surfaces.Contains(e.TournamentSurface) : true)
                && (indoorOnly ? e.TournamentIsIndoor : true)
            ).ToList();
        }

        /// <summary>
        /// Structure représentant une statistique relative à un joueur lors d'une édition.
        /// </summary>
        public struct Stats
        {
            /// <summary>
            /// Joueur.
            /// </summary>
            public Player Player { get; private set; }
            /// <summary>
            /// Valeur d'énumération indiquant le type de statistique.
            /// </summary>
            public StatType StatType { get; private set; }
            /// <summary>
            /// Valeur de la statistique.
            /// </summary>
            public uint Value { get; private set; }

            /// <summary>
            /// Constructeur.
            /// </summary>
            /// <param name="playerId">Identifiant du jouueur.</param>
            /// <param name="statType">Valeur d'énumération indiquant le type de statistique.</param>
            /// <param name="value">Valeur de la statistique.</param>
            public Stats(ulong playerId, StatType statType, uint value)
            {
                Player = GetByID<Player>(playerId);
                StatType = statType;
                Value = value;
            }
        }
    }
}
