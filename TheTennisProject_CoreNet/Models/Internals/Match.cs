using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente un match.
    /// </summary>
    public class Match : BaseService
    {
        #region Champs et propriétés

        // Indique que la création des matchs est en mode batch.
        private static volatile bool _batchMode = false;

        // Liste des sets triés chronologiquement. La clé du kvp est le joueur vainqueur du set.
        private List<KeyValuePair<Player, Set>?> _sets = new List<KeyValuePair<Player, Set>?>()
        {
            new KeyValuePair<Player, Set>(null, null),
            new KeyValuePair<Player, Set>(null, null),
            new KeyValuePair<Player, Set>(null, null),
            new KeyValuePair<Player, Set>(null, null),
            new KeyValuePair<Player, Set>(null, null)
        };

        /// <summary>
        /// Edition du tournoi pour laquelle ce match a eu lieu.
        /// </summary>
        public Edition Edition { get; private set; }
        /// <summary>
        /// Numéro du match dans l'ordre de l'édition.
        /// </summary>
        public ushort MatchNum { get; private set; }
        /// <summary>
        /// Le tour associé au match.
        /// </summary>
        public Round Round { get; private set; }
        /// <summary>
        /// Nombre de sets minimum pour gagner le match (3 ou 5).
        /// </summary>
        public byte BestOf { get; private set; }
        /// <summary>
        /// Nombre de minutes (0 si inconnu).
        /// </summary>
        public uint Minutes { get; private set; }
        /// <summary>
        /// Match terminé oui / non.
        /// </summary>
        public bool Unfinished { get; private set; }
        /// <summary>
        /// Abandon en cours de match d'un des joueurs.
        /// </summary>
        public bool Retirement { get; private set; }
        /// <summary>
        /// Forfait d'un des joueurs.
        /// </summary>
        public bool Walkover { get; private set; }
        /// <summary>
        /// Liste de sets du match, triés chronologiquement. La clé du kvp est le joueur vainqueur du set.
        /// </summary>
        /// <remarks>Si le set est interrompu, le vainqueur du match est vainqueur du set.</remarks>
        public ReadOnlyCollection<KeyValuePair<Player, Set>?> Sets
        {
            get
            {
                return _sets.AsReadOnly();
            }
        }
        /// <summary>
        /// Nombre total de jeux durant le match.
        /// </summary>
        public int CountGames
        {
            get
            {
                return _sets.Sum(_ => _.HasValue ? _.Value.Value.LScore + _.Value.Value.WScore : 0);
            }
        }

        #region Informations relatives au vainqueur

        /// <summary>
        /// Joueur vainqueur.
        /// </summary>
        public Player Winner { get; private set; }
        /// <summary>
        /// Numéro de tête de série du vainqueur (0 si inconnu ou non tête de série).
        /// </summary>
        public uint? WinnerSeed { get; private set; }
        /// <summary>
        /// Particularité d'introduction du vainqueur dans le tournoi (qualifié, lucky loser, wild-card...).
        /// </summary>
        public string WinnerEntry { get; private set; }
        /// <summary>
        /// Classement ATP du vainqueur avant le match.
        /// </summary>
        public uint? WinnerRank { get; private set; }
        /// <summary>
        /// Points ATP du vainqueur avant le match.
        /// </summary>
        public uint? WinnerRankPoints { get; private set; }

        #region Statistiques

        /// <summary>
        /// Nombre d'aces (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCountAce { get; private set; }
        /// <summary>
        /// Nombre de double-fautes (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCountDbFault { get; private set; }
        /// <summary>
        /// Nombre total de points au service (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCountServePt { get; private set; }
        /// <summary>
        /// Nombre de premiers services valides (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCount1stIn { get; private set; }
        /// <summary>
        /// Nombre de points gagnés sur le premier service (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCount1stWon { get; private set; }
        /// <summary>
        /// Nombre de points gagnés sur le second service (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCount2ndWon { get; private set; }
        /// <summary>
        /// Nombre de jeux de service joués (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCountServeGames { get; private set; }
        /// <summary>
        /// Nombre de balles de break sauvées (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCountBreakPtSaved { get; private set; }
        /// <summary>
        /// Nombre de balles de break concédées (vainqueur, null si inconnu).
        /// </summary>
        public uint? WinnerCountBreakPtFaced { get; private set; }

        #endregion

        #endregion

        #region Informations relatives au vaincu

        /// <summary>
        /// Joueur vaincu.
        /// </summary>
        public Player Loser { get; private set; }
        /// <summary>
        /// Numéro de tête de série du vaincu (0 si inconnu ou non tête de série).
        /// </summary>
        public uint? LoserSeed { get; private set; }
        /// <summary>
        /// Particularité d'introduction du vaincu dans le tournoi (qualifié, lucky loser, wild-card...).
        /// </summary>
        public string LoserEntry { get; private set; }
        /// <summary>
        /// Classement ATP du vaincu avant le match.
        /// </summary>
        public uint? LoserRank { get; private set; }
        /// <summary>
        /// Points ATP du vaincu avant le match.
        /// </summary>
        public uint? LoserRankPoints { get; private set; }

        #region Statistiques

        /// <summary>
        /// Nombre d'aces (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCountAce { get; private set; }
        /// <summary>
        /// Nombre de double-fautes (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCountDbFault { get; private set; }
        /// <summary>
        /// Nombre total de points au service (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCountServePt { get; private set; }
        /// <summary>
        /// Nombre de premiers services valides (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCount1stIn { get; private set; }
        /// <summary>
        /// Nombre de points gagnés sur le premier service (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCount1stWon { get; private set; }
        /// <summary>
        /// Nombre de points gagnés sur le second service (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCount2ndWon { get; private set; }
        /// <summary>
        /// Nombre de jeux de service joués (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCountServeGames { get; private set; }
        /// <summary>
        /// Nombre de balles de break sauvées (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCountBreakPtSaved { get; private set; }
        /// <summary>
        /// Nombre de balles de break concédées (vaincu, null si inconnu).
        /// </summary>
        public uint? LoserCountBreakPtFaced { get; private set; }

        #endregion

        #endregion

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Identifiant du match.</param>
        /// <param name="editionID">Identifiant de l'édition du tournoi.</param>
        /// <param name="matchNum">Numéro de match dans l'édition du tournoi.</param>
        /// <param name="round">Tour de compétition associé.</param>
        /// <param name="bestOf">Nombre de sets minimum (non casté depuis l'information en base).</param>
        /// <param name="minutes">Nombre de minutes.</param>
        /// <param name="unfinished">Match arrêté oui / non.</param>
        /// <param name="retirement">Victoire par abandon.</param>
        /// <param name="walkover">Victoire par forfait.</param>
        /// <param name="winnerID">Identifiant du vainqueur.</param>
        /// <param name="winnerSeed">Numéro de tête de série du vainqueur.</param>
        /// <param name="winnerEntry">Spécificité d'entrée dans le tournoi du vainqueur.</param>
        /// <param name="winnerRank">Classement ATP du vainqueur.</param>
        /// <param name="winnerRankPoints">Nombre de points ATP du vainqueur.</param>
        /// <param name="loserID">Identifiant du vaincu.</param>
        /// <param name="loserSeed">Numéro de tête de série du vaincu.</param>
        /// <param name="loserEntry">Spécificité d'entrée dans le tournoi du vaincu.</param>
        /// <param name="loserRank">Classement ATP du vaincu.</param>
        /// <param name="loserRankPoints">Nombre de points ATP du vaincu.</param>
        /// <exception cref="BaseService.NotUniqueIdException">L'identifiant n'est pas unique.</exception>
        /// <exception cref="ArgumentException">L'identifiant spécifié existe déjà dans la collection.</exception>
        /// <exception cref="ArgumentException">L'identifiant fourni est invalide.</exception>
        /// <exception cref="ArgumentException">Pour une édition, les numéros de matchs doivent être uniques.</exception>
        /// <exception cref="ArgumentException">Le vainqueur et le vaincu ne peuvent pas être identiques.</exception>
        /// <exception cref="ArgumentException">Le vainqueur n'est pas un joueur valide.</exception>
        /// <exception cref="ArgumentException">Le vaincu n'est pas un joueur valide.</exception>
        public Match(ulong id, uint editionID, ushort matchNum, Round round, byte bestOf, uint? minutes, bool unfinished, bool retirement, bool walkover,
            ulong winnerID, uint? winnerSeed, string winnerEntry, uint? winnerRank, uint? winnerRankPoints,
            ulong loserID, uint? loserSeed, string loserEntry, uint? loserRank, uint? loserRankPoints)
            : base(id)
        {
            Edition edition = GetByID<Edition>(editionID);
            if (edition == null)
            {
                throw new ArgumentException("L'identifiant fourni est invalide.", nameof(editionID));
            }

            // En mode batch, la cohérence de l'édition et du numéro de match n'est pas vérifiée.
            if (!_batchMode && GetByEditionAndMatchNum(editionID, matchNum) != null)
            {
                throw new ArgumentException("Pour une édition, les numéros de matchs doivent être uniques.", nameof(matchNum));
            }

            if (winnerID == loserID && winnerID != Player.UNKNOWN_PLAYER_ID)
            {
                throw new ArgumentException("Le vainqueur et le vaincu ne peuvent pas être identiques.");
            }

            Player winner = GetByID<Player>(winnerID);
            Player loser = GetByID<Player>(loserID);

            if (winner == null)
            {
                throw new ArgumentException("Le vainqueur n'est pas un joueur valide.", nameof(winnerID));
            }

            if (loser == null)
            {
                throw new ArgumentException("Le vaincu n'est pas un joueur valide.", nameof(loserID));
            }

            Edition = edition;
            MatchNum = matchNum;
            Round = round;
            BestOf = bestOf;
            Minutes = minutes.HasValue ? minutes.Value : 0;
            Unfinished = unfinished;
            Retirement = retirement;
            Walkover = walkover;

            Winner = winner;
            WinnerEntry = winnerEntry;
            WinnerRank = winnerRank;
            WinnerRankPoints = winnerRankPoints;
            WinnerRankPoints = winnerSeed;

            Loser = loser;
            LoserEntry = loserEntry;
            LoserRank = loserRank;
            LoserRankPoints = loserRankPoints;
            LoserRankPoints = loserSeed;
        }

        /// <summary>
        /// Ajoute un set.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="setNumber">Numéro de set, entre 1 et 5.</param>
        /// <param name="wScore">Score du vainqueur, si connu.</param>
        /// <param name="lScore">Score du vaincu, si connu.</param>
        /// <param name="tieBreak">Points du vaincu au tie-break s'il a eu lieu.</param>
        /// <exception cref="ArgumentException">Le numéro de set est invalide.</exception>
        /// <exception cref="ArgumentException">Le set n'a pas pu être crée. Voir le détail de l'exception.</exception>
        public void AddSetByNumber(byte setNumber, byte? wScore, byte? lScore, ushort? tieBreak)
        {
            if (setNumber < 1 || setNumber > 5 || (setNumber > this.BestOf && (wScore.HasValue || lScore.HasValue || tieBreak.HasValue)))
            {
                throw new ArgumentException("Le numéro de set est invalide.", nameof(setNumber));
            }

            if (!wScore.HasValue || !lScore.HasValue)
            {
                _sets[setNumber - 1] = null;
            }
            else
            {
                Player setWinner = Winner;
                if (wScore.Value < lScore.Value)
                    setWinner = Loser;

                Set set = new Set(wScore.Value < lScore.Value ? lScore.Value : wScore.Value,
                    wScore.Value < lScore.Value ? wScore.Value : lScore.Value, tieBreak, Unfinished || Retirement);

                _sets[setNumber - 1] = new KeyValuePair<Player, Set>(setWinner, set);
            }
        }

        /// <summary>
        /// Met à jour les statistiques du match pour chaque joueur.
        /// </summary>
        /// <param name="winnerStats">Statistiques du vainqueur (les clés sont les noms des colonnes de la base de données).</param>
        /// <param name="loserStats">Statistiques du vaincu (les clés sont les noms des colonnes de la base de données).</param>
        /// <exception cref="ArgumentNullException">Le paramètre <paramref name="winnerStats"/> ne peut pas être null.</exception>
        /// <exception cref="ArgumentNullException">Le paramètre <paramref name="loserStats"/> ne peut pas être null.</exception>
        /// <exception cref="ArgumentException">Impossible de récupérer les valeurs attendues depuis le tableau <paramref name="winnerStats"/>. Voir le détail de l'exception.</exception>
        /// <exception cref="ArgumentException">Impossible de récupérer les valeurs attendues depuis le tableau <paramref name="loserStats"/>. Voir le détail de l'exception.</exception>
        public void DefineStatistics(IDictionary<string, uint?> winnerStats, IDictionary<string, uint?> loserStats)
        {
            if (winnerStats == null)
            {
                throw new ArgumentNullException(nameof(winnerStats));
            }

            if (loserStats == null)
            {
                throw new ArgumentNullException(nameof(loserStats));
            }

            try
            {
                WinnerCountAce = winnerStats["w_ace"];
                WinnerCountDbFault = winnerStats["w_df"];
                WinnerCountServePt = winnerStats["w_svpt"];
                WinnerCount1stIn = winnerStats["w_1stIn"];
                WinnerCount1stWon = winnerStats["w_1stWon"];
                WinnerCount2ndWon = winnerStats["w_2ndWon"];
                WinnerCountServeGames = winnerStats["w_SvGms"];
                WinnerCountBreakPtSaved = winnerStats["w_bpSaved"];
                WinnerCountBreakPtFaced = winnerStats["w_bpFaced"];
            }
            catch (Exception innerException)
            {
                throw new ArgumentException("Impossible de récupérer les valeurs attendues depuis le tableau. Voir le détail de l'exception.", nameof(winnerStats), innerException);
            }

            try
            {
                LoserCountAce = loserStats["l_ace"];
                LoserCountDbFault = loserStats["l_df"];
                LoserCountServePt = loserStats["l_svpt"];
                LoserCount1stIn = loserStats["l_1stIn"];
                LoserCount1stWon = loserStats["l_1stWon"];
                LoserCount2ndWon = loserStats["l_2ndWon"];
                LoserCountServeGames = loserStats["l_SvGms"];
                LoserCountBreakPtSaved = loserStats["l_bpSaved"];
                LoserCountBreakPtFaced = loserStats["l_bpFaced"];
            }
            catch (Exception innerException)
            {
                throw new ArgumentException("Impossible de récupérer les valeurs attendues depuis le tableau. Voir le détail de l'exception.", nameof(loserStats), innerException);
            }
        }

        /// <summary>
        /// Recherche un match par son édition et son numéro dans cette édition.
        /// </summary>
        /// <param name="editionID">L'identifiant de l'édition à rechercher.</param>
        /// <param name="matchNum">Le numéro de match à rechercher.</param>
        /// <returns>Le match correspondant aux critères, null s'il n'a pas été trouvé.</returns>
        public static Match GetByEditionAndMatchNum(uint editionID, ushort matchNum)
        {
            IEnumerable<Match> baseList = GetList<Match>().Where(_ => _.Edition.ID == editionID);
            if (!baseList.Any())
            {
                baseList = SqlMapping.Instance.LoadMatches(editionID, null);
            }

            return baseList.FirstOrDefault(_ => _.MatchNum == matchNum);
        }

        /// <summary>
        /// Récupère la liste des matchs associés à une édition de tournoi.
        /// </summary>
        /// <param name="editionID">Identifiant de l'édition.</param>
        /// <returns>Liste des matchs.</returns>
        public static ReadOnlyCollection<Match> GetByEdition(uint editionID)
        {
            IEnumerable<Match> baseList = GetList<Match>().Where(_ => _.Edition.ID == editionID);
            if (!baseList.Any())
            {
                baseList = SqlMapping.Instance.LoadMatches(editionID, null);
            }

            return baseList.ToList().AsReadOnly();
        }

        /// <summary>
        /// Active ou désactive le mode batch.
        /// </summary>
        /// <remarks>Le mode batch permet de s'affranchir de certains contrôles de cohérence lors que beaucoup de matchs doivent être chargés.
        /// A la désactivation, les contrôles manquants sont effectués, une exception est levée à la première erreur.</remarks>
        /// <param name="batchMode">Si vrai, active le mode batch. Sinon, le désactive.</param>
        /// <exception cref="InvalidOperationException">Le mode batch a provoqué des incohérences d'unicité sur l'édition et le numéro de match ([numéro édition], [numéro match]).</exception>
        public static void SetBatchMode(bool batchMode)
        {
            _batchMode = batchMode;

            if (!_batchMode)
            {
                List<KeyValuePair<ushort, ulong>> matchNumAndEditionList =
                    GetList<Match>()
                        .Select(item => new KeyValuePair<ushort, ulong>(item.MatchNum, item.Edition.ID))
                        .OrderBy(item => item.Key)
                        .ThenBy(item => item.Value)
                        .ToList();
                for (int i = 0; i < matchNumAndEditionList.Count - 1; i++)
                {
                    if (matchNumAndEditionList[i + 1].Equals(matchNumAndEditionList[i]))
                    {
                        throw new InvalidOperationException(string.Format("Le mode batch a provoqué des incohérences d'unicité sur l'édition et le numéro de match (édition {0}, match {1}).",
                            matchNumAndEditionList[i].Value,
                            matchNumAndEditionList[i].Key));
                    }
                }
            }
        }

        /// <summary>
        /// Récupère tous les matchs joués par un joueur.
        /// </summary>
        /// <param name="playerID">Identifiant du joueur.</param>
        /// <returns>Liste des matchs du joueur.</returns>
        public static ReadOnlyCollection<Match> GetPlayerMatches(ulong playerID)
        {
            IEnumerable<Match> baseList = GetList<Match>().Where(item => item.Winner.ID == playerID || item.Loser.ID == playerID);
            if (!baseList.Any())
            {
                baseList = SqlMapping.Instance.LoadMatches(null, playerID);
            }

            return baseList.ToList().AsReadOnly();
        }

        /// <summary>
        /// Détermine si le joueur sélectionné était exempt au tour qui a précédé ce match.
        /// </summary>
        /// <param name="player">Le joueur a vérifié (qui doit être soit le vainqueur, soit le vaincu du match).</param>
        /// <returns>True s'il était exempt au tour précédent, False sinon.</returns>
        /// <exception cref="ArgumentException">Le joueur spécifié n'a aucun rapport avec le match en cours.</exception>
        public bool PlayerWasExempt(Player player)
        {
            if (Winner != player && Loser != player)
            {
                throw new ArgumentException("Le joueur spécifié n'a aucun rapport avec le match en cours.", nameof(player));
            }

            // Le tour précédent suit une formule "+ 1" sur les valeurs d'énumération.
            Round previousOne = (Round)(((int)Round) + 1);

            switch (Round)
            {
                // Pour ces deux tours, pas de précédent possible.
                case Round.R128:
                case Round.RR:
                    return false;
                // Pour ce tour, la formule habituelle ne fonctionne pas.
                case Round.BR:
                    previousOne = Round.SF;
                    break;
            }

            // Détermine si l'édition avait bien un tour associé à "previousOne". Si ce n'est pas le cas, l'exemption n'a pas eu lieu.
            bool editionHadPreviousRound = GetList<Match>().Any(item => item.Edition == Edition && item.Round == previousOne);
            if (!editionHadPreviousRound)
            {
                return false;
            }

            // Détermine l'exemption.
            return !GetPlayerMatches(player.ID).Any(item => item.Edition == Edition && item.Round == previousOne);
        }
    }
}
