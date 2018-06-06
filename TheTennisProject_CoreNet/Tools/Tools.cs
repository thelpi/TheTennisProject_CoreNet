using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using TheTennisProject_CoreNet.Models.Internals;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Méthodes et constantes statiques.
    /// </summary>
    public static class Tools
    {
        // Le nombre exact de jours dans une année en moyenne
        private const double DAYS_IN_A_YEAR = 365.256363;
        /// <summary>
        /// Année par défaut (date vide).
        /// </summary>
        public const int DEFAULT_YEAR = 1900;
        /// <summary>
        /// Points ELO par défaut.
        /// </summary>
        public const ushort DEFAULT_ELO = 2500;
        /// <summary>
        /// Year of begining of the Open era.
        /// </summary>
        public const int OPEN_ERA_YEAR = 1968;
        /// <summary>
        /// Date du premier classement ATP possible.
        /// </summary>
        public static readonly DateTime ATP_RANKING_DEBUT = new DateTime(OPEN_ERA_YEAR, 1, 1);

        /// <summary>
        /// Calcule l'âge d'un évènement.
        /// </summary>
        /// <param name="dateOfEvent">Date de l'évènement.</param>
        /// <param name="dateComparer">Point de comparaison. Null, valeur par défaut, permet de spécifier <see cref="DateTime.Now"/>.</param>
        /// <returns>L'âge en année de l'évènement spécifié.</returns>
        public static int GetEventAge(DateTime dateOfEvent, DateTime? dateComparer = null)
        {
            return (new DateTime(1, 1, 1) + ((!dateComparer.HasValue ? DateTime.Now : dateComparer.Value) - dateOfEvent)).Year - 1;
        }

        /// <summary>
        /// Calcule l'ordre de tri d'un tour de compétition (pour pallier à la mauvaise position naturelle du match de médaille de bronze olympique).
        /// </summary>
        /// <remarks>Le tri va du plus important (la finale) au moins important (le 1er tour).</remarks>
        /// <param name="round">Le tour à trier.</param>
        /// <returns>Une valeur décimale indiquant la position de tri du tour.</returns>
        public static decimal GetSortOrder(this Round round)
        {
            return round == Round.BR ? 1.5M : (decimal)round;
        }

        /// <summary>
        /// Détermine si le tour de compétition courant est antérieur au tour fourni en argument.
        /// </summary>
        /// <param name="round">Le tour de compétition courant.</param>
        /// <param name="comparer">Le tour de compétition à comparer.</param>
        /// <returns>True si <paramref name="round"/> est antérieur à <paramref name="comparer"/>, False sinon.</returns>
        public static bool RoundIsBefore(this Round round, Round comparer)
        {
            return comparer.GetSortOrder() < round.GetSortOrder();
        }

        /// <summary>
        /// Calcule une approximation de la date de naissance d'un joueur à partir de son âge décimal à une date précise.
        /// </summary>
        /// <param name="age">Age du joueur, incluant une partie décimale aussi précise que possible.</param>
        /// <param name="date">La date à laquelle le joueur à l'âge mentionné.</param>
        /// <returns>Approximation de la date de naissance du joueur.</returns>
        public static DateTime ComputeDateOfBirth(double age, DateTime date)
        {
            return date
                    .AddDays((1 - (age - Math.Floor(age))) * DAYS_IN_A_YEAR)
                    .AddDays(-(DAYS_IN_A_YEAR * (Math.Floor(age) + 1)));
        }

        /// <summary>
        /// Convertit un date au format CSV ("yyyymmdd") en une structure <see cref="DateTime"/>.
        /// </summary>
        /// <param name="date">la chaîen de caractères représentant la date.</param>
        /// <returns>La structure <see cref="DateTime"/>.</returns>
        public static DateTime FormatCsvDateTime(string date)
        {
            return Convert.ToDateTime(string.Format("{0}-{1}-{2} 00:00:00", date.Substring(0, 4), date.Substring(4, 2), date.Substring(6, 2)));
        }

        /// <summary>
        /// Procède à l'écriture d'un log dans le fichier journal de l'application.
        /// </summary>
        /// <param name="line">Log à écrire.</param>
        public static void WriteLog(string line)
        {
            // TODO : vérifier la validité du chemin d'accès au fichier
            using (StreamWriter writer = new StreamWriter(Config.GetString(AppKey.ErrorLogFilePath), true))
            {
                writer.WriteLine(line);
            }
#if (DEBUG)
            // TODO : vérifier que ce système fonctionne
            System.Diagnostics.Debug.WriteLine(line);
#endif
        }

        /// <summary>
        /// Calcule, à partir d'une liste de matchs, la meilleure série de victoires ou la pire série de défaites.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Suppose que les matchs de la liste sont contigus une fois triés.</item>
        /// <item>Pas besoin de trier au préalable.</item>
        /// <item>Les forfaits d'avant-match sont exclus.</item>
        /// </list>
        /// </remarks>
        /// <param name="matchesList">La liste de matchs.</param>
        /// <param name="playerId">L'identifiant du joueur considéré.</param>
        /// <param name="getLost"><c>True</c> pour récupérer une série de défaites ; <c>False</c> pour une série de victoires.</param>
        /// <returns>Le nombre de matchs gagnés ou perdus.</returns>
        public static int GetWinLoseRun(this IEnumerable<Match> matchesList, ulong playerId, bool getLost)
        {
            DateTime beginDate;
            ushort beginNumMatch;
            return matchesList.GetWinLoseRun(playerId, getLost, out beginDate, out beginNumMatch);
        }

        /// <summary>
        /// Calcule, à partir d'une liste de matchs, la meilleure série de victoires ou la pire série de défaites.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Suppose que les matchs de la liste sont contigus une fois triés.</item>
        /// <item>Pas besoin de trier au préalable.</item>
        /// <item>Les forfaits d'avant-match sont exclus.</item>
        /// </list>
        /// </remarks>
        /// <param name="matchesList">La liste de matchs.</param>
        /// <param name="playerId">L'identifiant du joueur considéré.</param>
        /// <param name="getLost"><c>True</c> pour récupérer une série de défaites ; <c>False</c> pour une série de victoires.</param>
        /// <param name="beginDate">La date de début du tournoi lors duquel la série a commencé (et non pas la date du match, qui est inconnue).</param>
        /// <param name="beginNumMatch">Le numéro du match dans le tournoi.</param>
        /// <returns>Le nombre de matchs gagnés ou perdus.</returns>
        public static int GetWinLoseRun(this IEnumerable<Match> matchesList, ulong playerId, bool getLost, out DateTime beginDate, out ushort beginNumMatch)
        {
            if (matchesList == null)
            {
                beginDate = new DateTime();
                beginNumMatch = 0;
                return 0;
            }

            // Filtre les forfaits et trie par date croissante
            // On ne connait pas la date réelle du match, donc on trie par date de début de tournoi, puis par tour
            List<Match> filteredAndChronologicalMatchesListWithoutWalkover =
                matchesList
                    .Where(_ => !_.Walkover)
                    .OrderBy(_ => _.Edition.DateBegin)
                    .ThenByDescending(_ => _.Round.GetSortOrder())
                    .ToList();

            int maxLose = 0, maxWin = 0, currentWin = 0, currentLose = 0;
            ushort winBeginNumMatch = 0, loseBeginNumMatch = 0;
            DateTime winBeginDate = new DateTime(), loseBeginDate = new DateTime();
            foreach (Match match in filteredAndChronologicalMatchesListWithoutWalkover)
            {
                if (match.Winner.ID == playerId)
                {
                    currentWin++;
                    currentLose = 0;
                    loseBeginNumMatch = 0;
                    if (maxWin < currentWin)
                    {
                        maxWin = currentWin;
                        if (winBeginNumMatch == 0)
                        {
                            winBeginNumMatch = match.MatchNum;
                            winBeginDate = match.Edition.DateBegin;
                        }
                    }
                }
                else
                {
                    currentLose++;
                    currentWin = 0;
                    winBeginNumMatch = 0;
                    if (maxLose < currentLose)
                    {
                        maxLose = currentLose;
                        if (loseBeginNumMatch == 0)
                        {
                            loseBeginNumMatch = match.MatchNum;
                            loseBeginDate = match.Edition.DateBegin;
                        }
                    }
                }
            }

            if (getLost)
            {
                beginDate = loseBeginDate;
                beginNumMatch = loseBeginNumMatch;
                return maxLose;
            }

            beginDate = winBeginDate;
            beginNumMatch = winBeginNumMatch;
            return maxWin;
        }

        /// <summary>
        /// Calcule le numéro de semaine associée à une date donnée.
        /// </summary>
        /// <remarks>La première semaine de l'année est celle incluant le premier jeudi de l'année.</remarks>
        /// <param name="date">La date.</param>
        /// <returns>Le numéro de semaine.</returns>
        public static int GetWeekNoFromDate(DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }
            
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Détermine si une année a 53 semaines officielles au lieu de 52.
        /// </summary>
        /// <param name="year">L'année.</param>
        /// <returns>Vrai si 53 semaines ; Faux sinon.</returns>
        public static bool YearIs53Week(int year)
        {
            DayOfWeek[] endOfWeekDays = new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

            DateTime firstDayOfNextYear = new DateTime(year + 1, 1, 1);

            return endOfWeekDays.Contains(firstDayOfNextYear.DayOfWeek);
        }

        /// <summary>
        /// Calcule un nouveau classement ELO après un match.
        /// </summary>
        /// <param name="eloBeforeP1">Classement ELO précédent du premier joueur.</param>
        /// <param name="eloBeforeP2">Classement ELO précédent du second joueur.</param>
        /// <param name="winnerP1"><c>Vrai</c> si le premier joueur gagne ; <c>Faux</c> sinon</param>
        /// <param name="coeffK">Coefficient indiquant l'importance du match.</param>
        /// <returns>Un tuple de données contenant les nouveaux classement ELO des deux joueurs.</returns>
        public static Tuple<double, double> ComputeElo(double eloBeforeP1, double eloBeforeP2, bool winnerP1, double coeffK)
        {
            double d1 = eloBeforeP1 + coeffK * ((winnerP1 ? 1 : 0) - (1 / (1 + Math.Pow(10, -(eloBeforeP1 - eloBeforeP2) / 400))));
            double d2 = eloBeforeP2 + coeffK * ((winnerP1 ? 0 : 1) - (1 / (1 + Math.Pow(10, -(eloBeforeP2 - eloBeforeP1) / 400))));

            return new Tuple<double, double>(d1, d2);
        }

        /// <summary>
        /// Rtourne, pour une valeur d'énumération <see cref="Level"/>, le coefficient "K" asssocié, nécessaire au calcul du ELO.
        /// </summary>
        /// <param name="l">La valeur d'énumération.</param>
        /// <returns>Le coefficient associé.</returns>
        public static double GetLevelEloCoeffK(Level l)
        {
            switch (l)
            {
                // TODO : à corriger en même temps que le barème de la table "points"
                // ou à intégrer dans la table en question
                case Level.atp_250:
                    return 2.5;
                case Level.atp_500:
                    return 5;
                case Level.grand_slam:
                    return 20;
                case Level.masters:
                    return 15;
                case Level.masters_1000:
                    return 10;
                case Level.olympics_games:
                    return 7.5;
                default:
                    return 1;
            }
        }

        #region Traductions et attributs d'énumérations

        /// <summary>
        /// Récupère la description associée à une valeur d'énumération.
        /// </summary>
        /// <typeparam name="T">Le type de l'énumération.</typeparam>
        /// <param name="value">La valeur d'énumération.</param>
        /// <returns>La description de la valeur d'énumération.
        /// <c>Null</c> si <paramref name="value"/> n'a pas pu être convertie en une valeur de l'énumération.
        /// <paramref name="value"/> si la valeur d'énumération n'a pas de description spécifiée.</returns>
        public static string GetEnumDescription<T>(object value) where T : struct, IConvertible
        {
            Type type = typeof(T);
            string name = Enum.GetNames(type).Where(f => f.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (name == null)
            {
                return string.Empty;
            }
            object[] customAttribute = type.GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : name;
        }

        /// <summary>
        /// Récupère le nom de colonne SQL associé à une valeur d'énumération.
        /// </summary>
        /// <typeparam name="T">Le type de l'énumération.</typeparam>
        /// <param name="value">La valeur d'énumération.</param>
        /// <returns>Le nom de colonne SQL de la valeur d'énumération.
        /// <c>Null</c> si <paramref name="value"/> n'a pas pu être convertie en une valeur de l'énumération.
        /// <paramref name="value"/> si la valeur d'énumération n'a pas de spécification sur sa colonne SQL associée.</returns>
        public static string GetEnumSqlMapping<T>(object value) where T : struct, IConvertible
        {
            Type type = typeof(T);
            string name = Enum.GetNames(type).Where(f => f.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (name == null)
            {
                return string.Empty;
            }
            object[] customAttribute = type.GetField(name).GetCustomAttributes(typeof(SqlMappingAttribute), false);
            return customAttribute.Length > 0 ? ((SqlMappingAttribute)customAttribute[0]).SqlMapping : name;
        }

        /// <summary>
        /// Récupère la valeur d'une énumération à partir de sa colonne SQL associée.
        /// </summary>
        /// <typeparam name="T">Le type d'énumération.</typeparam>
        /// <param name="sqlMapping">Le nom de colonne SQL.</param>
        /// <returns>La valeur de l'énumération.</returns>
        public static T GetEnumValueFromSqlMapping<T>(string sqlMapping) where T : struct, IConvertible
        {
            Type type = typeof(T);
            string[] names = Enum.GetNames(type);

            if (names != null && sqlMapping != null)
            {
                foreach (string name in names)
                {
                    object[] customAttribute = type.GetField(name).GetCustomAttributes(typeof(SqlMappingAttribute), false);
                    if (customAttribute.Length > 0
                        && sqlMapping.Equals(((SqlMappingAttribute)customAttribute[0]).SqlMapping, StringComparison.CurrentCultureIgnoreCase))
                    {
                        T result;
                        if (Enum.TryParse(name, out result))
                        {
                            return result;
                        }
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Récupère la traduction associée à une valeur de l'énumération de surface.
        /// </summary>
        /// <param name="surface">La surface à traduire.</param>
        /// <returns>Traduction de l'intitulé de la surface.</returns>
        public static string GetTranslation(this Surface surface)
        {
            System.Reflection.MemberInfo member = surface.GetType().GetMember(surface.ToString())[0];
            return ((TranslationAttribute)Attribute.GetCustomAttribute(member, typeof(TranslationAttribute))).Translation;
        }

        /// <summary>
        /// Récupère la traduction associée à une valeur de l'énumération de niveau.
        /// </summary>
        /// <param name="level">Le niveau à traduire.</param>
        /// <returns>Traduction de l'intitulé du niveau.</returns>
        public static string GetTranslation(this Level level)
        {
            System.Reflection.MemberInfo member = level.GetType().GetMember(level.ToString())[0];
            return ((TranslationAttribute)Attribute.GetCustomAttribute(member, typeof(TranslationAttribute))).Translation;
        }

        /// <summary>
        /// Récupère la traduction associée à une valeur de l'énumération de tour.
        /// </summary>
        /// <param name="round">Le tour à traduire.</param>
        /// <returns>Traduction de l'intitulé du tour.</returns>
        public static string GetTranslation(this Round round)
        {
            System.Reflection.MemberInfo member = round.GetType().GetMember(round.ToString())[0];
            return ((TranslationAttribute)Attribute.GetCustomAttribute(member, typeof(TranslationAttribute))).Translation;
        }

        #endregion
    }
}
