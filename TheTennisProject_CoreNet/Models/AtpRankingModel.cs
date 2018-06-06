using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheTennisProject_CoreNet.Models
{
    /// <summary>
    /// Underlaying model for "AtpRanking" view.
    /// </summary>
    public class AtpRankingModel
    {
        /// <summary>
        /// List of ranked players.
        /// </summary>
        public IReadOnlyCollection<Internals.AtpRanking> List { get; private set; }
        /// <summary>
        /// Date of ranking.
        /// </summary>
       // [DataType(DataType.Date)]
       // [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="date">Date of ranking.</param>
        public AtpRankingModel(DateTime date)
        {
            Date = date;
            OnDateChange();
        }

        /// <summary>
        /// Compute the ranking list at the specified <see cref="Date"/>.
        /// </summary>
        public void OnDateChange()
        {
            List = Internals.AtpRanking.GetAtpRankingAtDate(Date, true, -1);
        }
    }
}
