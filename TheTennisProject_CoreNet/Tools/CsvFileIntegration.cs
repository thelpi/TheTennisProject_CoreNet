using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Classe de gestion d'un fichier CSV annuel à intégrer dans la base de données.
    /// </summary>
    /// <remarks>
    /// Source des fichiers CSV : https://github.com\/JeffSackmann\/tennis_atp.
    /// Suivre la procédure grossièrement décrite dans <c>{root}/ExternalResources/procedure_manuelle_tournois.txt</c> pour commencer.
    /// </remarks>
    public class CsvFileIntegration
    {
        // liste des matchs
        private List<Dictionary<string, string>> _matchs;
        // année des matchs
        private int _year;
        // chemin d'accès vers le fichier source
        private string _sourceFile;
        // indique que, pour l'année en cours, le codage des tournois est différent
        private bool _yearHasOwnTournamentCodes;

        #region Gestion statique du singleton

        // instance singleton.
        private static CsvFileIntegration _default = null;

        /// <summary>
        /// Accesseur sur l'instance singleton. Ne l'instancie pas si la méthode <see cref="InitializeDefault(string, int, bool)"/> n'a jamais été appelée.
        /// </summary>
        public static CsvFileIntegration Default
        {
            get
            {
                return _default;
            }
        }

        /// <summary>
        /// Accesseur sur l'instance singleton.
        /// </summary>
        /// <param name="sourceFile">Chemin d'accès vers le fichier CSV source.</param>
        /// <param name="year">Année des données du fichier.</param>
        /// <param name="yearHasOwnTournamentCodes">Indique que, pour l'année en cours, le codage des tournois est différent.</param>
        public static CsvFileIntegration InitializeDefault(string sourceFile, int year, bool yearHasOwnTournamentCodes)
        {
            if (_default == null)
                _default = new CsvFileIntegration(sourceFile, year, yearHasOwnTournamentCodes);
            return _default;
        }

        #endregion

        /// <summary>
        /// Constructeur privé
        /// </summary>
        /// <param name="sourceFile">Chemin d'accès vers le fichier CSV source.</param>
        /// <param name="year">Année des données du fichier.</param>
        /// <param name="yearHasOwnTournamentCodes">Indique que, pour l'année en cours, le codage des tournois est différent.</param>
        private CsvFileIntegration(string sourceFile, int year, bool yearHasOwnTournamentCodes)
        {
            _year = year;
            _sourceFile = sourceFile;
            _yearHasOwnTournamentCodes = yearHasOwnTournamentCodes;
            _matchs = new List<Dictionary<string, string>>();

            // TODO : traiter le cas où le fichier est invalide
            List<string> rawRows = new List<string>();
            using (StreamReader reader = new StreamReader(sourceFile))
            {
                string rawContent = reader.ReadToEnd();
                rawRows.AddRange(rawContent.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
            }

            rawRows.RemoveAll(item => string.IsNullOrWhiteSpace(item));

            List<List<string>> dataRows = rawRows.Select(item => item.Split(',').ToList()).ToList();
            dataRows.RemoveAll(item => item == null || item.Count == 0 || item.Where(subItem => !string.IsNullOrWhiteSpace(subItem)).ToList().Count == 0);
            
            for (int i = 1; i < dataRows.Count; i++)
            {
                Dictionary<string, string> kvRow = new Dictionary<string, string>();
                for (int j = 0; j < dataRows[0].Count; j++)
                {
                    kvRow.Add(dataRows[0][j].Trim(), dataRows[i][j].Trim());
                }
                _matchs.Add(kvRow);
            }
        }

        /// <summary>
        /// Crée les éditions des tournois disputés dans une année.
        /// L'intégration manuelle des nouveaux tournois (ou ceux mis à jour) doit être réalisée au préalable.
        /// Les données relatives à la coupe Davis sont ignorées.
        /// </summary>
        public void IntegrateEditionOfTournaments()
        {
            string insertEditionQuery = SqlTools.BuildInsertQuery("editions", new Dictionary<string, string>()
            {
                { "tournament_ID", "@id" },
                { "year", "@year" },
                { "draw_size", "@drawsize" },
                { "date_begin", "@bdate" },
                { "date_end", "@edate" },
                { "surface_ID", "@surface" },
                { "slot_order", "@slot" },
                { "is_indoor", "@indoor" },
                { "level_ID", "@level" },
                { "substitute_ID", "@substitute" },
                { "name", "@name" },
                { "city", "@city" }
            });

            List<string> uniqueTournamentList = new List<string>();
            foreach (Dictionary<string, string> match in _matchs)
            {
                if (!uniqueTournamentList.Contains(match["tourney_id"]))
                {
                    uniqueTournamentList.Add(match["tourney_id"]);

                    string baseCode = match["tourney_id"].Substring(5);

                    using (DataTableReader reader = SqlTools.ExecuteReader("select * from tournaments where original_code in (@code2, @code1)",
                        new SqlParam("@code1", DbType.String, baseCode), new SqlParam("@code2", DbType.String, GetGenericTournamentCode(baseCode))))
                    {
                        if (reader.Read())
                        {
                            DateTime dateBegin = Tools.FormatCsvDateTime(match["tourney_date"]);
                            // Pas le vrai type SQL, mais san importance
                            int drawSize = Convert.ToInt32(match["draw_size"]);

                            // TODO : système de préparation de la requête SQL
                            SqlTools.ExecuteNonQuery(insertEditionQuery,
                                new SqlParam("@id", DbType.UInt32, reader.GetUint64("ID")),
                                new SqlParam("@year", DbType.UInt32, _year),
                                new SqlParam("@drawsize", DbType.UInt16, drawSize),
                                new SqlParam("@bdate", DbType.DateTime, dateBegin.ToString("yyyy-MM-dd")),
                                new SqlParam("@edate", DbType.DateTime, ComputeEditionEndDate(dateBegin, drawSize).ToString("yyyy-MM-dd")),
                                new SqlParam("@surface", DbType.Byte, reader.GetByte("surface_ID")),
                                new SqlParam("@slot", DbType.Byte, reader.GetByteNull("slot_order")),
                                new SqlParam("@indoor", DbType.Boolean, reader.GetByte("is_indoor") == 1),
                                new SqlParam("@level", DbType.Byte, reader.GetByte("level_ID")),
                                new SqlParam("@substitute", DbType.UInt32, reader.GetUint64Null("substitute_ID")),
                                new SqlParam("@name", DbType.String, reader.GetString("name")),
                                new SqlParam("@city", DbType.String, reader.GetString("city")));
                        }
                        else
                        {
                            Tools.WriteLog(string.Format("Le tournoi {0} a été ignoré. C'est une erreur s'il ne s'agit pas d'un match de coupe Davis.", match["tourney_id"]));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Crée les nouveaux joueurs issus du fichier fourni.
        /// Ne met pas à jour les informations sur les joueurs existants.
        /// Ne met pas à jour les dates de début et fin d'activité.
        /// </summary>
        public void IntegrateNewPlayers()
        {
            string insertPlayerQuery = SqlTools.BuildInsertQuery("players", new Dictionary<string, string>
            {
                { "ID", "@id" },
                { "name", "@name" },
                { "nationality", "@nat" },
                { "hand", "@hand" },
                { "height", "@height" },
                { "date_of_birth", "@dob" }
            });

            Dictionary<int, KeyValuePair<string, IEnumerable<SqlParam>>> queryByPlayer = new Dictionary<int, KeyValuePair<string, IEnumerable<SqlParam>>>();
            foreach (Dictionary<string, string> match in _matchs)
            {
                for (int i = 1; i <= 2; i++)
                {
                    string wOl = i == 1 ? "winner" : "loser";
                    if (SqlTools.ExecuteScalar("select count(*) from players where ID = @id and lower(replace(name, ' ', '')) = lower(replace(@name, ' ', ''))",
                        0, new SqlParam("@id", DbType.UInt64, match[wOl + "_id"]), new SqlParam("@name", DbType.String, match[wOl + "_name"])) <= 0)
                    {
                        string sqlName = SqlTools.ExecuteScalar<string>("select name from players where ID = @id",
                            null, new SqlParam("@id", DbType.UInt64, match[wOl + "_id"]));
                        if (string.IsNullOrWhiteSpace(sqlName))
                        {
                            DateTime dob = new DateTime(Tools.DEFAULT_YEAR, 1, 1);
                            if (!string.IsNullOrWhiteSpace(match[wOl + "_age"]))
                            {
                                // TODO : retravailler cette conversion
                                dob = Tools.ComputeDateOfBirth(Convert.ToDouble(match[wOl + "_age"].Replace('.', ',')),
                                    Tools.FormatCsvDateTime(match["tourney_date"]));
                            }
                            if (!queryByPlayer.ContainsKey(Convert.ToInt32(match[wOl + "_id"])))
                            {
                                queryByPlayer.Add(Convert.ToInt32(match[wOl + "_id"]),
                                    new KeyValuePair<string, IEnumerable<SqlParam>>(insertPlayerQuery,
                                    new List<SqlParam>
                                    {
                                        new SqlParam("id", DbType.UInt64, match[wOl + "_id"]),
                                        new SqlParam("name", DbType.String, match[wOl + "_name"]),
                                        new SqlParam("nat", DbType.String, string.IsNullOrWhiteSpace(match[wOl + "_ioc"]) ? DBNull.Value : (object)match[wOl + "_ioc"]),
                                        new SqlParam("hand", DbType.String, !new[] { "L", "R" }.Contains(match[wOl + "_hand"].ToUpper()) ? DBNull.Value : (object)match[wOl + "_hand"].ToUpper()),
                                        new SqlParam("height", DbType.UInt32, string.IsNullOrWhiteSpace(match[wOl + "_ht"]) ? DBNull.Value : (object)match[wOl + "_ht"]),
                                        new SqlParam("dob", DbType.DateTime, dob.Year == Tools.DEFAULT_YEAR ? DBNull.Value : (object)dob)
                                    }));
                            }
                        }
                        else
                        {
                            Tools.WriteLog(string.Format("L'identifiant {0} existe mais les noms '{1}' / '{2}' ne correspondent pas.", match[wOl + "_id"], match[wOl + "_name"], sqlName));
                        }
                    }
                }
            }

            foreach (int playerId in queryByPlayer.Keys)
            {
                try
                {
                    SqlTools.ExecuteNonQuery(queryByPlayer[playerId].Key, queryByPlayer[playerId].Value.ToArray());
                }
                catch (Exception ex)
                {
                    Tools.WriteLog(string.Format("Echec de l'insertion du joueur {0}, avec le message : {1}.", playerId, ex.Message));
                }
            }
        }

        /// <summary>
        /// Crée les matchs de l'année du fichier.
        /// Les valeurs des colonnes 'winner_entry' et 'loser_entry' doivent être surveillées à posteriori (voir <see cref="Models.Entry"/> pour les valeurs autorisées).
        /// </summary>
        public void IntegrateMatchs()
        {
            Dictionary<string, byte> rounds = new Dictionary<string, byte>();
            using (DataTableReader reader = SqlTools.ExecuteReader("select * from rounds"))
            {
                while (reader.Read())
                {
                    rounds.Add(SqlTools.GetString(reader, "original_code"), SqlTools.GetByte(reader, "ID"));
                }
            }

            #region Préparation de la requête (très longue liste)

            string insertMatchQuery = SqlTools.BuildInsertQuery("matches", new Dictionary<string, string>
            {
                { "original_key", "@original_key" },
                { "edition_ID", "@edition_ID" },
                { "match_num", "@match_num" },
                { "round_ID", "@round_ID" },
                { "best_of", "@best_of" },
                { "winner_ID", "@winner_ID" },
                { "winner_seed", "@winner_seed" },
                { "winner_entry", "@winner_entry" },
                { "winner_rank", "@winner_rank" },
                { "winner_rank_points", "@winner_rank_points" },
                { "loser_ID", "@loser_ID" },
                { "loser_seed", "@loser_seed" },
                { "loser_entry", "@loser_entry" },
                { "loser_rank", "@loser_rank" },
                { "loser_rank_points", "@loser_rank_points" },
                { "minutes", "@minutes" },
                { "unfinished", "@unfinished" },
                { "retirement", "@retirement" },
                { "walkover", "@walkover" },
                { "w_ace", "@w_ace" },
                { "w_df", "@w_df" },
                { "w_svpt", "@w_svpt" },
                { "w_1stIn", "@w_1stIn" },
                { "w_1stWon", "@w_1stWon" },
                { "w_2ndWon", "@w_2ndWon" },
                { "w_SvGms", "@w_SvGms" },
                { "w_bpSaved", "@w_bpSaved" },
                { "w_bpFaced", "@w_bpFaced" },
                { "l_ace", "@l_ace" },
                { "l_df", "@l_df" },
                { "l_svpt", "@l_svpt" },
                { "l_1stIn", "@l_1stIn" },
                { "l_1stWon", "@l_1stWon" },
                { "l_2ndWon", "@l_2ndWon" },
                { "l_SvGms", "@l_SvGms" },
                { "l_bpSaved", "@l_bpSaved" },
                { "l_bpFaced", "@l_bpFaced" },
                { "w_set_1", "@w_set_1" },
                { "w_set_2", "@w_set_2" },
                { "w_set_3", "@w_set_3" },
                { "w_set_4", "@w_set_4" },
                { "w_set_5", "@w_set_5" },
                { "l_set_1", "@l_set_1" },
                { "l_set_2", "@l_set_2" },
                { "l_set_3", "@l_set_3" },
                { "l_set_4", "@l_set_4" },
                { "l_set_5", "@l_set_5" },
                { "tb_set_1", "@tb_set_1" },
                { "tb_set_2", "@tb_set_2" },
                { "tb_set_3", "@tb_set_3" },
                { "tb_set_4", "@tb_set_4" },
                { "tb_set_5", "@tb_set_5" }
            });

            #endregion

            Dictionary<string, uint> editionsList = new Dictionary<string, uint>();
            foreach (Dictionary<string, string> match in _matchs)
            {
                string baseCode = match["tourney_id"].Substring(5);

                uint editionId = 0;
                if (!editionsList.ContainsKey(baseCode))
                {
                    string genericTournamentCode = GetGenericTournamentCode(baseCode);
                    editionId = SqlTools.ExecuteScalar<uint>("select e.ID from editions as e " +
                        "join tournaments as t on e.tournament_ID = t.ID " +
                        "where t.original_code in (@code2, @code1) and e.year = @year",
                        0,
                        new SqlParam("@code1", DbType.String, baseCode),
                        new SqlParam("@code2", DbType.String, genericTournamentCode),
                        new SqlParam("@year", DbType.UInt32, _year));
                    if (editionId > 0)
                    {
                        editionsList.Add(baseCode, editionId);
                    }
                    else
                    {
                        Tools.WriteLog(string.Format("Impossible de récupérer l'édition du tournoi {0}.", baseCode));
                        continue;
                    }
                }
                else
                {
                    editionId = editionsList[baseCode];
                }

                try
                {
                    SqlTools.ExecuteNonQuery(insertMatchQuery,
                        new SqlParam("@original_key", DbType.String, string.Concat(match["tourney_id"], "-", match["match_num"])),
                        new SqlParam("@edition_ID", DbType.UInt32, editionId),
                        new SqlParam("@match_num", DbType.UInt16, match["match_num"]),
                        new SqlParam("@round_ID", DbType.Byte, rounds[match["round"]]),
                        new SqlParam("@best_of", DbType.Byte, match["best_of"]),
                        new SqlParam("@winner_ID", DbType.UInt64, match["winner_id"]),
                        new SqlParam("@winner_seed", DbType.UInt32, string.IsNullOrWhiteSpace(match["winner_seed"]) ? null : match["winner_seed"]),
                        new SqlParam("@winner_entry", DbType.String, string.IsNullOrWhiteSpace(match["winner_entry"]) ? null : match["winner_entry"]),
                        new SqlParam("@winner_rank", DbType.UInt32, string.IsNullOrWhiteSpace(match["winner_rank"]) ? null : match["winner_rank"]),
                        new SqlParam("@winner_rank_points", DbType.UInt32, string.IsNullOrWhiteSpace(match["winner_rank_points"]) ? null : match["winner_rank_points"]),
                        new SqlParam("@loser_ID", DbType.UInt64, match["loser_id"]),
                        new SqlParam("@loser_seed", DbType.UInt32, string.IsNullOrWhiteSpace(match["loser_seed"]) ? null : match["loser_seed"]),
                        new SqlParam("@loser_entry", DbType.String, string.IsNullOrWhiteSpace(match["loser_entry"]) ? null : match["loser_entry"]),
                        new SqlParam("@loser_rank", DbType.UInt32, string.IsNullOrWhiteSpace(match["loser_rank"]) ? null : match["loser_rank"]),
                        new SqlParam("@loser_rank_points", DbType.UInt32, string.IsNullOrWhiteSpace(match["loser_rank_points"]) ? null : match["loser_rank_points"]),
                        new SqlParam("@minutes", DbType.UInt32, string.IsNullOrWhiteSpace(match["minutes"]) ? null : match["minutes"]),
                        new SqlParam("@unfinished", DbType.Boolean, false),
                        new SqlParam("@retirement", DbType.Boolean, false),
                        new SqlParam("@walkover", DbType.Boolean, false),
                        new SqlParam("@w_ace", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_ace"]) ? null : match["w_ace"]),
                        new SqlParam("@w_df", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_df"]) ? null : match["w_df"]),
                        new SqlParam("@w_svpt", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_svpt"]) ? null : match["w_svpt"]),
                        new SqlParam("@w_1stIn", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_1stIn"]) ? null : match["w_1stIn"]),
                        new SqlParam("@w_1stWon", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_1stWon"]) ? null : match["w_1stWon"]),
                        new SqlParam("@w_2ndWon", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_2ndWon"]) ? null : match["w_2ndWon"]),
                        new SqlParam("@w_SvGms", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_SvGms"]) ? null : match["w_SvGms"]),
                        new SqlParam("@w_bpSaved", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_bpSaved"]) ? null : match["w_bpSaved"]),
                        new SqlParam("@w_bpFaced", DbType.UInt32, string.IsNullOrWhiteSpace(match["w_bpFaced"]) ? null : match["w_bpFaced"]),
                        new SqlParam("@l_ace", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_ace"]) ? null : match["l_ace"]),
                        new SqlParam("@l_df", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_df"]) ? null : match["l_df"]),
                        new SqlParam("@l_svpt", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_svpt"]) ? null : match["l_svpt"]),
                        new SqlParam("@l_1stIn", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_1stIn"]) ? null : match["l_1stIn"]),
                        new SqlParam("@l_1stWon", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_1stWon"]) ? null : match["l_1stWon"]),
                        new SqlParam("@l_2ndWon", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_2ndWon"]) ? null : match["l_2ndWon"]),
                        new SqlParam("@l_SvGms", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_SvGms"]) ? null : match["l_SvGms"]),
                        new SqlParam("@l_bpSaved", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_bpSaved"]) ? null : match["l_bpSaved"]),
                        new SqlParam("@l_bpFaced", DbType.UInt32, string.IsNullOrWhiteSpace(match["l_bpFaced"]) ? null : match["l_bpFaced"]),
                        new SqlParam("@w_set_1", DbType.Byte, ExtractScore(match["score"], 1, true)),
                        new SqlParam("@w_set_2", DbType.Byte, ExtractScore(match["score"], 2, true)),
                        new SqlParam("@w_set_3", DbType.Byte, ExtractScore(match["score"], 3, true)),
                        new SqlParam("@w_set_4", DbType.Byte, ExtractScore(match["score"], 4, true)),
                        new SqlParam("@w_set_5", DbType.Byte, ExtractScore(match["score"], 5, true)),
                        new SqlParam("@l_set_1", DbType.Byte, ExtractScore(match["score"], 1, false)),
                        new SqlParam("@l_set_2", DbType.Byte, ExtractScore(match["score"], 2, false)),
                        new SqlParam("@l_set_3", DbType.Byte, ExtractScore(match["score"], 3, false)),
                        new SqlParam("@l_set_4", DbType.Byte, ExtractScore(match["score"], 4, false)),
                        new SqlParam("@l_set_5", DbType.Byte, ExtractScore(match["score"], 5, false)),
                        new SqlParam("@tb_set_1", DbType.UInt16, ExtractScore(match["score"], 1, null)),
                        new SqlParam("@tb_set_2", DbType.UInt16, ExtractScore(match["score"], 2, null)),
                        new SqlParam("@tb_set_3", DbType.UInt16, ExtractScore(match["score"], 3, null)),
                        new SqlParam("@tb_set_4", DbType.UInt16, ExtractScore(match["score"], 4, null)),
                        new SqlParam("@tb_set_5", DbType.UInt16, ExtractScore(match["score"], 5, null))
                    );
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Echec de l'insertion du match {0}, l'erreur suivante est survenue : {1}", string.Concat(match["tourney_id"], "-", match["match_num"]), ex.Message);
                    Tools.WriteLog(errorMessage);
                    throw new Exception("IntegrateMatchs failure."); // TODO
                }
            }
        }

        /// <summary>
        /// Après intégration des matches (voir <see cref="IntegrateMatchs"/>), procède à la mise à jour des informations suivantes :
        /// <list type="bullet">
        /// <item>unfinished (non terminé).</item>
        /// <item>retirement (abandon).</item>
        /// <item>walkover (forfait).</item>
        /// </list>
        /// </summary>
        public void SetUnfinishedMatchsDatas()
        {
            SqlParam sqlParam = new SqlParam("@yearlike", DbType.String, string.Concat(_year, "%"));

            System.Text.StringBuilder queryBuilder = new System.Text.StringBuilder();
            queryBuilder.AppendLine("update matches ");
            queryBuilder.AppendLine("set walkover = 1 ");
            queryBuilder.AppendLine("where original_key like @yearlike ");
            queryBuilder.AppendLine("and ifnull(w_set_1, 0) = 0 ");
            queryBuilder.AppendLine("and ifnull(l_set_1, 0) = 0 ");

            SqlTools.ExecuteNonQuery(queryBuilder.ToString(), sqlParam);

            // TODO : les matchs en 4 sets du Master Next-Gen ne sont pas traités correctements
            // TODO : doute sur la fiabilité de cette requête pour les matchs où l'abandon a lieu entre deux sets
            for (int i = 1; i <= 5; i++)
            {
                queryBuilder = new System.Text.StringBuilder();
                queryBuilder.AppendLine("update matches ");
                queryBuilder.AppendLine("set retirement = 1, unfinished = 1 ");
                queryBuilder.AppendLine("WHERE original_key like @yearlike ");
                queryBuilder.AppendLine("and walkover = 0 ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '76' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '67' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '75' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '57' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '64' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '46' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '63' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '36' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '62' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '26' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '61' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '16' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '60' ");
                queryBuilder.AppendLine("and concat(w_set_X, l_set_X) != '06' ");
                queryBuilder.AppendLine("and not (");
                queryBuilder.AppendLine("   w_set_X >= 6 and l_set_X >= 6 ");
                queryBuilder.AppendLine("   and if (w_set_X > l_set_X, w_set_X, l_set_X) - if (w_set_X > l_set_X, l_set_X, w_set_X) = 2 ");
                queryBuilder.AppendLine(")");

                SqlTools.ExecuteNonQuery(queryBuilder.ToString().Replace("_X", string.Concat("_", i)), sqlParam);
            }
        }

        /// <summary>
        /// Met à jour les dates d'activité des joueurs.
        /// A appeler après <see cref="IntegrateMatchs"/>.
        /// </summary>
        public void SetPlayersActivityPeriod()
        {
            System.Text.StringBuilder sbQuery = new System.Text.StringBuilder();
            sbQuery.AppendLine("UPDATE players");
            sbQuery.AppendLine("SET date_begin = (");
            sbQuery.AppendLine("	SELECT subq.dat");
            sbQuery.AppendLine("	FROM (");
            sbQuery.AppendLine("		SELECT e.date_begin AS dat, m.loser_id AS p_id");
            sbQuery.AppendLine("		FROM matches AS m JOIN editions AS e ON m.edition_id = e.id");
            sbQuery.AppendLine("		UNION ALL");
            sbQuery.AppendLine("		SELECT e.date_begin AS dat, m.winner_id AS p_id");
            sbQuery.AppendLine("		FROM matches AS m JOIN editions AS e ON m.edition_id = e.id");
            sbQuery.AppendLine("	) AS subq");
            sbQuery.AppendLine("	WHERE subq.p_id = players.id");
            sbQuery.AppendLine("	ORDER BY subq.dat ASC");
            sbQuery.AppendLine("	LIMIT 0, 1");
            sbQuery.AppendLine("), date_end = (");
            sbQuery.AppendLine("	SELECT subq.dat");
            sbQuery.AppendLine("	FROM (");
            sbQuery.AppendLine("		SELECT e.date_end AS dat, m.loser_id AS p_id");
            sbQuery.AppendLine("		FROM matches AS m JOIN editions AS e ON m.edition_id = e.id");
            sbQuery.AppendLine("		UNION ALL");
            sbQuery.AppendLine("		SELECT e.date_end AS dat, m.winner_id AS p_id");
            sbQuery.AppendLine("		FROM matches AS m JOIN editions AS e ON m.edition_id = e.id");
            sbQuery.AppendLine("	) AS subq");
            sbQuery.AppendLine("	WHERE subq.p_id = players.id");
            sbQuery.AppendLine("	ORDER BY subq.dat DESC");
            sbQuery.AppendLine("	LIMIT 0, 1");
            sbQuery.AppendLine(") WHERE id IN (");
            sbQuery.AppendLine("	SELECT m2.winner_id");
            sbQuery.AppendLine("	FROM matches AS m2 JOIN editions as e2 ON m2.edition_id = e2.id");
            sbQuery.AppendLine("	WHERE e2.year = @year");
            sbQuery.AppendLine("	UNION ALL");
            sbQuery.AppendLine("	SELECT m2.loser_id");
            sbQuery.AppendLine("	FROM matches AS m2 JOIN editions as e2 ON m2.edition_id = e2.id");
            sbQuery.AppendLine("	WHERE e2.year = @year");
            sbQuery.AppendLine(")");

            SqlTools.ExecuteNonQuery(sbQuery.ToString(), new SqlParam("@year", DbType.UInt32, _year));
        }

        /// <summary>
        /// Quand <see cref="_yearHasOwnTournamentCodes"/> est vrai, permet de récupérer le code générique du tournoi à partir de celui de l'année en cours.
        /// Sinon, le code est retourné à l'identique.
        /// </summary>
        /// <param name="baseCode">Le code du tournoi pour l'année courante.</param>
        /// <returns>Le code générique du tournoi.</returns>
        private string GetGenericTournamentCode(string baseCode)
        {
            string code = baseCode;
            if (_yearHasOwnTournamentCodes)
            {
                code = SqlTools.ExecuteScalar(string.Format("select code_original from tournaments_code_{0} where code_new = @code", _year),
                    baseCode, new SqlParam("@code", DbType.String, baseCode));
            }
            return code;
        }

        /// <summary>
        /// Tente d'extraire une information précise depuis un résultat de match.
        /// Exemple de format d'entrée : "7-6(6) 4-6 7-6(4)".
        /// </summary>
        /// <param name="rawScore">Le score au format brut.</param>
        /// <param name="set">Le numéro de set.</param>
        /// <param name="winner">Information relative au vainqueur (<c>True</c>) ou au perdant (<c>False</c>).
        /// <c>Null</c> permet de récupérer la valeur du tie-break.</param>
        /// <returns>La valeur associée, <c>Null</c> si impossible à extraire.</returns>
        private string ExtractScore(string rawScore, int set, bool? winner)
        {
            string[] sets = rawScore.Split(' ');

            if (sets.Length < set)
            {
                return null;
            }

            string setInfo = sets[set - 1];
            string regularSetInfo = setInfo.Contains("(") ? setInfo.Substring(0, setInfo.IndexOf("(")) : setInfo;
            string wSetInfo = regularSetInfo.Contains("-") ? regularSetInfo.Split('-')[0] : null;
            string lSetInfo = regularSetInfo.Contains("-") ? regularSetInfo.Split('-')[1] : null;

            if (winner.HasValue)
            {
                return winner.Value ? wSetInfo : lSetInfo;
            }

            string tbInfo = setInfo.Replace(regularSetInfo, string.Empty);
            int tbInfoInt = -1;
            if (tbInfo.Length > 2)
            {
                tbInfo = tbInfo.Substring(1);
                tbInfo = tbInfo.Substring(0, tbInfo.Length - 1);
                if (!int.TryParse(tbInfo, out tbInfoInt))
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return tbInfoInt.ToString();
        }

        /// <summary>
        /// Calcule la date de fin d'une édition de tournoi à partir de sa date de début et du nombre de matchs à jouer.
        /// </summary>
        /// <param name="dateBegin">Date de début.</param>
        /// <param name="drawSize">Nombre de joueurs.</param>
        /// <returns>Date de fin du tournoi.</returns>
        private DateTime ComputeEditionEndDate(DateTime dateBegin, int drawSize)
        {
            int daysToAdd = 0;
            switch (dateBegin.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    daysToAdd = drawSize > 64 ? 13 : 6;
                    break;
                case DayOfWeek.Tuesday:
                    daysToAdd = drawSize > 64 ? 12 : 5;
                    break;
                case DayOfWeek.Wednesday:
                    daysToAdd = drawSize > 32 ? 11 : 4;
                    break;
                case DayOfWeek.Thursday:
                    daysToAdd = drawSize > 16 ? 10 : 3;
                    break;
                case DayOfWeek.Friday:
                    daysToAdd = drawSize > 8 ? 9 : 2;
                    break;
                case DayOfWeek.Saturday:
                    daysToAdd = drawSize > 4 ? 8 : 1;
                    break;
                case DayOfWeek.Sunday:
                    daysToAdd = drawSize > 2 ? (drawSize > 64 ? 14 : 7) : 0;
                    break;
            }
            return dateBegin.AddDays(daysToAdd);
        }
    }
}
