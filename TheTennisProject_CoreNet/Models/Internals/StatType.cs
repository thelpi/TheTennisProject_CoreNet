using System.ComponentModel;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Enumération des statistiques disponibles pour un joueur pour une édition de tournoi.
    /// En description, la colonne SQL associée.
    /// </summary>
    public enum StatType
    {
        /// <summary>
        /// Dernier tour disputé dans le tournoi.
        /// </summary>
        [SqlMapping("round_ID"), Description("Dernier tour")]
        round,
        /// <summary>
        /// Vainqueur du tournoi (ou du match pour la troisième place) oui/non.
        /// </summary>
        [SqlMapping("is_winner"), Description("Vainqueur")]
        is_winner,
        /// <summary>
        /// Nombre de points ATP récoltés.
        /// </summary>
        [SqlMapping("points"), Description("Points")]
        points,
        /// <summary>
        /// Nombre de matchs gagnés.
        /// </summary>
        [SqlMapping("count_match_win"), Description("Victoires")]
        match_win,
        /// <summary>
        /// Nombre de matchs perdus.
        /// </summary>
        [SqlMapping("count_match_lost"), Description("Défaites")]
        match_lost,
        /// <summary>
        /// Nombre de sets gagnants.
        /// </summary>
        [SqlMapping("count_set_win"), Description("Sets g.")]
        set_win,
        /// <summary>
        /// Nombre de sets perdants.
        /// </summary>
        [SqlMapping("count_set_lost"), Description("Sets p.")]
        set_lost,
        /// <summary>
        /// Nombre de jeux gagnants.
        /// </summary>
        [SqlMapping("count_game_win"), Description("Jeux g.")]
        game_win,
        /// <summary>
        /// Nombre de jeux perdants.
        /// </summary>
        [SqlMapping("count_game_lost"), Description("Jeux p.")]
        game_lost,
        /// <summary>
        /// Nombre de tie-breaks gagnés.
        /// </summary>
        [SqlMapping("count_tb_win"), Description("Tie-break g.")]
        tb_win,
        /// <summary>
        /// Nomrbe de tie-breaks perdus.
        /// </summary>
        [SqlMapping("count_tb_lost"), Description("Tie-break p.")]
        tb_lost,
        /// <summary>
        /// Nombre d'aces.
        /// </summary>
        [SqlMapping("count_ace"), Description("Aces")]
        ace,
        /// <summary>
        /// Nombre de doubles-fautes.
        /// </summary>
        [SqlMapping("count_df"), Description("Doubles-fautes")]
        d_f,
        /// <summary>
        /// Nombre de points gagnés sur le service.
        /// </summary>
        [SqlMapping("count_svpt"), Description("Pts au service")]
        sv_pt,
        /// <summary>
        /// Nombre de premières balles dans le court.
        /// </summary>
        [SqlMapping("count_1stIn"), Description("1ère balle in")]
        first_in,
        /// <summary>
        /// Nombre de premières balles gagnantes.
        /// </summary>
        [SqlMapping("count_1stWon"), Description("1ère balle g.")]
        first_won,
        /// <summary>
        /// Nombre de secondes balles gagnantes.
        /// </summary>
        [SqlMapping("count_2ndWon"), Description("2e balle g.")]
        second_won,
        /// <summary>
        /// Nombre de jeux de service joués.
        /// </summary>
        [SqlMapping("count_SvGms"), Description("Nb jeu service")]
        sv_gms,
        /// <summary>
        /// Nombre de balles de break sauvées.
        /// </summary>
        [SqlMapping("count_bpSaved"), Description("B. break ok")]
        bp_saved,
        /// <summary>
        /// Nombre de balles de break concédées.
        /// </summary>
        [SqlMapping("count_bpFaced"), Description("Nb b. break")]
        bp_faced
    }
}
