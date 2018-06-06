using System;
using System.Collections.ObjectModel;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente un tournoi, toutes éditions confondues.
    /// La dernière édition du tournoi sert de référence pour la majorité des propriétés (<see cref="Name"/>, <see cref="Surface"/>, ...).
    /// </summary>
    public class Tournament : BaseService
    {
        #region Champs et propriétés

        // identifiant du tournoi de substitution. 0 si actif ou non substitué.
        private uint _substituteID = 0;

        /// <summary>
        /// Nom du tournoi.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Ville dans laquelle le tournoi est joué.
        /// </summary>
        /// <remarks>Par défaut, se confond avec <seealso cref="Name"/>.</remarks>
        public string City { get; private set; }
        /// <summary>
        /// Détermine le niveau de compétition du tournoi.
        /// </summary>
        public Level Level { get; private set; }
        /// <summary>
        /// Détermine la surface sur laquelle le tournoi est joué.
        /// </summary>
        public Surface Surface { get; private set; }
        /// <summary>
        /// Détermine si le tournoi est joué en intérieur.
        /// </summary>
        public bool IsIndoor { get; private set; }
        /// <summary>
        /// Position du tournoi dans le calendrier.
        /// </summary>
        /// <remarks>S'applique aux tournois de niveau Master 1000 et équivalent uniquement, 0 sinon.</remarks>
        public byte SlotOrder { get; private set; }
        /// <summary>
        /// Année de disparition.
        /// </summary>
        /// <remarks>0 Si le tournoi existe toujours.</remarks>
        public uint Lastyear { get; private set; }
        /// <summary>
        /// Tournoi ayant servi de substitution dans le calendrier.
        /// </summary>
        /// <remarks>Si applicable, null sinon.</remarks>
        /// <exception cref="TournamentSubstituteNotFoundException"><see cref="_substituteID"/> ne fait référence à aucun tournoi de la collection statique.</exception>
        public Tournament Substitute
        {
            get
            {
                Tournament substituteTournament = null;
                if (_substituteID > 0)
                {
                    substituteTournament = BaseService.GetByID<Tournament>(_substituteID);
                    if (substituteTournament == null)
                        throw new TournamentSubstituteNotFoundException(_substituteID);
                }
                return substituteTournament;
            }
        }

        /// <summary>
        /// Liste de tous les joueurs instanciés.
        /// </summary>
        public static ReadOnlyCollection<Tournament> GetList
        {
            get
            {
                return GetList<Tournament>();
            }
        }

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        /// <param name="name">Nom.</param>
        /// <param name="city">Ville.</param>
        /// <param name="level">Niveau de compétition.</param>
        /// <param name="surface">Surface.</param>
        /// <param name="isIndoor">Joué en intérieur (vrai) ou extérieur (faux).</param>
        /// <param name="slotOrder">Position dans le calendrier (Master 1000 ou équivalent uniquement). 0 par défaut. Aucun contrôle de cohérence n'est réalisé.</param>
        /// <param name="lastYear">Dernière année à laquelle le tournoi s'est joué. 0 si le tournoi est toujours actif.</param>
        /// <param name="substituteID">Identifiant du tournoi de substitution, ne doit être défini que si <paramref name="lastYear"/> est spécifié. 0 par défaut.</param>
        /// <exception cref="BaseService.NotUniqueIdException">L'identifiant n'est pas unique.</exception>
        /// <exception cref="ArgumentException">Le nom du tournoi ne doit pas être vide.</exception>
        public Tournament(uint id, string name, string city, Level level, Surface surface, bool isIndoor, byte slotOrder, uint lastYear, uint substituteID)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Le nom du tournoi ne doit pas être vide.", nameof(name));
            }
            name = name.Trim();

            city = string.IsNullOrWhiteSpace(city) ? name : city.Trim();

            // TODO : le créneau devrait être obligatoire et vérifié pour les Masters 1000 
            if (level != Level.masters_1000)
            {
                slotOrder = 0;
            }

            if (lastYear == 0)
            {
                substituteID = 0;
            }

            Name = name;
            City = city;
            Level = level;
            Surface = surface;
            IsIndoor = isIndoor;
            SlotOrder = slotOrder;
            Lastyear = lastYear;
            _substituteID = substituteID;
        }

        /// <summary>
        /// Implémentation de l'exception levée quand l'identifiant de substitution d'un tournoi n'a pas été trouvé dans la collection globale.
        /// </summary>
        public class TournamentSubstituteNotFoundException : Exception
        {
            #region Champs et propriétés

            /// <summary>
            /// Identifiant du tournoi de substitution non trouvé.
            /// </summary>
            public uint ID { get; private set; }

            #endregion

            /// <summary>
            /// Constructeur.
            /// </summary>
            /// <param name="id">Identifiant du tournoi de substitution non trouvé.</param>
            public TournamentSubstituteNotFoundException(uint id)
                : base()
            {
                this.ID = id;
            }
        }
    }
}
