using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Classe de manipulation des méthodes et données SQL.
    /// </summary>
    /// <remarks>Les exceptions liées aux échecs de connexion à la base de données ne sont pas rattrapées.</remarks>
    public static class SqlTools
    {
        /// <summary>
        /// SQL connection string.
        /// </summary>
        public static readonly string SQL_CONNECTION_STRING = string.Format(Config.GetString(AppKey.ConnectionStringPattern),
            Config.GetString(AppKey.SQL_instance),
            Config.GetString(AppKey.SQL_db),
            Config.GetString(AppKey.SQL_uid),
            Config.GetString(AppKey.SQL_pwd));

        /// <summary>
        /// Exécute une requête qui retourne un jeu de résultats.
        /// </summary>
        /// <remarks>Aucune exception relative à l'exécution du moteur SQL n'est traitée.</remarks>
        /// <param name="query">Commande à exécuter.</param>
        /// <param name="parameters">Liste de paramètres.</param>
        /// <returns>Le jeu de résultats complet déconnecté.</returns>
        /// <exception cref="ArgumentException">La requête ne peut pas être vide ou null.</exception>
        public static DataTableReader ExecuteReader(string query, params SqlParam[] parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("La requête ne peut pas être vide ou null.", nameof(query));

            DataSet set = new System.Data.DataSet();

            using (MySqlConnection connection = new MySqlConnection(SQL_CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    SetParameters(command, parameters);
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(set);
                    }
                }
            }

            return set.CreateDataReader();
        }

        /// <summary>
        /// Exécute une requête qui insère, met à jour ou supprime des données en base.
        /// </summary>
        /// <param name="query">Commande à exécuter.</param>
        /// <param name="parameters">Liste de paramètres.</param>
        /// <returns>Le nombre de lignes impactées par la requête.</returns>
        /// <exception cref="ArgumentException">La requête ne peut pas être vide ou null.</exception>
        public static int ExecuteNonQuery(string query, params SqlParam[] parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("La requête ne peut pas être vide ou null.", nameof(query));

            int results = -1;

            using (MySqlConnection connection = new MySqlConnection(SQL_CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    SetParameters(command, parameters);
                    results = command.ExecuteNonQuery();
                }
            }

            return results;
        }

        /// <summary>
        /// Exécute une requête qui retourne une valeur scalaire.
        /// </summary>
        /// <remarks>Aucune exception relative à l'exécution du moteur SQL n'est traitée.</remarks>
        /// <typeparam name="T">Le type de la valeur scalaire.</typeparam>
        /// <param name="query">Commande à exécuter.</param>
        /// <param name="ifDbNull">Valeur de substitution si le résultat est <see cref="DBNull.Value"/>.</param>
        /// <param name="parameters">Liste de paramètres.</param>
        /// <returns>Un résultat scalaire du type spécifié.</returns>
        /// <exception cref="ArgumentException">La requête ne peut pas être vide ou null.</exception>
        public static T ExecuteScalar<T>(string query, T ifDbNull, params SqlParam[] parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("La requête ne peut pas être vide ou null.", nameof(query));

            object result;
            using (MySqlConnection connection = new MySqlConnection(SQL_CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    SetParameters(command, parameters);
                    result = command.ExecuteScalar();
                    if (result == DBNull.Value || result == null)
                    {
                        result = ifDbNull;
                    }
                }
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        #region Récupération de la valeur typée d'une colonne

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier normal depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static uint GetUint32(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.ToUInt32(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier normal nullable depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière ou null.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static uint? GetUint32Null(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? (uint?)null : Convert.ToUInt32(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier long depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static ulong GetUint64(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.ToUInt64(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier long nullable depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière ou null.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static ulong? GetUint64Null(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? (ulong?)null : Convert.ToUInt64(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier court depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static ushort GetUint16(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.ToUInt16(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier court nullable depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière ou null.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static ushort? GetUint16Null(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? (ushort?)null : Convert.ToUInt16(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier très court depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static byte GetByte(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.ToByte(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un entier très court nullable depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur entière ou null.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static byte? GetByteNull(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? (byte?)null : Convert.ToByte(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère une date depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur date.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static DateTime GetDateTime(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.ToDateTime(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère une date nullable depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur date ou null.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static DateTime? GetDateTimeNull(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? (DateTime?)null : Convert.ToDateTime(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un booléen depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur booléenne.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static bool GetBoolean(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.ToBoolean(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère un booléen nullable depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur booléenne ou null.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static bool? GetBooleanNull(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? (bool?)null : Convert.ToBoolean(reader[columnName]);
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère une chaîne de caractères depuis un jeu de données.
        /// </summary>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La valeur chaîne de caractères.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static string GetString(this DataTableReader reader, string columnName)
        {
            if (!reader.ColumnExists(columnName))
                throw new ArgumentException("La colonne spécifiée n'existe pas dans le jeu de données.", nameof(columnName));
            return Convert.IsDBNull(reader[columnName]) ? null : reader[columnName].ToString();
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Récupère une liste d'identifiants depuis un jeu de données.
        /// </summary>
        /// <remarks>Le stockage est sous forme de chaîne avec séparateur ";".</remarks>
        /// <param name="reader">Ligne actuelle du jeu de données.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La liste d'identifiants.</returns>
        /// <exception cref="ArgumentException">La colonne spécifiée n'existe pas dans le jeu de données.</exception>
        public static IEnumerable<ulong> ToIdList(this DataTableReader reader, string columnName)
        {
            string[] components = reader.GetString(columnName).Split(';');

            List<ulong> idList = new List<ulong>();
            foreach (string component in components)
            {
                ulong id;
                if (ulong.TryParse(component, out id))
                {
                    idList.Add(id);
                }
            }

            return idList;
        }

        #endregion

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Convertit un jeu de données SQL en un dictionnaire de valeurs typées.
        /// Les colonnes n'étant pas du type spécifié (ou d'une classe fille) ne sont pas retournées.
        /// </summary>
        /// <typeparam name="T">Le type des valeurs du dictionnaire. N'importe quel type intégral, éventuellement nullable.</typeparam>
        /// <param name="reader">Le jeu de données SQL à convertir.</param>
        /// <param name="dbNullToDefault">Optionnel. Permet de convertir la valeur <see cref="DBNull.Value"/> dans la valeur par défaut du type cible.</param>
        /// <returns>Un dictionnaire de valeurs, où la clé est le nom de la colonne.</returns>
        public static IDictionary<string, T> ToDynamicDictionnary<T>(this DataTableReader reader, bool dbNullToDefault = false)
        {
            // Si T est nullable, récupère le type sous-jacent car c'est lui qui sera récupéré au DBType
            Type typeOfT = typeof(T);
            if (typeOfT.IsGenericType && typeOfT.GetGenericTypeDefinition() == typeof(Nullable<>))
                typeOfT = Nullable.GetUnderlyingType(typeOfT);

            Dictionary<string, T> values = new Dictionary<string, T>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetFieldType(i) == typeOfT || reader.GetFieldType(i).IsSubclassOf(typeOfT))
                {
                    values.Add(reader.GetName(i), reader.IsDBNull(i) && dbNullToDefault ? default(T) : (T)reader[i]);
                }
            }
            return values;
        }

        /// <summary>
        /// Extension <see cref="DataTableReader"/>. Détermine si une colonne existe dans un jeu de données.
        /// </summary>
        /// <remarks>Des faux positifs peuvent se produire pour certaines langues (voir la documentation de la classe <see cref="DataTableReader"/> pour plus de détails).</remarks>
        /// <param name="reader">Le jeu de données SQL à vérifier.</param>
        /// <param name="columnName">Le nom de la colonne à vérifier.</param>
        /// <returns>Vrai si la colonne existe, Faux si elle n'existe pas.</returns>
        public static bool ColumnExists(this DataTableReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Protège une chaîne de caractères pour une utilisation dans une requête SQL.
        /// </summary>
        /// <param name="data">La valeur à échapper.</param>
        /// <returns>Valeur échappée.</returns>
        public static string EscapeString(string data)
        {
            return data == null ? null : MySqlHelper.EscapeString(data);
        }

        /// <summary>
        /// Assigne une liste de paramètres à une commande SQL.
        /// </summary>
        /// <param name="command">Une instance de <see cref="MySqlCommand"/>.</param>
        /// <param name="parameters">Un tableau de <see cref="SqlParam"/>.</param>
        private static void SetParameters(MySqlCommand command, SqlParam[] parameters)
        {
            if (command != null && parameters != null)
            {
                foreach (SqlParam p in parameters)
                {
                    command.Parameters.Add(p.ToMySqlParameter());
                }
            }
        }

        /// <summary>
        /// Crée une requête SQL d'insertion.
        /// </summary>
        /// <param name="table">La table.</param>
        /// <param name="columns">Nom de la colonne / Valeur de la colonne (ou nom du paramètre).</param>
        /// <returns>Requête SQL d'insertion.</returns>
        public static string BuildInsertQuery(string table, Dictionary<string, string> columns)
        {
            return string.Concat("insert into ", table, " (" + string.Join(", ", columns.Keys) + ") values (" + string.Join(", ", columns.Values) + ")");
        }

        /// <summary>
        /// Classe de gestion des requêtes SQL préparées.
        /// </summary>
        public class SqlPrepared : IDisposable
        {
            // connexion interne
            private MySqlConnection _connection;
            // commande interne
            private MySqlCommand _command;

            /// <summary>
            /// Constructeur. Prépare une requête de type insertion, mise à jour ou suppression.
            /// </summary>
            /// <param name="query">Requête.</param>
            /// <param name="parameters">Paramètres (non valorisés).</param>
            /// <exception cref="ArgumentException">La requête ne peut pas être vide ou null.</exception>
            public SqlPrepared(string query, params SqlParam[] parameters)
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    throw new ArgumentException("La requête ne peut pas être vide ou null.", nameof(query));
                }

                _connection = new MySqlConnection(SQL_CONNECTION_STRING);
                _connection.Open();

                _command = _connection.CreateCommand();
                _command.CommandText = query;
                SetParameters(_command, parameters);

                _command.Prepare();
            }

            /// <summary>
            /// Procède à l'exécution de la requête préparée.
            /// </summary>
            /// <param name="paramsValues">Jeu de paramètres valorisés.</param>
            /// <returns>Nombre de lignes impactées.</returns>
            /// <exception cref="InvalidOperationException">Commande invalide.</exception>
            public int Execute(Dictionary<string, object> paramsValues)
            {
                if (_command == null)
                {
                    throw new InvalidOperationException("Commande invalide.");
                }

                if (_command.Parameters != null && paramsValues != null)
                {
                    foreach (string param in paramsValues.Keys)
                    {
                        _command.Parameters[param].Value = paramsValues[param] ?? DBNull.Value;
                    }
                }

                return _command.ExecuteNonQuery();
            }

            /// <summary>
            /// Libère les ressources associées à l'instance.
            /// </summary>
            public void Dispose()
            {
                try
                {
                    _command.Dispose();
                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Represents a simplified version of <see cref="MySqlParameter"/>.
    /// </summary>
    public struct SqlParam
    {
        #region Membres privés

        private string _name;
        private DbType _type;
        private object _value;

        #endregion

        #region Accesseurs publics

        /// <summary>
        /// Le nom du paramètre. Commence toujours par un @ (il sera automatiquement rajouté s'il est omis).
        /// </summary>
        public string Name { get { return _name; } }
        /// <summary>
        /// Le type SQL du paramètre.
        /// </summary>
        public DbType Type { get { return _type; } }
        /// <summary>
        /// La valeur du paramètre. <c>Null</c> est assimilé à <see cref="DBNull.Value"/>. La cohérence avec <see cref="Type"/> n'est pas vérifiée.
        /// </summary>
        public object Value { get { return _value ?? DBNull.Value; } }

        #endregion

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom du paramètre.</param>
        /// <param name="type">Type SQL du paramètre.</param>
        /// <exception cref="ArgumentNullException">Le paramètre spécifié ne peut pas être Null ou vide.</exception>
        public SqlParam(string name, DbType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            name = name.Trim();

            _name = name.StartsWith("@") ? name : string.Concat("@", name);
            _type = type;
            _value = DBNull.Value;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom du paramètre.</param>
        /// <param name="type">Type SQL du paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <exception cref="ArgumentNullException">Le paramètre spécifié ne peut pas être Null ou vide.</exception>
        public SqlParam(string name, DbType type, object value)
            : this(name, type)
        {
            _value = value ?? DBNull.Value;
        }

        /// <summary>
        /// Transforme l'instance en une instance de type <see cref="MySqlParameter"/>.
        /// </summary>
        /// <returns>Instance de <see cref="MySqlParameter"/>.</returns>
        public MySqlParameter ToMySqlParameter()
        {
            return new MySqlParameter()
            {
                DbType = _type,
                Direction = ParameterDirection.Input,
                ParameterName = _name,
                Value = _value
            };
        }
    }
}
