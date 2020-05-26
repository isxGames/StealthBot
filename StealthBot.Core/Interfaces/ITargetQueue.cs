using System;
using System.Collections.ObjectModel;

namespace StealthBot.Core.Interfaces
{
    public interface ITargetQueue
    {
        ReadOnlyCollection<QueueTarget> Targets { get; }
        bool IsQueued(Int64 entityId);

        /// <summary>
        /// Enqueue a target.
        /// </summary>
        /// <param name="entityId">Target's ID</param>
        /// <param name="priority">Priority of the target, lower number is higher priority</param>
        /// <param name="type">Type of the target, determines what handles it</param>
        void EnqueueTarget(Int64 entityId, int priority, TargetTypes type);

        /// <summary>
        /// Enqueue a target.
        /// </summary>
        /// <param name="entityId">Target's ID</param>
        /// <param name="priority">Priority of the target, lower number is higher priority</param>
        /// <param name="subPriority">Sub-priority for sorting</param>
        /// <param name="type">Type of the target, determines what handles it</param>
        void EnqueueTarget(Int64 entityId, int priority, int subPriority, TargetTypes type);

        void DequeueTarget(Int64 entityId);

        /// <summary>
        /// Clear the target queue.
        /// </summary>
        void ClearQueue();
    }
}
