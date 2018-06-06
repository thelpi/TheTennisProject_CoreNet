using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheTennisProject_CoreNet.Models.Internals
{
    /// <summary>
    /// Classe de base pour l'ensemble des structures métiers.
    /// </summary>
    public abstract class BaseService
    {
        // Liste de toutes les instances, groupées par type enfant.
        private static Dictionary<Type, Dictionary<ulong, BaseService>> _instances = new Dictionary<Type, Dictionary<ulong, BaseService>>();

        /// <summary>
        /// Identifiant unique.
        /// </summary>
        /// <remarks>Un identifiant unique par classe fille, ce n'est pas un identifiant global.</remarks>
        public ulong ID { get; private set; }

        /// <summary>
        /// Constructeur protégé.
        /// </summary>
        /// <param name="id">Identifiant unique pour le type enfant.</param>
        /// <exception cref="NotUniqueIdException">L'identifiant n'est pas unique.</exception>
        protected BaseService(ulong id)
        {
            Type instanceSubType = GetType();

            if (!_instances.ContainsKey(instanceSubType))
            {
                _instances.Add(instanceSubType, new Dictionary<ulong, BaseService>());
            }
            else if (_instances[instanceSubType].ContainsKey(id))
            {
                throw new NotUniqueIdException(id);
            }

            ID = id;
            _instances[instanceSubType].Add(id, this);
        }

        /// <summary>
        /// Récupère une instance d'un type enfant par son identifiant.
        /// </summary>
        /// <typeparam name="T">Le type de l'instance à cibler (un type enfant de <see cref="BaseService"/>).</typeparam>
        /// <param name="id">L'identifiant recherché.</param>
        /// <returns>L'instance associée, null si elle n'a pas pu être trouvée.</returns>
        protected static T GetByID<T>(ulong id) where T : BaseService
        {
            if (!_instances.ContainsKey(typeof(T)))
            {
                _instances.Add(typeof(T), new Dictionary<ulong, BaseService>());
            }

            if (!_instances[typeof(T)].ContainsKey(id))
            {
                return default(T);
            }

            return (T)Convert.ChangeType(_instances[typeof(T)][id], typeof(T));
        }

        /// <summary>
        /// Récupère toutes les instances d'un type enfant.
        /// </summary>
        /// <typeparam name="T">Le type enfant à cibler.</typeparam>
        /// <returns>Une collection en lecture-seule de toutes les instances du type enfant ciblé.</returns>
        protected static ReadOnlyCollection<T> GetList<T>() where T : BaseService
        {
            if (!_instances.ContainsKey(typeof(T)))
            {
                _instances.Add(typeof(T), new Dictionary<ulong, BaseService>());
            }

            return _instances[typeof(T)].Select(item => item.Value).Cast<T>().ToList().AsReadOnly();
        }

        /// <summary>
        /// Exception levée quand un identifiant n'est pas unique lors de l'appel au constructeur de <see cref="BaseService"/>.
        /// </summary>
        public class NotUniqueIdException : Exception
        {
            /// <summary>
            /// Identifiant en échec.
            /// </summary>
            public ulong ID { get; private set; }

            /// <summary>
            /// Constructeur.
            /// </summary>
            /// <param name="id">Identifiant en échec.</param>
            public NotUniqueIdException(ulong id)
                : base("L'identifiant n'est pas unique.")
            {
                ID = id;
            }
        }
    }
}
