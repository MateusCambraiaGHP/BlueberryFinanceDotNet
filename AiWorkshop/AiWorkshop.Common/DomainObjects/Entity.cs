using System;
using System.Collections.Generic;
using System.Text;

namespace BlueBerryFinance.Common.DomainObjects
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public byte Active { get; set; }
    }
}
