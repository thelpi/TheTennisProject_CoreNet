using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente un joueur.
    /// </summary>
    public class Player : BaseService
    {
        #region Champs et propriétés

        /// <summary>
        /// L'identifiant de joueur utilisé quand le joueur réel est inconnu.
        /// </summary>
        public const ulong UNKNOWN_PLAYER_ID = 199999;

        /// <summary>
        /// Code ISO utilisée quand la nationalitée est indéterminée.
        /// </summary>
        public const string UNKNOWN_NATIONALITY_CODE = "UNK";

        // Historique des changements de nationalité sportive.
        private Dictionary<string, DateTime> _nationalitiesHistory = new Dictionary<string, DateTime>();

        /// <summary>
        /// Nom complet.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Nationalité sportive (code ISO de trois lettres).
        /// </summary>
        public string Nationality { get; private set; }
        /// <summary>
        /// Détermine si le joueur est gaucher (vrai), droitier (faux) ou si l'information n'est pas connue (null).
        /// </summary>
        public bool? IsLeftHanded { get; private set; }
        /// <summary>
        /// Taille, en centimètres. 0 si sa taille n'est pas connue.
        /// </summary>
        public uint Height { get; private set; }
        /// <summary>
        /// Date de naissance.
        /// </summary>
        public DateTime? DateOfBirth { get; private set; }
        /// <summary>
        /// Taille, en mètres. Calculée depuis <see cref="Height"/>.
        /// </summary>
        public decimal HeightInMeters
        {
            get
            {
                return Math.Round(this.Height / 100M, 2);
            }
        }
        /// <summary>
        /// Date de début d'activité (du premier match).
        /// </summary>
        public DateTime? DateBegin { get; private set; }
        /// <summary>
        /// Date de fn d'activité (du dernier match).
        /// </summary>
        public DateTime? DateEnd { get; private set; }
        /// <summary>
        /// Historique des précédentes nationalités sportives. La date spécifiée est celle de fin.
        /// <remarks>Les résultats sont triés par date croissante.</remarks>
        /// </summary>
        public Dictionary<string, DateTime> NationalitiesHistory
        {
            get
            {
                return _nationalitiesHistory.OrderBy(item => item.Value).ToDictionary(item => item.Key, item => item.Value);
            }
        }

        /// <summary>
        /// Liste de tous les joueurs instanciés.
        /// </summary>
        public static ReadOnlyCollection<Player> GetList
        {
            get
            {
                return GetList<Player>();
            }
        }

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        /// <param name="name">Nom.</param>
        /// <param name="nationality">Nationalité (code ISO).</param>
        /// <param name="isLeftHanded">Détermine si le joueur est gaucher (vrai), droitier (faux) ou si l'information est inconnue (null).</param>
        /// <param name="height">Hauteur en centimètres (note : null sera remplacée par 0).</param>
        /// <param name="dateOfBirth">Date de naissance.</param>
        /// <param name="dateBegin">Date de début d'activité / du premier match.</param>
        /// <param name="dateEnd">Date de fin d'activité / du dernier match.</param>
        /// <exception cref="BaseService.NotUniqueIdException">L'identifiant n'est pas unique.</exception>
        /// <exception cref="ArgumentException">Un joueur avec le même identifiant existe déjà.</exception>
        /// <exception cref="ArgumentException">Le nom ne peut pas être vide.</exception>
        /// <exception cref="ArgumentException">La nationalité ne peut pas être vide.</exception>
        /// <exception cref="ArgumentException">La nationalité doit être un sigle de trois lettres.</exception>
        /// <exception cref="ArgumentException">L'argument spécifié n'est pas une date valide.</exception>
        /// <exception cref="ArgumentException">La date de fin d'activité doit être postérieure à la date de début d'activité.</exception>
        public Player(ulong id, string name, string nationality, bool? isLeftHanded, uint? height,
            DateTime? dateOfBirth, DateTime? dateBegin, DateTime? dateEnd)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Le nom ne peut pas être vide.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(nationality))
            {
                throw new ArgumentException("La nationalité ne peut pas être vide.", nameof(nationality));
            }

            nationality = nationality.Trim().ToUpper();
            if (nationality.Length != 3)
            {
                throw new ArgumentException("La nationalité doit être un sigle de trois lettres.", nameof(nationality));
            }

            if (dateEnd.HasValue && dateBegin.HasValue && dateEnd.Value < dateBegin.Value)
            {
                throw new ArgumentException("La date de fin d'activité doit être postérieure à la date de début d'activité.", nameof(dateEnd));
            }

            DateOfBirth = dateOfBirth;
            Name = name;
            Nationality = nationality;
            IsLeftHanded = isLeftHanded;
            Height = height.HasValue ? height.Value : 0;
            DateBegin = dateBegin;
            DateEnd = dateEnd;
        }

        /// <summary>
        /// Ajoute une nationalité à l'historique du joueur spécifié.
        /// </summary>
        /// <remarks>L'historique n'est pas destiné à stocker la nationalité actuelle du joueur.</remarks>
        /// <param name="playerId">L'identifiant du joueur.</param>
        /// <param name="nationality">Nationalité (code ISO).</param>
        /// <param name="endDate">Date de fin.</param>
        /// <exception cref="ArgumentException">La nationalité ne peut pas etre vide.</exception>
        /// <exception cref="ArgumentException">La nationalité doit être un sigle de trois lettres.</exception>
        /// <exception cref="ArgumentException">La nationalité à mettre en historique ne doit pas être l'actuelle du joueur.</exception>
        /// <exception cref="ArgumentException">La nationalité spécifiée est déjà existante dans l'historique.</exception>
        /// <exception cref="ArgumentException">La date de fin spécifiée est déjà existante dans l'historique.</exception>
        /// <exception cref="ArgumentException">Le joueur spécifié n'a pas été trouvé.</exception>
        public static void AddNationalitiesHistoryEntry(ulong playerId, string nationality, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(nationality))
            {
                throw new ArgumentException("La nationalité ne peut pas etre vide.", nameof(nationality));
            }

            nationality = nationality.Trim().ToUpper();
            if (nationality.Length < 3)
            {
                throw new ArgumentException("La nationalité doit être un sigle de trois lettres.", nameof(nationality));
            }

            Player p = GetByID<Player>(playerId);
            if (p == null)
            {
                throw new ArgumentException("Le joueur spécifié n'a pas été trouvé.", nameof(playerId));
            }

            if (nationality == p.Nationality)
            {
                throw new ArgumentException("La nationalité à mettre en historique ne doit pas être l'actuelle du joueur.", nameof(nationality));
            }

            if (p._nationalitiesHistory.ContainsKey(nationality))
            {
                throw new ArgumentException("La nationalité spécifiée est déjà existante dans l'historique.", nameof(nationality));
            }

            if (p._nationalitiesHistory.Select(item => item.Value.Date).ToList().Contains(endDate.Date))
            {
                throw new ArgumentException("La date de fin spécifiée est déjà existante dans l'historique.", nameof(endDate));
            }

            p._nationalitiesHistory.Add(nationality, endDate);
        }

        /// <summary>
        /// Calcule le nombre de points ATP (voir <see cref="PointsAtpScale"/> pour les détails) sur un an à partir d'une date donnée.
        /// </summary>
        /// <remarks>C'est la date de début de compétition qui fait foi, pas la date exacte du match (qui n'est pas connue).</remarks>
        /// <param name="beginDate">Date de début de prise en comtpe.</param>
        /// <returns>Le nombre de points ATP cumulés par le joueur sur cette période.</returns>
        /// <exception cref="ArgumentException">La date de début doit être antérieure à la date de fin.</exception>
        public long ComputeAtpPoints(DateTime beginDate)
        {
            return ComputeAtpPoints(beginDate, null);
        }

        /// <summary>
        /// Calcule le nombre de points ATP (voir <see cref="PointsAtpScale"/> pour les détails) sur une période donnée.
        /// </summary>
        /// <remarks>C'est la date de début de compétition qui fait foi, pas la date exacte du match (qui n'est pas connue).</remarks>
        /// <param name="beginDate">Date de début de prise en comtpe.</param>
        /// <param name="endDate">Date de fin de prise en compte. Si <c>Null</c>, la valeur retenue sera <paramref name="beginDate"/> incrémentée de un an.</param>
        /// <returns>Le nombre de points ATP cumulés par le joueur sur cette période.</returns>
        /// <exception cref="ArgumentException">La date de début doit être antérieure à la date de fin.</exception>
        public uint ComputeAtpPoints(DateTime beginDate, DateTime? endDate)
        {
            if (!endDate.HasValue)
            {
                endDate = beginDate.AddYears(1);
            }
            else if (endDate <= beginDate)
            {
                throw new ArgumentException("La date de début doit être antérieure à la date de fin.", nameof(endDate));
            }

            uint points = 0;

            // TODO : fonctionner par semaine entière
            List<Edition> editions = GetList<Edition>().Where(e => e.DateBegin >= beginDate && e.DateBegin <= endDate).ToList();
            foreach (Edition edition in editions)
            {
                points += ComputePlayerStatsForEdition(ID, edition.ID, StatType.points);
            }

            return points;
        }

        /// <summary>
        /// Calcule les statistiques d'un joueur spécifié pour une édition de tournoi.
        /// </summary>
        /// <param name="playerId">L'identifiant du joueur.</param>
        /// <param name="editionId">Identifiant de l'édition.</param>
        /// <param name="stats">Statistique à calculer.</param>
        /// <returns>La statistique.</returns>
        /// <exception cref="ArgumentException">Le joueur spécifié n'a pas été trouvé.</exception>
        public static uint ComputePlayerStatsForEdition(ulong playerId, ulong editionId, StatType stats)
        {
            Player p = GetByID<Player>(playerId);
            if (p == null)
            {
                throw new ArgumentException("Le joueur spécifié n'a pas été trouvé.", nameof(playerId));
            }

            List<Match> baseMatchesList =
                Match.GetPlayerMatches(p.ID)
                    .Where(item => item.Edition.ID == editionId)
                    .ToList();

            if (baseMatchesList.Count == 0)
            {
                return 0;
            }

            switch (stats)
            {
                case StatType.round:
                    return (uint)baseMatchesList.OrderBy(m => m.Round.GetSortOrder()).First().Round;
                case StatType.is_winner:
                    return (uint)(baseMatchesList.OrderBy(m => m.Round.GetSortOrder()).First().Winner == p ? 1 : 0);
                #region Calcul des points ATP
                case StatType.points:
                    // matchs avec points cumulés
                    List<Match> cumuledTypeMatches =
                        baseMatchesList
                            .Where(item =>
                                PointsAtpScale.GetLevelScale(item.Edition.TournamentLevel, item.Round)[0].IsCumuled)
                            .ToList();
                    long p1 = cumuledTypeMatches.Sum(item => PointsAtpScale.GetPoints(item, p, item.PlayerWasExempt(p)));

                    // matchs perdus dés l'entrée en lice
                    List<Match> nonCumuledFirstTurnLose =
                        baseMatchesList
                            .Where(item =>
                                !PointsAtpScale.GetLevelScale(item.Edition.TournamentLevel, item.Round)[0].IsCumuled &&
                                item.Loser == p &&
                                !baseMatchesList.Any(subItem => subItem.Edition == item.Edition && subItem.Round.RoundIsBefore(item.Round)))
                            .ToList();
                    long p2 = nonCumuledFirstTurnLose.Sum(item => PointsAtpScale.GetPoints(item, p, item.PlayerWasExempt(p)));

                    // matchs gagnés
                    List<Match> nonCumuledBestWin =
                        baseMatchesList
                            .Where(item =>
                                !PointsAtpScale.GetLevelScale(item.Edition.TournamentLevel, item.Round)[0].IsCumuled &&
                                item.Winner == p &&
                                !baseMatchesList.Any(subItem => subItem.Edition == item.Edition && item.Round.RoundIsBefore(subItem.Round) && subItem.Winner == p))
                            .ToList();
                    long p3 = nonCumuledBestWin.Sum(item => PointsAtpScale.GetPoints(item, p, item.PlayerWasExempt(p)));

                    return (uint)(p1 + p2 + p3);
                #endregion
                case StatType.match_win:
                    return (uint)baseMatchesList.Count(m => m.Winner == p && !m.Walkover);
                case StatType.match_lost:
                    return (uint)baseMatchesList.Count(m => m.Loser == p && !m.Walkover);
                case StatType.set_win:
                    return (uint)baseMatchesList.Sum(m => m.Sets.Count(s => s.HasValue && (m.Winner == p ? s.Value.Key == p : s.Value.Key != p)));
                case StatType.set_lost:
                    return (uint)baseMatchesList.Sum(m => m.Sets.Count(s => s.HasValue && (m.Winner == p ? s.Value.Key != p : s.Value.Key == p)));
                case StatType.game_win:
                    return (uint)baseMatchesList.Sum(m => m.Sets.Sum(s => !s.HasValue ? 0 : ((m.Winner == p ? s.Value.Key == p : s.Value.Key != p) ? s.Value.Value.WScore : s.Value.Value.LScore)));
                case StatType.game_lost:
                    return (uint)baseMatchesList.Sum(m => m.Sets.Sum(s => !s.HasValue ? 0 : ((m.Winner == p ? s.Value.Key != p : s.Value.Key == p) ? s.Value.Value.WScore : s.Value.Value.LScore)));
                case StatType.tb_win:
                    return (uint)baseMatchesList.Sum(m => m.Sets.Count(s => s.HasValue && (m.Winner == p ? s.Value.Key == p : s.Value.Key != p) && s.Value.Value.IsTieBreak));
                case StatType.tb_lost:
                    return (uint)baseMatchesList.Sum(m => m.Sets.Count(s => s.HasValue && (m.Winner == p ? s.Value.Key != p : s.Value.Key == p) && s.Value.Value.IsTieBreak));
                case StatType.ace:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCountAce ?? 0 : m.LoserCountAce ?? 0));
                case StatType.d_f:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCountDbFault ?? 0 : m.LoserCountDbFault ?? 0));
                case StatType.sv_pt:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCountServePt ?? 0 : m.LoserCountServePt ?? 0));
                case StatType.first_in:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCount1stIn ?? 0 : m.LoserCount1stIn ?? 0));
                case StatType.first_won:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCount1stWon ?? 0 : m.LoserCount1stWon ?? 0));
                case StatType.second_won:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCount2ndWon ?? 0 : m.LoserCount2ndWon ?? 0));
                case StatType.sv_gms:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCountServeGames ?? 0 : m.LoserCountServeGames ?? 0));
                case StatType.bp_saved:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCountBreakPtSaved ?? 0 : m.LoserCountBreakPtSaved ?? 0));
                case StatType.bp_faced:
                    return (uint)(baseMatchesList.Sum(m => m.Winner == p ? m.WinnerCountBreakPtFaced ?? 0 : m.LoserCountBreakPtFaced ?? 0));
            }

            return 0;
        }

        /// <summary>
        /// Récupère un joueur par son identifier.
        /// </summary>
        /// <param name="playerId">Identifiant du joueur.</param>
        /// <returns>Le joueur. <c>Null</c> si non trouvé.</returns>
        public static Player GetById(ulong playerId)
        {
            return GetByID<Player>(playerId);
        }
    }
}
