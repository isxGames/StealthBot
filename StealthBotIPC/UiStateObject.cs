using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using StealthBot.Core;
using StealthBot.Core.Config;

namespace StealthBotIpc
{
    /// <summary>
    /// Describes all the information StealthBotUI needs to share with StealthBot during updates.
    /// </summary>
    [ProtoContract()]
    public class UiStateObject
    {
        /// <summary>
        /// Character this state object is intended for.
        /// </summary>
        [ProtoMember(1)]
        public string CharacterName;

        /// <summary>
        /// Type of StateObject being sent to StealthBot.
        /// </summary>
        [ProtoMember(2)]
        public UiStateObjectTypes UiStateObjectType;

        /// <summary>
        /// Command we want the StealthBot instance to perform.
        /// </summary>
        [ProtoMember(3)]
        public SbCommands Command;

        /// <summary>
        /// Configuration instance for configuration updates.
        /// </summary>
        [ProtoMember(4)]
        public Configuration Configuration;

        /// <summary>
        /// Name of the old configuration profile.
        /// </summary>
        [ProtoMember(5)]
        public string OldConfigurationName;

        /// <summary>
        /// Name of the new configuration profile.
        /// </summary>
        [ProtoMember(6)]
        public string NewConfigurationName;

        /// <summary>
        /// Instantiate an instance of UiStateObject.
        /// </summary>
        public UiStateObject(UiStateObjectTypes uiStateObjectType)
        {
            
        }

        /// <summary>
        /// Constructor used for deserialization
        /// </summary>
        public UiStateObject()
        {

        }
    }

    public enum UiStateObjectTypes
    {
        Standard,
        UpdateConfiguration,
    }
}
