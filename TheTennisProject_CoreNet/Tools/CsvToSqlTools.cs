using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheTennisProject
{
    /// <summary>
    /// Jeu de méthodes pour traiter les fichiers CSV sources. Cette classe n'est plus maintenue.
    /// </summary>
    [Obsolete("Utilisez CsvFileIntegration pour les données des nouveaux fichiers.")]
    class CsvToSqlTools
    {
        #region Constantes SQL

        /// <summary>
        /// Modèle de la chaîne de connexion SQL.
        /// </summary>
        const string SQL_CONNECTION_PATTERN = "Server={0};Database={1};Uid={2};Pwd={3};";
        /// <summary>
        /// Nom de la base de données.
        /// </summary>
        const string SQL_DB = "the_tennis_project";
        /// <summary>
        /// Nom du serveur SQL.
        /// </summary>
        const string SQL_SERVER = "localhost";
        /// <summary>
        /// Nom d'utilisateur.
        /// </summary>
        const string SQL_UID = "root";
        /// <summary>
        /// Mot de passe.
        /// </summary>
        const string SQL_PASSWD = "";
        /// <summary>
        /// Chaîne de connexion complète.
        /// </summary>
        static readonly string SQL_CONNECTION = string.Format(SQL_CONNECTION_PATTERN, SQL_SERVER, SQL_DB, SQL_UID, SQL_PASSWD);
        /// <summary>
        /// Nom de la table contenant les matchs.
        /// </summary>
        const string SQL_MATCHES_TABLE = "matches";

        #endregion

        #region Constantes CSV

        /// <summary>
        /// Source des fichiers CSV.
        /// </summary>
        const string DATA_FILE_PATH = @"D:\tennis_atp-master\atp_matches_{0}.csv";
        /// <summary>
        /// Séparateur de lignes.
        /// </summary>
        const char ROW_SEPARATOR = '\n';
        /// <summary>
        /// Séparateur de colonnes.
        /// </summary>
        const char COL_SEPARATOR = ',';

        #endregion

        /// <summary>
        /// Année des premières données.
        /// </summary>
        const int YEAR_BEGIN = 1968;
        /// <summary>
        /// Année des dernières données.
        /// </summary>
        const int YEAR_END = 2015;

        /// <summary>
        /// Création des entêtes.
        /// </summary>
        public static void BuildHeaders()
        {
            var fileTest = GetFileRows(2014, false);

            var headerColumns = fileTest[0].Split(COL_SEPARATOR);

            using (var sqlConnection = new MySqlConnection(SQL_CONNECTION))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    foreach (var headerColumn in headerColumns)
                    {
                        sqlCommand.CommandText = string.Format("alter table {0} add `{1}` varchar(255) not null", SQL_MATCHES_TABLE, headerColumn);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Import initial.
        /// </summary>
        public static void InitialImport()
        {
            using (var sqlConnection = new MySqlConnection(SQL_CONNECTION))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    for (var year = YEAR_BEGIN; year <= YEAR_END; year++)
                    {
                        var rows = GetFileRows(year, false);
                        var headersSql = string.Empty;
                        foreach (var row in rows)
                        {
                            var isFirstRow = string.IsNullOrWhiteSpace(headersSql);

                            var sqlColumns = ColumnsStringToSqlInsert(row.Split(COL_SEPARATOR), isFirstRow);

                            if (isFirstRow)
                                headersSql = sqlColumns;
                            else
                            {
                                sqlCommand.CommandText = string.Format("insert into {0} ({1}) values ({2})", SQL_MATCHES_TABLE, headersSql, sqlColumns);
                                sqlCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Correction des joueurs.
        /// </summary>
        public static void PlayersFix()
        {
            var playersDatas = new Dictionary<ulong, Dictionary<Player, int>>();

            using (var sqlConnection = new MySqlConnection(SQL_CONNECTION))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "select * from players";
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            ulong ID = sqlReader.GetUInt64(sqlReader.GetOrdinal("ID"));
                            string name = sqlReader.GetString(sqlReader.GetOrdinal("name"));
                            string nationality = sqlReader.GetString(sqlReader.GetOrdinal("nationality"));
                            string hand = sqlReader.IsDBNull(sqlReader.GetOrdinal("hand")) ? null : sqlReader.GetString(sqlReader.GetOrdinal("hand"));
                            uint? height = sqlReader.IsDBNull(sqlReader.GetOrdinal("height")) ? null : (uint?)sqlReader.GetUInt32(sqlReader.GetOrdinal("height"));
                            DateTime? dateOfBirth = sqlReader.IsDBNull(sqlReader.GetOrdinal("date_of_birth")) ? null : (DateTime?)sqlReader.GetDateTime(sqlReader.GetOrdinal("date_of_birth"));

                            if (!playersDatas.ContainsKey(ID))
                                playersDatas.Add(ID, new Dictionary<Player, int>());

                            var player = new Player(name, nationality, hand, height, dateOfBirth);

                            if (!playersDatas[ID].ContainsKey(player))
                                playersDatas[ID].Add(player, 0);

                            playersDatas[ID][player]++;
                        }
                    }

                    foreach (var playerId in playersDatas.Keys)
                    {
                        var playerDataSelected = playersDatas[playerId].OrderByDescending(item => item.Value).ToList()[0].Key;

                        sqlCommand.CommandText = string.Format("delete from players where ID = {0}", playerId);
                        sqlCommand.ExecuteNonQuery();

                        sqlCommand.CommandText = string.Format("insert into players (ID, name, nationality, hand, height, date_of_birth) values ({0}, {1}, {2}, {3}, {4}, {5})",
                            playerId,
                            "'" + playerDataSelected.Name.Replace("'", @"\'") + "'",
                            "'" + playerDataSelected.Nationality + "'",
                            !string.IsNullOrWhiteSpace(playerDataSelected.Hand) ? "'" + playerDataSelected.Hand + "'" : "NULL",
                            playerDataSelected.Height.HasValue ? playerDataSelected.Height.ToString() : "NULL",
                            playerDataSelected.DateOfBirth.HasValue ? "'" + playerDataSelected.DateOfBirth.Value.ToString("yyyy-MM-dd") + "'" : "NULL"
                        );
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Correction des scores (suite écrasement).
        /// </summary>
        public static void RebuildScores()
        {
            var allMatchesFromFiles = new Dictionary<RowMatch, KeyValuePair<string, bool>>();
            for (var year = YEAR_BEGIN; year <= YEAR_END; year++)
            {
                Console.WriteLine(string.Format("Début de traitement de l'année {0}.", year));
                var rows = GetFileRows(year, false);
                rows.RemoveAt(0);
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row))
                        continue;

                    var columns = row.Split(COL_SEPARATOR);
                    allMatchesFromFiles.Add(new RowMatch(Convert.ToUInt32(columns[6]), columns[0]), new KeyValuePair<string, bool>(columns[27], false));
                }
            }

            var noMatchList = new List<RowMatch>();
            var queryToProceed = new List<string>();
            using (var sqlConnection = new MySqlConnection(SQL_CONNECTION))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "select ID, concat(tourney_year, '-', tourney_id) as tourney, match_num from matches";
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            var id = sqlReader.GetUInt64(sqlReader.GetOrdinal("ID")).ToString();
                            var matchToCompare = new RowMatch(
                                sqlReader.GetUInt32(sqlReader.GetOrdinal("match_num")),
                                sqlReader["tourney"].ToString()
                            );

                            using (var sqlCommandExecute = sqlConnection.CreateCommand())
                            {
                                if (allMatchesFromFiles.ContainsKey(matchToCompare))
                                {
                                    queryToProceed.Add(string.Format("update matches set score = '{0}' where ID = {1}", allMatchesFromFiles[matchToCompare].Key.Replace("'", "\'"), id));
                                    // marque comme traitée
                                    allMatchesFromFiles[matchToCompare] = new KeyValuePair<string, bool>(allMatchesFromFiles[matchToCompare].Key, true);
                                }
                                else
                                {
                                    noMatchList.Add(matchToCompare);
                                }
                            }
                        }
                    }

                    foreach (var query in queryToProceed)
                    {
                        sqlCommand.CommandText = query;
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }

            if (noMatchList.Any())
            {
                using (var writerIo = new StreamWriter("D:\tennis_atp-master\nomatch1.log"))
                {
                    foreach (var rowsMatch in noMatchList)
                        writerIo.WriteLine(rowsMatch.Tourney + "|" + rowsMatch.MatchNum + "|");
                }
            }

            var noMatchList2 = allMatchesFromFiles.Where(item => !item.Value.Value).ToList();
            if (noMatchList2.Any())
            {
                using (var writerIo = new StreamWriter(@"D:\tennis_atp-master\nomatch2.log"))
                {
                    foreach (var kvp in noMatchList2)
                        writerIo.WriteLine(kvp.Key.Tourney + "|" + kvp.Key.MatchNum + "|" + kvp.Value.Key);
                }
            }
        }

        /// <summary>
        /// Intégration des tournois.
        /// </summary>
        public static void IntegrateTourneys()
        {
            var sqlQueriesAddTourney = new List<string>();
            using (var sqlConnection = new MySqlConnection(SQL_CONNECTION))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "select tourney_id, tourney_name, tourney_level_id, surface_id from matches where tourney_level_id != 2 order by tourney_id asc, tourney_year desc";
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        var currentTourneyCode = string.Empty;
                        while (sqlDataReader.Read())
                        {
                            var tourneyId = sqlDataReader.GetString(sqlDataReader.GetOrdinal("tourney_id"));
                            if (tourneyId != currentTourneyCode)
                            {
                                var surfaceIdObject = sqlDataReader["surface_id"];
                                var tourneyName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("tourney_name")).Replace("'", @"\'");

                                sqlQueriesAddTourney.Add(string.Format("insert into tourneys (code, name, name_fr, city, level_ID, surface_ID) values ('{0}', '{1}', '{2}', '{3}', {4}, {5})",
                                    tourneyId.Replace("'", @"\'"),
                                    tourneyName, tourneyName, tourneyName,
                                    sqlDataReader.GetByte(sqlDataReader.GetOrdinal("tourney_level_id")),
                                    surfaceIdObject == DBNull.Value ? "NULL" : surfaceIdObject.ToString()
                                ));
                                currentTourneyCode = tourneyId;
                            }
                        }
                    }

                    foreach (var query in sqlQueriesAddTourney)
                    {
                        sqlCommand.CommandText = query;
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        
        /*public static void SetTourneysHistory()
        {
            using (var sqlConnection = new MySqlConnection(SQL_CONNECTION))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    var tourneysList = new List<Tournament>();
                    sqlCommand.CommandText = "select ID, code, name, level_ID, surface_ID, city from tourneys";
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            var surfaceId = sqlDataReader["surface_ID"];
                            tourneysList.Add(new Tournament(
                                sqlDataReader.GetUInt64(sqlDataReader.GetOrdinal("ID")),
                                sqlDataReader.GetString(sqlDataReader.GetOrdinal("code")),
                                sqlDataReader.GetString(sqlDataReader.GetOrdinal("name")),
                                sqlDataReader.GetByte(sqlDataReader.GetOrdinal("level_ID")),
                                surfaceId == DBNull.Value ? (byte)0 : Convert.ToByte(surfaceId),
                                sqlDataReader.GetString(sqlDataReader.GetOrdinal("city"))
                           ));
                        }
                    }

                    sqlCommand.CommandText = "select tourney_year, tourney_id, tourney_name, tourney_level_id, surface_id from matches order by tourney_id asc, tourney_year desc";
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        var currentTourneyCode = string.Empty;
                        while (sqlDataReader.Read())
                        {
                            var tourneyId = sqlDataReader.GetString(sqlDataReader.GetOrdinal("tourney_id"));
                            var tourneyYear = sqlDataReader.GetUInt32(sqlDataReader.GetOrdinal("tourney_year"));
                            var name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("tourney_name"));
                            var levelId = sqlDataReader.GetByte(sqlDataReader.GetOrdinal("tourney_level_id"));
                            var surfaceId = Convert.ToByte(sqlDataReader["surface_id"] == DBNull.Value ? 0 : sqlDataReader["surface_id"]);

                            var tourneyBaseIndex = tourneysList.FindIndex(item => item.Code == tourneyId);
                            if (tourneyBaseIndex < 0)
                            {
                                // ne doit pas se produire
                            }
                            else
                            {
                                if (tourneysList[tourneyBaseIndex].Name.ToUpper().Replace("WCT", string.Empty).Trim() != name.ToUpper().Replace("WCT", string.Empty).Trim())
                                {
                                    // update du nom
                                    using (var sqlConnectionBis = new MySqlConnection(SQL_CONNECTION))
                                    {
                                        sqlConnectionBis.Open();
                                        using (var sqlCommandBis = sqlConnectionBis.CreateCommand())
                                        {
                                            sqlCommandBis.CommandText = string.Format("insert into tourneys_name_and_city_history (ID, name, last_year) values ({0}, '{1}', {2})",
                                                tourneysList[tourneyBaseIndex].Id, name.Replace("'", @"\'"), tourneyYear);
                                            sqlCommandBis.ExecuteNonQuery();
                                        }
                                    }
                                    tourneysList[tourneyBaseIndex] = new Tournament(tourneysList[tourneyBaseIndex].Id, tourneyId, name, tourneysList[tourneyBaseIndex].LevelId, tourneysList[tourneyBaseIndex].SurfaceId);
                                }
                                if (tourneysList[tourneyBaseIndex].LevelId != levelId)
                                {
                                    // update du level Id
                                    using (var sqlConnectionBis = new MySqlConnection(SQL_CONNECTION))
                                    {
                                        sqlConnectionBis.Open();
                                        using (var sqlCommandBis = sqlConnectionBis.CreateCommand())
                                        {
                                            sqlCommandBis.CommandText = string.Format("insert into tourneys_level_history (ID, level_ID, last_year) values ({0}, {1}, {2})",
                                                tourneysList[tourneyBaseIndex].Id, levelId, tourneyYear);
                                            sqlCommandBis.ExecuteNonQuery();
                                        }
                                    }
                                    tourneysList[tourneyBaseIndex] = new Tournament(tourneysList[tourneyBaseIndex].Id, tourneyId, tourneysList[tourneyBaseIndex].Name, levelId, tourneysList[tourneyBaseIndex].SurfaceId);
                                }
                                if (tourneysList[tourneyBaseIndex].SurfaceId != surfaceId)
                                {
                                    // update du surface Id
                                    using (var sqlConnectionBis = new MySqlConnection(SQL_CONNECTION))
                                    {
                                        sqlConnectionBis.Open();
                                        using (var sqlCommandBis = sqlConnectionBis.CreateCommand())
                                        {
                                            sqlCommandBis.CommandText = string.Format("insert into tourneys_surface_history (ID, surface_ID, last_year) values ({0}, {1}, {2})",
                                                tourneysList[tourneyBaseIndex].Id, surfaceId == 0 ? "NULL" : (object)surfaceId, tourneyYear);
                                            sqlCommandBis.ExecuteNonQuery();
                                        }
                                    }
                                    tourneysList[tourneyBaseIndex] = new Tournament(tourneysList[tourneyBaseIndex].Id, tourneyId, tourneysList[tourneyBaseIndex].Name, tourneysList[tourneyBaseIndex].LevelId, surfaceId);
                                }
                            }
                        }
                    }
                }
            }
        }*/

        /// <summary>
        /// Charge les lignes d'un fichier de matchs ATP, avec ou sans les entêtes.
        /// </summary>
        /// <param name="year">Année du fichier.</param>
        /// <param name="noHeadersRow">Avec ou sans entêtes.</param>
        /// <returns>Liste des lignes du fichier.</returns>
        public static List<string> GetFileRows(int year, bool noHeadersRow)
        {
            var realFilePath = string.Format(DATA_FILE_PATH, year);

            if (!File.Exists(realFilePath))
                return new List<string>();

            var fullData = string.Empty;
            using (StreamReader reader = new StreamReader(realFilePath))
                fullData = reader.ReadToEnd();

            var rows = fullData.Split(ROW_SEPARATOR).ToList();

            if (noHeadersRow && rows.Count > 0)
                rows.RemoveAt(0);

            // la dernière ligne d'un fichier csv est souvent vide
            if (rows.Count > 0 && string.IsNullOrWhiteSpace(rows[rows.Count - 1]))
                rows.RemoveAt(rows.Count - 1);

            return rows;
        }

        // construit une chaîne valide SQL pour l'insertion de données chaîne issues d'un tableau
        public static string ColumnsStringToSqlInsert(string[] columns, bool noQuotes)
        {
            if (columns == null)
                columns = new string[] { };

            var cleanedColumns = new string[columns.Length];
            for (var i = 0; i < columns.Length; i++)
                cleanedColumns[i] = columns[i].Replace("'", @"\'");

            return noQuotes ? string.Join(", ", cleanedColumns) : string.Concat("'", string.Join("', '", cleanedColumns), "'");
        }

        // sert à la construction des joueurs
        public struct Player
        {
            private string _name;
            private string _nationality;
            private string _hand;
            private uint? _height;
            private DateTime? _dateOfBirth;

            public string Name { get { return _name; } }
            public string Nationality { get { return _nationality; } }
            public string Hand { get { return _hand; } }
            public uint? Height { get { return _height; } }
            public DateTime? DateOfBirth { get { return _dateOfBirth; } }

            public Player(string name, string nationality, string hand, uint? height, DateTime? dateOfBirth)
            {
                _name = name;
                _nationality = nationality;
                _hand = hand;
                _height = height;
                _dateOfBirth = dateOfBirth;
            }
        }

        // sert à la reconstruction des scores
        public struct RowMatch
        {
            private uint _matchNum;
            private string _tourney;

            public uint MatchNum { get { return _matchNum; } }
            public string Tourney { get { return _tourney; } }

            public RowMatch(uint matchNum, string tourney)
            {
                _matchNum = matchNum;
                _tourney = tourney;
            }
        }

        // sert à la refactorisation des tournois
        public struct Tournament
        {
            private ulong _id;
            private string _code;
            private string _name;
            private string _city;
            private byte _levelId;
            private byte _surfaceId;
            private byte? _slotOrder;
            private bool _isIndoor;
            private ulong? _substituteId;

            public ulong Id { get { return _id; } }
            public string Code { get { return _code; } }
            public string Name { get { return _name; } }
            public string City { get { return _city; } }
            public byte LevelId { get { return _levelId; } }
            public byte SurfaceId { get { return _surfaceId; } }
            public byte? SlotOrder { get { return _slotOrder; } }
            public bool IsIndoor { get { return _isIndoor; } }
            public ulong? SubstituteId { get { return _substituteId; } }

            public Tournament(ulong id, string code, string name, byte levelId, byte surfaceId, string city, byte? slotOrder, bool isIndoor, ulong? substituteId)
            {
                _id = id;
                _code = code;
                _name = name;
                _levelId = levelId;
                _surfaceId = surfaceId;
                _city = city;
                _slotOrder = slotOrder;
                _isIndoor = isIndoor;
                _substituteId = substituteId;
            }
        }

        public struct Edition
        {
            private ulong _id;
            private ulong _tournamentId;
            private int _year;
            private int _drawSize;
            private DateTime _dateBegin;
            private byte _levelId;
            private byte _surfaceId;
            private byte? _slotOrder;
            private bool _isIndoor;
            private ulong? _substituteId;
            private string _name;
            private string _city;

            public ulong Id { get { return _id; } }
            public ulong TournamentId { get { return _tournamentId; } }
            public int Year { get { return _year; } }
            public int DrawSize { get { return _drawSize; } }
            public DateTime DateBegin { get { return _dateBegin; } }
            public byte LevelId { get { return _levelId; } }
            public byte SurfaceId { get { return _surfaceId; } }
            public byte? SlotOrder { get { return _slotOrder; } }
            public bool IsIndoor { get { return _isIndoor; } }
            public ulong? SubstituteId { get { return _substituteId; } }
            public string Name { get { return _name; } }
            public string City { get { return _city; } }

            public Edition(Dictionary<string, string> rowValues, Tournament tournament, int year, ulong id = 0)
            {
                _id = id;
                _tournamentId = tournament.Id;
                _year = year;
                _drawSize = Convert.ToInt16(rowValues["draw_size"]);
                _dateBegin = Convert.ToDateTime(string.Concat(rowValues["tourney_date"].Substring(0, 4), "-", rowValues["tourney_date"].Substring(4, 2), "-", rowValues["tourney_date"].Substring(6, 2)));
                _levelId = tournament.LevelId;
                _surfaceId = tournament.SurfaceId;
                _name = tournament.Name;
                _city = tournament.City;
                _slotOrder = tournament.SlotOrder;
                _isIndoor = tournament.IsIndoor;
                _substituteId = tournament.SubstituteId;
            }
        }

        /// <summary>
        /// Procède à l'écriture dans un fichier de log.
        /// </summary>
        /// <remarks>Les logs seronts ajoutés à ceux déjà existants dans le fichier.</remarks>
        /// <param name="file">Chemin d'accès du fichier de log.</param>
        /// <param name="logs">Lignes à écrire.</param>
        private static void WriteLog(string file, params string[] logs)
        {
            logs = logs ?? new string[] { };
            logs = logs.Where(log => !string.IsNullOrWhiteSpace(log)).ToArray();

            if (logs.Length > 0)
            {
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    foreach (var log in logs)
                        sw.WriteLine(log);
                }
            }
        }
    }
}
