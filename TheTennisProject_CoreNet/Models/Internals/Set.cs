namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Représente un set.
    /// </summary>
    public class Set
    {
        #region Champs et propriétés

        /// <summary>
        /// Nombre de jeux gagnés par le vainqueur du set.
        /// </summary>
        public byte WScore { get; private set; }
        /// <summary>
        /// Nombre de jeux gagnés par le vaincu du set.
        /// </summary>
        public byte LScore { get; private set; }
        /// <summary>
        /// Nombre de point gagnés au tie-break par le vainqueur du set.
        /// </summary>
        public ushort WTieBreak { get; private set; }
        /// <summary>
        /// Nombre de point gagnés au tie-break par le vaincu du set.
        /// </summary>
        public ushort LTieBreak { get; private set; }
        /// <summary>
        /// Détermine si le set a été interrompu.
        /// <remarks>Dans ce cas, la cohérence des informations de score n'est pas garantie.</remarks>
        /// </summary>
        public bool Interrupted { get; private set; }
        /// <summary>
        /// Détermine si un tie-break a eu lieu pour ce set.
        /// </summary>
        /// <returns>Vrai si tie-break, faux sinon.</returns>
        public bool IsTieBreak
        {
            get
            {
                return this.WTieBreak > 0;
            }
        }
        /// <summary>
        /// Indique que les données utilisées pour construire le set contienne une ou des erreurs.
        /// </summary>
        public bool InvalidData { get; private set; }

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="wScore">Score du vainqueur en nombre de jeux.</param>
        /// <param name="lScore">Score du vaincu en nombre de jeux.</param>
        /// <param name="lTieBreak">Nombre de points gagnés par le vaincu au tie-break.</param>
        /// <param name="interrupted">Détermine si le set a été interrompu.</param>
        public Set(byte wScore, byte lScore, ushort? lTieBreak, bool interrupted)
        {
            // aucun contrôle de cohérence si le set a été interrompu
            if (!interrupted)
            {
                if (wScore < 6)
                {
                   // throw new ArgumentException("Le vainqueur doit avoir gagné six jeux minimum.", "wScore");
                    InvalidData = true;
                }

                if (lScore >= wScore)
                {
                   // throw new ArgumentException("Le nombre de jeux gagnés par le vaincu doit être strictement inférieur au nombre de jeux gagnés par le vainqueur.", "lScore");
                    InvalidData = true;
                }

                if (wScore > 6 && wScore - lScore > 2)
                {
                    //throw new ArgumentException("Au delà de six jeux gagnés par le vainqueur, l'écart de jeux entre les deux joueurs doit être au maximum de deux.", "lScore");
                    InvalidData = true;
                }

                if (wScore == 7 && lScore == 6)
                {
                    if (!lTieBreak.HasValue)
                    {
                        InvalidData = true;
                    }
                    else
                    {
                        WTieBreak = (ushort)(lTieBreak.Value < 5 ? 7 : lTieBreak.Value + 2);
                        LTieBreak = lTieBreak.Value;
                    }
                }
                else
                {
                    if (lTieBreak.HasValue)
                    {
                     //   throw new ArgumentException("Le score, en nombre de jeux, n'autorise pas de tie-break.", "lTieBreak");
                        InvalidData = true;
                    }

                    WTieBreak = 0;
                    LTieBreak = 0;
                }
            }

            WScore = wScore;
            LScore = lScore;
            Interrupted = interrupted;
        }
    }
}
