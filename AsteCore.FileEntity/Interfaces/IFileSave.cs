using System;
using System.Collections.Generic;
using System.Text;

namespace AsteCore.FileEntity.Interfaces
{
    internal interface IFileSave
    {
        void Save();
        Task SaveAsync();
    }
}
