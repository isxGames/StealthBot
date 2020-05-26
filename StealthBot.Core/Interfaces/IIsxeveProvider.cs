﻿using EVE.ISXEVE;
using EVE.ISXEVE.Interfaces;

namespace StealthBot.Core.Interfaces
{
    using EVE = EVE.ISXEVE.EVE;

    public interface IIsxeveProvider
    {
        ISXEVE Isxeve { get; }
        IEve Eve { get; }
        Me Me { get; }
    }
}
