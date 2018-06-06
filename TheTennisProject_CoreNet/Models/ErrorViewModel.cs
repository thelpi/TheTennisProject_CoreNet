using System;

namespace TheTennisProject_CoreNet.Models
{
    /// <summary>
    /// Todo
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Todo
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Todo
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}